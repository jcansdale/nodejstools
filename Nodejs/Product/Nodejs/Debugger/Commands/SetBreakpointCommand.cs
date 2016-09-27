﻿//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class SetBreakpointCommand : DebuggerCommand {
        private static string _pathSeperatorCharacterGroup = string.Format("[{0}{1}]", Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        private readonly Dictionary<string, object> _arguments;
        private readonly NodeBreakpoint _breakpoint;
        private readonly NodeModule _module;
        private readonly SourceMapper _sourceMapper;
        private readonly FilePosition _position;

        public SetBreakpointCommand(int id, NodeModule module, NodeBreakpoint breakpoint, bool withoutPredicate, bool remote, SourceMapper sourceMapper = null)
            : base(id, "setbreakpoint") {
            Utilities.ArgumentNotNull("breakpoint", breakpoint);

            _module = module;
            _breakpoint = breakpoint;
            _sourceMapper = sourceMapper;

            _position = breakpoint.GetPosition(_sourceMapper);

            // Zero based line numbers
            int line = _position.Line;

            // Zero based column numbers
            // Special case column to avoid (line 0, column 0) which
            // Node (V8) treats specially for script loaded via require
            // Script wrapping process: https://github.com/joyent/node/blob/v0.10.26-release/src/node.js#L880
            int column = _position.Column;
            if (line == 0) {
                column += NodeConstants.ScriptWrapBegin.Length;
            }

            _arguments = new Dictionary<string, object> {
                { "line", line },
                { "column", column }
            };

            if (_module != null) {
                _arguments["type"] = "scriptId";
                _arguments["target"] = _module.Id;
            } else if (remote) {
                _arguments["type"] = "scriptRegExp";
                _arguments["target"] = CreateRemoteScriptRegExp(_position.FileName);
            } else {
                _arguments["type"] = "scriptRegExp";
                _arguments["target"] = CreateLocalScriptRegExp(_position.FileName);
            }

            if (!NodeBreakpointBinding.GetEngineEnabled(_breakpoint.Enabled, _breakpoint.BreakOn, 0)) {
                _arguments["enabled"] = false;
            }

            if (withoutPredicate) {
                return;
            }

            int ignoreCount = NodeBreakpointBinding.GetEngineIgnoreCount(_breakpoint.BreakOn, 0);
            if (ignoreCount > 0) {
                _arguments["ignoreCount"] = ignoreCount;
            }

            if (!string.IsNullOrEmpty(_breakpoint.Condition)) {
                _arguments["condition"] = _breakpoint.Condition;
            }
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }

        public int BreakpointId { get; private set; }

        public int? ScriptId { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JToken body = response["body"];
            BreakpointId = (int)body["breakpoint"];

            int? moduleId = _module != null ? _module.Id : (int?)null;
            ScriptId = (int?)body["script_id"] ?? moduleId;

            // Handle breakpoint actual location fixup
            JArray actualLocations = (JArray)body["actual_locations"] ?? new JArray();
            if (actualLocations.Count > 0) {
                var location = actualLocations[0];
                ScriptId = ScriptId ?? (int?)location["script_id"];
                Line = (int)location["line"];
                var column = (int)location["column"];
                Column = Line == 0 ? column - NodeConstants.ScriptWrapBegin.Length : column;
            } else {
                Line = _position.Line;
                Column = _position.Column;
            }
        }

        private static string CreateRemoteScriptRegExp(string filePath) {
            string fileName = Path.GetFileName(filePath) ?? string.Empty;
            string start = fileName == filePath ? "^" : _pathSeperatorCharacterGroup;
            return string.Format("{0}{1}$", start, CreateCaseInsensitiveRegExpFromString(fileName));
        }

        public static string CreateLocalScriptRegExp(string filePath) {
            return string.Format("^{0}$", CreateCaseInsensitiveRegExpFromString(filePath));
        }

        /// <summary>
        /// Convert a string into a case-insensitive regular expression.
        /// 
        /// This is a workaround for the fact that we cannot pass a regex case insensitive modifier to the Node (V8) engine.
        /// </summary>
        private static string CreateCaseInsensitiveRegExpFromString(string str) {
            var builder = new StringBuilder();
            foreach (var ch in Regex.Escape(str)) {
                string upper = ch.ToString(CultureInfo.InvariantCulture).ToUpper();
                string lower = ch.ToString(CultureInfo.InvariantCulture).ToLower();
                if (upper != lower) {
                    builder.Append('[');
                    builder.Append(upper);
                    builder.Append(lower);
                    builder.Append(']');
                } else {
                    builder.Append(upper);
                }
            }
            return builder.ToString();
        }
    }
}
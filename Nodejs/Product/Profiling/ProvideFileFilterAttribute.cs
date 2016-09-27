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

using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools {
    class ProvideFileFilterAttribute : RegistrationAttribute {
        private readonly string _id, _name, _filter;
        private readonly int _sortPriority;

        public ProvideFileFilterAttribute(string projectGuid, string name, string filter, int sortPriority) {
            _name = name;
            _id = projectGuid;
            _filter = filter;
            _sortPriority = sortPriority;
        }

        public override void Register(RegistrationContext context) {
            using (var engineKey = context.CreateKey("Projects\\" + _id + "\\Filters\\" + _name)) {
                engineKey.SetValue(String.Empty, _filter);
                engineKey.SetValue("SortPriority", _sortPriority);
            }
        }

        public override void Unregister(RegistrationContext context) {
        }
    }
}

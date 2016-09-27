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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.NodejsTools.Project {
    //Set the projectsTemplatesDirectory to a non-existant path to prevent VS from including the working directory as a valid template path
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Description("Node.js Project Package")]
    [Guid("2C52FA27-791E-4D04-9F82-234BBB58DE78")]
    [DeveloperActivity(NodejsConstants.JavaScript, typeof(NodejsProjectPackage))]
    [ProvideObject(typeof(NodejsGeneralPropertyPage))]
    [ProvideProjectFactory(typeof(BaseNodeProjectFactory), NodejsConstants.JavaScript, NodeFileFilter, "njsproj", "njsproj", ".\\NullPath", LanguageVsTemplate = NodejsConstants.Nodejs, SortPriority = 0x17)]
    public class NodejsProjectPackage : CommonProjectPackage {
        internal const string NodeFileFilter = "Node.js Project Files (*.njsproj);*.njsproj";

        public override ProjectFactory CreateProjectFactory() {
            return new BaseNodeProjectFactory(this);
        }

        public override CommonEditorFactory CreateEditorFactory() {
            return null;
        }

        public override CommonEditorFactory CreateEditorFactoryPromptForEncoding() {
            return null;
        }

        protected override void Initialize() {
            RegisterProjectFactory(new BaseNodeProjectFactory(this));

            base.Initialize();
        }

        /// <summary>
        /// This method is called to get the icon that will be displayed in the
        /// Help About dialog when this package is selected.
        /// </summary>
        /// <returns>The resource id corresponding to the icon to display on the Help About dialog</returns>
        public override uint GetIconIdForAboutBox() {
            return 0;
        }
        /// <summary>
        /// This method is called during Devenv /Setup to get the bitmap to
        /// display on the splash screen for this package.
        /// </summary>
        /// <returns>The resource id corresponding to the bitmap to display on the splash screen</returns>
        public override uint GetIconIdForSplashScreen() {
            return 0;
        }
        /// <summary>
        /// This methods provides the product official name, it will be
        /// displayed in the help about dialog.
        /// </summary>
        public override string GetProductName() {
            return "Node.js Project";
        }

        /// <summary>
        /// This methods provides the product description, it will be
        /// displayed in the help about dialog.
        /// </summary>
        public override string GetProductDescription() {
            return NodejsConstants.Nodejs;
            //return Resources.ProductDescription;
        }
        /// <summary>
        /// This methods provides the product version, it will be
        /// displayed in the help about dialog.
        /// </summary>
        public override string GetProductVersion() {
            return this.GetType().Assembly.GetName().Version.ToString();
        }
    }
}

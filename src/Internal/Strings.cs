using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNetOpenAuth.AspNet.Internal
{
    [DebuggerNonUserCode, CompilerGenerated, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    internal class Strings
    {
        private static CultureInfo resourceCulture;
        private static ResourceManager resourceMan;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    ResourceManager manager = new ResourceManager("DotNetOpenAuth.Strings", typeof(DotNetOpenAuth.IEmbeddedResourceRetrieval).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }

        internal static string EmptyStringNotAllowed
        {
            get
            {
                return ResourceManager.GetString("EmptyStringNotAllowed", resourceCulture);
            }
        }

        internal static string InvalidArgument
        {
            get
            {
                return ResourceManager.GetString("InvalidArgument", resourceCulture);
            }
        }
    }
}

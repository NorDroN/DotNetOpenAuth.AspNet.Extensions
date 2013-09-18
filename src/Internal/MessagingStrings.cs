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
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), CompilerGenerated, DebuggerNonUserCode]
    internal class MessagingStrings
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
                    ResourceManager manager = new ResourceManager("DotNetOpenAuth.Messaging.MessagingStrings", typeof(DotNetOpenAuth.IEmbeddedResourceRetrieval).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }

        internal static string UnexpectedNullOrEmptyKey
        {
            get
            {
                return ResourceManager.GetString("UnexpectedNullOrEmptyKey", resourceCulture);
            }
        }

        internal static string UnexpectedNullValue
        {
            get
            {
                return ResourceManager.GetString("UnexpectedNullValue", resourceCulture);
            }
        }
    }
}

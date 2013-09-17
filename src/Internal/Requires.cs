using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace nordron.OAuth
{
    internal static class Requires
    {
        [DebuggerStepThrough]
        internal static T NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        [DebuggerStepThrough]
        internal static string NotNullOrEmpty(string value, string parameterName)
        {
            NotNull<string>(value, parameterName);
            True(value.Length > 0, parameterName, Strings.EmptyStringNotAllowed);
            return value;
        }

        [DebuggerStepThrough]
        internal static void True(bool condition, [Optional, DefaultParameterValue(null)] string parameterName, [Optional, DefaultParameterValue(null)] string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message ?? Strings.InvalidArgument, parameterName);
            }
        }
    }
}

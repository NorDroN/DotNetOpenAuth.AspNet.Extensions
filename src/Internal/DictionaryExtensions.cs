using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nordron.OAuth
{
    internal static class DictionaryExtensions
    {
        public static void AddItemIfNotEmpty(this IDictionary<string, string> dictionary, string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!string.IsNullOrEmpty(value))
            {
                dictionary[key] = value;
            }
        }
    }
}

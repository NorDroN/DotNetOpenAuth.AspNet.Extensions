using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nordron.OAuth
{
    public static class MessagingUtilities
    {
        private static readonly string[] UriRfc3986CharsToEscape;

        static MessagingUtilities()
        {
            UriRfc3986CharsToEscape = new string[] { "!", "*", "'", "(", ")" };
        }

        internal static void AppendQueryArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args)
        {
            Requires.NotNull<UriBuilder>(builder, "builder");
            if ((args != null) && (args.Count<KeyValuePair<string, string>>() > 0))
            {
                StringBuilder builder2 = new StringBuilder(50 + (args.Count<KeyValuePair<string, string>>() * 10));
                if (!string.IsNullOrEmpty(builder.Query))
                {
                    builder2.Append(builder.Query.Substring(1));
                    builder2.Append('&');
                }
                builder2.Append(CreateQueryString(args));
                builder.Query = builder2.ToString();
            }
        }

        internal static string CreateQueryString(IEnumerable<KeyValuePair<string, string>> args)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<string, string>>>(args, "args");
            if (!args.Any<KeyValuePair<string, string>>())
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(args.Count<KeyValuePair<string, string>>() * 10);
            foreach (KeyValuePair<string, string> pair in args)
            {
                ErrorUtilities.VerifyArgument(!string.IsNullOrEmpty(pair.Key), MessagingStrings.UnexpectedNullOrEmptyKey, new object[0]);
                ErrorUtilities.VerifyArgument(pair.Value != null, MessagingStrings.UnexpectedNullValue, new object[] { pair.Key });
                builder.Append(EscapeUriDataStringRfc3986(pair.Key));
                builder.Append('=');
                builder.Append(EscapeUriDataStringRfc3986(pair.Value));
                builder.Append('&');
            }
            builder.Length--;
            return builder.ToString();
        }

        internal static string EscapeUriDataStringRfc3986(string value)
        {
            Requires.NotNull<string>(value, "value");
            StringBuilder builder = new StringBuilder(Uri.EscapeDataString(value));
            for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                builder.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
            }
            return builder.ToString();
        }
    }
}

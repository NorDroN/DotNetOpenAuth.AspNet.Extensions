using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;
using nordron.OAuth.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace nordron.OAuth.Clients
{
    /// <summary>
    /// Mail.Ru authentication client.
    /// </summary>
    public sealed class MailRuClient : OAuth2Client
    {
        private const string AuthorizationServiceEndpoint = "https://connect.mail.ru/oauth/authorize";
        private const string AccessTokenServiceEndpoint = "https://connect.mail.ru/oauth/token";
        private const string UserInfoServiceEndpoint = "http://www.appsmail.ru/platform/api";

        private readonly string appId;
        private readonly string appSecret;

        /// <summary>
        /// Initializes a new instance of the MailRuClient class.
        /// </summary>
        /// <param name="appId">
        /// The app id.
        /// </param>
        /// <param name="appSecret">
        /// The app secret.
        /// </param>
        public MailRuClient(string appId, string appSecret)
            : base("mailru")
        {
            Requires.NotNullOrEmpty(appId, "appId");
            Requires.NotNullOrEmpty(appSecret, "appSecret");
            this.appId = appId;
            this.appSecret = appSecret;
        }

        /// <summary>
        /// The get service login url.
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>An absolute URI.</returns>
        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var builder = new UriBuilder(AuthorizationServiceEndpoint);

            var args = new Dictionary<string, string>();

            args.Add("client_id", this.appId);
            args.Add("response_type", "code");
            args.Add("redirect_uri", returnUrl.AbsoluteUri);

            builder.AppendQueryArgs(args);

            return builder.Uri;
        }

        /// <summary>
        /// Obtains an access token given an authorization code and callback URL.
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <param name="authorizationCode">
        /// The authorization code.
        /// </param>
        /// <returns>
        /// The access token.
        /// </returns>
        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            // Source documents
            // http://api.mail.ru/docs/guides/oauth/sites/

            var uriBuilder = new UriBuilder(AccessTokenServiceEndpoint);

            var args = new Dictionary<string, string>();

            args.Add("client_id", this.appId);
            args.Add("client_secret", this.appSecret);
            args.Add("grant_type", "authorization_code");
            args.Add("code", authorizationCode);
            args.Add("redirect_uri", returnUrl.AbsoluteUri);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                var text = webClient.UploadString(uriBuilder.Uri, string.Empty);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);
                return data["access_token"].Value<string>();
            }
        }

        /// <summary>
        /// The get user data.
        /// </summary>
        /// <param name="accessToken">
        /// The access token.
        /// </param>
        /// <returns>A dictionary of profile data.</returns>
        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            // Source documents
            // http://api.mail.ru/docs/guides/restapi/
            // http://api.mail.ru/docs/reference/rest/users.getInfo/

            var uriBuilder = new UriBuilder(UserInfoServiceEndpoint);

            var args = new Dictionary<string, string>();

            args.Add("app_id", this.appId);
            args.Add("method", "users.getInfo");
            args.Add("secure", "1");
            args.Add("session_key", accessToken);

            // workaround for current design, oauth_token is always present in URL, so we need emulate it for correct request signing 
            args.Add("oauth_token", accessToken);

            // sign=hex_md5('app_id={client_id}method=users.getInfosecure=1session_key={access_token}{secret_key}')
            var signature = string.Concat(args.OrderBy(x => x.Key).Select(x => string.Format("{0}={1}", x.Key, x.Value)).ToList());
            signature = (signature + this.appSecret).GetMd5Hash();

            // removing fake param to prevent dups
            args.Remove("oauth_token");

            args.Add("sig", signature);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                var text = webClient.DownloadString(uriBuilder.Uri);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);

                var user = data[0];

                var dictionary = new Dictionary<string, string>();
                dictionary.AddItemIfNotEmpty("id", user["uid"].Value<string>());
                dictionary.AddItemIfNotEmpty("firstName", user["first_name"].Value<string>());
                dictionary.AddItemIfNotEmpty("lastName", user["last_name"].Value<string>());
                dictionary.AddItemIfNotEmpty("email", user["email"].Value<string>());
                return dictionary;
            }
        }
    }
}

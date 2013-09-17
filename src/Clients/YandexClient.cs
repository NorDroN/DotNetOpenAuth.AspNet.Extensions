using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace nordron.OAuth.Clients
{
    /// <summary>
    /// Yandex authentication client.
    /// </summary>
    public sealed class YandexClient : OAuth2Client
    {
        private const string AuthorizationServiceEndpoint = "https://oauth.yandex.ru/authorize";
        private const string AccessTokenServiceEndpoint = "https://oauth.yandex.ru/token";
        private const string UserInfoServiceEndpoint = "https://login.yandex.ru/info";

        private readonly string appId;
        private readonly string appSecret;


        /// <summary>
        /// Initializes a new instance of the YandexClient class.
        /// </summary>
        /// <param name="appId">
        /// The app id.
        /// </param>
        /// <param name="appSecret">
        /// The app secret.
        /// </param>
        public YandexClient(string appId, string appSecret)
            : base("yandex")
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
            // Source document 
            // http://api.yandex.com/oauth/doc/dg/yandex-oauth-dg.pdf

            var uriBuilder = new UriBuilder(UserInfoServiceEndpoint);

            var args = new Dictionary<string, string>();

            args.Add("oauth_token", accessToken);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                var text = webClient.DownloadString(uriBuilder.Uri);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);

                var names = data["real_name"].Value<string>().Split(' ');

                var dictionary = new Dictionary<string, string>();
                dictionary.AddItemIfNotEmpty("id", data["id"].Value<string>());
                dictionary.AddItemIfNotEmpty("firstName", names.Any() ? names.First() : data["display_name"].Value<string>());
                dictionary.AddItemIfNotEmpty("lastName", names.Count() > 1 ? names.Last() : string.Empty);
                dictionary.AddItemIfNotEmpty("email", data["default_email"].Value<string>());
                return dictionary;
            }
        }
    }
}

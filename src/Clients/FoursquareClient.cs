using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace nordron.OAuth.Clients
{
    /// <summary>
    /// Foursquare authentication client.
    /// </summary>
    public sealed class FoursquareClient : OAuth2Client
    {
        private const string AuthorizationServiceEndpoint = "https://foursquare.com/oauth2/authorize";
        private const string AccessTokenServiceEndpoint = "https://foursquare.com/oauth2/access_token";
        private const string UserInfoServiceEndpoint = "https://api.foursquare.com/v2/users/self";

        private readonly string appId;
        private readonly string appSecret;

        /// <summary>
        /// Initializes a new instance of the FoursquareClient class.
        /// </summary>
        /// <param name="appId">
        /// The app id.
        /// </param>
        /// <param name="appSecret">
        /// The app secret.
        /// </param>
        public FoursquareClient(string appId, string appSecret)
            : base("foursquare")
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
            var uriBuilder = new UriBuilder(AccessTokenServiceEndpoint);

            var args = new Dictionary<string, string>();

            args.Add("client_id", this.appId);
            args.Add("client_secret", this.appSecret);
            args.Add("grant_type", "authorization_code");
            args.Add("redirect_uri", returnUrl.AbsoluteUri);
            args.Add("code", authorizationCode);

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
            var uriBuilder = new UriBuilder(UserInfoServiceEndpoint);

            var args = new Dictionary<string, string>();

            args.Add("oauth_token", accessToken);
            args.Add("v", DateTime.Now.ToString("yyyyMMdd"));

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                var text = webClient.DownloadString(uriBuilder.Uri);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);

                var user = data["response"]["user"];

                var dictionary = new Dictionary<string, string>();
                dictionary.AddItemIfNotEmpty("id", user["id"].Value<string>());
                dictionary.AddItemIfNotEmpty("firstName", user["firstName"].Value<string>());
                dictionary.AddItemIfNotEmpty("lastName", user["lastName"].Value<string>());
                dictionary.AddItemIfNotEmpty("email", user["contact"]["email"].Value<string>());
                return dictionary;
            }
        }
    }
}

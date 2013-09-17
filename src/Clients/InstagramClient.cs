using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace nordron.OAuth.Clients
{
    /// <summary>
    /// Instagram authentication client.
    /// </summary>
    public sealed class InstagramClient : OAuth2Client
    {
        private const string AuthorizationServiceEndpoint = "https://api.instagram.com/oauth/authorize";
        private const string AccessTokenServiceEndpoint = "https://api.instagram.com/oauth/access_token";
        private const string UserInfoServiceEndpoint = "https://api.instagram.com/v1/users/{{user-id}}/";

        private readonly string appId;
        private readonly string appSecret;

        private string currentUserId;

        /// <summary>
        /// Initializes a new instance of the InstagramClient class.
        /// </summary>
        /// <param name="appId">
        /// The app id.
        /// </param>
        /// <param name="appSecret">
        /// The app secret.
        /// </param>
        public InstagramClient(string appId, string appSecret)
            : base("instagram")
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
            var args = new Dictionary<string, string>();

            args.Add("client_id", this.appId);
            args.Add("client_secret", this.appSecret);
            args.Add("grant_type", "authorization_code");
            args.Add("redirect_uri", returnUrl.AbsoluteUri);
            args.Add("code", authorizationCode);

            using (var webClient = new WebClient())
            {
                var text = webClient.UploadString(AccessTokenServiceEndpoint, MessagingUtilities.CreateQueryString(args));
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);
                currentUserId = data["user"]["id"].Value<string>();
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
            var uriBuilder = new UriBuilder(UserInfoServiceEndpoint.Replace("{{user-id}}", currentUserId));

            var args = new Dictionary<string, string>();

            args.Add("access_token", accessToken);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                var text = webClient.DownloadString(uriBuilder.Uri);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);

                var user = data["data"];

                var names = user["full_name"].Value<string>().Split(' ');

                var dictionary = new Dictionary<string, string>();
                dictionary.AddItemIfNotEmpty("id", user["id"].Value<string>());
                dictionary.AddItemIfNotEmpty("firstName", names.Any() ? names.First() : data["username"].Value<string>());
                dictionary.AddItemIfNotEmpty("lastName", names.Count() > 1 ? names.Last() : string.Empty);
                return dictionary;
            }
        }

    }
}
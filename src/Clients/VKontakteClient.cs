using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace nordron.OAuth.Clients
{
    /// <summary>
    /// VKontakte authentication client.
    /// </summary>
    public sealed class VKontakteClient : OAuth2Client
    {
        private const string AuthorizationServiceEndpoint = "http://oauth.vk.com/authorize";
        private const string AccessTokenServiceEndpoint = "https://oauth.vk.com/access_token";
        private const string UserInfoServiceEndpoint = "https://api.vk.com/method/users.get";

        private readonly string appId;
        private readonly string appSecret;

        private int currentUserId;

        /// <summary>
        /// Initializes a new instance of the VKontakteClient class.
        /// </summary>
        /// <param name="appId">
        /// The app id.
        /// </param>
        /// <param name="appSecret">
        /// The app secret.
        /// </param>
        public VKontakteClient(string appId, string appSecret)
            : base("vkontakte")
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
            args.Add("code", authorizationCode);
            args.Add("redirect_uri", returnUrl.AbsoluteUri);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                var text = webClient.UploadString(uriBuilder.Uri, string.Empty);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);
                currentUserId = data["user_id"].Value<int>();
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

            args.Add("uids", currentUserId.ToString());
            args.Add("fields", "uid,first_name,last_name,photo");
            args.Add("access_token", accessToken);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                var text = webClient.DownloadString(uriBuilder.Uri);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);

                var user = data["response"][0];

                var dictionary = new Dictionary<string, string>();
                dictionary.AddItemIfNotEmpty("id", user["uid"].Value<string>());
                dictionary.AddItemIfNotEmpty("firstName", user["first_name"].Value<string>());
                dictionary.AddItemIfNotEmpty("lastName", user["last_name"].Value<string>());
                return dictionary;
            }
        }
    }
}
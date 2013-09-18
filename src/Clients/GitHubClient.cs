using DotNetOpenAuth.AspNet.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DotNetOpenAuth.AspNet.Clients
{
/// <summary>
    /// GitHub authentication client.
    /// </summary>
    public sealed class GitHubClient : OAuth2Client
    {
        private const string AuthorizationServiceEndpoint = "https://github.com/login/oauth/authorize";
        private const string AccessTokenServiceEndpoint = "https://github.com/login/oauth/access_token";
        private const string UserInfoServiceEndpoint = "https://api.github.com/user";

        private readonly string appId;
        private readonly string appSecret;

        /// <summary>
        /// Initializes a new instance of the GitHubClient class.
        /// </summary>
        /// <param name="appId">
        /// The app id.
        /// </param>
        /// <param name="appSecret">
        /// The app secret.
        /// </param>
        public GitHubClient(string appId, string appSecret)
            : base("github")
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
            args.Add("redirect_uri", returnUrl.AbsoluteUri);
            args.Add("scope", "user,user:email");

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
            args.Add("redirect_uri", returnUrl.AbsoluteUri);
            args.Add("code", authorizationCode);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Accept", "application/json");

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

            args.Add("access_token", accessToken);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                var text = webClient.DownloadString(uriBuilder.Uri);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);

                var names = (data["name"].Value<string>() ?? string.Empty).Split(' ').ToList();

                var dictionary = new Dictionary<string, string>();
                dictionary.AddItemIfNotEmpty("id", data["id"].Value<string>());
                dictionary.AddItemIfNotEmpty("firstName", names.Count > 0 ? names.First() : data["login"].Value<string>());
                dictionary.AddItemIfNotEmpty("lastName", names.Count > 1 ? names.Last() : string.Empty);
                dictionary.AddItemIfNotEmpty("email", data["email"].Value<string>());
                return dictionary;
            }
        }
    }
}
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
    /// Odnoklassniki authentication client.
    /// </summary>
    public sealed class OdnoklassnikiClient : OAuth2Client
    {
        private const string AuthorizationServiceEndpoint = "http://www.odnoklassniki.ru/oauth/authorize";
        private const string AccessTokenServiceEndpoint = "http://api.odnoklassniki.ru/oauth/token.do";
        private const string UserInfoServiceEndpoint = "http://api.odnoklassniki.ru/fb.do";

        private readonly string appId;
        private readonly string appPublic;
        private readonly string appSecret;

        /// <summary>
        /// Initializes a new instance of the OdnoklassnikiClient class.
        /// </summary>
        /// <param name="appId">
        /// The app id.
        /// </param>
        /// <param name="appSecret">
        /// The app secret.
        /// </param>
        public OdnoklassnikiClient(string appId, string appPublic, string appSecret)
            : base("odnoklassniki")
        {
            Requires.NotNullOrEmpty(appId, "appId");
            Requires.NotNullOrEmpty(appPublic, "appPublic");
            Requires.NotNullOrEmpty(appSecret, "appSecret");
            this.appId = appId;
            this.appPublic = appPublic;
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
            args.Add("scope", "VALUABLE ACCESS");

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
            
            args.Add("code", authorizationCode);
            args.Add("redirect_uri", returnUrl.AbsoluteUri);
            args.Add("grant_type", "authorization_code");
            args.Add("client_id", this.appId);
            args.Add("client_secret", this.appSecret);

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
            // http://dev.odnoklassniki.ru/wiki/pages/viewpage.action?pageId=12878032

            var uriBuilder = new UriBuilder(UserInfoServiceEndpoint);

            var args = new Dictionary<string, string>();

            args.Add("application_key", this.appPublic);
            args.Add("method", "users.getCurrentUser");

            // Signing.
            // Call API methods using access_token instead of session_key parameter
            // Calculate every request signature parameter sig using a little bit different way described in
            // http://dev.odnoklassniki.ru/wiki/display/ok/Authentication+and+Authorization
            // sig = md5( request_params_composed_string+ md5(access_token + application_secret_key)  )
            // Don't include access_token into request_params_composed_string
            var signature = string.Concat(args.OrderBy(x => x.Key).Select(x => string.Format("{0}={1}", x.Key, x.Value)).ToList());
            signature = (signature + (accessToken + this.appSecret).GetMd5Hash()).GetMd5Hash();

            args.Add("access_token", accessToken);
            args.Add("sig", signature);

            uriBuilder.AppendQueryArgs(args);

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                var text = webClient.DownloadString(uriBuilder.Uri);
                if (string.IsNullOrEmpty(text))
                    return null;

                var data = JObject.Parse(text);

                var dictionary = new Dictionary<string, string>();
                dictionary.AddItemIfNotEmpty("id", data["uid"].Value<string>());
                dictionary.AddItemIfNotEmpty("firstName", data["first_name"].Value<string>());
                dictionary.AddItemIfNotEmpty("lastName", data["last_name"].Value<string>());
                return dictionary;
            }
        }
    }
}

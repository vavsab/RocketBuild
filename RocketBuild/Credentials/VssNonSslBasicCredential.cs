using System;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.Services.Common;

namespace RocketBuild.Credentials
{
    /// <summary>
    /// Same as VssBasicCredential, but doesn't throw when URL is a non SSL, i.e. http, URL.
    /// </summary>
    /// <inheritdoc cref="FederatedCredential"/>
    internal sealed class VssNonSslBasicCredential : FederatedCredential
    {
        public VssNonSslBasicCredential()
            : this((VssBasicToken) null)
        {
        }

        public VssNonSslBasicCredential(string userName, string password)
            : this(new VssBasicToken(new NetworkCredential(userName, password)))
        {
        }

        public VssNonSslBasicCredential(ICredentials initialToken)
            : this(new VssBasicToken(initialToken))
        {
        }

        public VssNonSslBasicCredential(VssBasicToken initialToken)
            : base(initialToken)
        {
        }

        public override VssCredentialsType CredentialType => VssCredentialsType.Basic;

        public override bool IsAuthenticationChallenge(IHttpResponse webResponse)
        {
            if (webResponse == null
                || webResponse.StatusCode != HttpStatusCode.Found
                && webResponse.StatusCode != HttpStatusCode.Found
                && webResponse.StatusCode != HttpStatusCode.Unauthorized)
            {
                return false;
            }

            return webResponse.Headers.GetValues("WWW-Authenticate")
                .Any(x => x.StartsWith("Basic", StringComparison.OrdinalIgnoreCase));
        }

        protected override IssuedTokenProvider OnCreateTokenProvider(Uri serverUrl, IHttpResponse response)
        {
            return new BasicAuthTokenProvider(this, serverUrl);
        }

        private sealed class BasicAuthTokenProvider : IssuedTokenProvider
        {
            public BasicAuthTokenProvider(IssuedTokenCredential credential, Uri serverUrl)
                : base(credential, serverUrl, serverUrl)
            {
            }

            protected override string AuthenticationScheme => "Basic";

            public override bool GetTokenIsInteractive => CurrentToken == null;
        }
    }
}
using System;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using RocketBuild.Credentials;

namespace RocketBuild.Helpers
{
    public static class VssClientHelper
    {
        public static T GetClient<T>(string accountUrl, string apiKey, bool useSsl = true)
            where T : VssHttpClientBase
        {
            var accountUri = new Uri(accountUrl);

            FederatedCredential credential = useSsl
                ? (FederatedCredential) new VssBasicCredential(null, apiKey)
                : new VssNonSslBasicCredential(null, apiKey);
            var connection = new VssConnection(accountUri, credential);

            return connection.GetClient<T>();
        }
    }
}
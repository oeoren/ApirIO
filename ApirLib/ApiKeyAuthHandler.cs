using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApirLib
{

    public class ApiKeyAuthHandler : DelegatingHandler
    {
        private const string ApiKeySchemeName = "ApiKey";
        private const string AuthResponseHeader = "WWW-Authenticate";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = request.Headers.Authorization;

            if (authHeader != null && authHeader.Scheme == ApiKeySchemeName)
            {
                var principal = ValidateApiKey(authHeader.Parameter);

                if (principal != null)
                {
                    Thread.CurrentPrincipal = principal;
                }
            }

            return base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    var response = task.Result;

                    if (response.StatusCode == HttpStatusCode.Unauthorized && !response.Headers.Contains(AuthResponseHeader))
                    {
                        response.Headers.Add(AuthResponseHeader, ApiKeySchemeName);
                    }

                    return response;
                });
        }


        IPrincipal ValidateApiKey(string authParameter)
        {
            if (String.IsNullOrEmpty(authParameter) || authParameter != "1234-5678")
            {
                return null;
            }

            return new GenericPrincipal(new GenericIdentity("Test User", ApiKeySchemeName), null);
        }
    }
}
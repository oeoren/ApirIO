using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
    public class CorsDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            const string origin = "Origin";
            const string accessControlRequestMethod = "Access-Control-Request-Method";
            const string accessControlRequestHeaders = "Access-Control-Request-Headers";
            const string accessControlAllowOrigin = "Access-Control-Allow-Origin";
            const string accessControlAllowMethods = "Access-Control-Allow-Methods";
            const string accessControlAllowHeaders = "Access-Control-Allow-Headers";
            const string accessControlExposeHeaders = "Access-Control-Expose-Headers";

            bool isCorsRequest = request.Headers.Contains(origin);
            bool isPreflightRequest = request.Method == HttpMethod.Options;

            if (isCorsRequest)
            {
                 if (isPreflightRequest)
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.OK);
                        response.Headers.Add(accessControlAllowOrigin, request.Headers.GetValues(origin).First());

                        string controlRequestMethod =
                        request.Headers.GetValues(accessControlRequestMethod).FirstOrDefault();

                        if (controlRequestMethod != null)
                        {
                            response.Headers.Add(accessControlAllowMethods, controlRequestMethod);
                        }

                        string requestedHeaders =
                          string.Join(", ", request.Headers.GetValues(accessControlRequestHeaders));

                        if (!string.IsNullOrEmpty(requestedHeaders))
                        {
                            response.Headers.Add(accessControlAllowHeaders, requestedHeaders);
                        }
                        return response;

                    }, cancellationToken);
                }
                return base.SendAsync(request, cancellationToken).ContinueWith(t =>
                {
                    HttpResponseMessage response = t.Result;
                    response.Headers.Add(accessControlAllowOrigin,
                             request.Headers.GetValues(origin).First());
                      //       request.Headers.GetValues("Host").First());
                    response.Headers.Add(accessControlExposeHeaders, "Location");
                    return response;
                });
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}

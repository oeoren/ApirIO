using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

namespace ApirLib
{
    public class NullFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var response = actionExecutedContext.Response;

            //object responseValue;
            //bool hasContent = response.TryGetContentValue(out responseValue);

            //if (!hasContent)
            //    throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}

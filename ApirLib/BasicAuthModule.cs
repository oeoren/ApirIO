using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ApirLib
{
        public class BasicAuthHttpModule : IHttpModule
        {
            private const string Realm = "Apir";

            private static UserValidator userValidator;
            public void Init(HttpApplication context)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
                string userValidatorProcName = ConfigurationManager.AppSettings["UserValidator"];
                if (userValidatorProcName != null && userValidatorProcName.Length > 0)
                    userValidator = new UserValidator(connectionString, userValidatorProcName);
                else
                    userValidator = null;
  
                // Register event handlers
                context.AuthenticateRequest += OnApplicationAuthenticateRequest;
                context.EndRequest += OnApplicationEndRequest;
            }

            private static void SetPrincipal(IPrincipal principal)
            {
                Thread.CurrentPrincipal = principal;
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.User = principal;
                }
            }

            private static bool CheckPassword(string username, string password)
            {
                if (userValidator == null)
                    return false;

                try
                {
                    userValidator.Validate(username, password);
                    return true;
                }
                catch (SecurityTokenException)
                {
                    return false;
                }
            }

            private static bool AuthenticateUser(string credentials)
            {
                bool validated = false;
                try
                {
                    var encoding = Encoding.GetEncoding("iso-8859-1");
                    credentials = encoding.GetString(Convert.FromBase64String(credentials));

                    int separator = credentials.IndexOf(':');
                    string name = credentials.Substring(0, separator);
                    string password = credentials.Substring(separator + 1);

                    validated = CheckPassword(name, password);
                    if (validated)
                    {
                        var identity = new GenericIdentity(name);
                        SetPrincipal(new GenericPrincipal(identity, null));
                    }
                }
                catch (FormatException)
                {
                    // Credentials were not formatted correctly.
                    validated = false;

                }
                return validated;
            }

            private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
            {
                var request = HttpContext.Current.Request;
                var authHeader = request.Headers["Authorization"];
                if (authHeader != null)
                {
                    var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                    // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                    if (authHeaderVal.Scheme.Equals("basic",
                            StringComparison.OrdinalIgnoreCase) &&
                        authHeaderVal.Parameter != null)
                    {
                        AuthenticateUser(authHeaderVal.Parameter);
                    }
                }
            }

            // If the request was unauthorized, add the WWW-Authenticate header 
            // to the response.
            private static void OnApplicationEndRequest(object sender, EventArgs e)
            {
                var response = HttpContext.Current.Response;
                if (response.StatusCode == 401)
                {
                    response.Headers.Add("WWW-Authenticate",
                        string.Format("Basic realm=\"{0}\"", Realm));
                }
            }

            public void Dispose()
            {
            }
        }
}

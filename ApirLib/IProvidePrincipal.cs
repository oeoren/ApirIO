using System.Security.Principal;

namespace Apir
{
    public interface IProvidePrincipal
    {
        IPrincipal CreatePrincipal(string username, string password);
    }
}
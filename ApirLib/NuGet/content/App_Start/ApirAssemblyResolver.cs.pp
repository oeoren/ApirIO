using ApirLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;

namespace $rootnamespace$.App_Start
{
    public class ApirAssemblyResolver : DefaultAssembliesResolver
    {
        private string _connectionString;
        private string _sourcePath;
        private string _userValidator;


        public ApirAssemblyResolver(string connectionString, string sourcePath, string userValidator)
        {
            _connectionString = connectionString;
            _sourcePath = sourcePath;
            _userValidator = userValidator;
        }

        public override ICollection<Assembly> GetAssemblies()
        {
            ICollection<Assembly> baseAssemblies = base.GetAssemblies();
            List<Assembly> assemblies = new List<Assembly>(baseAssemblies);
            try
            {
                Assembly onTheFly = ApirBuilder.Build(_connectionString, _sourcePath, _userValidator);
                if (onTheFly != null)
                {
                    assemblies.Add(onTheFly);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return assemblies;
        }
    }
}

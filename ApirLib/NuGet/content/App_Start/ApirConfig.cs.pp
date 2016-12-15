using Apir;
using ApirLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;

[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof($rootnamespace$.App_Start.ApirPackage), "PreStart")]

namespace $rootnamespace$.App_Start {

    public static class ApirPackage {
        public static void PreStart() {
            ApirConfig.RegisterAssemblyResolver(GlobalConfiguration.Configuration);
        }
    }

    public static class ApirConfig
    {
        public static void RegisterAssemblyResolver(HttpConfiguration config) 
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string sourcePath = ConfigurationManager.AppSettings["OutPath"];
            if (sourcePath == null)
                sourcePath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\";
            string userValidator = ConfigurationManager.AppSettings["UserValidator"];
            ApirAssemblyResolver assemblyResolver = new ApirAssemblyResolver(connectionString, sourcePath, userValidator);
            config.Services.Replace(typeof(IAssembliesResolver), assemblyResolver);
/* 
            if (userValidator != null && userValidator.Length > 0)
            config
                .MessageHandlers.Add(new BasicAuthMessageHandler()
                {
                    PrincipalProvider = new SwaPrincipalProvider(connectionString, userValidator)
                });
*/
        } 
    }
}
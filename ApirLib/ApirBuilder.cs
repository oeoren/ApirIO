using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
    public static class ApirBuilder
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static Assembly Build(string connectionString, string sourcePath,string userValidator)
        {

            Model model = ModelBuilder.ConstructModel(connectionString);
            LogModel(model);
            bool fAuthorize = (userValidator != null && userValidator.Length > 1);
            String code = CodeBuilder.BuildCode(model, fAuthorize );

            if (sourcePath != null && sourcePath.Length > 0)
            {
                //Console.WriteLine(code);

                using (StreamWriter outfile = new StreamWriter(sourcePath + "swaSource.cs"))
                {
                    outfile.Write(code);
                }
                logger.Debug("Code built and saved at {0}", sourcePath + "swaSource.cs");
            }

            Assembly assembly = DllBuilder.BuildDll(code, sourcePath);
            if (assembly == null)
                logger.Fatal("Compile Error. See errorfile at:{0} and source at {1} ", sourcePath + "swaError.txt", sourcePath + "swaFile.cs");
            else
                logger.Debug("Compiled  OK");
            return assembly;
        }

        private static void LogModel(Model model)
        {
            int procs = 0;
            foreach (var c in model.controllers)
                foreach (var p in c.procs)
                    procs++;
            logger.Debug("API Info read from database. Resources={0}, Procedures={1}", model.controllers.Count, procs);
        }
    }
}

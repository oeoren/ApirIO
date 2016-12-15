using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
    public class DllBuilder
    {
        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        static public Assembly BuildDll(string code, string sourcePath)
        {
            //IDictionary<string, string> compParams =
            //     new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } };
            //CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp", compParams);
            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            string outputDll = sourcePath + "ApirController.dll";

            string path = DllBuilder.AssemblyDirectory;


            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = outputDll;
            parameters.ReferencedAssemblies.Add(@"System.dll");
            parameters.ReferencedAssemblies.Add(@"System.Core.dll");
            parameters.ReferencedAssemblies.Add(@"System.Net.Http.dll");
            parameters.ReferencedAssemblies.Add(@"System.Net.Http.WebRequest.dll");
            parameters.ReferencedAssemblies.Add(path + @"\System.Net.Http.Formatting.dll");
            parameters.ReferencedAssemblies.Add(path + @"\System.Web.Http.dll");
            //parameters.ReferencedAssemblies.Add(@"System.Net.Http.Formatting.dll");
            //parameters.ReferencedAssemblies.Add(@"System.Web.Http.dll");
            parameters.ReferencedAssemblies.Add(@"System.Data.dll");
            parameters.ReferencedAssemblies.Add(@"System.Data.DataSetExtensions.dll");
            parameters.ReferencedAssemblies.Add(@"System.xml.dll");
            parameters.ReferencedAssemblies.Add(@"System.xml.Linq.dll");
            parameters.ReferencedAssemblies.Add(@"System.Configuration.dll");

            parameters.ReferencedAssemblies.Add(path + @"\NLog.dll");

            parameters.CompilerOptions = "/doc:" + sourcePath + "xmlComments.xml";
#if DEBUG
            parameters.IncludeDebugInformation = true;
            parameters.GenerateInMemory = false;
            var fileName = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\swaDebug.cs"));
            File.WriteAllText(fileName, code);
            CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, fileName);
#else
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, code);
#endif


            using (StreamWriter outfile = new StreamWriter(sourcePath + "swaError.txt"))
            {
                if (results.Errors.Count > 0)
                {
                    if (sourcePath != null)
                        foreach (CompilerError CompErr in results.Errors)
                        {
                            outfile.WriteLine(
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine);
                        }
                    throw new Exception("Compile Error");
                }
                else
                {
                    outfile.WriteLine("Build Succeeded");
                    return Assembly.LoadFrom(outputDll);
                }
            }
        }
    }
}

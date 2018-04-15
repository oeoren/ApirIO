using HandlebarsDotNet;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
    class TSGenColumn
    {
        public string name { get; set; }
        public string label { get; set; }
        public string sqlType { get; set; }
        public int? maxLen { get; set; }
        public string tsType { get; set; }
        public string endChar { get; set; }
        public string resourceName { get; set; }
        public string hs = "{{";
        public string he = "}}";


    }
    class CodeGenModel
    {
        public string ResourceName { get; set; }
        public string ResourceNamePlural { get; set; }
        public string ResourceClass { get; set; }

        public string RestResourceName { get; set; }

        public string ProviderResourceClass { get; set; }
          
        public string ResourceId { get; set; }

        public string PageListClass { get; set; }
        public string PageAddClass { get; set; }
        public string PageEditClass { get; set; }

        public string PageListFile { get; set; }
        public string PageAddFile { get; set; }
        public string PageEditFile { get; set; }

        public string PageListModule { get; set; }
        public string PageAddModule { get; set; }
        public string PageEditModule { get; set; }

        public string Url { get; set; }
        public List<TSGenColumn> Columns { get; set; }
        public bool GetDefined { get; set; }
        public bool PutDefined { get; set; }
        public bool PostDefined { get; set; }
        public bool DeleteDefined { get; set; }
        public string ListText1 { get; set; }
        public string ListText2 { get; set; }
        public string hs = "{{";
        public string he = "}}";
    }
    public class IonicGenerator
    {
        static string UppercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        static string LowercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return char.ToLower(str[0]) + str.Substring(1);
        }

        static public string Build(Model model, string type, string resource, string templateString, string url)
        {
            CodeGenModel ioModel = null;
            var lastName = "";
            var restResource = resource;
            if (resource.EndsWith("s"))
                resource = resource.Substring(0, resource.Length - 1);
            foreach(var controller in model.controllers)
            {
                if (controller.name.ToLower() == restResource.ToLower())
                {
                    ioModel = new CodeGenModel {
                        ResourceId = controller.columns[0].name,
                        RestResourceName = restResource,
                        ResourceName = LowercaseFirst(resource),
                        Columns = new List<TSGenColumn>(),
                        ResourceClass = UppercaseFirst(resource),
                        ResourceNamePlural = LowercaseFirst(resource) + "s",
                        ProviderResourceClass = UppercaseFirst(resource) + "s",
                        Url = url,
                        PageAddClass = UppercaseFirst(resource) + "AddPage",
                        PageEditClass = UppercaseFirst(resource) + "EditPage",
                        PageListClass = UppercaseFirst(resource) + "ListPage",
                        PageAddFile = LowercaseFirst(resource) + "-add",
                        PageEditFile = LowercaseFirst(resource) + "-edit",
                        PageListFile = LowercaseFirst(resource) + "-list",
                        PageAddModule = UppercaseFirst(resource) + "AddPageModule",
                        PageEditModule = UppercaseFirst(resource) + "EditPageModule",
                        PageListModule = UppercaseFirst(resource) + "ListPageModule",
                    };
                    foreach (var column in controller.columns)
                        lastName = column.name;
                    int i = 0;
                    foreach (var column in controller.columns)
                    {
                        ioModel.Columns.Add(new TSGenColumn { name = column.name, maxLen = column.maxLen, sqlType = column.sqlType,
                            endChar = (column.name == lastName) ? "" : ",", tsType = TypeMapper.GetTSTypeName(column.sqlType),
                            resourceName = ioModel.ResourceName,
                            label = column.name.Humanize(LetterCasing.Title)
                        });
                        if (i == 1)
                            ioModel.ListText1 = "{{" + ioModel.ResourceName + "." + column.name + "}}";
                        if (i == 2)
                            ioModel.ListText2 = "{{" + ioModel.ResourceName + "." + column.name + "}}";
                        i++;
                    }
                    foreach (var proc in controller.procs)
                    {
                        if (proc.GetVerb() == HttpVerb.httpGet)
                            ioModel.GetDefined = true;
                        if (proc.GetVerb() == HttpVerb.httpPut)
                            ioModel.PutDefined = true;
                        if (proc.GetVerb() == HttpVerb.httpPost)
                            ioModel.PostDefined = true;
                        if (proc.GetVerb() == HttpVerb.httpDelete)
                            ioModel.DeleteDefined = true;
                    }
                }
            }
            if (ioModel == null)
                throw new Exception("Unknown resource:" + resource);
            var template = Handlebars.Compile(templateString);
            var result = template(ioModel);
            return result;
        }
    }
}

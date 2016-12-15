using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ApirLib
{
    public enum HttpVerb
    {
        httpUnknown = -1,
        httpGet = 0,
        httpPut = 1,
        httpPost = 2,
        httpDelete = 3
    }

    public class Model
    {
        public Model()
        {
            this.controllers = new List<ControllerInfo>();
        }
        public List<ControllerInfo> controllers { get; set; }
        public string dbConnection { get; set; }

    }


    public class TableModel
    {
        public TableModel()
        {
            this.columns = new List<ColumnInfo>();
        }
        public string tableName;
        internal string resource;
        public string KeyColum { get; set; }
        public bool isIdentity;
        public List<ColumnInfo> columns { get; set; }
        public List<ColumnInfo> nonKeyColumns { get; set; }

    }

    public class ControllerInfo
    {
        public ControllerInfo()
        {
            this.columns = new List<ColumnInfo>();
            this.procs = new List<ProcInfo>();
        }
        public string name { get; set; }
        public List<ColumnInfo> columns { get; set; }
        public List<ProcInfo> procs { get; set; }
    }

    public class ProcInfo
    {
        public ProcInfo()
        {
            parameters = new List<ParameterInfo>();
        }

        public string name { get; set; }
        public List<ParameterInfo> parameters { get; set; }
        public string xmlComments;
        public HttpVerb GetVerb()
        {
            if (name.ToLower().EndsWith("get"))
                return HttpVerb.httpGet;
            if (name.ToLower().EndsWith("put"))
                return HttpVerb.httpPut;
            if (name.ToLower().EndsWith("post"))
                return HttpVerb.httpPost;
            if (name.ToLower().EndsWith("delete"))
                return HttpVerb.httpDelete;
            return HttpVerb.httpUnknown;
        }
    }

    public class ColumnInfo
    {
        public string name { get; set; }
        public string sqlType { get; set; }
        public int? maxLen { get; set; }
        public string csType { get; set; }
    }

    public class ParameterInfo
    {
        public string name { get; set; }
        public string sqlType { get; set; }
        public string csType { get; set; }
        public int precision { get; set; }
        public int scale { get; set; }
        public int maxLen { get; set; }
        public bool isOutput { get; set; }
    }

}
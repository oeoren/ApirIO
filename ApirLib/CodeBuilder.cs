using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Data;


namespace ApirLib
{
    public class CodeBuilder
    {
        static public string BuildDropProc(string resource, string verb)
        {
            var srcDrop = string.Format("if exists (select * from sysobjects where type = 'P' and name = 'API_{0}_{1}') \n ", resource, verb);
            srcDrop += string.Format("  DROP PROCEDURE [dbo].[API_{0}_{1}]", resource, verb);
            return srcDrop;
        }
        static public string BuildTableProc(TableModel m, string verb)
        {
            switch (verb.ToLower()) {
                case "get":
                    return BuildGetTableProc(m);
                case "put":
                    return BuildPutTableProc(m);
                case "post":
                    return BuildPostTableProc(m);
                case "delete":
                    return BuildDeleteTableProc(m);
            }
            return "";
        }


        static public string BuildPostTableProc(TableModel m)
        {
            var sb = new StringBuilder(6000)
            .AppendFormat("--- <summary> \r\n")
            .AppendFormat("--- Insert {0} \r\n", m.resource)
            .AppendFormat("--- </summary>  \r\n")
            .AppendFormat("--- <remarks> Insert new {0} </remarks> \r\n", m.tableName)
            .AppendFormat("--- <response code=\"200\">OK</response>\r\n")
            .AppendFormat("     CREATE PROCEDURE[dbo].[API_{0}_Post]( \r\n", m.resource);
            int j = 1;
            foreach (var col in m.nonKeyColumns)
            {
                char sep = (j < m.nonKeyColumns.Count) ? ',' : ' ';
                if (col.sqlType.ToLower().IndexOf("char") >= 0)
                    sb.AppendFormat("@{0} {1}({2}) = NULL{3}", col.name, col.sqlType,
                        (col.maxLen == -1) ? "max" : col.maxLen.ToString(), sep);
                else {

                    sb.AppendFormat("@{0} {1} = NULL{2}", col.name, col.sqlType, sep);
                    sb.AppendLine("");
                }
                ++j;
            }
            sb.AppendLine(") AS");
            sb.AppendFormat("INSERT INTO {0} ( ", m.tableName);
            int i = 1;
            foreach (var col in m.nonKeyColumns)
            {
                char sep = (i < m.nonKeyColumns.Count) ? ',' : ' ';
                sb.AppendFormat(" {0} {1} ", col.name, sep);
                i += 1;
            }
            sb.AppendLine(") VALUES (");
            i = 1;
            foreach (var col in m.nonKeyColumns)
            {
                sb.AppendFormat(" @{0} {1} ", col.name, (i < m.nonKeyColumns.Count) ? ',' : ' ');
                i += 1;
            }

            sb.AppendLine(")");
            sb.AppendLine(" RETURN 200 -- OK");
            return sb.ToString();
        }
        static public string BuildDeleteTableProc(TableModel m)
        {
            var sb = new StringBuilder(6000)
            .AppendFormat("--- <summary> \r\n")
            .AppendFormat("--- Delete {0} \r\n", m.resource)
            .AppendFormat("--- </summary>  \r\n")
            .AppendFormat("--- <param name = \"ID\" > {0} ID </param> \r\n", m.tableName)
            .AppendFormat("--- <remarks> Delete {0} </remarks> \r\n", m.tableName)
            .AppendFormat("--- <response code=\"200\">OK</response>\r\n")
            .AppendFormat("--- <response code=\"404\">Not Found</response>\r\n")
            .AppendFormat("     CREATE PROCEDURE[dbo].[API_{0}_Delete](@ID varchar(max) = NULL) \r\n", m.resource);
            sb.AppendLine("AS");
            sb.AppendFormat("IF NOT EXISTS(SELECT {0} FROM {1} WHERE @ID = {0})  \r\n", m.KeyColum, m.tableName);
            sb.AppendLine("BEGIN");
            sb.AppendFormat("   RAISERROR('Unknown {0}',1,1) \r\n", m.resource);
            sb.AppendLine("   RETURN 404");
            sb.AppendLine("END");
            sb.AppendFormat("DELETE {0}  \r\n", m.tableName);
            sb.AppendFormat("    WHERE @ID = {0} \r\n", m.KeyColum);
            sb.AppendLine(" RETURN 200 -- OK");
            return sb.ToString();
        }


        static public string BuildPutTableProc(TableModel m)
        {
            var sb = new StringBuilder(6000)
            .AppendFormat("--- <summary> \r\n")
            .AppendFormat("--- Update {0} \r\n", m.resource)
            .AppendFormat("--- </summary>  \r\n")
            .AppendFormat("--- <param name = \"ID\" > {0} ID </param> \r\n", m.tableName)
            .AppendFormat("--- <remarks> Updates {0} </remarks> \r\n", m.tableName)
            .AppendFormat("--- <response code=\"200\">OK</response>\r\n")
            .AppendFormat("--- <response code=\"404\">Not Found</response>\r\n")
            .AppendFormat("     CREATE PROCEDURE[dbo].[API_{0}_Put](@ID varchar(max) = NULL \r\n", m.resource);
            foreach (var col in m.nonKeyColumns)
            {
                if (col.sqlType.ToLower().IndexOf("char") >=0 )
                    sb.AppendFormat(", @{0} {1}({2})   = NULL ", col.name, col.sqlType,(col.maxLen == -1) ? "max" : col.maxLen.ToString());
                else
                    sb.AppendFormat(", @{0} {1} = NULL ", col.name, col.sqlType);
                sb.AppendLine("");
            }
            sb.AppendLine(") AS");
            sb.AppendFormat("IF NOT EXISTS(SELECT {0} FROM {1} WHERE @ID = {0})  \r\n", m.KeyColum, m.tableName);
            sb.AppendLine("BEGIN");
            sb.AppendFormat("   RAISERROR('Unknown {0}',1,1) \r\n", m.resource);
            sb.AppendLine  ("   RETURN 404");
            sb.AppendLine("END");
            sb.AppendFormat("UPDATE {0}  SET \r\n", m.tableName);
            int i = 1;
            foreach (var col in m.nonKeyColumns)
            {
                sb.AppendFormat("    {0} = COALESCE(@{0},{0}){1} \r\n ", col.name, (i<m.nonKeyColumns.Count) ? ',' : ' ');
                i += 1;
            }
            sb.AppendFormat("    WHERE @ID = {0} \r\n", m.KeyColum);
            sb.AppendLine(" RETURN 200 -- OK");
            return sb.ToString();
        }

        static public string BuildGetTableProc(TableModel m)
        {
            var sb = new StringBuilder(6000)
            .AppendFormat("--- <summary> \r\n")
            .AppendFormat("--- Retrieve {0} \r\n", m.resource)
            .AppendFormat("--- </summary>  \r\n")
            .AppendFormat("--- <param name = \"ID\" > {0} ID </param> \r\n", m.tableName)
            .AppendFormat("--- <remarks> Returns all or a single {0} </remarks> \r\n", m.tableName)
            .AppendFormat("     CREATE PROCEDURE[dbo].[API_{0}_Get](@ID varchar(max) = NULL)  AS \r\n", m.resource)
            .AppendFormat("         SELECT  ");
            int i = 0;
            foreach (var col in m.columns)
            {
                sb.Append(col.name);
                i += 1;
                if (i < m.columns.Count)
                    sb.Append(", ");
            }
            sb.AppendFormat("\r\n           FROM {0}  \r\n", m.tableName);
            sb.AppendFormat("           WHERE @ID IS NULL OR @ID = {0} \r\n", m.KeyColum);
            return sb.ToString();
        }
        static public string BuildCode(Model model, bool fAuthorize, string freeResources = null)
        {
            List<string> _freeResources = null;

            if (freeResources != null)
            {
                _freeResources = freeResources.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }


            var sb = new StringBuilder(60000)
                .AppendLine("using System;")
                .AppendLine("using System.Linq;")
                .AppendLine("using System.Web.Http;")
                .AppendLine("using System.Data;")
                .AppendLine("using System.Data.SqlClient;")
                .AppendLine("using System.Configuration;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine("using System.Reflection;")
                .AppendLine("using System.Net.Http;")
                .AppendLine("using System.Net;")
                .AppendLine("using System.Diagnostics;")
                .AppendLine("using NLog;")
                .AppendLine("namespace ControllerLibrary")
                .AppendLine("{");

            foreach (var controller in model.controllers)
            {
                // Create Entity
                sb.AppendFormat("public class {0} \n {{ \n", controller.name);
                foreach (var column in controller.columns)
                {
                    string csTypeName = GetCSTypeName(column);
                    sb.AppendFormat(" public {0} {1} {{ get; set;}} \n", csTypeName, column.name);
                }
                sb.AppendLine("}");
            }

            sb.AppendLine("static public class DbUtil \n");
            sb.AppendLine("{");
            sb.AppendLine("static public string message;");

            sb.AppendLine("\tstatic public SqlConnection GetConnection() {");
            sb.AppendLine("\t\tstring conStr = ConfigurationManager.ConnectionStrings[\"DefaultConnection\"].ConnectionString;");
            sb.AppendLine("\t\tSqlConnection con = new SqlConnection(conStr);");
            sb.AppendLine("DbUtil.message = \"\";");
            sb.AppendLine("con.InfoMessage += delegate(object sender, SqlInfoMessageEventArgs e)");
            sb.AppendLine("{");
            sb.AppendLine("if (DbUtil.message.Length > 0) DbUtil.message +=  \"\\n\";");
            sb.AppendLine("DbUtil.message +=  e.Message;");
            sb.AppendLine("};");


            sb.AppendLine("\t\treturn con;        ");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            sb.AppendLine(@"
    public static class DbExtensions
    {
        public static List<T> ToListCollection<T>(this DataTable dt)
        {
            List<T> lst = new System.Collections.Generic.List<T>();
            Type tClass = typeof(T);
            PropertyInfo[] pClass = tClass.GetProperties();
            List<DataColumn> dc = dt.Columns.Cast<DataColumn>().ToList();
            T cn;
            foreach (DataRow item in dt.Rows)
            {
                cn = (T)Activator.CreateInstance(tClass);
                foreach (PropertyInfo pc in pClass)
                {
                    // Can comment try catch block. 
                    try
                    {
                        DataColumn d = dc.Find(c => c.ColumnName == pc.Name);
                        if (d != null && item[pc.Name] != DBNull.Value)
                            pc.SetValue(cn, item[pc.Name], null);
                    }
                    catch (Exception ex)
                    {
                       throw ex;
                    }
                }
                lst.Add(cn);
            }
            return lst;
        }
    }
        ");


            foreach (var controller in model.controllers)
            {
                if (AddAuthorize(fAuthorize, controller.name, _freeResources))
                    sb.AppendLine("[Authorize]");
                sb.AppendFormat("public class {0}Controller:ApiController \n", controller.name);
                sb.AppendLine("{");
                sb.AppendLine("\t private static Logger logger = LogManager.GetCurrentClassLogger();");

                foreach (var proc in controller.procs)
                {
                    sb.Append(proc.xmlComments);
                    if (proc.name.ToLower().EndsWith("get"))
                    {
                        sb.AppendFormat("   public IEnumerable<{0}> Get(", controller.name);

                        AddParameters(true, sb, controller, proc, false, true);
                        sb.AppendLine(")");
                        sb.AppendLine("\t{");


                        AddProc(sb, proc);
                        AddAdoParams(sb, controller, proc, "", true);

                        sb.AppendLine("\t\tSqlDataAdapter da = new SqlDataAdapter(com);");
                        sb.AppendLine("\t\tcon.Open();");

                        sb.AppendLine("\t\tDataSet ds = new DataSet();");
                        sb.AppendLine("\t\tda.Fill(ds);");
                        sb.AppendLine("\t\tda.Dispose();");

                        AddTrace(sb, controller, proc, true);

                        sb.AppendLine("\t\tDataTable dt = ds.Tables[0];");
                        sb.AppendFormat("\t\tList<{0}> ret = dt.ToListCollection<{0}>();\n", controller.name);
                        sb.AppendFormat("\t\treturn ret.AsEnumerable<{0}>();\n", controller.name);
                        sb.AppendLine("\t\t}\n");
                        sb.AppendLine("\t}\n\n");



                        // Get single
                        if (HasIdParam(proc))
                        {
                            sb.AppendFormat("   public {0} Get(", controller.name);

                            AddParameters(true, sb, controller, proc, false, false);
                            sb.AppendLine(")");
                            sb.AppendLine("\t{");

                            AddProc(sb, proc);
                            AddAdoParams(sb, controller, proc, "", false);

                            sb.AppendLine("\t\tSqlDataAdapter da = new SqlDataAdapter(com);");
                            sb.AppendLine("\t\tcon.Open();");

                            sb.AppendLine("\t\tDataSet ds = new DataSet();");
                            sb.AppendLine("\t\tda.Fill(ds);");
                            sb.AppendLine("\t\tda.Dispose();");

                            AddTrace(sb, controller, proc, false);

                            sb.AppendLine("\t\tif ( ds.Tables.Count == 0)");
                            sb.AppendLine("\t\t\tthrow new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));");
                            sb.AppendLine("\t\tDataTable dt = ds.Tables[0];");
                            sb.AppendLine("\t\tif (dt.Rows.Count > 0) ");
                            sb.AppendFormat("\t\t\treturn  dt.ToListCollection<{0}>()[0];\n", controller.name);
                            sb.AppendLine("\t\telse ");
                            sb.AppendLine("\t\t\tthrow new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));");
                            sb.AppendLine("\t\t}\n");
                            sb.AppendLine("\t}\n\n");

                        }
                    }
                    else if (proc.name.ToLower().EndsWith("put"))
                    {
                        sb.AppendFormat("   public HttpResponseMessage Put(");
                        if (controller.columns.Count != 0)
                            sb.AppendFormat("{0} res ", controller.name);
                        AddParameters((controller.columns.Count == 0), sb, controller, proc, true);
                        sb.AppendLine(")\n \t{");
                        AddProc(sb, proc);
                        AddAdoParams(sb, controller, proc, "res");

                        sb.AppendLine("\tcon.Open();");
                        sb.AppendLine("\tcom.ExecuteNonQuery();");
                        AddTrace(sb, controller, proc);
                        AddReturn(sb);
                        sb.AppendLine("\t\t}\n");
                        sb.AppendLine("\t}\n\n");
                    }
                    else if (proc.name.ToLower().EndsWith("post"))
                    {
                        sb.AppendFormat("   public HttpResponseMessage  Post({0} res ", controller.name);
                        AddParameters(false, sb, controller, proc, true, true);
                        sb.AppendLine(")\n \t{ ");
                        sb.AppendLine("\tif (res == null) { ");
                        sb.AppendLine("\t\tlogger.Fatal(\""  + proc.name + ": Cannot parse resource. Check parameters\" );");
                        sb.AppendLine("\t\tthrow new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));");
                        sb.AppendLine("\t}");

                        bool hasNewId = AddNewId(sb, controller, proc);
                        AddProc(sb, proc);
                        AddAdoParams(sb, controller, proc, "res", false);
                        sb.AppendLine("\ttry {");
                        sb.AppendLine("\t\tcon.Open();");
                        sb.AppendLine("\t\tcom.ExecuteNonQuery();");
                        sb.AppendLine("\t\t} catch (Exception ex) {");
                        AddTrace(sb, controller, proc,true);
                        sb.AppendLine("\t\t\tlogger.Fatal(\"" + proc.name + ": SqlException:\" + ex.Message  );");
                        sb.AppendLine("\t\t\t return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);");
                        sb.AppendLine("\t\t} ");
                        AddTrace(sb, controller, proc, true);
                        AddReturn(sb, hasNewId);
                        sb.AppendLine("\t\t}\n");
                        sb.AppendLine("\t}\n\n");
                    }
                    else if (proc.name.ToLower().EndsWith("delete"))
                    {
                        sb.AppendFormat("   public HttpResponseMessage Delete(");
                        AddParameters(true, sb, controller, proc, false);
                        sb.AppendLine(")\n \t{");

                        AddProc(sb, proc);
                        AddAdoParams(sb, controller, proc, "");

                        sb.AppendLine("\t\tcon.Open();");
                        sb.AppendLine("\t\tcom.ExecuteNonQuery();");
                        AddTrace(sb, controller, proc);
                        AddReturn(sb);
                        sb.AppendLine("\t\t}\n");
                        sb.AppendLine("\t}\n\n");
                    }

                }
                sb.AppendLine("}");
            }

            sb.AppendLine("}");
            sb.AppendLine("");
            string code = sb.ToString();
            return code;
        }

        // Check if authorize attribute should be used.
        private static bool AddAuthorize(bool fAuthorize, string className, List<string> freeResources)
        {
            if (!fAuthorize)
                return false;
            if (freeResources == null)
                return true;
            foreach (var name in freeResources)
                if (name.ToLower() == className)
                    return false;
            return true;
        }

        private static bool AddNewId(StringBuilder sb, ControllerInfo controller, ProcInfo proc)
        {
            foreach (var param in proc.parameters)
                if (param.name.ToLower() == "@newid")
                {
                    sb.AppendFormat("\t {0} {1} = null;\n", param.csType, param.name.Substring(1));
                    return true;
                }
            return false;
        }

        private static bool HasIdParam(ProcInfo proc)
        {
            foreach (var p in proc.parameters)
                if (p.name.ToLower() == "@id")
                    return true;
            return false;
        }


        private static void AddReturn(StringBuilder sb, bool newId = false)
        {
            sb.AppendLine("\tif (0 == (int) RetVal.Value)");
            sb.AppendLine("\t\t RetVal.Value = 200;");
            sb.AppendLine("\tif (200 == (int) RetVal.Value || 201 == (int)RetVal.Value)  ");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tvar response = Request.CreateResponse((HttpStatusCode)RetVal.Value, \"null\");");
            if (newId)
            {
                sb.AppendLine("\t\tstring uri=Url.Link(\"DefaultApi\", new { id = com.Parameters[\"NewID\"].Value.ToString() });");
                sb.AppendLine("\t\tresponse.Headers.Location = new Uri(uri);");
            }
            sb.AppendLine("\t\treturn response;");
            sb.AppendLine("\t}");
            sb.AppendLine("\tif (DbUtil.message.Length > 0) ");
            sb.AppendLine("\t\treturn Request.CreateErrorResponse((HttpStatusCode)RetVal.Value, DbUtil.message);");
            sb.AppendLine("\telse");
            sb.AppendLine("\t\treturn Request.CreateResponse((HttpStatusCode)RetVal.Value);");
        }

        private static void AddAdoParams(StringBuilder sb, ControllerInfo controller, ProcInfo proc, String varName, bool skipId = false)
        {
            bool hasMemberNamedId = false;
            foreach (var col in controller.columns)
                if (col.name.ToLower() == "id")
                    hasMemberNamedId = true;

            foreach (var param in proc.parameters)
            {
                if (!(param.name.ToLower() == "@id" && skipId))
                {
                    var resourceFieldName = "";
                    if (varName != null && varName.Length > 0)
                        foreach (var col in controller.columns)
                            if (col.name.ToLower() == param.name.Substring(1).ToLower())
                                resourceFieldName = varName + "." + col.name;
                    var paramType = param.sqlType;
                    if (!(varName.ToLower() == "id" && skipId))
                    {
                        if (param.name.ToLower() == "@username")
                            sb.AppendFormat("\tcom.Parameters.Add(\"{0}\", SqlDbType.{1}, {2}).Value = User.Identity.Name;\n",
                                param.name.Substring(1), GetTypeName(param.sqlType), param.maxLen);
                        else if (param.name.ToLower() == "@id" && (!hasMemberNamedId || proc.name.EndsWith("put")) )
                            sb.AppendFormat("\tcom.Parameters.Add(\"{0}\", SqlDbType.{1}).Value = {2};\n",
                                param.name.Substring(1), GetTypeName(param.sqlType), param.name.Substring(1));
                        else if (paramType.ToLower().EndsWith("char"))
                            sb.AppendFormat("\tcom.Parameters.Add(\"{0}\", SqlDbType.{1}, {2}).Value = {3};\n",
                                param.name.Substring(1), GetTypeName(param.sqlType), param.maxLen,
                                (resourceFieldName.Length > 0) ? resourceFieldName : param.name.Substring(1));
                        else
                            sb.AppendFormat("\tcom.Parameters.Add(\"{0}\", SqlDbType.{1}).Value = {2};\n", param.name.Substring(1), GetTypeName(param.sqlType),
                                (resourceFieldName.Length > 0) ? resourceFieldName : param.name.Substring(1));
                        if (param.isOutput)
                            sb.AppendFormat("\tcom.Parameters[\"{0}\"].Direction = ParameterDirection.Output; \n",
                                (resourceFieldName.Length > 0) ? resourceFieldName : param.name.Substring(1));

                    }
                }
            }
        }

        private static void AddProc(StringBuilder sb, ProcInfo proc)
        {
            sb.AppendLine("\tusing(SqlConnection con = DbUtil.GetConnection()) {");
            sb.AppendFormat("\t\tSqlCommand com = new SqlCommand(\"API_{0}\",con);\n", proc.name);
            sb.AppendLine("\t\tcom.CommandType = CommandType.StoredProcedure;");

            sb.AppendLine("\t\tSqlParameter RetVal = com.Parameters.Add(\"RetVal\", SqlDbType.Int);");
            sb.AppendLine("\t\tRetVal.Direction = ParameterDirection.ReturnValue;");
        }

        private static void AddParameters(bool first, StringBuilder sb, ControllerInfo controller, ProcInfo proc, bool skipMembers, bool skipId = false)
        {
            // Add parameters not in resource   
            foreach (var param in proc.parameters)
            {
                var paramName = param.name.Substring(1);
                if (skipMembers)
                    foreach (var col in controller.columns)
                        if (col.name.ToLower() == param.name.Substring(1).ToLower() &&
                            col.name.ToLower() != "id")
                            paramName = "";
                if (paramName.Length > 0)
                {
                    if (paramName.ToLower() != "username")
                        if (!(skipId && paramName.ToLower() == "id") &&
                            !(proc.name.ToLower().EndsWith("post") && paramName.ToLower() == "newid"))
                        {
                            if (!first)
                                sb.Append(", ");
                            if (proc.name.ToLower().EndsWith("get") && paramName.ToLower() == "id")
                                sb.AppendFormat(" {0} {1} ", param.csType, paramName);
                            else
                                sb.AppendFormat(" {0} {1} = null", param.csType, paramName);
                            first = false;
                        }
                }
            }

        }

        private static bool IsMember(ControllerInfo controller, string varName, out string memberName)
        {
            foreach (var col in controller.columns)
                if (col.name.ToLower() == varName.ToLower())
                {
                    memberName = col.name;
                    return true;
                }
            memberName = "";
            return false;
        }

        private static void AddTrace(StringBuilder sb, ControllerInfo controller, ProcInfo proc, bool skipId = false)
        {
            string fmt = ""; int pi = 0;
            foreach (var param in proc.parameters)
            {
                if (!(param.name.ToLower() == "@id" && skipId))
                {
                    fmt += param.name + "={" + (pi).ToString() + "}";
                    pi += 1;
                    if (pi < (proc.parameters.Count))
                        fmt += ", ";
                }
            }
            fmt += ", return={" + (pi).ToString() + "}\",";
            pi = 0;
            foreach (var param in proc.parameters)
            {
                if (!(param.name.ToLower() == "@id" && skipId))
                {
                    if (pi > 0)
                        fmt += ",";
                    if (param.name.ToLower() == "@username")
                        fmt += "User.Identity.Name";
                    else
                    {
                        if (!proc.name.ToLower().EndsWith("get"))
                        {
                            string memberName = "";
                            if (IsMember(controller, param.name.Substring(1), out memberName) && !(param.name.ToLower() == "@id"))
                                fmt += "res." + memberName;
                            else
                                fmt += param.name.Substring(1);
                        }
                        else
                            fmt += param.name.Substring(1);
                    }
                    pi++;
                }
            }
            if (pi > 0)
                fmt += ", ";
            fmt += " RetVal.Value ";
            fmt += ")";
            sb.Append("\tlogger.Info(\"" + proc.name + ":" + fmt + ";\n");
        }


        public static string GetCSTypeName(ColumnInfo column)
        {
            switch (column.sqlType.ToLower())
            {
                case "bigint": return "long?";
                case "binary": return "binary";  // test??
                case "bit": return "bool?";
                case "boolean": return "bool?";
                case "char": return "string";
                case "date": return "DateTime?";
                case "datetime": return "DateTime?";
                case "datetimeoffset": return "DateTimeOffset?";
                case "decimal": return "decimal?";
                case "filestream": return "byte?[]";
                case "single": return "float?";
                case "float": return "double?";
                case "guid": return "Guid?";
                case "image": return "byte?[]";
                case "int": return "int?";
                case "int32": return "int?";
                case "int16": return "short?";
                case "int64": return "long?";
                case "money": return "decimal?";
                case "nchar": return "string";
                case "ntext": return "string";
                case "numeric": return "decimal?";
                case "nvarchar": return "string";
                case "real": return "Single";
                case "smalldatetime": return "DateTime?";
                case "smallint": return "short?";
                case "smallmoney": return "decimal?";
                case "string": return "string";
                case "text": return "string";
                case "timestamp": return "byte?[]";
                case "tinyint": return "byte";
                case "uniqueidentifier": return "Guid?";
                case "varbinary": return "byte?[]";
                case "varchar": return "string";
                case "xml": return "string";
                case "": return "";
                case "byte[]": return "byte[]";
                default: throw (new Exception("Unknown SQL Datatype: " + column.sqlType));
            }
        }

        private static string GetTypeName(string TypeName)
        {
            switch (TypeName)
            {
                case "nvarchar": return "NVarChar";
                case "varchar": return "VarChar";
                case "bigint": return "BigInt";
                case "binary": return "Binary";
                case "image": return "Binary";
                case "bit": return "Bit";
                case "date": return "Date";
                case "datetime": return "DateTime";
                case "decimal": return "Decimal";
                case "real": return "Float";
                case "float": return "Float";
                case "money": return "Money";
                case "nchar": return "NChar";
                case "char": return "Char";
                case "smallint": return "SmallInt";
                case "int": return "Int";
                case "ntext": return "NText";
                case "uniqueidentifier": return "UniqueIdentifier";
                case "guid": return "UniqueIdentifier";
                case "": return "";
                default: return TypeName;
            }
        }

    }
}

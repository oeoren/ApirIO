using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
    public static class TypeMapper
    {
        public static string GetCSTypeName(string sqlName)
        {
            switch (sqlName.ToLower())
            {
                case "varchar": return "string";
                case "string": return "string";
                case "bigint": return "int?";
                case "smallint": return "short?";
                case "int": return "int?";
                case "int32": return "int?";
                case "int16": return "int?";
                case "int64": return "long?";
                case "binary": return "binary";
                case "bit": return "bool?";
                case "date": return "DateTime?";
                case "datetime": return "DateTime?";
                case "decimal": return "decimal?";
                case "single": return "float?";
                case "real": return "float?";
                case "float": return "float?";
                case "double": return "float?";
                case "money": return "decimal?";
                case "nchar": return "string";
                case "char": return "string";
                case "ntext": return "string";
                case "boolean": return "bool?";
                case "uniqueidentifier": return "Guid?";
                case "image": return "binary";
                default: 
                    if (sqlName.ToLower().EndsWith("char"))
                        return "string";
                    else throw(new Exception("Unknown SQL Datatype: " + sqlName));

            }
        }

        public static string GetTSTypeName(string sqlName)
        {
            switch (sqlName.ToLower())
            {
                case "varchar": return "String";
                case "string": return "String";
                case "bigint": return "Number";
                case "smallint": return "Number";
                case "int": return "Number";
                case "int32": return "Number";
                case "int16": return "Number";
                case "int64": return "Number";
                case "binary": return "binary";
                case "bit": return "bool?";
                case "date": return "Date";
                case "datetime": return "Date";
                case "decimal": return "Number";
                case "single": return "Number";
                case "real": return "Number";
                case "float": return "Number";
                case "double": return "Number";
                case "money": return "Number";
                case "nchar": return "String";
                case "char": return "String";
                case "ntext": return "String";
                case "boolean": return "boolean";
                case "uniqueidentifier": return "String";
                case "image": return "binary";
                default:
                    if (sqlName.ToLower().EndsWith("char"))
                        return "string";
                    else throw (new Exception("Unknown SQL Datatype: " + sqlName));

            }
        }

    }
}

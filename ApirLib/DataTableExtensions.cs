using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomExtensions
{

    public static class MyExtensionClass
    {
        public static List<T> ToCollection<T>(this DataTable dt)
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
                        if (d != null)
                            pc.SetValue(cn, item[pc.Name], null);
                    }
                    catch
                    {
                    }
                }
                lst.Add(cn);
            }
            return lst;
        }
    }

        static class DataTableExtensions
        {
            // Get a list of custom class from datatable
            // Usage: List<Customer> customers = dsCustomers.Tables[0].ToGenericList<Customer>();
            public static List<T> ToGenericList<T>(this DataTable datatable) where T : new()
            {
                return (from row in datatable.AsEnumerable()
                        select Convert<T>(row)).ToList();
            }

            private static T Convert<T>(DataRow row) where T : new()
            {
                var result = new T();
                var type = result.GetType();

                foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var rowValue = row[fieldInfo.Name];
                    if (rowValue != null)
                    {
                        fieldInfo.SetValue(result, rowValue);
                    }
                }

                return result;
            }
        }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
    public class SqlProcBuilder
    {
        static public void Exec(string conString, string src)
        {
            var con = new SqlConnection(conString);
            SqlCommand cmd = new SqlCommand(src, con);
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
                cmd = null;
                con.Close();
            }

        }
    }
}

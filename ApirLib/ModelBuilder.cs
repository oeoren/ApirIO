using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
	public class ModelBuilder
	{

        static public TableModel ConstructTableModel(string conString, string tableName, string resource)
        {
            TableModel tableModel = new TableModel();
            tableModel.tableName = tableName;
            tableModel.resource = resource;
            var con = new SqlConnection(conString);
            SqlCommand cmd = con.CreateCommand();
            SqlCommand cmd2 = con.CreateCommand();
            SqlCommand cmd3 = con.CreateCommand();

            cmd.CommandType = CommandType.Text;
            string query = string.Format( @"
                SELECT  
                          b.column_name
                FROM      information_schema.table_constraints a,
                          information_schema.key_column_usage b
                WHERE     a.table_name = '{0}'
                AND       a.table_name = b.table_name
                AND       a.table_schema = b.table_schema
                AND       a.constraint_name = b.constraint_name
                AND       a.constraint_type = 'PRIMARY KEY'
            ", tableName);
            cmd.CommandText = query;

            cmd2.CommandType = CommandType.Text;
            query = string.Format(@"
                    select column_name, data_type, is_nullable,
                        character_maximum_length
                        from information_schema.columns
                        where table_name = '{0}';
                    ", tableName);
            cmd2.CommandText = query;

            DataTable table = new DataTable();
            DataTable table2 = new DataTable();

            cmd3.CommandType = CommandType.Text;
            con.Open();
            try
            {
                SqlDataAdapter da = null;
                using (da = new SqlDataAdapter(cmd))
                {
                    da.Fill(table);
                }
                if (table.Rows.Count != 1)
                    throw new Exception("Table must have one primary key column");
                tableModel.KeyColum = table.Rows[0]["column_name"].ToString();

                using (da = new SqlDataAdapter(cmd2))
                {
                    da.Fill(table2);
                }

                foreach (DataRow row in table2.Rows)
                {
                    tableModel.columns.Add(new ColumnInfo
                    {
                        name = row["column_name"].ToString(),
                        sqlType = row["data_type"].ToString(),
                        maxLen = (DBNull.Value.Equals(row["character_maximum_length"])) ? null : (int?)row["character_maximum_length"]
                    });
                }

                cmd3.CommandText = string.Format("select columnproperty(object_id('{0}'),'{1}','IsIdentity')", tableModel.tableName, tableModel.KeyColum);
                int? isIdentity = (int?)cmd3.ExecuteScalar();
                tableModel.isIdentity = (isIdentity == 1);

                tableModel.nonKeyColumns = new List<ColumnInfo>();
                foreach (var col in tableModel.columns)
                    if (col.name != tableModel.KeyColum) // || !tableModel.isIdentity)
                        tableModel.nonKeyColumns.Add(col);




            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
                cmd = null;
                cmd2.Dispose();
                cmd2 = null;
                cmd3.Dispose();
                cmd3 = null;
                con.Close();
            }


            return tableModel;
        }


        static public Model ConstructModel(string conString)
		{
			Model model = new Model();
			var con = new SqlConnection(conString);
			SqlCommand cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			string query = @"SELECT  
					ProcedureName = ir.ROUTINE_NAME, 
					ParameterName = COALESCE(ip.PARAMETER_NAME, '<no params>'),
                    SqlType = ip.DATA_TYPE,  Precision = ip.NUMERIC_PRECISION, Scale = ip.NUMERIC_SCALE, MaxLen = ip.CHARACTER_MAXIMUM_LENGTH,
					DataType = COALESCE(UPPER(ip.DATA_TYPE) + CASE 
						WHEN ip.DATA_TYPE IN ('NUMERIC', 'DECIMAL') THEN  
							'(' + CAST(ip.NUMERIC_PRECISION AS VARCHAR)  
							+ ', ' + CAST(ip.NUMERIC_SCALE AS VARCHAR) + ')'  
						WHEN RIGHT(ip.DATA_TYPE, 4) = 'CHAR' THEN 
							'(' + CAST(ip.CHARACTER_MAXIMUM_LENGTH AS VARCHAR) + ')' 
						ELSE '' END + CASE ip.PARAMETER_MODE  
						WHEN 'INOUT' THEN ' OUTPUT' ELSE ' ' END, '-'),
					ParameterMode =	ip.PARAMETER_MODE 
				FROM  
					INFORMATION_SCHEMA.ROUTINES ir 
					LEFT OUTER JOIN 
					INFORMATION_SCHEMA.PARAMETERS ip 
					ON ir.ROUTINE_NAME = ip.SPECIFIC_NAME 
				WHERE 
					ir.ROUTINE_NAME LIKE 'API%' 
					AND ir.ROUTINE_TYPE = 'PROCEDURE' 
					AND COALESCE(OBJECTPROPERTY 
					( 
						OBJECT_ID(ip.SPECIFIC_NAME), 
						'IsMsShipped' 
					), 0) = 0 
				ORDER BY  
					ir.ROUTINE_NAME, 
					ip.ORDINAL_POSITION";
			cmd.CommandText = query;
			DataTable table = new DataTable();
			con.Open();

			try
			{
				SqlDataAdapter da = null;
				using (da = new SqlDataAdapter(cmd))
				{
					da.Fill(table);
				}

				foreach (DataRow row in table.Rows)
				{
					string procName = row["ProcedureName"].ToString();
					ControllerInfo controller = GetControllerInfo( model, procName);
                    if (controller != null)
                    {
                        ProcInfo proc = GetProcInfo(controller, procName);
                        string parameterName = row["ParameterName"].ToString();
                        if (parameterName != "<no params>")
                            proc.parameters.Add(new ParameterInfo
                            {
                                name = parameterName,
                                precision = (DBNull.Value == row["Precision"]) ? 0 : int.Parse(row["Precision"].ToString()),
                                scale = (DBNull.Value == row["Scale"]) ? 0 : int.Parse(row["Scale"].ToString()),
                                maxLen = (DBNull.Value == row["Maxlen"]) ? 0 : int.Parse(row["MaxLen"].ToString()),
                                sqlType = row["SqlType"].ToString(),
                                isOutput = (row["ParameterMode"].ToString() != "IN")
                            });
                    }
				}

				foreach (ControllerInfo controller in model.controllers)
				{
                    foreach (ProcInfo proc in controller.procs)
                    {
                        if ((proc.name == controller.name + "get") || (proc.name == controller.name + "_get"))
                            if (controller.columns.Count == 0)
                            {
                                ConstructEntity(con, controller, proc.name);
                            }

                        foreach (var par in proc.parameters)
                                par.csType = TypeMapper.GetCSTypeName(par.sqlType);
                        GetXmlComments(con, proc);
                    }
				}
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
			
			return model;
		}

        private static void GetXmlComments(SqlConnection con, ProcInfo proc)
        {
            SqlCommand sqlCommand = new SqlCommand("sys.sp_helptext", con);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@objname","api_"+proc.name);
            DataSet ds = new DataSet();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
            sqlDataAdapter.SelectCommand = sqlCommand;
            sqlDataAdapter.Fill(ds);
            string comments = "";
            if (ds.Tables.Count > 0)
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    if (r.ItemArray.Length > 0)
                    {
                        string l = r[0].ToString();
                        l = l.Trim();
                        if (l != null && l.Length > 3 && l.Substring(0, 3) == "---") 
                            comments += "///" +  l.Substring(3) + "\n";
                    }
                }
            proc.xmlComments = comments;
        }


		static void ConstructEntity(SqlConnection con, ControllerInfo controller, string nameEntityGet)
		{
			SqlCommand cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			string query = string.Format( @"        
				SET FMTONLY ON;
				EXEC dbo.api_{0}
				SET FMTONLY OFF;
				", nameEntityGet);
			cmd.CommandText = query;
			DataTable table = new DataTable();
//			con.Open();

			try
			{
				SqlDataAdapter da = null;
				using (da = new SqlDataAdapter(cmd))
				{
					da.Fill(table);
				}
				foreach (DataColumn col in table.Columns)
				{
					ColumnInfo newColumn = new ColumnInfo { 
                        name = col.ColumnName, 
                        sqlType = col.DataType.Name, 
                        maxLen = col.MaxLength
                    };
					controller.columns.Add(newColumn);
				}
			}
			catch (Exception ex)
			{
                Console.WriteLine("ConstructEntity: ", ex.Message);
				throw ex;
			}
			finally
			{
				cmd.Dispose();
				cmd = null;
			}
		}

        private static string SkipAPI(string name)
        {
            name = name.ToLower();
            if (!name.StartsWith("api"))
                return null;
            name = name.Substring(3);
            if (name.StartsWith("_"))
                name = name.Substring(1);
            return name;
        }

		private static ProcInfo GetProcInfo(ControllerInfo controller, string procName)
		{
            procName = SkipAPI(procName);
            foreach (var proc in controller.procs)
				if (proc.name == procName)
					return proc;
			ProcInfo newProc = new ProcInfo { name = procName };
			controller.procs.Add(newProc);
			return newProc;
		}
		
		private static ControllerInfo GetControllerInfo( Model model , string procName)
		{
            procName = SkipAPI(procName);
            if (procName.EndsWith("delete"))
				procName = procName.Substring(0,procName.Length - 6);
			else if (procName.EndsWith("get"))
				procName = procName.Substring(0, procName.Length - 3);
            else if (procName.EndsWith("put"))
				procName = procName.Substring(0, procName.Length - 3);
            else if (procName.EndsWith("post"))
				procName = procName.Substring(0, procName.Length - 4);
            else
				return null;

            if (procName.EndsWith("_"))
                procName = procName.Substring(0, procName.Length - 1);

			foreach (var controller in model.controllers)
				if (controller.name == procName)
					return controller;
			ControllerInfo newController = new ControllerInfo { name = procName };
			model.controllers.Add(newController);
			return newController;
		}


	}
}

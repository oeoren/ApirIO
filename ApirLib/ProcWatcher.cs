using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace ApirLib
{
    public class ProcWatcher
    {
        Timer _timer;
        string _connectionString;
        Func<int> _whenProcChanged;
        DateTime? _lastDate;
        
        public void DoWatch(string connectionString, Func<int> WhenProcChanged)
        {
            _whenProcChanged = WhenProcChanged;
            _lastDate = null;
            _connectionString = connectionString;
            _timer = new Timer(10000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) => CheckProcs();
            _timer.Enabled = true;
        }


        void CheckProcs()
        {

            SqlConnection con = null;
            SqlCommand cmd;
            try
            {
                con = new SqlConnection(_connectionString);
                con.Open();
            }
            catch (Exception ex)
            {
                return;
            }
            cmd = con.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select MAX(modify_date) from sys.procedures where name like 'API%'";
            var dt = (DateTime?) cmd.ExecuteScalar();
            if (_lastDate == null)
                _lastDate = dt;
            else if (dt > _lastDate)
                _whenProcChanged();
        }

    }
}
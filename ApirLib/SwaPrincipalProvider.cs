using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;


namespace Apir
{
    public class SwaPrincipalProvider : IProvidePrincipal
    {


        private string _procName;
        private string _connectionString;
        private string _domainName;
        private string _machineName;

        public SwaPrincipalProvider(string connectionString, string procName, string domainName=null, string machineName =null)
        {
            _procName = procName;
            _domainName = domainName;
            _machineName = machineName;
            _connectionString = connectionString;
        }

        private bool  SqlValidate(string userName, string password)
        {
            if (_procName == null || _procName.Length == 0)
                return true;
            SqlConnection con = new SqlConnection(_connectionString);
            SqlCommand com = new SqlCommand(_procName, con); 
            com.CommandType = CommandType.StoredProcedure;
            SqlParameter RetVal = com.Parameters.Add
               ("RetVal", SqlDbType.Int);
            RetVal.Direction = ParameterDirection.ReturnValue;
            com.Parameters.Add("UserName", SqlDbType.VarChar,60).Value = userName;
            com.Parameters.Add("Password", SqlDbType.VarChar, 60).Value = password;
            con.Open();
            try
            {
                com.ExecuteNonQuery();
                int r = (int)RetVal.Value;
                bool ret = (r == 1);
                return (ret) ;
            }
            catch (SqlException ex)
            {
                throw(ex);
            }
        }
        public  bool ValidateCredentials(string userName, string password)
        {

            if (_machineName != null && _machineName.Length > 0)
                return DomainValidate(userName, password, null, _machineName);
            else if (_domainName != null && _domainName.Length > 0)
                return DomainValidate(userName, password, _domainName, null);
            else
                return SqlValidate(userName, password);
        }

        private bool DomainValidate(string userName, string password, string domainName, string machineName)
        {
            if (machineName != null && machineName.Length > 0)
                using (PrincipalContext pc = new PrincipalContext(ContextType.Machine, machineName))
                {
                    return pc.ValidateCredentials(userName, password);
                }
            else
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    return pc.ValidateCredentials(userName, password);
                }
        }



        public IPrincipal CreatePrincipal(string username, string password)
        {
            if (!ValidateCredentials(username, password))
            {
                return null;
            }

            var identity = new GenericIdentity(username);
            IPrincipal principal = new GenericPrincipal(identity, new[] { "User" });
            return principal;
        }
    }
}
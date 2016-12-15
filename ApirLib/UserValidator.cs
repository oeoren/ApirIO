using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
    public class UserValidator : UserNamePasswordValidator
    {
        private string _procName;
        private string _connectionString;
        public UserValidator(string connectionString, string procName)
        {
            _procName = procName;
            _connectionString = connectionString;
        }

        public override void Validate(string userName, string password)
        {
            string userValidator = ConfigurationManager.AppSettings["UserValidator"];
            string domainValidate = ConfigurationManager.AppSettings["DomainValidate"];
            if (domainValidate != null && domainValidate.Length > 0)
                DomainValidate(userName, password, domainValidate);
            else
                SqlValidata(userName, password);
        }

        private void DomainValidate(string userName, string password, string domainName)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
            {
                bool isValid = pc.ValidateCredentials(userName, password);
                if (!isValid)
                    throw new SecurityTokenException("Unknown Username or Incorrect Password in domain");
            }         
        }

        private void SqlValidata(string userName, string password)
        {
            SqlConnection con = new SqlConnection(_connectionString);
            SqlCommand com = new SqlCommand(_procName, con);
            com.CommandType = CommandType.StoredProcedure;
            SqlParameter RetVal = com.Parameters.Add
               ("RetVal", SqlDbType.Int);
            RetVal.Direction = ParameterDirection.ReturnValue;
            com.Parameters.Add("UserName", SqlDbType.VarChar, 60).Value = userName;
            com.Parameters.Add("Password", SqlDbType.VarChar, 60).Value = password;
            con.Open();
            try
            {
                com.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                throw new SecurityTokenException("Unknown Username or Incorrect Password");
            }
            if ((int)RetVal.Value != 1)
                throw new SecurityTokenException("Unknown Username or Incorrect Password");
        }
    }
}

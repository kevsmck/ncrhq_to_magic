using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace NCRHQ_to_Magic.Datasources
{
    class MSSQL : IDatabase
    {
        /// getConnectionString()
        /// <summary>
        /// Returns a connection string for a Microsoft SQL Server database from
        /// the settings file
        /// </summary>
        /// <returns>string</returns>
        private string getConnectionString()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.connectionString))
            {
                return Properties.Settings.Default.connectionString;
            }
            else
            {
                return string.Empty;
            }
        }

        /// getDataFromStoredProc(string, List<SqlParameter>)
        /// <summary>
        ///  Returns data from the Microsoft SQL Server database from a stored procedure name
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParameters"></param>
        /// <returns>ReturnData</returns>
        public ReturnData getDataFromStoredProc(string procedureName, List<SqlParameter> sqlParameters)
        {
            ReturnData returnData = new ReturnData(null, false, 0, "Query has not been run yet");
            string connectionString = getConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                returnData = new ReturnData(null, false, 0, "No connectionString value in App.config");
                return returnData;
            }
            DataTable dtReturn = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(procedureName, connection);
                foreach (SqlParameter _sqlParam in sqlParameters)
                {
                    command.Parameters.Add(_sqlParam.ParameterName, _sqlParam.SqlDbType);
                    command.Parameters[_sqlParam.ParameterName].Value = _sqlParam.Value;
                }
                try
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    SqlDataReader _drData;
                    _drData = command.ExecuteReader();
                    dtReturn.Load(_drData);
                    _drData.Close();

                    returnData = new ReturnData(dtReturn, true, dtReturn.Rows.Count, string.Empty);
                }
                catch (SqlException sqlex)
                {
                    returnData = new ReturnData(null, false, 0, string.Concat("Unexpected SQL Exception: " + sqlex.Message));
                }
                catch (Exception ex)
                {
                    returnData = new ReturnData(null, false, 0, string.Concat("Unexpected Exception: " + ex.Message));
                }
                finally
                {
                    command.Dispose();
                    if (connection.State.Equals(ConnectionState.Open))
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                    connectionString = string.Empty;
                }
            }
            return returnData;
        }

        /// getDataFromSQLString(string)
        /// <summary>
        ///  Returns data from the Microsoft SQL Server database from SQL code
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>ReturnData</returns>
        public ReturnData getDataFromSQLString(string sql)
        {
            ReturnData returnData = new ReturnData(null, false, 0, "Query has not been run yet");

            string connectionString = getConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                returnData = new ReturnData(null, false, 0, "No connectionString value in App.config");
                return returnData;
            }
            DataTable dtReturn = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                SqlCommand command = new SqlCommand(sql, connection);
                try
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();


                    SqlDataReader _drData;
                    _drData = command.ExecuteReader();
                    dtReturn.Load(_drData);
                    _drData.Close();

                    returnData = new ReturnData(dtReturn, true, dtReturn.Rows.Count, string.Empty);
                }
                catch (SqlException sqlex)
                {
                    returnData = new ReturnData(null, false, 0, string.Concat("Unexpected SQL Exception: " + sqlex.Message));
                }
                catch (Exception ex)
                {
                    returnData = new ReturnData(null, false, 0, string.Concat("Unexpected Exception: " + ex.Message));
                }
                finally
                {
                    command.Dispose();
                    if (connection.State.Equals(ConnectionState.Open))
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                    connectionString = string.Empty;
                }
            }
            return returnData;
        }

        /// nonQueryStoredProcedure(string, List<SqlParameter>)
        /// <summary>
        ///  Runs a non-query stored procedure in the Microsoft SQL Server database
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParameters"></param>
        public void nonQueryStoredProcedure(string procedureName, List<SqlParameter> sqlParameters)
        {

        }

        /// nonQueryFromSQLString(string)
        /// <summary>
        ///  Runs a non-query SQL statement in the Microsoft SQL Server database
        /// </summary>
        /// <param name="sql"></param>
        public void nonQueryFromSQLString(string sql)
        {

        }
    }
}

using System.Collections.Generic;
using System.Data.SqlClient;

namespace NCRHQ_to_Magic.Datasources
{
    // Use an interface in case we want to expand on multiple data source types
    interface IDatabase
    {
        /// getDataFromStoredProc(string, List<SqlParameter>)
        /// <summary>
        ///  Returns data from the database from a stored procedure name
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParameters"></param>
        /// <returns>ReturnData</returns>
        ReturnData getDataFromStoredProc(string procedureName, List<SqlParameter> sqlParameters);

        /// getDataFromSQLString(string)
        /// <summary>
        ///  Returns data from the database from SQL code
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>ReturnData</returns>
        ReturnData getDataFromSQLString(string sql);

        /// nonQueryStoredProcedure(string, List<SqlParameter>)
        /// <summary>
        ///  Runs a non-query stored procedure in the database
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParameters"></param>
        void nonQueryStoredProcedure(string procedureName, List<SqlParameter> sqlParameters);

        /// nonQueryFromSQLString(string)
        /// <summary>
        ///  Runs a non-query SQL statement in the database
        /// </summary>
        /// <param name="sql"></param>
        void nonQueryFromSQLString(string sql);
    }
}

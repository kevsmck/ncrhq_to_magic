using System.Data;

namespace NCRHQ_to_Magic.Datasources
{
    class ReturnData
    {
        private DataTable table { get; set; }
        private bool success { get; set; }
        private int rowCount { get; set; }
        private string returnMessage { get; set; }
        public ReturnData(DataTable table, bool success, int rowCount, string returnMessage)
        {
            this.table = table;
            this.success = success;
            this.rowCount = rowCount;
            this.returnMessage = returnMessage;
        }

        /// getDataTable()
        /// <summary>
        /// Returns the DataTable object assigned to the ReturnData class
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable getDataTable()
        {
            return table;
        }

        /// isSuccess()
        /// <summary>
        /// Returns the a true/false flag for if the task completed successfully
        /// </summary>
        /// <returns>bool</returns>
        public bool isSuccess()
        {
            return success;
        }

        /// getRowCount()
        /// <summary>
        /// Returns a count of rows in the DataTable
        /// </summary>
        /// <returns>int</returns>
        public int getRowCount()
        {
            return rowCount;
        }

        /// getReturnMessage()
        /// <summary>
        /// Returns any additional message such as an error message
        /// </summary>
        /// <returns>string</returns>
        public string getReturnMessage()
        {
            return returnMessage;
        }
    }
}

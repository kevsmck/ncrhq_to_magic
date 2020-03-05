using NCRHQ_to_Magic.Datasources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NCRHQ_to_Magic
{
    class Program
    {
        #region Objects
        static string exportFolder = string.Empty; // Base export folder
        static string storeSQLFile = string.Empty; // SQL file that delivers the list of stores
        static string tablesJSON = string.Empty; // The tables.json file that holds the values for which SQL file to use, what export filename should be etc.
        static string fillerChar = ","; // The filler character (can be replaced in settings file)
        static bool showDebug = false; // True/False for if debug information should be written to the console
        #endregion
        #region Methods
        static void Main(string[] args)
        {

            string codeBase = Assembly.GetExecutingAssembly().GetName().ToString();
            string[] appInfo = codeBase.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            Util.Logging.Log(Util.Logging.Type.None, string.Concat(appInfo[0], appInfo[1].Replace("=", ": ")), string.Empty, string.Empty);

            // Get the settings from the settings file
            if (!getSettings())
            {
                // There was an error getting the settings.
                // Actual error is reported in getSettings()
                return; // Exit out of the application
            }

            var jsonInput = string.Empty; // Used to hold the json input
            if (File.Exists(tablesJSON)) {
                // If the tables.json file exists, read the text, then deserialize to a List<TableSettings> object
                jsonInput = File.ReadAllText(tablesJSON);
                List<TableSettings> tableSettingList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TableSettings>>(jsonInput);
                if (tableSettingList.Count < 1)
                {
                    // There must be at least one table in the tables.json file
                    Util.Logging.Log(
                        Util.Logging.Type.Critical,
                        "Unable to get data from tables.json",
                        "Main_tablesJSON_001",
                        "No tables were returned in tables.json"
                        );
                    return;
                }

                // Get the list of stores
                List<Tables.Store> listStores = getStores();

                if (listStores != null)
                {
                    if (listStores.Count < 1)
                    {
                        // There must be at least one store
                        Util.Logging.Log(
                            Util.Logging.Type.Critical,
                            "Unable to process stores",
                            "Main_getStores_001",
                            "There are no stores available"
                            );
                        return;
                    }

                    // Loop through each table in the TableSettings list
                    foreach (TableSettings tableSettings in tableSettingList)
                    {
                        // Check if the table is active
                        if (tableSettings.active)
                        {
                            // Display the table name that is being processed
                            Util.Logging.Log(Util.Logging.Type.Info, string.Concat("* Processing ", tableSettings.table), string.Empty, string.Empty);

                            /*
                              Check if we're using the store number in the query for this table
                              If we are, we'll need to run the query once for each store as well
                              as writing the data once for each store. If we are not, then we can 
                              run the query once and then just write it out for each store in turn.
                            */
                            if (tableSettings.use_store_number)
                            {
                                // Get data for each store, write for each store
                                foreach (Tables.Store store in listStores)
                                {
                                    ReturnData returnData = getData(tableSettings, store);
                                    writeData(returnData, tableSettings, store);
                                }
                            }
                            else
                            {
                                // Get data once, write for each store
                                ReturnData returnData = getData(tableSettings, null);
                                foreach (Tables.Store store in listStores)
                                {
                                    writeData(returnData, tableSettings, store);
                                }
                            }
                        }
                        else
                        {
                            // Display the inactive table name that will NOT be processed
                            Util.Logging.Log(Util.Logging.Type.Info, string.Concat("* NOT Processing ", tableSettings.table, " (Inactive)"), string.Empty, string.Empty);
                        }
                    }
                }
                else
                {
                    // Store list is null, unable to continue processing
                    Util.Logging.Log(
                        Util.Logging.Type.Critical,
                        "Unable to process stores",
                        "Main_getStores_001",
                        "Store list is null"
                        );
                    return;
                }
            } else {
                // tables.json file does not exist
                Util.Logging.Log(
                    Util.Logging.Type.Critical,
                    "Unable to process tables.json",
                    "Main_getStores_001",
                    string.Concat("File does not exist.  Filename: ", tablesJSON)
                    );
                return;
            }

            Console.ReadLine(); // TODO: Remove this in production
        }

        /// getSettings()
        /// <summary>
        ///  Tries to read the settings for the program
        /// <example> For example:
        /// <code>bool result = getSettings()</code>
        /// would populate the various settings and return true if all load correctly
        /// </example>
        /// Error messages will be output to the console
        /// </summary>
        /// <returns>bool</returns>
        static bool getSettings()
        {
            // Get the 'base' export folder from pathExport setting
            if (!string.IsNullOrEmpty(Properties.Settings.Default.pathExport))
            {
                exportFolder = Properties.Settings.Default.pathExport;
                // Remove any trailing \ in the folder name
                if (exportFolder.Substring(exportFolder.Length - 1, 1) == @"\")
                {
                    exportFolder = exportFolder.Substring(0, exportFolder.Length - 1);
                }
                // Check if the folder exists
                if (!Directory.Exists(exportFolder))
                {
                    // If folder does not exist, try and create it
                    if (!createFolder(exportFolder))
                    {
                        // If unable to create the folder, unable to continue
                        Util.Logging.Log(Util.Logging.Type.Critical, "Unable to get or create export folder from App.config", "Program_getExportFolder_001", string.Empty);
                        return false;
                    }
                }
            }
            else
            {
                // Setting is not entered, unable to continue.
                Util.Logging.Log(Util.Logging.Type.Critical, "Unable to get export folder from App.config", "Program_getExportFolder_002", string.Empty);
                return false;
            }

            // Get the SQL file that has the store information
            // TODO: Possibly make the store lookup stored procedure based
            //       If so, this setting will need to be deprecated.
            if (!string.IsNullOrEmpty(Properties.Settings.Default.storeSQLFile))
            {
                storeSQLFile = Properties.Settings.Default.storeSQLFile;
                // Check if the file exists
                if (!File.Exists(storeSQLFile))
                {
                    Util.Logging.Log(Util.Logging.Type.Critical, "Store SQL file does not exist.  Unable to continue", "Program_StoreSQL_001", string.Empty);
                    return false;
                }
            }
            else
            {
                Util.Logging.Log(Util.Logging.Type.Critical, "Unable to get SQL path by store from App.config", "Program_StoreSQL_002", string.Empty);
                return false;
            }

            // Get the tables.json filename
            if (!string.IsNullOrEmpty(Properties.Settings.Default.tablesJSON))
            {
                tablesJSON = Properties.Settings.Default.tablesJSON;
                // Check if the file exists
                if (!File.Exists(storeSQLFile))
                {
                    Util.Logging.Log(Util.Logging.Type.Critical, "tables.json file does not exist.  Unable to continue", "Program_TablesJSON_001", string.Empty);
                    return false;
                }
            }
            else
            {
                Util.Logging.Log(Util.Logging.Type.Critical, "Unable to get SQL path by store from App.config", "Program_TablesJSON_002", string.Empty);
                return false;
            }

            // Get the filler character if it exists in the settings file
            // If it does not, the default can be used
            if (!string.IsNullOrEmpty(Properties.Settings.Default.fillerChar))
            {
                fillerChar = Properties.Settings.Default.fillerChar;
            }

            // Get the flag which indicates if debug messages should be shown
            if (!string.IsNullOrEmpty(Properties.Settings.Default.showDebug.ToString()))
            {
                // If not set, stays as false
                showDebug = Properties.Settings.Default.showDebug;
            }

            return true;
        }

        /// createFolder(string)
        /// <summary>Tries to create a folder for the path specified
        /// <example> For example:
        /// <code>bool result = createFolder(@"C:\Temp\NewFolder");</code>
        /// would result in the NewFolder folder being created in C:\Temp
        /// </example>
        /// </summary>
        /// <returns>bool</returns>
        static bool createFolder(string path)
        {
            // Check to make sure a blank path was not provided
            if (!string.IsNullOrEmpty(path))
            {
                // Check if the folder already exists
                if (Directory.Exists(path))
                {
                    Util.Logging.Log(
                        Util.Logging.Type.Error,
                        "Folder already exists",
                        "Program_createFolder_001",
                        string.Concat("Folder name: ", path)
                        );
                    return false;
                }
                try
                {
                    // Try and create the folder
                    Directory.CreateDirectory(path);
                    return true;
                }
                catch (Exception ex)
                {
                    // Unexpected error trying to create the folder
                    Util.Logging.Log(
                        Util.Logging.Type.Error, 
                        "Unable to create folder", 
                        "Program_createFolder_002",
                        string.Concat("Folder name: ", path, " | Error: ", ex.Message)
                        );
                    return false;
                }
            }
            else
            {
                // A blank path was provided
                Util.Logging.Log(
                    Util.Logging.Type.Error,
                    "Unable to create folder",
                    "Program_createFolder_003",
                    "No path specified"
                    );
                return false;
            }
        }

        /// getStores()
        /// <summary>
        ///  Gets a list of stores from the STORE_TABLE table
        /// </summary>
        /// <param name="path"></param>
        /// <returns>List<Store></returns>
        static List<Tables.Store> getStores()
        {
            // TODO: Look into pulling this data from a stored procedure
            //       instead of using a SQL file
            string sql = string.Empty;
            // Check if the SQL file exists
            if (File.Exists(storeSQLFile))
            {
                // Read the data
                sql = File.ReadAllText(storeSQLFile);
            }
            else
            {
                // ERROR - File does not exist
                Util.Logging.Log(
                    Util.Logging.Type.Critical,
                    "STORE.SQL does not exist",
                    "Program_getStores_001",
                    string.Empty
                    );
                return null;
            }

            IDatabase database = new MSSQL(); // Create the database object
            ReturnData returnData = database.getDataFromSQLString(sql); // Pull the data from the database
            // Check if the data was returned successfully
            if (returnData.isSuccess())
            {
                // Check to make sure we have at least 1 row
                if (returnData.getRowCount() < 1)
                {
                    // If we don't have any rows returned, we can't continue
                    Util.Logging.Log(Util.Logging.Type.Critical, "Unable to get Store data", "Program_getStores_001", "No rows returned");
                    return null;
                }

                // Create a new list for the Stores
                List<Tables.Store> listStores = new List<Tables.Store>();

                // Loop through the datatable and populate a Store object to add to the list
                foreach (DataRow storeRow in returnData.getDataTable().Rows)
                {
                    // Check if STORE_ID is blank or null
                    if (string.IsNullOrEmpty(storeRow["STORE_ID"].ToString()))
                    {
                        // Unable to continue without STORE_ID
                        Util.Logging.Log(Util.Logging.Type.Critical, "Unable to get Store data", "Program_getStores_002", "Store ID is returning blank or null");
                        return null;
                    }
                    // Check to make sure that STORE_ID is numeric
                    if (Util.Helper.isNumeric(storeRow["STORE_ID"].ToString()))
                    {
                        bool folderExists = true;
                        // Check to see if the folder exists for this PRM_STORE_NUMBER
                        if (!Directory.Exists(string.Concat(exportFolder, @"\", storeRow["PRM_STORE_NUMBER"].ToString())))
                        {
                            // If it does not exist, try and create it
                            if (!createFolder(string.Concat(exportFolder, @"\", storeRow["PRM_STORE_NUMBER"].ToString()))) {
                                folderExists = false;
                            }
                        }
                        if (folderExists)
                        {
                            // Add a new Store object to the list with STORE_ID and PRM_STORE_NUMBER
                            listStores.Add(
                                new Tables.Store(
                                        Convert.ToInt32(storeRow["STORE_ID"].ToString()),
                                        storeRow["PRM_STORE_NUMBER"].ToString()
                                    )
                                );
                        }
                    }
                    else
                    {
                        // STORE_ID is not numeric, do not add it to the list
                        Util.Logging.Log(
                            Util.Logging.Type.Warning, 
                            "STORE_ID is not returning as a number", 
                            "Program_getStores_003", 
                            "Store ID: " + storeRow["STORE_ID"].ToString()
                            );
                    }
                }
                return listStores;
            }
            else
            {
                // Database did not return any Store data
                Util.Logging.Log(Util.Logging.Type.Critical, "Unable to get Store data", "Program_getStores_004", returnData.getReturnMessage());
                return null;
            }
        }

        /// getData(TableSettings, Tables.Store)
        /// <summary>
        ///  Tries to get the data for the table specified in the tableSettings
        ///  object, for the store specified,
        /// </summary>
        /// <param name="tableSettings"></param>
        /// <param name="store"></param>
        static ReturnData getData(TableSettings tableSettings, Tables.Store store) {

            if (showDebug)
            {
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("Getting ", tableSettings.table, " data"), string.Empty, string.Empty);
                if (store != null)
                {
                    Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("PRM_STORE_NUMBER: ", store.PRM_STORE_NUMBER), string.Empty, string.Empty);
                }
                else
                {
                    Util.Logging.Log(Util.Logging.Type.Debug, "Store not passed", string.Empty, string.Empty);
                }
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("Table: ", tableSettings.table), string.Empty, string.Empty);
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("Record Type: ", tableSettings.record_type), string.Empty, string.Empty);
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("Export File: ", tableSettings.export_file), string.Empty, string.Empty);
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("Stored Procedure: ", tableSettings.stored_procedure), string.Empty, string.Empty);
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("Use Stored Procedure?: ", tableSettings.use_stored_proc.ToString()), string.Empty, string.Empty);
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("SQL Filename: ", tableSettings.sql_filename.ToString()), string.Empty, string.Empty);
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("Use Store Number?: ", tableSettings.use_store_number.ToString()), string.Empty, string.Empty);
            }

            // Get the database object
            IDatabase database = new MSSQL();
            // Define returnData as unsuccessful
            ReturnData returnData = new ReturnData(null, false, 0, string.Empty);

            // Check if we're using a stored procedure or a SQL file
            if (tableSettings.use_stored_proc) {
                // Use a stored procedure instead of a SQL file
                if (!string.IsNullOrEmpty(tableSettings.stored_procedure)) {
                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                    if (tableSettings.use_store_number)
                    {
                        if (store == null)
                        {
                            // Store must be passed if we are using it to generate the query
                            Util.Logging.Log(
                                Util.Logging.Type.Error,
                                "Unable to continue, store is null or blank",
                                "Program_writeData_001",
                                string.Concat("Stored Proc to use: ", tableSettings.stored_procedure)
                                );
                            return new ReturnData(null, false, 0, "Unable to continue, store is null or blank");
                        }
                        if (!string.IsNullOrEmpty(store.PRM_STORE_NUMBER))
                        {
                            // Parameterize the PRM_STORE_NUMBER to @store
                            SqlParameter sqlParameter = new SqlParameter();
                            sqlParameter.ParameterName = "@store";
                            sqlParameter.Value = store.PRM_STORE_NUMBER;
                            sqlParameters.Add(sqlParameter);
                        }
                        else
                        {
                            // PRM_STORE_NUMBER was not passed
                            Util.Logging.Log(
                                Util.Logging.Type.Error,
                                "Unable to use store number, PRM_STORE_NUMBER is null or blank",
                                "Program_writeData_002",
                                string.Concat("SQL File Used: ", tableSettings.sql_filename)
                                );
                            return new ReturnData(null, false, 0, "PRM_STORE_NUMBER is null or blank");
                        }
                    }
                    // Get the data from the database using the stored procedure
                    return database.getDataFromStoredProc(tableSettings.stored_procedure, sqlParameters);
                } else {
                    Util.Logging.Log(
                        Util.Logging.Type.Error,
                        "Unable to use stored procedure",
                        "Program_writeData_002",
                        "Stored procedure name is null or blank in tables.json"
                        );
                    return new ReturnData(null, false, 0, "Stored procedure name is null or blank in tables.json");
                }
            }
            else
            {
                // Use a SQL file (if it exists)
                string sql = string.Empty;
                if (File.Exists(string.Concat(tableSettings.sql_filename))) {
                    // Read the SQL file
                    sql = File.ReadAllText(tableSettings.sql_filename);
                } else {
                    // SQL File does not exist
                    Util.Logging.Log(
                        Util.Logging.Type.Error,
                        "Unable to use SQL File, File does not exist",
                        "Program_writeData_003",
                        string.Concat("SQL File Used: ", tableSettings.sql_filename)
                        );
                    return new ReturnData(null, false, 0, "Unable to use SQL File, File does not exist");
                }
                if (tableSettings.use_store_number)
                {
                    if (store == null)
                    {
                        // Store must be passed if we are using it to generate the query
                        Util.Logging.Log(
                            Util.Logging.Type.Error,
                            "Unable to continue, store is null or blank",
                            "Program_writeData_004",
                            string.Concat("SQL File to use: ", tableSettings.sql_filename)
                            );
                        return new ReturnData(null, false, 0, "Unable to continue, store is null or blank");
                    }
                    if (!string.IsNullOrEmpty(store.PRM_STORE_NUMBER)) {
                        // Replace all instances of @store with the PRM_STORE_NUMBER in quotes
                        sql = sql.Replace("@store", string.Concat("'", store.PRM_STORE_NUMBER, "'"));
                    } else {
                        // PRM_STORE_NUMBER was not passed
                        Util.Logging.Log(
                            Util.Logging.Type.Error,
                            "Unable to use store number, PRM_STORE_NUMBER is null or blank",
                            "Program_writeData_005",
                            string.Concat("SQL File Used: ", tableSettings.sql_filename)
                            );
                        return new ReturnData(null, false, 0, "PRM_STORE_NUMBER is null or blank");
                    }
                }
                // Get the data from the database using the SQL script
                return database.getDataFromSQLString(sql);
            }
        
        }

        /// writeData(ReturnData, TableSettings, Tables.Store)
        /// <summary>
        /// Takes the data returned from getData and tries to write it to a file
        /// for each store passed to it
        /// </summary>
        /// <param name="PRM_STORE_NUMBER"></param>
        /// <param name="tableSettings"></param>
        static void writeData(ReturnData returnData, TableSettings tableSettings, Tables.Store store)
        {
            if (showDebug)
            {
                Util.Logging.Log(Util.Logging.Type.Debug, string.Concat("Writing ", tableSettings.table, " data for Store: ", store.PRM_STORE_NUMBER), string.Empty, string.Empty);
            }

            // Check if the pull of data from the database was successful
            if (returnData.isSuccess())
            {
                try
                {
                    /*
                     * Note:
                     * If we can't rely on the SQL script to get proper lengths, we could read in the format file
                     * and resize at the datatable, but it would add some additional processing time for the
                     * larger datasets.  If so, add format_file to tables.json
                    */

                    // Create a StringBuilder object
                    StringBuilder sb = new StringBuilder();
                    int rowCount = 0; // Used to return the number of written rows
                    foreach (DataRow row in returnData.getDataTable().Rows)
                    {
                        // Get the fields from the row
                        IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                        // Append the line to the StringBuilder object using the filler character
                        sb.AppendLine(string.Join(fillerChar, fields));
                        rowCount++; // Increment the counter
                    }

                    // Create the full path for the output file
                    // {base folder}\{store number}\{export filename}
                    string output = string.Concat(exportFolder, @"\", store.PRM_STORE_NUMBER, @"\", tableSettings.export_file);

                    // Attempt to write the text to a file
                    File.WriteAllText(output, sb.ToString());

                    if (showDebug)
                    {
                        // Show that the file has been saved (if showing debug messages)
                        Util.Logging.Log(
                            Util.Logging.Type.Debug,
                            string.Concat("Saving ", output),
                            string.Empty,
                            string.Concat("SQL File Used: ", tableSettings.sql_filename, " | Rows written: " + rowCount.ToString())
                            );
                    }
                }
                catch (Exception ex)
                {
                    // Unexpected error
                    Util.Logging.Log(
                        Util.Logging.Type.Error,
                        "Unexpected Error",
                        "Program_writeData_001",
                        ex.Message
                        );
                    return;
                }
            }
            else
            {
                // Database did not return the data as expected
                Util.Logging.Log(
                    Util.Logging.Type.Error,
                    string.Concat("Unexpected Error getting ", "tablenamehere", " data"),
                    "Main_writeData_002",
                    string.Concat("SQL File Used: ", tableSettings.sql_filename, " | ", returnData.getReturnMessage())
                    );
                return;
            }
        }
        #endregion
    }
}

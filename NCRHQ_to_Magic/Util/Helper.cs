using System;

namespace NCRHQ_to_Magic.Util
{
    class Helper
    {
        /// checkDate(string)
        /// <summary>
        /// Checks if a date in a string object is a valid DateTime object
        /// </summary>
        /// <param name="date"></param>
        /// <returns>bool</returns>
        public static bool checkDate(string date)
        {
            // Check if a valid date
            DateTime temp;

            if (DateTime.TryParse(date, out temp) == true)
                return true;
            else
                return false;
        }
        
        /// toBool(string)
        /// <summary>
        /// Converts a string value into a nullable boolean
        /// </summary>
        /// <param name="val"></param>
        /// <returns>bool?</returns>
        public static bool? toBool(string val)
        {
            // Convert to a nullable bool
            bool? ret = null;

            if (!string.IsNullOrEmpty(val))
            {
                if (val == "1" || val.ToLower() == "true" || val.ToLower() == "y")
                {
                    ret = true;
                }
                else if (val == "0" || val.ToLower() == "false" || val.ToLower() == "n")
                {
                    ret = false;
                }
            }

            return ret;
        }
        
        /// isNumeric(object)
        /// <summary>
        /// Checks if an object is numeric
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public static bool isNumeric(object obj)
        {
            double retNum;

            bool isNum = double.TryParse(Convert.ToString(obj), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        /// isGUID(string)
        /// <summary>
        /// Checks if a string is a valid Guid
        /// </summary>
        /// <param name="val"></param>
        /// <returns>bool</returns>
        public static bool isGUID(string val)
        {
            Guid guidOutput = new Guid();
            return Guid.TryParse(val, out guidOutput);
        }

        /// <summary>
        /// Replaces a null or empty string with a blank string
        /// Used when a null string may cause issues with an output
        /// If a value exists, it will not be replaced
        /// </summary>
        /// <param name="val"></param>
        /// <returns>string</returns>
        public static string denull(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return string.Empty;
            }
            else
            {
                return val;
            }
        }
    }
}

using LivelinkConfigurationManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateMigrations
{
    public enum DatabaseType
    {
        Oracle,
        MSSQL,
        MSSQLTrusted
    }

    public static class Utils
    {
        private const string _fmtMSSQLTrustedConnectionString = "Server={0};Database={1};Trusted_Connection=True;";
        private const string _fmtMSSQLConnectionString = "Server={0}; Database={1}; User Id={2}; Password={3}";
        private const string _fmtOracleConnectionString = "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1}))(CONNECT_DATA = (SERVICE_NAME = {2})));User Id = {3}; Password={4};";
        public const string FMT_DateTime = "{0:yyyy}{0:MM}{0:dd}{0:HH}{0:mm}{0:ss}";
        public static string GetDatabaseConnectionString(DatabaseType dbType)
        {
            string result = null;

            if (dbType == DatabaseType.Oracle)
            {
                result = Utils.GetOracleDatabaseConnectionString();
            }
            else if (dbType == DatabaseType.MSSQL)
            {
                result = Utils.GetMSSQLDatabaseConnectionString();
            }
            else if (dbType == DatabaseType.MSSQLTrusted)
            {
                result = Utils.GetMSSQLTrustedDatabaseConnectionString();
            }
            else
            {
                throw new ArgumentException("Database type does not exist.");
            }

            return result;
        }
        public static string GetMSSQLDatabaseConnectionString()
        {
            string servername = Utils.GetMSSQLDatabaseServerName();
            string databasename = Utils.GetMSSQLDatabaseDatabaseName();
            string username = Utils.GetMSSQLDatabaseUsername();
            string password = Utils.GetMSSQLDatabasePassword();

            return String.Format(_fmtMSSQLConnectionString, servername, databasename, username, password);
        }

        public static string GetMSSQLTrustedDatabaseConnectionString()
        {
            string servername = Utils.GetMSSQLDatabaseServerName();
            string databasename = Utils.GetMSSQLDatabaseDatabaseName();
            string username = Utils.GetMSSQLDatabaseUsername();
            string password = Utils.GetMSSQLDatabasePassword();

            return String.Format(_fmtMSSQLTrustedConnectionString, servername, databasename);
        }

        public static string GetMSSQLDatabaseServerName()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-mssql-database-servername", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return result;
        }
        
        public static string GetAreaFolderLoadFileOutputPath()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-area-folder-load-file-output-path", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return result;
        }
        
        public static string GetMSSQLDatabaseDatabaseName()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-mssql-database-databasename", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return result;
        }

        public static string GetMSSQLDatabaseUsername()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-mssql-database-username", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return result;
        }

        public static string GetMSSQLDatabasePassword()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-mssql-database-password", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            result = CryptoUtils.Decrypt(result);

            return result;
        }

        public static string GetOracleDatabaseConnectionString()
        {
            string host = Utils.GetOracleDatabaseHostname();
            int port = Utils.GetOracleDatabasePort();
            string servicename = Utils.GetOracleServicename();
            string username = Utils.GetOracleDatabaseUsername();
            string password = Utils.GetOracleDatabasePassword();

            return String.Format(_fmtOracleConnectionString, host, port, servicename, username, password);
        }

        public static string GetOracleDatabaseHostname()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-oracle-database-hostname", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return result;
        }

        public static int GetOracleDatabasePort()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-oracle-database-port", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return Convert.ToInt32(result);
        }
        
        public static string GetOracleServicename()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-oracle-database-servicename", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return result;
        }

        public static string GetOracleDatabaseUsername()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-oracle-database-username", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return result;
        }

        public static string GetOracleDatabasePassword()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-oracle-database-password", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            result = CryptoUtils.Decrypt(result);

            return result;
        }
        /// <summary>
        /// Configuration accessor method to retrieve the "environment" key value.
        /// </summary>
        /// <returns>String value representing the current environment</returns>
        public static string GetCurrentEnvironment()
        {
            string result = ConfigurationManager.AppSettings["environment"];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", "environment"));
            }

            return result;
        }

        /// <summary>
        /// Configuration accessor method to retrieve the "{ENV}-contentserver-url" key value.
        /// </summary>
        /// <returns>String value representing the content server url for the current environment</returns>
        public static string GetContentServerHostName()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string hostnameKey = String.Format("{0}-contentserver-url", currentEnvironment);
            string result = ConfigurationManager.AppSettings[hostnameKey];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", hostnameKey));
            }

            return result;
        }

        public static string GetContentServerAdminUsername()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string adminUsername = String.Format("{0}-{1}", currentEnvironment, "admin-username");
            string result = ConfigurationManager.AppSettings[adminUsername];

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", adminUsername));
            }

            return result;
        }

        public static string GetContentServerAdminPassword()
        {
            string currentEnvironment = Utils.GetCurrentEnvironment();
            string adminPassword = String.Format("{0}-{1}", currentEnvironment, "admin-password");
            string result = CryptoUtils.Decrypt(ConfigurationManager.AppSettings[adminPassword]);

            if (String.IsNullOrEmpty(result))
            {
                throw new ConfigurationErrorsException(String.Format("{0} key needs to be configured in configuration file.", adminPassword));
            }

            return result;
        }

        public static T GetValueOrDefault<T>(this IDataRecord row, string fieldName)
        {
            int ordinal = row.GetOrdinal(fieldName);
            return row.GetValueOrDefault<T>(ordinal);
        }

        public static T GetValueOrDefault<T>(this IDataRecord row, int ordinal)
        {
            return (T)(row.IsDBNull(ordinal) ? default(T) : row.GetValue(ordinal));
        }
        
        public static void WriteListToFile(List<string> sourceList, string outputPath)
        {
            StringBuilder sb = new StringBuilder();

            if ((sourceList != null) && (sourceList.Count > 0))
            {
                sourceList.ForEach(delegate (string entry)
                {
                    sb.AppendLine(String.Format("\"{0}\"", entry));
                });

                File.WriteAllText(outputPath, sb.ToString());
            }
        }

        public static void WriteFileSystemObjectListToFile(List<ListingObject> sourceList, string outputPath)
        {
            StringBuilder sb = new StringBuilder();

            if ((sourceList != null) && (sourceList.Count > 0))
            {
                sb.AppendLine("\"Path\",Type");

                sourceList.ForEach(delegate (ListingObject listingObject)
                {
                    sb.AppendLine(String.Format("\"{0}\",{1}", listingObject.Path, listingObject.ListingType.ToString("g")));
                });

                File.WriteAllText(outputPath, sb.ToString());
            }
        }

        private static string QuoteValue(string value)
        {
            return String.Concat("\"", value.Replace("\"", "\"\""), "\"");
        }

        public static void WriteDataTableToCSV(DataTable sourceTable, TextWriter writer, bool includeHeaders)
        {
            string temp = null;
            List<string> rowList = new List<string>();

            if (includeHeaders)
            {
                List<string> headerValues = new List<string>();
                foreach (DataColumn column in sourceTable.Columns)
                {
                    headerValues.Add(Utils.QuoteValue(column.ColumnName));
                }

                writer.WriteLine(String.Join(",", headerValues.ToArray()));
            }

            string[] items = null;

            foreach (DataRow row in sourceTable.Rows)
            {
                rowList.Clear();

                for (int i = 0; i < sourceTable.Columns.Count; i++)
                {
                    //foreach column
                    temp = String.Empty;

                    if (sourceTable.Columns[i].DataType == typeof(DateTime))
                    {
                        if (!row.IsNull(sourceTable.Columns[i]))
                        {
                            temp = String.Format("{0:MM}/{0:dd}/{0:yyyy}", row[i]);
                        }
                    }
                    else
                    {
                        temp = row[i].ToString();
                    }

                    //rowList.Add(Utils.CleanString(temp));
                    rowList.Add(temp);
                }

                items = rowList.Select(o => Utils.QuoteValue(o.ToString())).ToArray();

                writer.WriteLine(String.Join(",", items));
            }

            writer.Flush();
        }
    }
}

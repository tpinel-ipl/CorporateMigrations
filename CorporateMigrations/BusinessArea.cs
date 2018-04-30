using NLog;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CorporateMigrations
{
    public enum OutputType
    {
        OI_XML,
        DM_CSV,
        CWS
    }

    public class BusinessArea
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private int _processedItems;
        private string _outputMasterListingPathTemplate = "{0}\\{1}\\easymorph\\{1}_master_listing.xlsx";
        private string _outputAreaLoadFilePath = "{0}\\{1}\\controlfiles\\";
        private string _areaName = null;
        private List<string> _exclusionPaths = null;
        private List<string> _sourcePaths = null;
        private List<ListingObject> _sourceListing = null;
        private List<ObjectImporterNode> _folders;
        private List<ObjectImporterNode> _documents;

        public OutputType MigrationOutput { get; set; }

        public string OutputRootPath { get; set; }
        public string AreaName
        {
            get
            {
                return this._areaName;
            }
        }

        public List<string> SourcePaths
        {
            get
            {
                return this._sourcePaths;
            }
        }

        public List<string> ExclusionPaths
        {
            get
            {
                return this._exclusionPaths;
            }
        }

        public BusinessArea(string areaName) : this()
        {
            this._areaName = areaName;
        }

        private BusinessArea()
        {
            this._sourcePaths = new List<string>();
            this._exclusionPaths = new List<string>();
            this._sourceListing = new List<ListingObject>();
            this._folders = new List<ObjectImporterNode>();
        }

        public void AddSourcePath(string sourcePath)
        {
            this._sourcePaths.Add(sourcePath);
        }

        public void AddExclusionPath(string exclusionPath)
        {
            this._exclusionPaths.Add(exclusionPath);
        }

        public void ProcessPaths()
        {
            if (this._sourcePaths.Count > 0)
            {
                this._sourceListing.Clear();

                //we have some paths to process
                for (int i = 0; i < this._sourcePaths.Count; i++)
                {
                    this._navigatePathListing(this._sourcePaths[i]);
                };

                this._insertSourceListingsIntoTempTable();
            }
        }

        public void ReplaceMasterListing()
        {
            //for this one we want to delete the master listing, and then copy over the temp listing
            string qryClear = "DELETE FROM SourcePathListing WHERE Area = @AreaName";
            string qryCopy = "INSERT INTO SourcePathListing (KeyPath, ListingType, Area, CreateDate) SELECT KeyPath, ListingType, Area, CreateDate FROM SourcePathListingTemp WHERE Area = @AreaName";
            SqlCommand cmdSql = null;

            using (SqlConnection connSQL = new SqlConnection(Utils.GetDatabaseConnectionString(DatabaseType.MSSQLTrusted)))
            {
                connSQL.Open();

                //clear master table of any entries for this area
                cmdSql = new SqlCommand(qryClear, connSQL);
                cmdSql.Parameters.Add(new SqlParameter("AreaName", this._areaName));
                cmdSql.ExecuteNonQuery();

                //now we need to copy the areas temp entries over
                cmdSql = new SqlCommand(qryCopy, connSQL);
                cmdSql.Parameters.Add(new SqlParameter("AreaName", this._areaName));
                cmdSql.ExecuteNonQuery();
            }
        }

        public void ExportMasterListing()
        {
            string outputPath = String.Format(this._outputMasterListingPathTemplate, this.OutputRootPath, this.AreaName);
            string qryStr = "SELECT * FROM SourcePathListing SPL WHERE SPL.Area = @AreaName";
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            ExcelPackage excel = new ExcelPackage();

            using (SqlConnection connSQL = new SqlConnection(Utils.GetDatabaseConnectionString(DatabaseType.MSSQLTrusted)))
            {
                connSQL.Open();
                cmd = new SqlCommand(qryStr, connSQL);
                cmd.Parameters.Add("@AreaName", SqlDbType.VarChar).Value = this.AreaName;

                dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                    workSheet.Cells[1, 1].LoadFromDataReader(dr, true);
                    excel.SaveAs(new FileInfo(outputPath));
                }
            }
        }

        public void CreateFolderOIXML()
        {
            string qryStr = String.Format("SELECT * FROM vw_{0}_folders ORDER BY [TargetPath] ASC, [ObjectName] ASC", this.AreaName);
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            using (SqlConnection connSQL = new SqlConnection(Utils.GetDatabaseConnectionString(DatabaseType.MSSQLTrusted)))
            {
                connSQL.Open();
                cmd = new SqlCommand(qryStr, connSQL);

                dr = cmd.ExecuteReader();

                this._processedItems = 0;
                this._folders = new List<ObjectImporterNode>();

                while (dr.Read())
                {
                    ObjectImporterNode oin = new ObjectImporterNode();
                    oin.Action = "create";
                    oin.Type = "folder";

                    oin.Title = dr.GetValueOrDefault<string>("ObjectName");
                    oin.Location = dr.GetValueOrDefault<string>("TargetPath");
                    oin.Created = dr.GetValueOrDefault<DateTime>("CreateDate");

                    if(oin.Created == DateTime.MinValue)
                    {
                        //if the value for created is null, the datetime will come across as the min datetime value
                        //so in this scenario set the create date to today date/time
                        oin.Created = DateTime.Now;
                    }

                    this._addCustomCategory(dr, oin);

                    this._folders.Add(oin);
                    this._processedItems++;

                    if (this._processedItems % 100 == 0)
                    {
                        _logger.Info(String.Format("Processed {0} folder objects.", this._processedItems));
                    }

                    if (this._processedItems % 1000 == 0)
                    {
                        //let's output the xml file and clear our folder list
                        this._outputFolderNodeXML(this._processedItems, this._folders);

                        //after outputtting the xml for these folders let's clear our list and continue
                        this._folders.Clear();
                    }
                }

                //there may be some items left in the list to output
                if (this._folders.Count > 0)
                {
                    //let's output the xml file and clear our folder list
                    this._outputFolderNodeXML(999999, this._folders);
                }
            }
        }

        private void _addCustomCategory(SqlDataReader dr, ObjectImporterNode oin)
        {
            switch (this._areaName)
            {
                case "csox":
                    this._addCsoxCategoryInfo(dr, oin);
                    break;
                case "internalaudit":
                    this._addInternalAuditCategoryInfo(dr, oin);
                    break;
                case "insurance":
                    this._addInsuranceCategoryInfo(dr, oin);
                    break;
                case "erm":
                    this._addErmCategoryInfo(dr, oin);
                    break;
                case "tax":
                    this._addTaxCategoryInfo(dr, oin);
                    break;
                default:
                    break;
            }
        }

        private void _addCsoxCategoryInfo(SqlDataReader dr, ObjectImporterNode oin)
        {
            ObjectImporterCategory customCategory = new ObjectImporterCategory { CategoryName = "Tax And Finance:CORP-TAF-CSOX IPF" };

            customCategory.AddCategoryAttribute("Working Year", Convert.ToString(dr["WorkingYear"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Country", Convert.ToString(dr["Country"]), false, AttributeClearMode.DoNotClear, true);

            oin.AddNodeCategory(customCategory);
        }

        private void _addInternalAuditCategoryInfo(SqlDataReader dr, ObjectImporterNode oin)
        {
            ObjectImporterCategory customCategory = new ObjectImporterCategory { CategoryName = "Tax And Finance:CORP-TAF-Internal Audit" };

            customCategory.AddCategoryAttribute("Working Year", Convert.ToString(dr["WorkingYear"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Country", Convert.ToString(dr["Country"]), false, AttributeClearMode.DoNotClear, true);

            oin.AddNodeCategory(customCategory);
        }

        private void _addInsuranceCategoryInfo(SqlDataReader dr, ObjectImporterNode oin)
        {
            ObjectImporterCategory customCategory = new ObjectImporterCategory { CategoryName = "Tax And Finance:CORP-TAF-General" };

            customCategory.AddCategoryAttribute("Start Year", Convert.ToString(dr["StartYear"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("End Year", Convert.ToString(dr["EndYear"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Legal Corporate Entity", Convert.ToString(dr["CorporateEntity"]), false, AttributeClearMode.DoNotClear, true);

            oin.AddNodeCategory(customCategory);
        }

        private void _addErmCategoryInfo(SqlDataReader dr, ObjectImporterNode oin)
        {
            ObjectImporterCategory customCategory = new ObjectImporterCategory { CategoryName = "Tax And Finance:CORP-TAF-General" };

            customCategory.AddCategoryAttribute("End Year", Convert.ToString(dr["EndYear"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Legal Corporate Entity", Convert.ToString(dr["CorporateEntity"]), false, AttributeClearMode.DoNotClear, true);

            oin.AddNodeCategory(customCategory);
        }
        private void _addTaxCategoryInfo(SqlDataReader dr, ObjectImporterNode oin)
        {
            ObjectImporterCategory customCategory = new ObjectImporterCategory { CategoryName = "Tax And Finance:CORP-TAF-Tax" };

            customCategory.AddCategoryAttribute("Start Year", Convert.ToString(dr["StartYear"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("End Year", Convert.ToString(dr["EndYear"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Legal Corporate Entity", Convert.ToString(dr["CorporateEntity"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Document Author", Convert.ToString(dr["Author"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Tax Form Type", Convert.ToString(dr["TaxFormType"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Property Tax Type", Convert.ToString(dr["PropertyTaxFormType"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Property Tax Year", Convert.ToString(dr["PropertyTaxYear"]), false, AttributeClearMode.DoNotClear, true);
            customCategory.AddCategoryAttribute("Municipality", Convert.ToString(dr["District"]).Trim(), false, AttributeClearMode.DoNotClear, true);

            oin.AddNodeCategory(customCategory);
        }

        public void CreateFileOIXML()
        {
            string qryStr = String.Format("SELECT * FROM vw_{0}_files ORDER BY TargetPath ASC, ObjectName ASC", this.AreaName);
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            using (SqlConnection connSQL = new SqlConnection(Utils.GetDatabaseConnectionString(DatabaseType.MSSQLTrusted)))
            {
                connSQL.Open();
                cmd = new SqlCommand(qryStr, connSQL);

                dr = cmd.ExecuteReader();

                this._processedItems = 0;
                this._documents = new List<ObjectImporterNode>();

                while (dr.Read())
                {
                    ObjectImporterNode oin = new ObjectImporterNode();
                    oin.Action = "create";
                    oin.Type = "document";

                    oin.Title = dr.GetValueOrDefault<string>("ObjectName");
                    oin.Location = dr.GetValueOrDefault<string>("TargetPath");
                    oin.Created = dr.GetValueOrDefault<DateTime>("CreateDate");

                    oin.ProviderFilePath = dr.GetValueOrDefault<string>("ProviderPath");

                    this._documents.Add(oin);
                    this._processedItems++;

                    if (this._processedItems % 100 == 0)
                    {
                        _logger.Info(String.Format("Processed {0} document objects.", this._processedItems));
                    }

                    //using batches of 1000
                    if (this._processedItems % 1000 == 0)
                    {
                        //let's output the xml file and clear our folder list
                        this._outputDocumentNodeXML(this._processedItems, this._documents);

                        //after outputtting the xml for these folders let's clear our list and continue
                        this._documents.Clear();
                    }
                }

                //there may be some items left in the list to output
                if (this._documents.Count > 0)
                {
                    //let's output the xml file and clear our folder list
                    this._outputDocumentNodeXML(999999, this._documents);
                }
            }
        }

        private void _navigatePathListing(string sourcePath)
        {
            //go through this path
            ListingObject tempObject = null;

            try
            {
                //get files in this folder
                foreach (string tempFile in Directory.GetFiles(sourcePath))
                {
                    if (!this._exclusionPaths.Contains(tempFile, StringComparer.OrdinalIgnoreCase))
                    {
                        var fso = new FileInfo(tempFile);

                        tempObject = new ListingObject {
                            Area = this.AreaName,
                            Path = tempFile,
                            ListingType = ListingObjectType.File,
                            CreateDate = fso.CreationTime
                        };

                        this._sourceListing.Add(tempObject);
                    }
                    else
                    {
                        _logger.Debug(String.Format("Exluded item: {0}", tempFile));
                    }
                }

                //go through each sub-folder of the current folder
                foreach (string tempDir in Directory.GetDirectories(sourcePath))
                {
                    if (!this._exclusionPaths.Contains(tempDir, StringComparer.OrdinalIgnoreCase))
                    {
                        var dso = new DirectoryInfo(tempDir);

                        tempObject = new ListingObject {
                            Area = this.AreaName,
                            Path = tempDir,
                            ListingType = ListingObjectType.Folder,
                            CreateDate = dso.CreationTime
                        };

                        this._sourceListing.Add(tempObject);

                        this._navigatePathListing(tempDir);
                    }
                    else
                    {
                        _logger.Debug(String.Format("Exluded item: {0}", tempDir));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.Message);
            }
        }

        private void _insertSourceListingsIntoTempTable()
        {
            string qryClear = "DELETE FROM SourcePathListingTemp WHERE Area = @AreaName";
            SqlCommand cmdSql = null;

            //our source listing list should be full of lo objects that we can place in the database
            if (this._sourceListing.Count > 0)
            {
                DataTable listingTable = this._makeDataTableFromSourceListings();

                using (SqlConnection connSQL = new SqlConnection(Utils.GetDatabaseConnectionString(DatabaseType.MSSQLTrusted)))
                {
                    connSQL.Open();

                    //clear temp table of any entries for this area
                    cmdSql = new SqlCommand(qryClear, connSQL);
                    cmdSql.Parameters.Add(new SqlParameter("AreaName", this._areaName));
                    cmdSql.ExecuteNonQuery();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connSQL))
                    {
                        bulkCopy.DestinationTableName = "SourcePathListingTemp";

                        try
                        {
                            // Write from the source to the destination.
                            bulkCopy.WriteToServer(listingTable);
                        }
                        catch (Exception ex)
                        {
                            _logger.Debug(ex.Message);
                        }
                    }
                }
            }
        }

        private DataTable _makeDataTableFromSourceListings()
        {
            DataTable listingTable = new DataTable();

            DataColumn id = new DataColumn();
            id.DataType = System.Type.GetType("System.Int64");
            id.ColumnName = "Id";
            id.AutoIncrement = true;

            DataColumn keyPath = new DataColumn();
            keyPath.DataType = System.Type.GetType("System.String");
            keyPath.ColumnName = "KeyPath";

            DataColumn listingType = new DataColumn();
            listingType.DataType = System.Type.GetType("System.String");
            listingType.ColumnName = "ListingType";

            DataColumn area = new DataColumn();
            area.DataType = System.Type.GetType("System.String");
            area.ColumnName = "Area";

            DataColumn createDate = new DataColumn();
            createDate.DataType = System.Type.GetType("System.DateTime");
            createDate.ColumnName = "CreateDate";

            listingTable.Columns.Add(id);
            listingTable.Columns.Add(keyPath);
            listingTable.Columns.Add(listingType);
            listingTable.Columns.Add(area);
            listingTable.Columns.Add(createDate);

            // Add some new rows to the table
            this._sourceListing.ForEach(delegate (ListingObject lo)
            {
                DataRow row = listingTable.NewRow();
                row["KeyPath"] = lo.Path;
                row["ListingType"] = lo.ListingType;
                row["Area"] = lo.Area;
                row["CreateDate"] = lo.CreateDate;

                listingTable.Rows.Add(row);
            });

            listingTable.AcceptChanges();

            return listingTable;
        }

        private void _outputFolderNodeXML(int count, List<ObjectImporterNode> nodes)
        {
            this._outputNodeXML(count, "folder", nodes, String.Format(this._outputAreaLoadFilePath, this.OutputRootPath, this.AreaName));
        }

        private void _outputDocumentNodeXML(int count, List<ObjectImporterNode> nodes)
        {
            if (this.MigrationOutput == OutputType.OI_XML)
            {
                //old output
                this._outputNodeXML(count, "document", nodes, String.Format(this._outputAreaLoadFilePath, this.OutputRootPath, this.AreaName));
            }
        }

        private void _outputNodeXML(int count, string nodeType, List<ObjectImporterNode> nodes, string outputPath)
        {
            XElement nodeImport = new XElement("import");
            bool nodesAdded = false;

            nodes.ForEach(delegate (ObjectImporterNode oin)
            {
                //create node
                nodeImport.Add(oin.GetNodeXml());
                nodesAdded = true;
            });

            //output the file if there were nodes added.
            if (nodesAdded)
            {
                nodeImport.Save(Path.Combine(outputPath, String.Format("{0}-{1}-{2:000000}.xml", this._areaName, nodeType, count)));
            }
        }
    }
}

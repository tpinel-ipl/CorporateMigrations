using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateMigrations
{
    public enum BusinessAreaName
    {
        taf_csox,
        taf_erm,
        taf_insurance,
        taf_internalaudit,
        taf_tax,
        dm_all,
        land_all
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<BusinessAreaName> definedBusinessAreaNames = new List<BusinessAreaName> {
                //BusinessAreaName.taf_erm,
                BusinessAreaName.taf_tax
                //BusinessAreaName.taf_csox,
                //BusinessAreaName.taf_insurance,
                //BusinessAreaName.taf_internalaudit
            };

            //task
            //1 - process file system paths
            //2 - replace master listing
            //3 - export master file listing
            //4 - create folder load sheet
            //5 - create file load sheet

            List<int> definedMigrationTasks = new List<int> { 4, 5 };

            List<MigrationTask> migrationTasks = new List<MigrationTask>();

            definedBusinessAreaNames.ForEach(delegate (BusinessAreaName ban)
            {
                definedMigrationTasks.ForEach(delegate (int migTask)
                {
                    migrationTasks.Add(new MigrationTask { AreaName = ban, Task = migTask });
                });
            });

            migrationTasks.ForEach(delegate (MigrationTask mt)
            {
                Program.PerformTask(mt.AreaName, mt.Task);
            });
        }

        public static void PerformTask(BusinessAreaName areaName, int task)
        {
            BusinessArea businessArea = null;

            switch (areaName)
            {
                case BusinessAreaName.taf_csox:
                    businessArea = new BusinessArea("csox");
                    businessArea.OutputRootPath = @"\\fs01\Shared\FSDATA\Shared\Content_server\TAF\";
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\CSOX IPF");
                    break;
                case BusinessAreaName.taf_erm:
                    businessArea = new BusinessArea("erm");
                    businessArea.OutputRootPath = @"\\fs01\Shared\FSDATA\Shared\Content_server\TAF\";
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\Insurance\erm");
                    break;
                case BusinessAreaName.taf_insurance:
                    businessArea = new BusinessArea("insurance");
                    businessArea.OutputRootPath = @"\\fs01\Shared\FSDATA\Shared\Content_server\TAF\";
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\Insurance");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\fsdata$\departmental2$\land\insurance");

                    businessArea.AddExclusionPath(@"\\?\UNC\ipf.lan\Public$\Insurance\erm");
                    break;
                case BusinessAreaName.taf_internalaudit:
                    businessArea = new BusinessArea("internalaudit");
                    businessArea.OutputRootPath = @"\\fs01\Shared\FSDATA\Shared\Content_server\TAF\";
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\Internal Audit");
                    break;
                case BusinessAreaName.taf_tax:
                    businessArea = new BusinessArea("tax");
                    businessArea.OutputRootPath = @"\\fs01\Shared\FSDATA\Shared\Content_server\TAF\";
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\Tax");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\fsdata$\departmental2$\Land\Property Tax");                    
                    break;
                case BusinessAreaName.dm_all:
                    businessArea = new BusinessArea("drawing");
                    businessArea.OutputRootPath = @"\\fs01\Shared\FSDATA\Shared\Content_server\DM\";
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\Shared Drafting");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\GIS_Drafting\Drafting Files\Superseded Files\Bow River Pipeline");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\GIS_Drafting\Drafting Files\Superseded Files\Central Alberta Pipeline");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\GIS_Drafting\Drafting Files\Superseded Files\Cold Lake Pipeline");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\GIS_Drafting\Drafting Files\Superseded Files\Corridor Pipeline");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\GIS_Drafting\Drafting Files\Superseded Files\Mid Saskatchewan Pipeline");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\GIS_Drafting\Drafting Files\Superseded Files\Polaris Pipeline");
                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\Public$\GIS_Drafting\Drafting Files\Drafting Standards and Templates\Standard EPC Drafting Package\Standard EPC Drafting Package_2018\_IPL Template");
                    break;
                case BusinessAreaName.land_all:
                    businessArea = new BusinessArea("land");
                    businessArea.OutputRootPath = @"\\fs01\Shared\FSDATA\Shared\Content_server\LAND\";

                    businessArea.AddSourcePath(@"\\?\UNC\ipf.lan\fsdata$\departmental2$\land");

                    businessArea.AddExclusionPath(@"\\?\UNC\ipf.lan\fsdata$\departmental2$\land\insurance");
                    businessArea.AddExclusionPath(@"\\?\UNC\ipf.lan\fsdata$\departmental2$\land\property tax");
                    break;
                default:
                    break;
            }

            if (businessArea != null)
            {
                switch (task)
                {
                    case 1:
                        businessArea.ProcessPaths();
                        break;
                    case 2:
                        businessArea.ReplaceMasterListing();
                        break;
                    case 3:
                        businessArea.ExportMasterListing();
                        break;
                    case 4:
                        businessArea.CreateFolderOIXML();
                        break;
                    case 5:
                        businessArea.CreateFileOIXML();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CorporateMigrations
{
    public class ObjectImporterNode
    {
        private List<ObjectImporterPermissions> _nodePermissions;
        private List<ObjectImporterCategory> _nodeCategories;
        public string Action { get; set; }
        public string AliasPath { get; set; }
        public string CADAppVersion { get; set; }
        public string CADCRC { get; set; }
        public string CADDocType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        public string MimeType { get; set; }
        public string Owner { get; set; }
        public string OwnerGroup { get; set; }
        public string Description { get; set; }
        public string ProviderFilePath { get; set; }
        public string Location { get; set; }
        public DateTime? Modified { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string UrlPath { get; set; }
        public long? VersionNumber { get; set; }
        public string VersionFileName { get; set; }

        public List<ObjectImporterPermissions> NodePermissions
        {
            get
            {
                return this._nodePermissions;
            }
        }

        public List<ObjectImporterCategory> NodeCategories
        {
            get
            {
                return this._nodeCategories;
            }
        }

        public void AddNodeCategory(ObjectImporterCategory oic)
        {
            if (this._nodeCategories != null)
            {
                this._nodeCategories.Add(oic);
            }
        }

        public void AddNodeCategory(List<ObjectImporterCategory> oics)
        {
            if (this._nodeCategories != null)
            {
                this._nodeCategories.AddRange(oics);
            }
        }

        public void AddNodePermission(ObjectImporterPermissions oip)
        {
            if (this._nodePermissions != null)
            {
                this._nodePermissions.Add(oip);
            }
        }

        public void AddNodePermission(List<ObjectImporterPermissions> oips)
        {
            if (this._nodePermissions != null)
            {
                this._nodePermissions.AddRange(oips);
            }
        }

        public ObjectImporterNode()
        {
            this._nodePermissions = new List<ObjectImporterPermissions>();
            this._nodeCategories = new List<ObjectImporterCategory>();
        }

        public void ResetObject()
        {
            this.Action = null;
            this.AliasPath = null;
            this.Created = null;
            this.CreatedBy = null;
            this.Owner = null;
            this.Description = null;
            this.Owner = null;
            this.OwnerGroup = null;
            this.ProviderFilePath = null;
            this.Location = null;
            this.Modified = null;
            this.MimeType = null;
            this.Title = null;
            this.Type = null;
            this.CADAppVersion = null;
            this.CADCRC = null;
            this.CADDocType = null;
            this._nodePermissions = new List<ObjectImporterPermissions>();
            this._nodeCategories = new List<ObjectImporterCategory>();
        }

        public XElement GetNodeXml()
        {
            if (String.IsNullOrEmpty(this.Action))
            {
                throw new ArgumentException("Object Importer Node requires an Action");
            }

            if (String.IsNullOrEmpty(this.Type))
            {
                throw new ArgumentException("Object Importer Node requires a Type");
            }

            if (String.IsNullOrEmpty(this.Location))
            {
                throw new ArgumentException("Object Importer Node requires an Location");
            }

            XElement node = new XElement("node",
                new XAttribute("action", this.Action),
                new XAttribute("type", this.Type));

            if ((this.Action.ToLower() == "addversion") || (this.Action.ToLower() == "update"))
            {
                //if it is an addversion or an update action then the location is a combination of the location and title
                node.Add(new XElement("location", new XCData(String.Format("{0}:{1}", this.Location, this.Title))));
            }
            else
            {
                //if it is not an addversion action then the location of the object is indicated by the location and title
                node.Add(new XElement("location", new XCData(this.Location)));
                node.Add(new XElement("title", new XCData(this.Title)));
            }

            if (!String.IsNullOrEmpty(this.AliasPath))
            {
                node.Add(new XElement("alias", new XCData(this.AliasPath)));
            }

            if (!String.IsNullOrEmpty(this.UrlPath))
            {
                node.Add(new XElement("url", new XCData(this.UrlPath)));
            }

            if (!String.IsNullOrEmpty(this.CreatedBy))
            {
                node.Add(new XElement("createdby", new XCData(this.CreatedBy)));
            }

            if (!String.IsNullOrEmpty(this.Owner))
            {
                node.Add(new XElement("owner", new XCData(this.Owner)));
            }

            if (!String.IsNullOrEmpty(this.OwnerGroup))
            {
                node.Add(new XElement("ownergroup", new XCData(this.OwnerGroup)));
            }

            if (this.Created.HasValue)
            {
                node.Add(new XElement("created", new XCData(String.Format(Utils.FMT_DateTime, this.Created))));
            }

            if (this.Modified.HasValue)
            {
                node.Add(new XElement("modified", new XCData(String.Format(Utils.FMT_DateTime, this.Modified))));
            }

            if (!String.IsNullOrEmpty(this.ProviderFilePath))
            {
                node.Add(new XElement("file", new XCData(this.ProviderFilePath)));
            }

            if (!String.IsNullOrEmpty(this.MimeType))
            {
                node.Add(new XElement("mime", new XCData(this.MimeType)));
            }

            if (!String.IsNullOrEmpty(this.Description))
            {
                node.Add(new XElement("description", new XCData(this.Description)));
            }

            if (this.Type.ToLower() == "caddocument")
            {
                //let's go through all the caddocument specific settings here
                if (!String.IsNullOrEmpty(this.CADAppVersion))
                {
                    node.Add(new XElement("cadappversion", new XCData(this.CADAppVersion)));
                }

                if (!String.IsNullOrEmpty(this.CADCRC))
                {
                    node.Add(new XElement("cadcrc", new XCData(this.CADCRC)));
                }

                if (!String.IsNullOrEmpty(this.CADDocType))
                {
                    node.Add(new XElement("caddoctype", new XCData(this.CADDocType)));
                }
            }

            if (this.NodePermissions.Count > 0)
            {
                this.NodePermissions.ToList().ForEach(delegate (ObjectImporterPermissions oip)
                {
                    node.Add(oip.GetNodeXml());
                });
            }

            if (this.NodeCategories.Count > 0)
            {
                this.NodeCategories.ToList().ForEach(delegate (ObjectImporterCategory oic)
                {
                    bool addCategory = false;

                    //if it's not set to attach the category
                    if (!oic.AlwaysAttachCategory)
                    {
                        //let's check for any actual values
                        if (oic.CategoryAttributes.Exists(ca => (ca.ForceClear == false) || !String.IsNullOrEmpty(ca.Value)))
                        {
                            addCategory = true;
                        }
                    }
                    else
                    {
                        //we are being told to attach the category regardless
                        addCategory = true;
                    }

                    if (addCategory)
                    {
                        node.Add(oic.GetNodeXml());
                    }
                });
            }

            return node;
        }
    }
}

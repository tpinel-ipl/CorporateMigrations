using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CorporateMigrations
{
    public class ObjectImporterCategory
    {
        List<ObjectImporterAttribute> _attributes;
        public string CategoryName { get; set; }
        public bool NonInherit { get; set; }
        public bool AlwaysAttachCategory { get; set; }

        public List<ObjectImporterAttribute> CategoryAttributes
        {
            get
            {
                return this._attributes;
            }
        }

        public ObjectImporterCategory()
        {
            this._attributes = new List<ObjectImporterAttribute>();
        }

        public void AddCategoryAttribute(string name, string value, bool forceClearAttribute, AttributeClearMode clearMode, bool alwaysAttachCategory)
        {
            if (this._attributes != null)
            {
                this._attributes.Add(new ObjectImporterAttribute { Name = name, Value = value, ClearMode = clearMode, ForceClear = forceClearAttribute });
            }

            if (alwaysAttachCategory)
            {
                this.AlwaysAttachCategory = alwaysAttachCategory;
            }
        }        

        public XElement GetNodeXml()
        {
            XElement result = new XElement("category",
                new XAttribute("name", String.Format("Content Server Categories:{0}", this.CategoryName)));

            if (this.NonInherit)
            {
                result.Add(new XElement("noninherit", "TRUE"));
            }

            this.CategoryAttributes.ForEach(delegate (ObjectImporterAttribute oia)
            {
                result.Add(oia.GetNodeXML());
            });

            return result;
        }
    }
}

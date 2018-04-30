using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CorporateMigrations
{
    public enum AttributeClearMode
    {
        ClearIfNullOrEmpty,
        DoNotClear
    }

    public class ObjectImporterAttribute
    {
        public bool ForceClear { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public AttributeClearMode ClearMode { get; set; }

        public XElement GetNodeXML()
        {
            XElement result = new XElement("attribute",
                new XAttribute("name", this.Name));

            if (this.ForceClear)
            {
                //we want to clear this attribute value
                result.Add(new XAttribute("clear", "true"));
            }
            else if ((this.ClearMode == AttributeClearMode.ClearIfNullOrEmpty) && String.IsNullOrEmpty(this.Value))
            {
                //if the value is null or empty then let's clear it for the caller
                result.Add(new XAttribute("clear", "true"));
            }
            else
            {
                result.Add(new XCData(this.Value));
            }

            return result;
        }

        //public XElement GetNodeXML()
        //{
        //    XElement result = new XElement("attribute",
        //        new XAttribute("name", this.Name));

        //    if (this.Clear || String.IsNullOrEmpty(this.Value))
        //    {
        //        //we want to clear this attribute value
        //        result.Add(new XAttribute("clear", "true"));
        //    }
        //    else
        //    {
        //        result.Add(new XCData(this.Value));
        //    }

        //    return result;
        //}
    }
}

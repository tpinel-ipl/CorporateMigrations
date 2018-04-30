using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateMigrations
{
    public enum ListingObjectType
    {
        File,
        Folder
    }

    public class ListingObject
    {
        public string Path { get; set; }
        public string Area { get; set; }
        public ListingObjectType ListingType { get; set; }
        public DateTime CreateDate { get; set; }
    }
}

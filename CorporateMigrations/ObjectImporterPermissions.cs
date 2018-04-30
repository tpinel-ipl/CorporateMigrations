using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CorporateMigrations
{
    public enum PermissionsRightType
    {
        User,
        Group
    }

    public class ObjectImporterPermissions
    {
        public long ACLType { get; set; }
        public string RightName { get; set; }
        public long Permissions { get; set; }
        public PermissionsRightType RightType { get; set; }
        public string Perms { get; set; }
        //public string PermissionsSet
        //{
        //    get
        //    {
        //        return this._translatePermissions();
        //    }
        //}

        public string TranslatePermissions(int permissions)
        {
            //Position 1 - See
            //Position 2 - See Contents
            //Position 3 - Modify
            //Position 4 - Edit Node Permissions
            //Position 5 - Edit Node Attributes
            //Position 6 - Add Items
            //Position 7 - Delete Node Versions
            //Position 8 - Delete Node
            //Position 9 - Reserve Node
            string pos1 = "0";
            string pos2 = "0";
            string pos3 = "0";
            string pos4 = "0";
            string pos5 = "0";
            string pos6 = "0";
            string pos7 = "0";
            string pos8 = "0";
            string pos9 = "0";

            //Position 1 - See
            if (bitSet(permissions, 1))
            {
                pos1 = "1";
            }

            //Position 2 - See Contents
            if ((bitSet(permissions, 0)) &&
                (bitSet(permissions, 12)) &&
                (bitSet(permissions, 15)))
            {
                pos2 = "1";
            }

            //Position 3 - Modify
            if (bitSet(permissions, 16))
            {
                pos3 = "1";
            }

            //Position 4 - Edit Node Permissions
            if (bitSet(permissions, 4))
            {
                pos4 = "1";
            }

            //Position 5 - Edit Node Attributes
            if (bitSet(permissions, 17))
            {
                pos5 = "1";
            }

            //Position 6 - Add Items
            if (bitSet(permissions, 2))
            {
                pos6 = "1";
            }

            //Position 7 - Delete Node Versions
            if (bitSet(permissions, 14))
            {
                pos7 = "1";
            }

            //Position 8 - Delete Node
            if (bitSet(permissions, 3))
            {
                pos8 = "1";
            }

            //Position 9 - Reserve Node
            if (bitSet(permissions, 13))
            {
                pos9 = "1";
            }

            return String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", pos1, pos2, pos3, pos4, pos5, pos6, pos7, pos8, pos9);
        }

        private bool bitSet(int number, int bitNumber)
        {
            int bitValue = binaryPower(bitNumber);

            return ((number & bitValue) == bitValue);
        }

        private int binaryPower(int exponent)
        {
            int result = 1;

            for (int i = 0; i < exponent; i++)
            {
                result *= 2;
            }

            return result;
        }
        public XElement GetNodeXml()
        {
            if (String.IsNullOrEmpty(this.RightName))
            {
                throw new ArgumentException("Object Importer ACL Node requires a User/Group Name");
            }

            XElement node = new XElement("acl");

            //what type of ACL is this
            node.Add(new XAttribute(((this.RightType == PermissionsRightType.Group) ? "group" : "user"), this.RightName));
            node.Add(new XAttribute("permissions", this.Perms));

            return node;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;

namespace ClassySC.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : System.Attribute
    {
        private ID _id;

        public FieldAttribute(string fieldID)
        {
            _id = new ID(fieldID);
        }

        public ID ID
        {
            get
            {
                return _id;
            }
        }
    }
}

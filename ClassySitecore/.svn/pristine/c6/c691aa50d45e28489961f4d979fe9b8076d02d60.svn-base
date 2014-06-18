using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using System.Xml.Linq;
using Sitecore;

namespace ClassySC.Builder.Configuration
{
    public static class XElementExtensions
    {

        public static ID GetID(this XElement el, string attrName)
        {
            XAttribute attr = el.Attribute(attrName);
            if (attr != null && ID.IsID(attr.Value))
                return new ID(attr.Value);
            return ID.Null;
        }

        public static bool GetBool(this XElement el, string attrName)
        {
            XAttribute attr = el.Attribute(attrName);
            if (attr != null && !string.IsNullOrEmpty(attr.Value))
                return MainUtil.GetBool(attr.Value, false);
            return false;
        }

        public static string GetString(this XElement el, string attrName)
        {
            XAttribute attr = el.Attribute(attrName);
            if (attr != null && !string.IsNullOrEmpty(attr.Value))
                return attr.Value;
            return string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;

namespace ClassySC.Data
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface)]
    public class TemplateAttribute: System.Attribute
    {
        private ID _id;

        public TemplateAttribute(string templateID)
        {
            _id = new ID(templateID);
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

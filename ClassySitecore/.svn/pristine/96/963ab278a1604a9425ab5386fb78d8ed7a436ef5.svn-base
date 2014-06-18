using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Caching;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore;

namespace ClassySC.Data.Manager
{
    public class ClassTemplateCache : CustomCache
    {
        public ClassTemplateCache(string name, long maxSize)
            : base(name, maxSize)
        {

        }


        public void AddClassTemplateRef(ID templateId, Type classType)
        {
            Assert.ArgumentNotNull(templateId, "templateId");
            Assert.ArgumentNotNull(classType, "classType");

            this.SetObject(templateId, classType, StringUtil.ParseSizeString("10KB"));

        }

        public Type GetClassTemplateRef(ID templateId)
        {
            Assert.ArgumentNotNull(templateId, "templateId");

            object entry = this.GetObject(templateId);
            if (entry == null)
                return null;
            return (Type)entry;
        }

    }
}

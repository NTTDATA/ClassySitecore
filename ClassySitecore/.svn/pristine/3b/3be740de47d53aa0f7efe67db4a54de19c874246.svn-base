using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System.Xml.Serialization;
using System.Xml.Linq;
using Sitecore;

namespace ClassySC.Builder.Configuration
{
    [Serializable]
    public class ConfigNode
    {
        private ID _id;
        public ID ID { get { return _id; } set { _id = value; } }

        public ConfigNode(Item item) : this(item.ID) { }
        public ConfigNode(ID id)
        {
            Assert.ArgumentNotNull(id, "id");
            _id = id;
        }

        public Item GetItem()
        {
            Assert.IsNotNull(_id, "no {ID}");
            return ConfigManager.GetContentDB.GetItem(_id);
        }

        


    }
}

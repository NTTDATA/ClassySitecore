using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Xml.Linq;

namespace ClassySC.Builder.Configuration
{
    [Serializable]
    public class FieldNode : ConfigNode
    {

        private string _propertyName;
        public string PropertyName { get { return _propertyName; } set { _propertyName = value; } }

        private ID _classTypeID;
        public ID ClassTypeID { get { return _classTypeID; } set { _classTypeID = value; } }

        private string _classComments;
        public string ClassComments { get { return _classComments; } set { _classComments = value; } }

        private bool _observable;
        public bool Observable { get { return _observable; } set { _observable = value; } }




        public FieldNode(Item item) : this(item.ID) { }
        public FieldNode(ID id)
            : base(id)
        {
            _propertyName = string.Empty;
            _classTypeID = ID.Null;
            _classComments = string.Empty;
            _observable = false;
        }




    }
}

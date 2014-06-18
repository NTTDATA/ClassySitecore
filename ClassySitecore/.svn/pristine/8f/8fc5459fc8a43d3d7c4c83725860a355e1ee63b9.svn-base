using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System.Xml.Linq;

namespace ClassySC.Builder.Configuration
{
    [Serializable]
    public class TemplateNode : ConfigNode
    {

        private bool _generate;
        public bool Generate { get { return _generate; } set { _generate = value; } }

        private string _className;
        public string ClassName { get { return _className; } set { _className = value; } }

        private string _namespace;
        public string Namespace { get { return _namespace; } set { _namespace = value; } }

        private string _filePath;
        public string FilePath { get { return _filePath; } set { _filePath = value; } }

        private ID _baseTemplateID;
        public ID BaseTemplateID { get { return _baseTemplateID; } set { _baseTemplateID = value; } }

        private List<FieldNode> _fields;
        public List<FieldNode> Fields { get { return _fields; } set { _fields = value; } }


        public TemplateNode(Item item) : this(item.ID) { }
        public TemplateNode(ID id)
            : base(id)
        {
            _generate = false;
            _className = string.Empty;
            _namespace = string.Empty;
            _filePath = string.Empty;
            _baseTemplateID = ID.Null;
            _fields = new List<FieldNode>();

        }

        public FieldNode GetFieldNode(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return GetFieldNode(item.ID);
        }

        public FieldNode GetFieldNode(ID id)
        {
            Assert.ArgumentNotNull(id, "id");
            FieldNode output = Fields.SingleOrDefault(fn => fn.ID == id);
            if (output == null)
            {
                output = new FieldNode(id);
                Fields.Add(output);
            }
            return output;
        }

        
        

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace ClassySC.Builder.Configuration
{
    [Serializable]
    public class FolderNode : ConfigNode
    {
        private string _namespace;
        public string Namespace { get { return _namespace; } set { _namespace = value; } }
        
        
        private string _folderFilePath;
        public string FolderFilePath { get { return _folderFilePath; } set { _folderFilePath = value; } }


        public FolderNode(Item item) : this(item.ID) { }
        public FolderNode(ID id)
            : base(id)
        {
            _namespace = string.Empty;
            _folderFilePath = string.Empty;
        }




    }
}

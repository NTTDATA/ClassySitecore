using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Web;
using ClassySC.Builder.Builder;

namespace ClassySC.Builder.Configuration
{
    public class ConfigManager
    {
        private Database _db;
        RootNode _root;

        public ConfigManager()
        {
            _db = ConfigManager.GetContentDB;
            Load();
        }

        public static Database GetContentDB
        {
            get
            {
                return Sitecore.Context.ContentDatabase ?? Sitecore.Configuration.Factory.GetDatabase("master");
            }
        }

        public static ConfigManager Instance
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context == null)
                    return new ConfigManager();
                if (!context.Items.Contains("CM_INSTANCE"))
                    context.Items["CM_INSTANCE"] = new ConfigManager();
                return context.Items["CM_INSTANCE"] as ConfigManager;
            }
        }


        public List<TemplateNode> Templates
        {
            get
            {
                Assert.IsNotNull(_root, "RootNode is null");
                return _root.Templates;
            }
        }

        public List<FolderNode> Folders
        {
            get
            {
                Assert.IsNotNull(_root, "RootNode is null");
                return _root.Folders;
            }
        }

        public string SerializeRoot(RootNode root)
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(RootNode));
            //StringWriter outStream = new StringWriter();
            //serializer.Serialize(outStream, root);
            //return outStream.ToString();

            return NodeSerialization.Serialize.FromRootNode(root).ToString();
        }

        public RootNode DeserializeRoot(string xml)
        {
            return NodeSerialization.Deserialize.ToRootNode(XElement.Parse(xml));
            //XmlSerializer serializer = new XmlSerializer(typeof(RootNode));
            //StringReader reader = new StringReader(xml);
            //RootNode root = new RootNode();
            //root = (RootNode)serializer.Deserialize(reader);
            //return root;
        }

        public void Load()
        {
            Item configItem = _db.GetItem(BuilderConst.ClassySCConfigPath);
            Assert.IsNotNull(configItem, "Can't find " + BuilderConst.ClassySCConfigPath);

            string xml = configItem[BuilderConst.ClassySCConfig.ConfigXml];
            if (string.IsNullOrEmpty(xml))
            {
                _root = new RootNode();
            }
            else
            {
                try
                {
                    _root = DeserializeRoot(xml);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void Save()
        {
            Item configItem = _db.GetItem(BuilderConst.ClassySCConfigPath);
            Assert.IsNotNull(configItem, "Can't find " + BuilderConst.ClassySCConfigPath);


            for (int i = Folders.Count-1; i >= 0; i--)
            {
                if (_db.GetItem(Folders[i].ID) == null)
                {
                    Folders.RemoveAt(i);
                }
            }

            for (int i = Templates.Count - 1; i >= 0; i--)
            {
                if (_db.GetItem(Templates[i].ID) == null)
                {
                    Templates.RemoveAt(i);
                }
                else
                {
                    for (int j = Templates[i].Fields.Count - 1; j >= 0; j--)
                    {
                        if (_db.GetItem(Templates[i].Fields[j].ID) == null)
                        {
                            Templates[i].Fields.RemoveAt(j);
                        }
                    }
                }
            }


            using (new EditContext(configItem))
            {
                configItem[BuilderConst.ClassySCConfig.ConfigXml] = SerializeRoot(_root);
            }
        }

        public static ClassySCConfig GetClassySCConfig()
        {
            return new ClassySCConfig(GetContentDB.GetItem(BuilderConst.ClassySCConfigPath));
        }

        public FolderNode GetFolderNode(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return GetFolderNode(item.ID);
        }

        public FolderNode GetFolderNode(ID id)
        {
            Assert.ArgumentNotNull(id, "id");
            FolderNode output = _root.Folders.FirstOrDefault(fn=>fn.ID == id);
            if (output == null)
            {
                output = new FolderNode(id);
                _root.Folders.Add(output);
            }
            return output;
        }


        public TemplateNode GetTemplateNode(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return GetTemplateNode(item.ID);
        }

        public TemplateNode GetTemplateNode(ID id)
        {
            Assert.ArgumentNotNull(id, "id");
            TemplateNode output = _root.Templates.FirstOrDefault(tn => tn.ID == id);
            if (output == null)
            {
                output = new TemplateNode(id);
                _root.Templates.Add(output);
            }
            return output;
        }

    }


   
}

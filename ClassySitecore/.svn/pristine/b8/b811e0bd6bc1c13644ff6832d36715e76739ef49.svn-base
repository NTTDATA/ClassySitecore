using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Sitecore.Data;

namespace ClassySC.Builder.Configuration
{
    public static class NodeSerialization
    {

        public static class Serialize
        {

            public static XElement FromFieldNode(FieldNode node)
            {
                XElement el = new XElement("FieldNode");
                el.SetAttributeValue("id", node.ID);
                el.SetAttributeValue("PropertyName", node.PropertyName);
                el.SetAttributeValue("ClassTypeID", ID.IsNullOrEmpty(node.ClassTypeID) ? null : node.ClassTypeID);
                el.SetAttributeValue("ClassComments", node.ClassComments);
                el.SetAttributeValue("Observable", node.Observable);
                return el;
            }


            public static XElement FromTemplateNode(TemplateNode node)
            {
                XElement el = new XElement("TemplateNode");

                el.SetAttributeValue("id", node.ID);
                el.SetAttributeValue("Generate", node.Generate);
                el.SetAttributeValue("ClassName", node.ClassName);
                el.SetAttributeValue("Namespace", node.Namespace);
                el.SetAttributeValue("FilePath", node.FilePath);
                el.SetAttributeValue("BaseTemplateID", ID.IsNullOrEmpty(node.BaseTemplateID) ? null : node.BaseTemplateID);

                node.Fields.ForEach(f => el.Add(FromFieldNode(f)));
                return el;
            }

            public static XElement FromFolderNode(FolderNode node)
            {
                XElement el = new XElement("FolderNode");

                el.SetAttributeValue("id", node.ID);
                el.SetAttributeValue("Namespace", node.Namespace);
                el.SetAttributeValue("FolderFilePath", node.FolderFilePath);
                

                return el;
            }

            public static XElement FromRootNode(RootNode node)
            {
                XElement el = new XElement("RootNode");
                XElement foldersEl = new XElement("Folders");
                XElement templatesEl = new XElement("Templates");
                el.Add(foldersEl, templatesEl);

                node.Folders.ForEach(f => foldersEl.Add(FromFolderNode(f)));
                node.Templates.ForEach(t => templatesEl.Add(FromTemplateNode(t)));

                return el;
            }

        }

        public static class Deserialize
        {

            public static RootNode ToRootNode(XElement el)
            {
                RootNode node = new RootNode();
                XElement foldersEl = el.Element("Folders");

                foreach (XElement el1 in foldersEl.Elements("FolderNode").ToList())
                {
                    if (el1.GetID("id") != ID.Null)
                    {
                        node.Folders.Add(ToFolderNode(el1));
                    }
                }

                XElement templatesEl = el.Element("Templates");

                foreach (XElement el1 in templatesEl.Elements("TemplateNode").ToList())
                {
                    if (el1.GetID("id") != ID.Null)
                    {
                        node.Templates.Add(ToTemplateNode(el1));
                    }
                }


                return node;
            }

            public static FolderNode ToFolderNode(XElement el)
            {
                ID id = el.GetID("id");
                FolderNode node = new FolderNode(id);
                node.Namespace = el.GetString("Namespace");                
                node.FolderFilePath = el.GetString("FolderFilePath");

                return node;
            }

            public static TemplateNode ToTemplateNode(XElement el)
            {
                ID id = el.GetID("id");
                TemplateNode node = new TemplateNode(id);
                node.BaseTemplateID = el.GetID("BaseTemplateID");
                node.ClassName = el.GetString("ClassName");
                node.Namespace = el.GetString("Namespace");
                node.FilePath = el.GetString("FilePath");
                node.Generate = el.GetBool("Generate");

                foreach (XElement el1 in el.Elements("FieldNode").ToList())
                {
                    if (el1.GetID("id") != ID.Null)
                    {
                        node.Fields.Add(ToFieldNode(el1));
                    }
                }

                return node;
            }

            public static FieldNode ToFieldNode(XElement el)
            {
                ID id = el.GetID("id");
                FieldNode node = new FieldNode(id);
                node.PropertyName = el.GetString("PropertyName");
                node.ClassComments = el.GetString("ClassComments");
                node.ClassTypeID = el.GetID("ClassTypeID");
                node.Observable = el.GetBool("Observable");
                return node;
            }


        }
    }
}

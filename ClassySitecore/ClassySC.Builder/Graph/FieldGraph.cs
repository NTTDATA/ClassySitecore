using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Data.Items;
using ClassySC.Builder.Configuration;

namespace ClassySC.Builder.Graph
{
    public class FieldGraph
    {
        public ID FieldID { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string PropertyType { get; set; }
        public string PropertyName { get; set; }
        public string Comments { get; set; }

        public bool IsObservable { get; set; }

        public FieldGraph(Item fieldItem)
        {
            FieldID = fieldItem.ID;
            FieldName = BuilderUtil.FormatPublicName(fieldItem.Name);
            IsObservable = false;
            ConfigManager mgr = ConfigManager.Instance;
            FieldNode fieldConfigNode = mgr.Templates.SelectMany(tn => tn.Fields).SingleOrDefault(fn => fn.ID == fieldItem.ID);

            FieldType = fieldItem[BuilderConst.FieldTypeFieldID];
            if (fieldConfigNode != null)
            {
                PropertyName = fieldConfigNode.PropertyName;


                Type fieldType = BuilderUtil.GetFieldType(this);

                if (!ID.IsNullOrEmpty(fieldConfigNode.ClassTypeID))
                {
                    PropertyType = BuilderUtil.GetFullTypeName(fieldConfigNode.ClassTypeID);
                    
                    if (fieldType == typeof(IEnumerable<Item>))
                    {
                        PropertyType = "IEnumerable<" + PropertyType + ">";
                    }
                }
                else
                    PropertyType = "";

                if (fieldType == typeof(IEnumerable<Item>))
                {
                    IsObservable = fieldConfigNode.Observable;
                }

                Comments = fieldConfigNode.ClassComments;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldGraph)
                return this.FieldID.Guid == (obj as FieldGraph).FieldID.Guid;
            return false;
        }

        public override int GetHashCode()
        {
            return FieldID.GetHashCode();
        }
    }
}

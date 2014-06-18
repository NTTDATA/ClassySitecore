using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassySC.Builder;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Collections;
using ClassySC.Builder.Configuration;
using Sitecore;

namespace ClassySC.Builder.Graph
{
    public class ClassGraph
    {
        public ID TemplateId { get; set; }
        public string ClassName { get; set; }
        public string BaseClassName { get; set; }
        public List<FieldGraph> FieldGraphs { get; set; }
        public TemplateItem TemplateItem { get; set; }
        public TemplateBuildConfig BuildConfig { get; set; }
        public ClassGraph BaseClassGraph { get; set; }
        public List<ClassGraph> InheritedClassGraphs { get; set; }

        public ClassGraph(TemplateBuildConfig templateConfig)
        {
            ConfigManager configMgr = ConfigManager.Instance;

            TemplateItem = templateConfig.Database.Templates[templateConfig.TemplateID];
            this.InheritedClassGraphs = new List<ClassGraph>();
            this.BuildConfig = templateConfig;
            this.TemplateId = templateConfig.TemplateID;
            this.ClassName = templateConfig.ClassName;

            if (this.TemplateId != TemplateIDs.TemplateFolder)
            {
                this.BaseClassName = string.Empty;
                if (templateConfig.BaseTemplateID != ID.Null && configMgr.Templates.Exists(tn => tn.ID == templateConfig.BaseTemplateID))
                {
                    Item baseTemplateItem = templateConfig.Database.GetItem(templateConfig.BaseTemplateID);
                    if (baseTemplateItem != null)
                    {
                        this.BaseClassGraph = new ClassGraph(new TemplateBuildConfig(baseTemplateItem));
                        this.BaseClassName = this.BaseClassGraph.ClassName;
                    }
                }

                //all inherited templates that aren't the base template
                this.InheritedClassGraphs = TemplateItem.BaseTemplates.Where(ti => ti.ID != templateConfig.BaseTemplateID && ti.ID != BuilderConst.StandardTemplateID).Select(ti => new ClassGraph(new TemplateBuildConfig(ti))).Where(cg => cg.ClassName != "" && cg.BuildConfig.Generate).ToList();

                this.FieldGraphs = new List<FieldGraph>();
                foreach (ID fieldID in templateConfig.FieldList)
                {
                    Item fieldItem = templateConfig.Database.GetItem(fieldID);
                    if (fieldItem != null)
                        this.FieldGraphs.Add(new FieldGraph(fieldItem));
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ClassGraph)
                return this.TemplateId.Guid == (obj as ClassGraph).TemplateId.Guid;
            return false;
        }

        public override int GetHashCode()
        {
            return TemplateId.GetHashCode();
        }
    }
}

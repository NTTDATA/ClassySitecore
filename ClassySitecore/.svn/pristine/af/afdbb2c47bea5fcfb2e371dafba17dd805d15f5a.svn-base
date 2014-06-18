using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore;
using Sitecore.Diagnostics;
using ClassySC.Builder.Builder;
using ClassySC.Builder.Configuration;


namespace ClassySC.Builder
{
    public class TemplateBuildConfig
    {
        public ID TemplateID { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string FilePath { get; set; }
        public ID BaseTemplateID { get; set; }
        public List<ID> FieldList { get; set; }
        public Database Database { get; set; }
        public bool Generate { get; set; }


        public TemplateBuildConfig(Item templateItem)
        {
            TemplateID = templateItem.ID;
            Database = templateItem.Database;

            ClassySCConfig config = ConfigManager.GetClassySCConfig();
            Assert.IsNotNull(config.InnerItem, "/sitecore/system/Modules/ClassySC/ClassySCConfig  not found");

            if (templateItem.TemplateID != TemplateIDs.TemplateFolder)
            {
                string slnPath = Sitecore.Configuration.Settings.GetSetting("ClassySC.SolutionPath");
                Assert.IsNotNullOrEmpty(slnPath, @"<setting name='ClassySC.SolutionPath' value='C:\Inetpub\wwwroot\Input' /> is missing in config");

                ConfigManager configManager = ConfigManager.Instance;
                TemplateNode configNode = configManager.GetTemplateNode(templateItem.ID);


                ClassName = StringUtil.GetString(configNode.ClassName, BuilderUtil.FormatPublicName(templateItem.Name));
                FilePath = StringUtil.GetString(configNode.FilePath, BuilderUtil.GetTemplateFilePath(templateItem)).Replace("[slnpath]", slnPath);
                BaseTemplateID = configNode.BaseTemplateID;
                FieldList = configNode.Fields.Select(fn => fn.ID).ToList();
                Generate = configNode.Generate;

                //namespace
                if (!string.IsNullOrEmpty(configNode.Namespace))
                {
                    Namespace = configNode.Namespace;
                }
                else
                {
                    Namespace = BuilderUtil.GetClassNamespace(templateItem);
                }

            }
        }
    }
}

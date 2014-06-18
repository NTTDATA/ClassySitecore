using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;

namespace ClassySC.Builder.Builder
{
    public class ClassySCConfig : CustomItemBase
    {
        public ClassySCConfig(Item innerItem)
            : base(innerItem)
        {
        }

       
        public string UsingStatement
        {
            get { return InnerItem[BuilderConst.ClassySCConfig.UsingStatement]; }
            set { InnerItem[BuilderConst.ClassySCConfig.UsingStatement] = value; }
        }

        public string StandardTemplateClassName
        {
            get { return InnerItem[BuilderConst.ClassySCConfig.StandardTemplateClassName]; }
            set { InnerItem[BuilderConst.ClassySCConfig.StandardTemplateClassName] = value; }
        }

        public string DefaultNamespace
        {
            get { return InnerItem[BuilderConst.ClassySCConfig.DefaultNamespace]; }
            set { InnerItem[BuilderConst.ClassySCConfig.DefaultNamespace] = value; }
        }

        public string DefaultFilePath
        {
            get { return InnerItem[BuilderConst.ClassySCConfig.DefaultFilePath]; }
            set { InnerItem[BuilderConst.ClassySCConfig.DefaultFilePath] = value; }
        }

        public string SolutionPath
        {
            get { return Sitecore.Configuration.Settings.GetSetting("ClassySC.SolutionPath"); }
        }
    }
}

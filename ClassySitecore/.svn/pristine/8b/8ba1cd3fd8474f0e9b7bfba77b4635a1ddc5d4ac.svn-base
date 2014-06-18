using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Controls;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.XamlSharp.Xaml;
using Sitecore.Web;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.Configuration;
using System.Web.UI.WebControls;
using Sitecore.Text;

namespace ClassySC.Builder.Client
{
    public class ClassySCSetup : DialogPage
    {
        protected TextBox UsingStatementsTB;
        protected TextBox StandardTemplateClassNameTB;
        protected TextBox DefaultNamespaceTB;
        protected TextBox DefaultFilePathTB;
        protected Panel SolutionPathWarningPanel;
        protected Literal SolutionPathLit;

        public Database DB
        {
            get
            {
                return Factory.GetDatabase("master");
            }
        }

        // Methods
        protected override void OK_Click()
        {

            UrlString result = new UrlString();
            result.Add("UsingStatement", UsingStatementsTB.Text);
            result.Add("StandardTemplateClassName", StandardTemplateClassNameTB.Text);
            result.Add("DefaultNamespace", DefaultNamespaceTB.Text);
            result.Add("DefaultFilePath", DefaultFilePathTB.Text);
            AjaxScriptManager.SetDialogValue(result.ToString());
            base.OK_Click();

        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!XamlControl.AjaxScriptManager.IsEvent)
            {
                UsingStatementsTB.Text = WebUtil.GetQueryString("UsingStatement");
                StandardTemplateClassNameTB.Text = WebUtil.GetQueryString("StandardTemplateClassName");
                DefaultNamespaceTB.Text = WebUtil.GetQueryString("DefaultNamespace");
                DefaultFilePathTB.Text = WebUtil.GetQueryString("DefaultFilePath");
                string slnPath = WebUtil.GetQueryString("SolutionPath");
                SolutionPathWarningPanel.Visible = string.IsNullOrEmpty(slnPath);
                SolutionPathLit.Text = slnPath;
            }
        }
    }


}

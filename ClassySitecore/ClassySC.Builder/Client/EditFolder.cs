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
    public class EditFolder : DialogPage
    {
        protected TextBox NamespaceTB;
        protected Literal NamespaceLit;

        protected TextBox FolderFilePathTB;
        protected Literal FolderFilePathLit;
        
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

            result.Add("Namespace", NamespaceTB.Text);
            result.Add("FolderFilePath", FolderFilePathTB.Text);


            AjaxScriptManager.SetDialogValue(result.ToString());
            base.OK_Click();
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!XamlControl.AjaxScriptManager.IsEvent)
            {
                UrlHandle handle = UrlHandle.Get();

                NamespaceTB.Text = handle["Namespace"];
                NamespaceLit.Text = handle["NamespaceDefault"];

                FolderFilePathTB.Text = handle["FolderFilePath"];
                FolderFilePathLit.Text = handle["FolderFilePathDefault"];

            }
        }
    }


}

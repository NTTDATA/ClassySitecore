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
using Sitecore.Web.UI.HtmlControls;

namespace ClassySC.Builder.Client
{
    public class PreviewCode : DialogPage
    {
        protected Border OutputDiv;


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

           
            base.OK_Click();
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!XamlControl.AjaxScriptManager.IsEvent)
            {
                UrlHandle handle = UrlHandle.Get();
                this.Header = handle["templatename"];
                OutputDiv.InnerHtml = string.Format("<pre>{0}</pre>", handle["code"]);
            }
        }
    }


}

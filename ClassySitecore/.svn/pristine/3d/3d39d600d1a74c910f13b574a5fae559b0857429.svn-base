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
using ClassySC.Builder.Configuration;
using ClassySC.Builder.Graph;
using Sitecore;

namespace ClassySC.Builder.Client
{
    public class EditField : DialogPage
    {
        protected TextBox PropertyNameTB;

        protected TextBox PropertyCommentsTB;

        protected Literal PropertyNameLit;
        protected Literal PropertyClassTypeLit;

        protected DropDownList PropertyTypeDD;

        protected Sitecore.Web.UI.HtmlControls.Groupbox PropertyTypeGB;

        protected Sitecore.Web.UI.HtmlControls.Groupbox ObservableGB;
        protected Sitecore.Web.UI.HtmlControls.Checkbox ObservableCB;

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

            result.Add("PropertyName", PropertyNameTB.Text);
            result.Add("PropertyComments", PropertyCommentsTB.Text);
            result.Add("PropertyClassID", PropertyTypeDD.SelectedValue);
            result.Add("Observable", ObservableCB.Checked ? "1" : "");

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

                PropertyNameTB.Text = handle["PropertyName"];
                PropertyCommentsTB.Text = handle["PropertyComments"];

                PropertyNameLit.Text = handle["PropertyNameDefault"];
                PropertyClassTypeLit.Text = handle["PropertyClassTypeDefault"];

                ObservableCB.Checked = MainUtil.GetBool(handle["Observable"], false);

                Item fieldItem = DB.GetItem(handle["FieldID"]);
                Type fieldType = BuilderUtil.GetFieldType(new FieldGraph(fieldItem));

                if (fieldType == typeof(Item) || fieldType == typeof(IEnumerable<Item>))
                {
                    ConfigManager configMgr = ConfigManager.Instance;

                    PropertyTypeDD.Items.Add(new ListItem());

                    PropertyTypeDD.Items.Add(new ListItem("StandardTemplate", TemplateIDs.StandardTemplate.ToString()) { Selected = handle["PropertyClassID"] == TemplateIDs.StandardTemplate.ToString() });

                    foreach (TemplateNode templateConfigNode in configMgr.Templates.Where(tn => tn.Generate).OrderBy(tn => BuilderUtil.GetFullTypeName(tn.ID)))
                    {
                        string className = BuilderUtil.GetFullTypeName(templateConfigNode.ID);
                        ListItem listItem = new ListItem(className);
                        listItem.Value = templateConfigNode.ID.ToString();
                        listItem.Selected = handle["PropertyClassID"] == templateConfigNode.ID.ToString();
                        PropertyTypeDD.Items.Add(listItem);
                    }

                    if (fieldType == typeof(IEnumerable<Item>))
                    {
                        ObservableGB.Visible = true;
                    }
                }
                else
                {
                    PropertyTypeGB.Visible = false;
                }
            }
        }


    }


}

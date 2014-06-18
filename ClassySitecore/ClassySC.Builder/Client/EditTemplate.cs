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

namespace ClassySC.Builder.Client
{
    public class EditTemplate : DialogPage
    {
        protected Sitecore.Web.UI.HtmlControls.Checkbox GenerateCB;
        protected TextBox NamespaceTB;
        protected TextBox ClassNameTB;
        protected TextBox FilePathTB;
        protected CheckBoxList FieldsCL;
        protected DropDownList BaseTemplateDD;

        protected Literal NamespaceLit;
        protected Literal ClassNameLit;
        protected Literal FilePathLit;

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

            result.Add("Generate", GenerateCB.Checked ? "1" : "");
            result.Add("Namespace", NamespaceTB.Text);
            result.Add("ClassName", ClassNameTB.Text);
            result.Add("FilePath", FilePathTB.Text);
            List<string> fields = new List<string>();
            foreach (ListItem item in FieldsCL.Items)
            {
                if (item.Selected)
                    fields.Add(item.Value);
            }
            result.Add("Fields", string.Join("|", fields.ToArray()));

            result.Add("BaseTemplate", BaseTemplateDD.SelectedValue);


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
                TemplateItem template = new TemplateItem(DB.GetItem(handle["TemplateID"]));
                GenerateCB.Checked = handle["Generate"] == "1";

                NamespaceTB.Text = handle["Namespace"];
                ClassNameTB.Text = handle["ClassName"];
                FilePathTB.Text = handle["FilePath"];
                NamespaceLit.Text = handle["NamespaceDefault"];
                ClassNameLit.Text = handle["ClassNameDefault"];
                FilePathLit.Text = handle["ClassFilePathDefault"];



                foreach (TemplateFieldItem field in template.OwnFields)
                {
                    ListItem listItem = new ListItem(field.Name, field.ID.ToString());
                    listItem.Selected = handle["Fields"].Contains(field.ID.ToString());

                    FieldsCL.Items.Add(listItem);
                }

                BaseTemplateDD.Items.Add(new ListItem("", ""));

                ConfigManager configManager = ConfigManager.Instance;

                foreach (TemplateItem baseTemplateItem in template.BaseTemplates.Where(ti => ti.ID != BuilderConst.StandardTemplateID && configManager.Templates.Exists(tn => tn.ID == ti.ID && tn.Generate)))
                {
                    ListItem listItem = new ListItem(baseTemplateItem.InnerItem.Paths.FullPath.Replace("/sitecore/templates", ""), baseTemplateItem.ID.ToString());
                    listItem.Selected = handle["BaseTemplate"] == baseTemplateItem.ID.ToString();
                    BaseTemplateDD.Items.Add(listItem);
                }
            }
        }

        protected void CheckedBox()
        {
            if (GenerateCB.Checked)
            {
                if (FieldsCL.Items.Cast<ListItem>().All(li => !li.Selected))
                {
                    foreach (ListItem item in FieldsCL.Items)
                    {
                        item.Selected = true;
                    }
                    AjaxScriptManager.SetOuterHtml(FieldsCL.ClientID, FieldsCL);
                }
            }
            AjaxScriptManager.SetReturnValue(true);
        }
    }


}

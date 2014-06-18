using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Diagnostics;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Security.AccessControl;
using System.Web.UI;
using Sitecore.Security.Accounts;
using Sitecore.Text;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Framework;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Security;
using Sitecore.Web.UI.WebControls.Ribbons;
using Sitecore.Web;
using Sitecore.Shell.Web.UI.WebControls;
using System.IO;
using Sitecore.Globalization;
using System.Collections.Specialized;
using Sitecore.Data;
using Sitecore.Configuration;
using Sitecore.Web.UI.XamlSharp;
using System.Web;
using ClassySC.Builder.Builder;
using Sitecore.Data.Templates;
using Sitecore.Collections;
using System.Web.UI.HtmlControls;
using Sitecore.Web.UI.WebControls;
using ClassySC.Builder.Graph;
using ClassySC.Builder.Configuration;

namespace ClassySC.Builder.Client
{
    public class ClassySCForm : ApplicationForm
    {


        public Database DB
        {
            get
            {
                return Factory.GetDatabase("master");
            }
        }

        // Fields
        protected DataContext DataContext;
        protected DataContext EntityDataContext;

        protected Border RibbonPanel;
        protected ClassySCTreeview Treeview;

        protected Checklist BuildListCL;


        // Methods
        private void DataContext_Changed(object sender)
        {
            if (this.DataContext.GetFolder() != null)
            {
                //this.Explain(this.DataContext.GetFolder().ID.ToString(), this.RightName);
            }
        }


        protected void GenerateToBuildList()
        {

            Database db = Factory.GetDatabase("master");

            ConfigManager configMgr = new ConfigManager();

            Item[] templates = configMgr.Templates.Where(tn => tn.Generate).Select(tn => db.GetItem(tn.ID)).OrderBy(itm => itm.Paths.FullPath).ToArray();



            foreach (ChecklistItem child in BuildListCL.Items)
            {
                BuildListCL.Controls.Remove(child);
            }

            foreach (Item template in templates)
            {
                ChecklistItem child = new ChecklistItem
                {
                    ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("Template")
                };
                BuildListCL.Controls.Add(child);
                child.Header = template.Paths.FullPath.Replace("/sitecore/templates", "");
                child.Value = template.ID.ToString();
            }
            Context.ClientPage.ClientResponse.Refresh(BuildListCL);

        }


        private Item[] GetCurrentItem(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            string str = message["id"];
            Item[] selected = this.DataContext.GetSelected();
            if (selected.Length > 0)
            {
                return selected;
            }
            Item folder = this.DataContext.GetFolder();
            if (!string.IsNullOrEmpty(str))
            {
                return new Item[] { folder.Database.Items[str, folder.Language] };
            }
            return new Item[] { folder };
        }

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            Item[] currentItem = this.GetCurrentItem(message);

            if (message.Name.StartsWith("classysc:"))
            {
                if (CheckConfig())
                {
                    return;
                }
            }


            if (message.Name == "classysc:config")
            {
                NameValueCollection parameters = new NameValueCollection();
                Context.ClientPage.Start(this, "Config", parameters);
            }
            else if (message.Name == "classysc:generate")
            {
                Database db = Factory.GetDatabase("master");
                List<ID> templateIds = new List<ID>(); //db.SelectItems(BuilderConst.AllGeneratedTemplatesQuery).Select(itm => itm.ID).ToList();

                try
                {
                    foreach (ChecklistItem child in BuildListCL.Items)
                    {
                        if (child.Checked)
                        {
                            templateIds.Add(new ID(child.Value));
                        }
                    }
                    BuildManager mgr = new BuildManager(db);
                    mgr.WriteToFile = true;
                    mgr.Generate(templateIds);
                    SheerResponse.Alert("Generated {0} templates", templateIds.Count.ToString());

                }
                catch (Exception ex)
                {
                    SheerResponse.ShowError(ex);
                }
            }
            else if (message.Name == "classysc:edit")
            {
                Item item = Treeview.GetSelectionItem();
                if (item == null)
                {
                    SheerResponse.Alert("Please select a template or field");
                    return;
                }
                if (item.TemplateID == TemplateIDs.Template)
                {
                    NameValueCollection parameters = new NameValueCollection();
                    parameters.Add("id", item.ID.ToString());
                    Context.ClientPage.Start(this, "EditTemplate", parameters);
                }
                else if (item.TemplateID == TemplateIDs.TemplateField)
                {
                    NameValueCollection parameters = new NameValueCollection();
                    parameters.Add("id", item.ID.ToString());
                    Context.ClientPage.Start(this, "EditField", parameters);
                }
                else if (item.TemplateID == TemplateIDs.TemplateFolder)
                {
                    NameValueCollection parameters = new NameValueCollection();
                    parameters.Add("id", item.ID.ToString());
                    Context.ClientPage.Start(this, "EditFolder", parameters);
                }
                else
                {
                    SheerResponse.Alert("Please select a template or field (no sections or folders)");
                    return;
                }
            }
            else if (message.Name == "classysc:preview")
            {
                Item item = Treeview.GetSelectionItem();
                if (item != null && item.TemplateID == TemplateIDs.Template)
                {

                    TemplateBuildConfig config = new TemplateBuildConfig(item);
                    if (config.Generate)
                    {

                        UrlString dialogUrl = new UrlString(ControlManager.GetControlUrl(new ControlName("ClassySC.Builder.Client.PreviewCode")));
                        BuildManager mgr = new BuildManager(item.Database);
                        mgr.CacheConstants(mgr.GetTemplates());
                        string code = mgr.GenerateTemplateCode(new ClassGraph(config));

                        UrlHandle handle = new UrlHandle();
                        handle["templatename"] = item.Name;
                        handle["code"] = code.Replace("<", "&lt;").Replace(">", "&gt;");

                        handle.Add(dialogUrl);
                        SheerResponse.ShowModalDialog(dialogUrl.ToString(), false);
                    }
                    else
                    {
                        SheerResponse.Alert("This template isn't marked for generation.");
                    }
                }
                else
                {
                    SheerResponse.Alert("Please select a template to preview code");
                }

            }

        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);

            this.DataContext.Changed += new DataContext.DataContextChangedDelegate(this.DataContext_Changed);
            if (!Context.ClientPage.IsEvent)
            {
                this.DataContext.GetFromQueryString();

                int num2 = 0;
                this.Treeview.ColumnNames.Add(num2.ToString(), "Name");
                this.Treeview.ColumnNames.Add("desc", "Description");

                this.UpdateRibbon();
                this.GenerateToBuildList();
            }
        }

        protected bool CheckConfig()
        {
            string slnpath = Sitecore.Configuration.Settings.GetSetting("ClassySC.SolutionPath");


            if (string.IsNullOrEmpty(slnpath))
            {
                SheerResponse.Alert(@"Please specifiy <setting name='ClassySC.SolutionPath' value='C:\Inetpub\wwwroot\[myprojectroot]' /> in /App_Config/Include/ClassySC.config");
                return true;
            }
            else
            {
                ClassySCConfig config = ConfigManager.GetClassySCConfig();

                //TemplateItem templateTemplate = new TemplateItem(DB.GetItem("/sitecore/templates/System/Templates/Template"));
                //Assert.IsNotNull(templateTemplate, "/sitecore/templates/System/Templates/Template not found");

                //List<string> templateBaseTemplates = templateTemplate.InnerItem[FieldIDs.BaseTemplate].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //TemplateItem templateFieldTemplate = new TemplateItem(DB.GetItem("/sitecore/templates/System/Templates/Template field"));
                //Assert.IsNotNull(templateFieldTemplate, "/sitecore/templates/System/Templates/Template field not found");

                //List<string> fieldBaseTemplates = templateFieldTemplate.InnerItem[FieldIDs.BaseTemplate].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (string.IsNullOrEmpty(config.UsingStatement) ||
                    string.IsNullOrEmpty(config.DefaultNamespace) ||
                    string.IsNullOrEmpty(config.DefaultFilePath) /*||
                    !templateBaseTemplates.Contains(BuilderConst.ClassySCTemplateTemplateID) ||
                    !fieldBaseTemplates.Contains(BuilderConst.ClassySCFieldTemplateID)*/
                    )
                {
                    NameValueCollection parameters = new NameValueCollection();
                    Context.ClientPage.Start(this, "Config", parameters);
                    return true;
                }
            }

            return false;
        }

        protected void SelectAll()
        {
            BuildListCL.CheckAll();
        }

        protected void SelectNone()
        {
            BuildListCL.UncheckAll();
        }


        private void UpdateRibbon()
        {
            Ribbon ctl = new Ribbon
            {
                ID = "Ribbon"
            };
            CommandContext context = new CommandContext(this.DataContext.GetFolder());
            ctl.CommandContext = context;
            ctl.ShowContextualTabs = false;
            Item item = Context.Database.GetItem("/sitecore/content/Applications/ClassySC/ClassySCManager/Ribbon");
            Assert.IsNotNull(item, "/sitecore/content/Applications/ClassySC/ClassySCManager/Ribbon");
            context.RibbonSourceUri = item.Uri;
            context.Folder = this.DataContext.GetFolder();
            this.RibbonPanel.InnerHtml = HtmlUtil.RenderControl(ctl);
        }


        protected void EditTemplate(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item = DB.GetItem(args.Parameters["id"]);
            Assert.IsNotNull(item, "template item");

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    UrlString results = new UrlString(args.Result);
                    ConfigManager mgr = ConfigManager.Instance;

                    TemplateNode templateConfigNode = mgr.GetTemplateNode(item.ID);

                    templateConfigNode.Generate = MainUtil.GetBool(HttpUtility.UrlDecode(results["Generate"]), false);
                    templateConfigNode.Namespace = HttpUtility.UrlDecode(results["Namespace"]);
                    templateConfigNode.ClassName = HttpUtility.UrlDecode(results["ClassName"]);
                    templateConfigNode.FilePath = HttpUtility.UrlDecode(results["FilePath"]);

                    string fieldIds = HttpUtility.UrlDecode(results["Fields"]);
                    List<FieldNode> fields = templateConfigNode.Fields;
                    fields.RemoveAll(fn => !fieldIds.Contains(fn.ID.ToString()));
                    foreach (string fieldId in StringUtil.Split(fieldIds, '|', true))
                    {
                        ID fID = MainUtil.GetID(fieldId);
                        if (!ID.IsNullOrEmpty(fID))
                            if (!fields.Exists(fn => fn.ID == fID))
                                fields.Add(new FieldNode(fID));
                    }

                    templateConfigNode.BaseTemplateID = MainUtil.GetID(HttpUtility.UrlDecode(results["BaseTemplate"]), ID.Null);


                    mgr.Save();


                    GenerateToBuildList();

                    DataTreeNode parentNode = Treeview.GetNodeByItemID(item.Parent.Paths.LongID) as DataTreeNode;
                    DataTreeNode itemNode = Treeview.GetNodeByItemID(item.Paths.LongID) as DataTreeNode;

                    if (parentNode != null)
                    {
                        Treeview.Toggle(parentNode.ID);
                        Treeview.AddSelected(itemNode.ID);
                        Treeview.Toggle(parentNode.ID);
                    }

                }
            }
            else
            {
                ConfigManager mgr = ConfigManager.Instance;
                TemplateNode templateConfigNode = mgr.GetTemplateNode(item.ID);

                UrlString dialogUrl = new UrlString(ControlManager.GetControlUrl(new ControlName("ClassySC.Builder.Client.EditTemplate")));

                UrlHandle handle = new UrlHandle();

                handle["TemplateID"] = item.ID.ToString();
                handle["Generate"] = templateConfigNode.Generate ? "1" : "";
                handle["Namespace"] = templateConfigNode.Namespace;
                handle["ClassName"] = templateConfigNode.ClassName;
                handle["FilePath"] = templateConfigNode.FilePath;
                handle["Fields"] = StringUtil.Join(templateConfigNode.Fields.Select(fn => fn.ID.ToString()), "|");
                handle["BaseTemplate"] = templateConfigNode.BaseTemplateID.ToString();


                ClassySCConfig config = ConfigManager.GetClassySCConfig();


                handle["NamespaceDefault"] = config.DefaultNamespace;
                handle["ClassNameDefault"] = BuilderUtil.FormatPublicName(item.Name);
                handle["ClassFilePathDefault"] = String.Format(config.DefaultFilePath, handle["ClassNameDefault"]).Replace("[slnpath]", config.SolutionPath);

                handle.Add(dialogUrl);

                SheerResponse.ShowModalDialog(dialogUrl.ToString(), true);
                args.WaitForPostBack();
            }
        }

        protected void EditField(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item = DB.GetItem(args.Parameters["id"]);
            Assert.IsNotNull(item, "template item");

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    TemplateFieldItem field = item;
                    ConfigManager mgr = ConfigManager.Instance;

                    TemplateNode templateConfigNode = mgr.GetTemplateNode(field.Template.ID);
                    FieldNode fieldConfigNode = templateConfigNode.GetFieldNode(item.ID);


                    UrlString results = new UrlString(args.Result);

                    fieldConfigNode.PropertyName = HttpUtility.UrlDecode(results["PropertyName"]);
                    fieldConfigNode.ClassTypeID = MainUtil.GetID(HttpUtility.UrlDecode(results["PropertyClassID"]), ID.Null);
                    fieldConfigNode.ClassComments = HttpUtility.UrlDecode(results["PropertyComments"]);
                    fieldConfigNode.Observable = MainUtil.GetBool(HttpUtility.UrlDecode(results["Observable"]), false);

                    mgr.Save();

                    DataTreeNode parentNode = Treeview.GetNodeByItemID(item.Parent.Paths.LongID) as DataTreeNode;
                    DataTreeNode itemNode = Treeview.GetNodeByItemID(item.Paths.LongID) as DataTreeNode;

                    if (parentNode != null)
                    {
                        Treeview.Toggle(parentNode.ID);
                        Treeview.AddSelected(itemNode.ID);
                        Treeview.Toggle(parentNode.ID);
                    }

                }
            }
            else
            {
                ConfigManager mgr = ConfigManager.Instance;

                TemplateFieldItem field = item;
                TemplateNode templateConfigNode = mgr.GetTemplateNode(field.Template.ID);
                FieldNode fieldConfigNode = templateConfigNode.GetFieldNode(item.ID);

                UrlString dialogUrl = new UrlString(ControlManager.GetControlUrl(new ControlName("ClassySC.Builder.Client.EditField")));
                UrlHandle handle = new UrlHandle();



                handle["FieldID"] = item.ID.ToString();
                handle["PropertyName"] = fieldConfigNode.PropertyName;
                handle["PropertyClassID"] = fieldConfigNode.ClassTypeID.ToString();
                handle["PropertyComments"] = fieldConfigNode.ClassComments;
                handle["Observable"] = fieldConfigNode.Observable ? "1" : "";


                FieldGraph fieldGraph = new FieldGraph(item);
                fieldGraph.PropertyType = "";

                handle["PropertyNameDefault"] = fieldGraph.FieldName;
                handle["PropertyClassTypeDefault"] = BuilderUtil.GetTypeOutput(BuilderUtil.GetCodeTypeReference(fieldGraph));

                handle.Add(dialogUrl);

                SheerResponse.ShowModalDialog(dialogUrl.ToString(), true);
                args.WaitForPostBack();
            }
        }

        protected void EditFolder(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item = DB.GetItem(args.Parameters["id"]);
            Assert.IsNotNull(item, "template item");

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {

                    ConfigManager mgr = ConfigManager.Instance;
                    FolderNode folderNode = mgr.GetFolderNode(item.ID);
                    UrlString results = new UrlString(args.Result);

                    folderNode.Namespace = HttpUtility.UrlDecode(results["Namespace"]);
                    folderNode.FolderFilePath = HttpUtility.UrlDecode(results["FolderFilePath"]);

                    mgr.Save();

                    DataTreeNode parentNode = Treeview.GetNodeByItemID(item.Parent.Paths.LongID) as DataTreeNode;
                    DataTreeNode itemNode = Treeview.GetNodeByItemID(item.Paths.LongID) as DataTreeNode;

                    if (parentNode != null)
                    {
                        Treeview.Toggle(parentNode.ID);
                        Treeview.AddSelected(itemNode.ID);
                        Treeview.Toggle(parentNode.ID);
                    }
                }
            }
            else
            {

                ClassySCConfig config = ConfigManager.GetClassySCConfig();
                ConfigManager mgr = ConfigManager.Instance;

                FolderNode folderNode = mgr.GetFolderNode(item.ID);

                UrlString dialogUrl = new UrlString(ControlManager.GetControlUrl(new ControlName("ClassySC.Builder.Client.EditFolder")));
                UrlHandle handle = new UrlHandle();

                handle["Namespace"] = folderNode.Namespace;
                handle["NamespaceDefault"] = config.DefaultNamespace;
                handle["FolderFilePath"] = folderNode.FolderFilePath;
                handle["FolderFilePathDefault"] = BuilderUtil.GetFolderFilePath(item);

                handle.Add(dialogUrl);

                SheerResponse.ShowModalDialog(dialogUrl.ToString(), true);
                args.WaitForPostBack();
            }
        }

        protected void Config(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    UrlString results = new UrlString(args.Result);

                    ClassySCConfig config = ConfigManager.GetClassySCConfig();

                    Assert.IsNotNull(config.InnerItem, "/sitecore/system/Modules/ClassySC/ClassySCConfig  not found");

                    using (new EditContext(config.InnerItem))
                    {
                        config.UsingStatement = HttpUtility.UrlDecode(results["UsingStatement"]);
                        config.StandardTemplateClassName = HttpUtility.UrlDecode(results["StandardTemplateClassName"]);
                        config.DefaultNamespace = HttpUtility.UrlDecode(results["DefaultNamespace"]);
                        config.DefaultFilePath = HttpUtility.UrlDecode(results["DefaultFilePath"]);

                    }
                    //ConfigureTemplates();
                    SheerResponse.Alert("Configuration Updated");
                }
            }
            else
            {
                UrlString dialogUrl = new UrlString(ControlManager.GetControlUrl(new ControlName("ClassySC.Builder.Client.ClassySCSetup")));

                ClassySCConfig config = ConfigManager.GetClassySCConfig();

                Assert.IsNotNull(config.InnerItem, "/sitecore/system/Modules/ClassySC/ClassGenConfig  not found");

                dialogUrl.Add("UsingStatement", config.UsingStatement);
                dialogUrl.Add("StandardTemplateClassName", config.StandardTemplateClassName);
                dialogUrl.Add("DefaultNamespace", config.DefaultNamespace);
                dialogUrl.Add("DefaultFilePath", config.DefaultFilePath);
                dialogUrl.Add("SolutionPath", config.SolutionPath);


                SheerResponse.ShowModalDialog(dialogUrl.ToString(), true);
                args.WaitForPostBack();
            }
        }

        //protected void ConfigureTemplates()
        //{
        //    TemplateItem templateTemplate = new TemplateItem(DB.GetItem("/sitecore/templates/System/Templates/Template"));
        //    Assert.IsNotNull(templateTemplate, "/sitecore/templates/System/Templates/Template not found");


        //    List<string> templateBaseTemplates = templateTemplate.InnerItem[FieldIDs.BaseTemplate].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    if (!templateBaseTemplates.Contains(BuilderConst.ClassySCTemplateTemplateID))
        //    {
        //        templateBaseTemplates.Insert(0, BuilderConst.ClassySCTemplateTemplateID);
        //        using (new EditContext(templateTemplate.InnerItem))
        //        {
        //            templateTemplate.InnerItem[FieldIDs.BaseTemplate] = string.Join("|", templateBaseTemplates.ToArray());
        //        }
        //    }


        //    TemplateItem templateFieldTemplate = new TemplateItem(DB.GetItem("/sitecore/templates/System/Templates/Template field"));
        //    Assert.IsNotNull(templateFieldTemplate, "/sitecore/templates/System/Templates/Template field not found");

        //    List<string> fieldBaseTemplates = templateFieldTemplate.InnerItem[FieldIDs.BaseTemplate].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    if (!fieldBaseTemplates.Contains(BuilderConst.ClassySCFieldTemplateID))
        //    {
        //        fieldBaseTemplates.Insert(0, BuilderConst.ClassySCFieldTemplateID);
        //        using (new EditContext(templateFieldTemplate.InnerItem))
        //        {
        //            templateFieldTemplate.InnerItem[FieldIDs.BaseTemplate] = string.Join("|", fieldBaseTemplates.ToArray());
        //        }
        //    }

        //}
    }

}

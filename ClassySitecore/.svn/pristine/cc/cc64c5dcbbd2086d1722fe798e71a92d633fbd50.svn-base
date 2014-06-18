using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Security.Accounts;
using Sitecore.Diagnostics;
using Sitecore;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Security.AccessControl;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.SecurityModel;
using Sitecore.Resources;
using Sitecore.Globalization;
using Sitecore.Text;
using Sitecore.Workflows;
using System.Web.UI;
using System.IO;
using Sitecore.Web;
using ClassySC.Builder.Graph;
using System.CodeDom;
using ClassySC.Builder.Builder;
using ClassySC.Builder.Configuration;

namespace ClassySC.Builder.Client
{
    public class ClassySCTreeview : DataTreeview
    {

        private ConfigManager _configManager = ConfigManager.Instance;

        protected override void GetContextMenu(string where)
        {
            Assert.ArgumentNotNull(where, "where");
            string source = Sitecore.Context.ClientPage.ClientRequest.Source;
            Menu contextMenu = new Menu();
            if (source == this.ID)
            {
                contextMenu.Add("__SelectColumns", "Select Columns", "Business/16x16/column.png", string.Empty, "accessviewer:selectcolumns", false, string.Empty, MenuItemType.Normal);
            }
            else
            {
                if (source.IndexOf("_") >= 0)
                {
                    source = source.Substring(0, source.LastIndexOf("_"));
                }
                DataTreeNode node = base.FindNode(source) as DataTreeNode;
                if (node == null)
                {
                    return;
                }
                contextMenu.Add("__Refresh", "Refresh", "Applications/16x16/refresh.png", string.Empty, "Treeview.Refresh(\"" + node.ID + "\")", false, string.Empty, MenuItemType.Normal);
            }
            SheerResponse.ShowContextMenu(Sitecore.Context.ClientPage.ClientRequest.Control, where, contextMenu);
        }


        protected override TreeNode GetTreeNode(Item item, System.Web.UI.Control parent)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(parent, "parent");
            DataTreeNode child = new DataTreeNode();
            parent.Controls.Add(child);
            string uniqueID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("T");
            child.Expandable = item.HasChildren;
            child.Expanded = false;
            child.Header = item.DisplayName;

            child.Icon = item.Appearance.Icon;
            child.ID = uniqueID;
            child.ItemID = item.Paths.LongID;
            child.ToolTip = item.Name;
            child.DataContext = this.DataContext;
            if (!UIUtil.IsIE(8))
            {
                child.Attributes["onmouseover"] = "javascript: $(this).addClassName('hover');";
                child.Attributes["onmouseout"] = "javascript: $(this).removeClassName('hover');";
            }
            this.UpdateColumnValues(child, item);
            return child;
        }





        private void UpdateColumnValues(DataTreeNode treeNode, Item item)
        {
            Assert.ArgumentNotNull(treeNode, "treeNode");
            Assert.ArgumentNotNull(item, "item");

            if (item.TemplateID == TemplateIDs.Template)
            {
                TemplateNode templateConfigNode = _configManager.GetTemplateNode(item.ID);

                if (templateConfigNode.Generate)
                {

                    ClassySCConfig config = ConfigManager.GetClassySCConfig();


                    ClassGraph graph = new ClassGraph(new TemplateBuildConfig(item));

                    string classDesc = string.Format("{0}.{1} : ", graph.BuildConfig.Namespace, graph.ClassName);

                    List<string> inherits = new List<string>();
                    if (graph.BaseClassGraph != null)
                    {
                        inherits.Add(graph.BaseClassGraph.ClassName);
                    }
                    else
                    {
                        inherits.Add(config.StandardTemplateClassName);
                    }

                    inherits.Add("I" + graph.ClassName);

                    foreach (ClassGraph childGraph in graph.InheritedClassGraphs)
                    {
                        if (childGraph.BuildConfig.Generate)
                            inherits.Add("I" + childGraph.ClassName);
                    }

                    if (inherits.Count > 0)
                    {
                        classDesc += string.Join(", ", inherits.ToArray());
                    }
                    treeNode.HeaderStyle = "font-weight:bold";
                    treeNode.ColumnValues["desc"] = classDesc;
                }
                else
                {
                    treeNode.HeaderStyle = "";
                    treeNode.ColumnValues["desc"] = "";
                }
            }
            else if (item.TemplateID == TemplateIDs.TemplateField)
            {
                TemplateFieldItem field = item;

                TemplateNode templateConfigNode = _configManager.GetTemplateNode(field.Template.ID);
                FieldNode fieldConfigNode = templateConfigNode.Fields.SingleOrDefault(tf => tf.ID == field.ID);

                if (templateConfigNode.Generate &&
                    fieldConfigNode != null)
                {
                    FieldGraph fieldGraph = new FieldGraph(item);
                    string fieldName = StringUtil.GetString(fieldGraph.PropertyName, fieldGraph.FieldName.Replace(" ", ""));
                    CodeTypeReference typeRef = BuilderUtil.GetCodeTypeReference(fieldGraph);
                    string fieldType = BuilderUtil.GetTypeOutput(typeRef);

                    if (fieldType.Contains("IEnumerable&lt;") && fieldConfigNode.Observable)
                    {
                        fieldType = fieldType.Replace("IEnumerable&lt;", "ClassyCollection&lt;");
                    }

                    string fieldDesc = string.Format("{0} {1}", fieldType, fieldName);

                    treeNode.HeaderStyle = "font-weight:bold";
                    treeNode.ColumnValues["desc"] = fieldDesc;
                }
                else
                {
                    treeNode.HeaderStyle = "";
                    treeNode.ColumnValues["desc"] = "";
                }
            }
            else if (item.TemplateID == TemplateIDs.TemplateFolder)
            {
                FolderNode folderNode = _configManager.GetFolderNode(item.ID);

                if (!string.IsNullOrEmpty(folderNode.Namespace))
                {
                    treeNode.HeaderStyle = "font-weight:bold";
                    treeNode.ColumnValues["desc"] = string.Format("{{{0}}}", folderNode.Namespace);
                }
                else
                {
                    treeNode.HeaderStyle = "color:#999;";
                    treeNode.ColumnValues["desc"] = "";
                }

            }
            else
            {
                treeNode.HeaderStyle = "color:#999;";
                treeNode.ColumnValues["desc"] = "";
            }



        }

        public static string WidthOf(string innerText, int width)
        {
            System.Web.UI.HtmlControls.HtmlGenericControl span = new System.Web.UI.HtmlControls.HtmlGenericControl("span");
            span.Style.Add(HtmlTextWriterStyle.Width, width.ToString() + "px");
            span.InnerHtml = innerText;
            return HtmlUtil.RenderControl(span);
        }


        public void UpdateNode(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            DataTreeNode nodeByItemID = base.GetNodeByItemID(item.Paths.LongID) as DataTreeNode;
            Assert.IsNotNull(nodeByItemID, typeof(TreeNode));
            this.UpdateColumnValues(nodeByItemID, item);
            this.RefreshNode2(nodeByItemID);
        }

        public virtual void RefreshNode2(TreeNode node)
        {
            if (node != null)
            {
                RenderNode2(node);
            }
        }



        private static void RenderNode2(TreeNode node)
        {
            Assert.ArgumentNotNull(node, "node");
            node.ChildrenOnly = false;
            node.NodeOnly = true;
            string str = HtmlUtil.RenderControl(node);
            SheerResponse.Refresh(node);
            //Sitecore.Context.ClientPage.ClientResponse.Insert(node.ID, "div", str);
            //Sitecore.Context.ClientPage.ClientResponse.Remove(node.ID);
        }


    }




}

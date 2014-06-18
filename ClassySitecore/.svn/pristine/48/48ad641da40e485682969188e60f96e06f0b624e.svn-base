using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ClassySC.Builder.Builder;
using ClassySC.Builder.Configuration;
using ClassySC.Builder.Graph;
using Microsoft.CSharp;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.IO;

namespace ClassySC.Builder
{
    public class BuildManager
    {

        /// <summary>
        /// sitecore db
        /// </summary>
        private Database _db;

        /// <summary>
        /// cache for const names
        /// </summary>
        Dictionary<ID, string> _constCache;

        /// <summary>
        /// configuration Item for classysc settings
        /// </summary>
        private Item _classySCConfig;



        /// <summary>
        /// determines whether the file will be written or just keep in memory
        /// </summary>
        public bool WriteToFile { get { return _writeToFile; } set { _writeToFile = value; } }
        bool _writeToFile = false;


        /// <summary>
        /// Construct Build Manager
        /// </summary>
        /// <param name="db"></param>
        public BuildManager(Database db)
        {
            _db = db;
            _constCache = new Dictionary<ID, string>();
            _classySCConfig = _db.GetItem(BuilderConst.ClassySCConfigPath);
        }


        /// <summary>
        /// Generate code for all templates
        /// </summary>
        public void Generate()
        {
            List<ClassGraph> templates = GetTemplates();

            CacheConstants(templates);

            foreach (ClassGraph classGraph in templates)
            {
                GenerateTemplateCode(classGraph);
            }
        }

        /// <summary>
        /// Generate code for a list of templates
        /// </summary>
        /// <param name="templateIds"></param>
        public void Generate(List<ID> templateIds)
        {
            List<ClassGraph> templates = GetTemplates();

            //generate constants from all templates
            CacheConstants(templates);

            foreach (ClassGraph classGraph in templates)
            {
                // if the template is in the list
                if (templateIds.Contains(classGraph.TemplateId))
                {
                    // generate code to a string
                    string code = GenerateTemplateCode(classGraph);

                    // decide to write string to file
                    if (WriteToFile)
                    {
                        Stream s = FileUtil.OpenFileStream(classGraph.BuildConfig.FilePath, FileMode.Create, FileAccess.Write, FileShare.Inheritable, true, true); //File.Open(classGraph.BuildConfig.FilePath, FileMode.Create);
                        StreamWriter sw = new StreamWriter(s);
                        sw.Write(code);
                        sw.Close();
                        s.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Get a list of all templates that are set to generate
        /// </summary>
        /// <returns></returns>
        public List<ClassGraph> GetTemplates()
        {
            ConfigManager mgr = ConfigManager.Instance;

            var templateItems = mgr.Templates.Select(tn => tn.GetItem()).ToList();
            List<ClassGraph> templates = templateItems.Select(ti => new ClassGraph(new TemplateBuildConfig(ti))).ToList();
            templates = templates.OrderBy(cg => cg.BuildConfig.Namespace + "." + cg.ClassName).ToList();

            return templates;
        }




        /// <summary>
        /// Generate code for a single template
        /// </summary>
        /// <param name="classGraph"></param>
        /// <returns></returns>
        public string GenerateTemplateCode(ClassGraph classGraph)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CodeGeneratorOptions genOptions = new CodeGeneratorOptions();

            CodeNamespace @namespace = new CodeNamespace(classGraph.BuildConfig.Namespace);

            // generate interface code
            @namespace.Types.Add(GenerateInterface(classGraph));

            // generate class code
            @namespace.Types.Add(GeneratePartialClass(classGraph));

            Stream s = new MemoryStream();
            StreamWriter sw = new StreamWriter(s);

            StringBuilder sb = new StringBuilder();
            StringWriter strw = new StringWriter(sb);

            ICodeGenerator codeGenerator = codeProvider.CreateGenerator(sw);


            // add using statements
            string usingStatements = _classySCConfig[BuilderConst.ClassySCConfig.UsingStatement];
            foreach (string usingStatment in usingStatements.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                codeGenerator.GenerateCodeFromCompileUnit(new CodeSnippetCompileUnit(String.Format("using {0};", usingStatment)), sw, genOptions);
            }

            //codeGenerator.GenerateCodeFromNamespace(@constantsNamespace, strw, genOptions);
            codeGenerator.GenerateCodeFromNamespace(@namespace, strw, genOptions);

            string output = sb.ToString();

            // special syntactical candy for nullables
            output = output.Replace("System.Nullable<int>", "int?");
            output = output.Replace("System.Nullable<double>", "double?");
            output = output.Replace("System.Nullable<bool>", "bool?");
            output = output.Replace("System.Nullable<System.DateTime>", "DateTime?");

            // shorten any namespaces that were declared in "usings"
            //foreach (string usingStatment in
            //    usingStatements.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).OrderByDescending(s1 => s1))
            //{
            //    output = output.Replace(usingStatment + ".", "");
            //}


            sw.Write(output);
            sw.Flush();

            s.Flush();
            s.Position = 0;

            StreamReader sr = new StreamReader(s);
            output = sr.ReadToEnd();

            s.Close();
            sw.Close();

            return output;
        }

        /// <summary>
        /// Generate the Interface code for a template
        /// </summary>
        /// <param name="classGraph"></param>
        /// <returns></returns>
        public CodeTypeDeclaration GenerateInterface(ClassGraph classGraph)
        {

            CodeTypeDeclaration @interface = new CodeTypeDeclaration();
            @interface.Name = "I" + classGraph.ClassName;
            @interface.IsInterface = true;
            @interface.IsPartial = true;
            @interface.TypeAttributes = TypeAttributes.Interface | TypeAttributes.Public;

            // for each field, make a property interface reference
            foreach (FieldGraph fieldGraph in classGraph.FieldGraphs)
            {
                CodeMemberProperty fieldProperty = new CodeMemberProperty();

                fieldProperty.Type = BuilderUtil.GetCodeTypeReference(fieldGraph);
                fieldProperty.Name = StringUtil.GetString(new string[] { fieldGraph.PropertyName, fieldGraph.FieldName });
                fieldProperty.HasGet = true;
                fieldProperty.HasSet = true;
                if (!string.IsNullOrEmpty(fieldGraph.Comments))
                {
                    fieldProperty.Comments.Add(new CodeCommentStatement("<summary>", true));
                    fieldProperty.Comments.Add(new CodeCommentStatement(fieldGraph.Comments, true));
                    fieldProperty.Comments.Add(new CodeCommentStatement("</summary>", true));
                }

                if (fieldGraph.IsObservable)
                {
                    CodeTypeReference classyCollectionTypeRef = new CodeTypeReference("ClassyCollection");
                    classyCollectionTypeRef.TypeArguments.Add(fieldProperty.Type.TypeArguments.Cast<CodeTypeReference>().FirstOrDefault());

                    fieldProperty.Type = classyCollectionTypeRef;
                }

                @interface.Members.Add(fieldProperty);
            }


            return @interface;
        }

        /// <summary>
        /// sets the [Field("{ID}")] attribute
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CodeAttributeDeclaration GenerateFieldAttribute(ID id)
        {
            return new CodeAttributeDeclaration(
                        new CodeTypeReference("Field"),
                        new CodeAttributeArgument[] { 
                            new CodeAttributeArgument(new CodeArgumentReferenceExpression(_constCache[id])) });
        }

        /// <summary>
        /// sets the [Template("{ID}")] attribute
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CodeAttributeDeclaration GenerateTemplateAttribute(ID id)
        {
            return new CodeAttributeDeclaration(
                        new CodeTypeReference("Template"),
                        new CodeAttributeArgument[] { 
                            new CodeAttributeArgument(new CodeArgumentReferenceExpression(_constCache[id])) });
        }

        /// <summary>
        /// Generate Class Code for a template
        /// </summary>
        /// <param name="classGraph"></param>
        /// <returns></returns>
        public CodeTypeDeclaration GeneratePartialClass(ClassGraph classGraph)
        {
            CodeTypeDeclaration @class = new CodeTypeDeclaration();
            @class.Name = classGraph.ClassName;
            @class.IsClass = true;
            @class.IsPartial = true;
            @class.TypeAttributes = TypeAttributes.Public;

            @class.CustomAttributes.Add(GenerateTemplateAttribute(classGraph.TemplateId));

            // determine baseclass, either as specified or default to StandardTemplate
            string baseClassName = _classySCConfig[BuilderConst.ClassySCConfig.StandardTemplateClassName];
            if (classGraph.BaseClassGraph != null)
                baseClassName = BuilderUtil.GetFullTypeName(classGraph.BaseClassGraph.TemplateItem);
            @class.BaseTypes.Add(new CodeTypeReference(baseClassName));


            // generate a list of inherited templates to make interfaces from
            List<ClassGraph> classGraphs = GetClassGraphList(classGraph, false);

            // add interfaces of inherited templates
            foreach (ClassGraph interfaceClassGraph in classGraphs)
            {
                @class.BaseTypes.Add(new CodeTypeReference(BuilderUtil.GetClassNamespace(interfaceClassGraph.TemplateItem) + "." + "I" + interfaceClassGraph.ClassName));
            }

            // generate a constructor
            CodeConstructor @constructor = new CodeConstructor();
            @constructor.Attributes = MemberAttributes.Public;
            CodeParameterDeclarationExpression innerItemParam = new CodeParameterDeclarationExpression("Item", "innerItem");
            @constructor.Parameters.Add(innerItemParam);
            @constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("innerItem"));

            @class.Members.Add(@constructor);


            //Add Field and Tempalte IDs

            CodeMemberField templateIdField = new CodeMemberField(typeof(string), "TemplateID");
            templateIdField.Attributes = MemberAttributes.Public | MemberAttributes.Const | MemberAttributes.New;
            templateIdField.InitExpression = new CodePrimitiveExpression(classGraph.TemplateId.ToString());

            @class.Members.Add(templateIdField);

            List<FieldGraph> fieldGraphs = new List<FieldGraph>();
            Template template = TemplateManager.GetTemplate(classGraph.TemplateId, _db);

            foreach (TemplateField templateField in template.GetFields(false))
            {
                Item fieldItem = _db.GetItem(templateField.ID);
                FieldGraph fieldGraph = new FieldGraph(fieldItem);
                string memberName = BuilderUtil.FormatConstName(fieldGraph.FieldName) + "_FID";
                CodeMemberField memberField = new CodeMemberField(typeof(string), memberName);
                memberField.Attributes = MemberAttributes.Public | MemberAttributes.Const;
                memberField.InitExpression = new CodePrimitiveExpression(fieldGraph.FieldID.ToString());
                @class.Members.Add(memberField);
                //_constCache.Add(fieldGraph.FieldID, string.Format("{0}.{1}.{2}", _classySCConfig[BuilderConst.ClassySCConfig.ConstClassName], classGraph.ClassName, memberField.Name));
            }


            // for each inherited template, implement it's interface members
            List<FieldGraph> inheritedFieldGraphs = GetClassFieldList(classGraph, false);

            foreach (FieldGraph fieldGraph in inheritedFieldGraphs)
            {
                GenerateProperty(@class, fieldGraph);
            }

            // add code region "Members"
            CodeMemberField firstMember = @class.Members.OfType<CodeMemberField>().FirstOrDefault();
            if (firstMember != null)
            {
                firstMember.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Members"));

                CodeTypeMember lastMember = @class.Members.OfType<CodeMemberField>().LastOrDefault();
                if (lastMember != null)
                {
                    lastMember.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, string.Empty));
                }
            }

            // add code region "Properties"
            CodeMemberProperty firstProperty = @class.Members.OfType<CodeMemberProperty>().FirstOrDefault();
            if (firstProperty != null)
            {
                firstProperty.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Properties"));
                CodeMemberProperty lastProperty = @class.Members.OfType<CodeMemberProperty>().LastOrDefault();
                if (lastProperty != null)
                {
                    lastProperty.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, string.Empty));
                }
            }

            // add code region "Constructors"
            CodeConstructor firstConstructor = @class.Members.OfType<CodeConstructor>().FirstOrDefault();
            if (firstConstructor != null)
            {
                firstConstructor.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Constructors"));
                CodeConstructor lastConstructor = @class.Members.OfType<CodeConstructor>().LastOrDefault();
                if (lastConstructor != null)
                {
                    lastConstructor.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, string.Empty));
                }
            }

            return @class;
        }

        /// <summary>
        /// generate the property for a field
        /// </summary>
        /// <param name="class"></param>
        /// <param name="fieldGraph"></param>
        public void GenerateProperty(CodeTypeDeclaration @class, FieldGraph fieldGraph)
        {

            CodeMemberField privateVar = null;

            CodeMemberProperty fieldProperty = new CodeMemberProperty();
            Type fieldType = BuilderUtil.GetFieldType(fieldGraph);
            string typeName = BuilderUtil.FormatTypeName(fieldType);
            fieldProperty.CustomAttributes.Add(GenerateFieldAttribute(fieldGraph.FieldID));
            fieldProperty.Type = BuilderUtil.GetCodeTypeReference(fieldGraph);
            fieldProperty.Attributes = MemberAttributes.Public;
            fieldProperty.Name = StringUtil.GetString(new string[] { fieldGraph.PropertyName, fieldGraph.FieldName });
            fieldProperty.HasGet = true;
            fieldProperty.HasSet = true;

            // add comments
            if (!string.IsNullOrEmpty(fieldGraph.Comments))
            {
                fieldProperty.Comments.Add(new CodeCommentStatement("<summary>", true));
                fieldProperty.Comments.Add(new CodeCommentStatement(fieldGraph.Comments, true));
                fieldProperty.Comments.Add(new CodeCommentStatement("</summary>", true));
            }

            string fullTypeName = fieldType.FullName;

            // decide on get/set methods per type
            if (fieldType == typeof(int?))
            {
                privateVar = new CodeMemberField(fieldType, BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                BasicGetSetStatments(fieldGraph, fieldProperty, typeName, privateVar.Name, "GetInt", "SetInt");
            }
            else if (fieldType == typeof(double?))
            {
                privateVar = new CodeMemberField(fieldType, BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                BasicGetSetStatments(fieldGraph, fieldProperty, typeName, privateVar.Name, "GetDouble", "SetDouble");
            }
            else if (fieldType == typeof(DateTime?))
            {
                privateVar = new CodeMemberField(fieldType, BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                BasicGetSetStatments(fieldGraph, fieldProperty, typeName, privateVar.Name, "GetDateTime", "SetDateTime");
            }
            else if (fieldType == typeof(bool?))
            {
                privateVar = new CodeMemberField(fieldType, BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                BasicGetSetStatments(fieldGraph, fieldProperty, typeName, privateVar.Name, "GetBool", "SetBool");
            }
            else if (fullTypeName == "Sitecore.Data.Items.Item" || fullTypeName == "Sitecore.Data.Fields.ReferenceField" || fullTypeName == "Sitecore.Data.Fields.LookupField")
            {
                // for any fields who representing a single Item

                // if this field is set to a custom PropertyType
                if (!string.IsNullOrEmpty(fieldGraph.PropertyType))
                {
                    privateVar = new CodeMemberField(fieldGraph.PropertyType, BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                    BasicGetSetStatments(fieldGraph, fieldProperty, fieldGraph.PropertyType, privateVar.Name, string.Format("GetClassObject<{0}>", fieldGraph.PropertyType), string.Format("SetClassObject<{0}>", fieldGraph.PropertyType));
                }
                else // user regular item accessors
                {
                    privateVar = new CodeMemberField("Item", BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                    BasicGetSetStatments(fieldGraph, fieldProperty, typeName, privateVar.Name, "GetItem", "SetItem");
                }
            }
            else if (fieldType.IsSubclassOf(typeof(CustomField)))
            {
                // for any custom field types

                fieldProperty.GetStatements.Add(new CodeSnippetExpression(string.Format("return this.GetField<{0}>({1})", fieldType.Name, _constCache[fieldGraph.FieldID])));
                fieldProperty.SetStatements.Add(new CodeSnippetExpression(string.Format("this.SetField<{0}>({1}, value)", fieldType.Name, _constCache[fieldGraph.FieldID])));
            }
            else if (fullTypeName.Contains("IEnumerable") && !string.IsNullOrEmpty(fieldGraph.PropertyType))
            {
                // for any fields representing a list of Items  with a custom PropertyType

                string outerTypeName = fieldGraph.PropertyType;
                string innerTypeName = fieldProperty.Type.TypeArguments[0].BaseType;
                string getMethod = "GetClassObjects<{0}>";
                string setMethod = "SetClassObjects<{0}>";

                if (fieldGraph.IsObservable)
                {
                    CodeTypeReference classyCollectionTypeRef = new CodeTypeReference("ClassyCollection");
                    classyCollectionTypeRef.TypeArguments.Add(new CodeTypeReference(innerTypeName));

                    fieldProperty.Type = classyCollectionTypeRef;

                    getMethod = "GetClassObjectsObservable<{0}>";
                    outerTypeName = outerTypeName.Replace("IEnumerable<", "ClassyCollection<");
                    privateVar = new CodeMemberField(outerTypeName, BuilderUtil.FormatPrivateName(fieldGraph.FieldName));

                    BasicGetSetStatments(fieldGraph, fieldProperty, outerTypeName, privateVar.Name, string.Format(getMethod, innerTypeName), string.Format(setMethod, innerTypeName));
                }
                else
                {
                    privateVar = new CodeMemberField(outerTypeName, BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                    BasicGetSetStatments(fieldGraph, fieldProperty, outerTypeName, privateVar.Name, string.Format(getMethod, innerTypeName), string.Format(setMethod, innerTypeName));
                }
            }
            else if (fullTypeName.Contains("IEnumerable") && fullTypeName.Contains("Sitecore.Data.Items.Item"))
            {
                // for any fields representing a list of Items

                string getMethod = "GetItems";
                string setMethod = "SetItems";

                if (fieldGraph.IsObservable)
                {
                    CodeTypeReference classyCollectionTypeRef = new CodeTypeReference("ClassyCollection");
                    classyCollectionTypeRef.TypeArguments.Add(new CodeTypeReference("Item"));

                    fieldProperty.Type = classyCollectionTypeRef;

                    getMethod = "GetItemsObservable";
                    privateVar = new CodeMemberField("ClassyCollection<Item>", BuilderUtil.FormatPrivateName(fieldGraph.FieldName));

                    BasicGetSetStatments(fieldGraph, fieldProperty, typeName, privateVar.Name, getMethod, setMethod);
                }
                else
                {
                    privateVar = new CodeMemberField("IEnumerable<Item>", BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                    BasicGetSetStatments(fieldGraph, fieldProperty, typeName, privateVar.Name, getMethod, setMethod);
                }
            }

            else if (!string.IsNullOrEmpty(fieldGraph.PropertyType))
            {
                // for any fields representing an Item with custom PropertyType
                privateVar = new CodeMemberField(fieldGraph.PropertyType, BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                BasicGetSetStatments(fieldGraph, fieldProperty, fieldGraph.PropertyType, privateVar.Name, string.Format("GetClassObject<{0}>", fieldGraph.PropertyType), string.Format("SetClassObject<{0}>", fieldGraph.PropertyType));
            }
            else
            {
                // default to a string type
                privateVar = new CodeMemberField(typeof(string), BuilderUtil.FormatPrivateName(fieldGraph.FieldName));
                BasicGetSetStatments(fieldGraph, fieldProperty, typeName, privateVar.Name, "GetString", "SetString");
            }

            // add private member field for property
            if (privateVar != null)
                @class.Members.Add(privateVar);
            @class.Members.Add(fieldProperty);
        }

        /// <summary>
        /// Create property Get/Set statements
        /// </summary>
        /// <param name="fieldGraph"></param>
        /// <param name="fieldProperty"></param>
        /// <param name="typeName"></param>
        /// <param name="privateVarName"></param>
        /// <param name="getMethodName"></param>
        /// <param name="setMethodName"></param>
        private void BasicGetSetStatments(FieldGraph fieldGraph, CodeMemberProperty fieldProperty, string typeName, string privateVarName, string getMethodName, string setMethodName)
        {

            if (!string.IsNullOrEmpty(privateVarName))
            {
                CodeConditionStatement nullCheck = new CodeConditionStatement(
                    new CodeSnippetExpression(string.Format("{0} == null", privateVarName)),
                    new CodeSnippetStatement(string.Format("{0} = this.{1}({2});", privateVarName, getMethodName, _constCache[fieldGraph.FieldID])));
                fieldProperty.GetStatements.Add(nullCheck);
                fieldProperty.GetStatements.Add(new CodeSnippetExpression(string.Format("return {0}", privateVarName)));
            }
            else
            {
                fieldProperty.GetStatements.Add(new CodeSnippetExpression(string.Format("return this.{0}({1})", setMethodName, _constCache[fieldGraph.FieldID])));
            }

            // property backed by a private field member.
            if (!string.IsNullOrEmpty(privateVarName))
            {
                // clears out private member when setting a field
                fieldProperty.SetStatements.Add(new CodeSnippetExpression(string.Format("{0} = null", privateVarName)));
            }
            fieldProperty.SetStatements.Add(new CodeSnippetExpression(string.Format("this.{0}({1}, value)", setMethodName, _constCache[fieldGraph.FieldID])));
        }




        /// <summary>
        /// Caches mappings of {ID} => [ConstNameSpace.ConstClassName.[TemplateName].[Template/FieldID]
        /// for later use
        /// </summary>
        /// <param name="classGraphs"></param>
        public void CacheConstants(List<ClassGraph> classGraphs)
        {
            _constCache.Clear();
            ClassySCConfig config = new ClassySCConfig(_classySCConfig);

            foreach (ClassGraph classGraph in classGraphs)
            {
                string classType = BuilderUtil.GetFullTypeName(classGraph.TemplateItem);

                _constCache.Add(classGraph.TemplateId, string.Format("{0}.{1}", classType, "TemplateID"));

                Template template = TemplateManager.GetTemplate(classGraph.TemplateId, _db);

                foreach (TemplateField templateField in template.GetFields(false))
                {
                    Item fieldItem = _db.GetItem(templateField.ID);
                    FieldGraph fieldGraph = new FieldGraph(fieldItem);
                    string memberName = BuilderUtil.FormatConstName(fieldGraph.FieldName) + "_FID";
                    _constCache.Add(fieldGraph.FieldID, string.Format("{0}.{1}", classType, memberName));
                }
            }
        }



        /// <summary>
        /// Get a graph of template/inherited templates, flattened and distinct
        /// </summary>
        /// <param name="classGraph"></param>
        /// <returns></returns>
        public List<ClassGraph> FlattenClassGraphs(ClassGraph classGraph)
        {
            List<ClassGraph> output = new List<ClassGraph>();

            output.Add(classGraph);

            if (classGraph.BaseClassGraph != null && classGraph.BaseClassGraph.TemplateId != TemplateIDs.StandardTemplate)
            {
                output.AddRange(FlattenClassGraphs(classGraph.BaseClassGraph));
            }

            foreach (ClassGraph inheritedClassGraph in classGraph.InheritedClassGraphs)
            {
                output.AddRange(FlattenClassGraphs(inheritedClassGraph));
            }

            output = output.Distinct().ToList();

            return output;
        }

        /// <summary>
        /// Get a list of graphs/childgraphs for a classgraph
        /// </summary>
        /// <param name="classGraph"></param>
        /// <param name="includeBaseClass">Include all graphs of BaseClassGraph</param>
        /// <returns></returns>
        public List<ClassGraph> GetClassGraphList(ClassGraph classGraph, bool includeBaseClass)
        {
            List<ClassGraph> output = new List<ClassGraph>();

            // add self
            output.Add(classGraph);

            // add inherited
            foreach (ClassGraph inheritedClassGraph in classGraph.InheritedClassGraphs)
            {
                output.AddRange(GetClassGraphList(inheritedClassGraph, true));
            }

            // if baseClass declared add or remove baseClass's child graphs
            if (classGraph.BaseClassGraph != null)
            {
                List<ClassGraph> baseClassGraphs = GetClassGraphList(classGraph.BaseClassGraph, true);
                if (includeBaseClass)
                {
                    output.AddRange(baseClassGraphs);
                }
                else
                {
                    foreach (ClassGraph baseClassClassGraph in baseClassGraphs)
                    {
                        output.Remove(baseClassClassGraph);
                    }
                }
            }
            output = output.Distinct().ToList();

            return output;
        }

        /// <summary>
        /// Get a list of FieldGraphs for classGraph
        /// </summary>
        /// <param name="classGraph"></param>
        /// <param name="includeBaseClass">Include FieldGraphs of BaseClassGraph</param>
        /// <returns></returns>
        public List<FieldGraph> GetClassFieldList(ClassGraph classGraph, Boolean includeBaseClass)
        {
            List<FieldGraph> output = new List<FieldGraph>();

            output.AddRange(classGraph.FieldGraphs);

            foreach (ClassGraph inheritedClassGraph in classGraph.InheritedClassGraphs)
            {
                output.AddRange(GetClassFieldList(inheritedClassGraph, true));
            }

            output = output.Distinct().ToList();

            if (classGraph.BaseClassGraph != null)
            {
                List<FieldGraph> baseFieldGraphs = GetClassFieldList(classGraph.BaseClassGraph, true);
                if (includeBaseClass)
                {
                    output.AddRange(baseFieldGraphs);
                }
                else
                {
                    foreach (FieldGraph baseFieldGraph in baseFieldGraphs)
                    {
                        output.Remove(baseFieldGraph);
                    }
                }
            }

            output = output.Distinct().ToList();

            return output;
        }










    }
}

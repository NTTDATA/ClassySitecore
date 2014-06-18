using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore;
using Sitecore.Data.Items;
using ClassySC.Builder.Configuration;
using Sitecore.IO;
using Sitecore.Data;
using Sitecore.Diagnostics;
using System.CodeDom;
using ClassySC.Builder.Graph;
using System.Text.RegularExpressions;
using Sitecore.Data.Fields;
using System.Globalization;

namespace ClassySC.Builder
{
    public class BuilderUtil
    {

        /// <summary>
        /// Force template names to PascalCase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FormatPublicName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            if (input.Contains(" "))
            {
                StringBuilder sb = new StringBuilder();
                foreach (string word in StringUtil.Split(input, ' ', true))
                {
                    sb.Append(CapitalizeWord(word));
                }
                return sb.ToString();
            }
            else
            {
                return CapitalizeWord(input);
            }

        }

        /// <summary>
        /// Capitalize first letter
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CapitalizeWord(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            char[] outputArr = input.ToCharArray();
            outputArr[0] = char.ToUpper(outputArr[0]);
            return new string(outputArr);
        }

        /// <summary>
        /// format ConstName
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FormatConstName(string input)
        {
            string output = FormatPublicName(input);

            //StringBuilder sb = new StringBuilder();
            //char[] charArray = output.ToCharArray();
            //for (int i = 0; i < charArray.Length; i++)
            //{
            //    char chr = charArray[i];
            //    if (chr.Equals(Char.ToUpper(chr)))
            //    {
            //        if (i > 0 && !charArray[i - 1].Equals(char.ToUpper(charArray[i - 1])))
            //            sb.Append("_");
            //    }
            //    sb.Append(char.ToUpper(chr));
            //}
            //return sb.ToString();

            return output;
        }

        /// <summary>
        /// format private variableName _camelCase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FormatPrivateName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            string output = FormatPublicName(input);  // to PascalCase
            char[] outputArr = output.ToCharArray();
            outputArr[0] = Char.ToLower(outputArr[0]);  // to camelCase

            return "_" + new string(outputArr); // to _camelCase
        }


        /// <summary>
        /// Gets a Type from a field graph
        /// </summary>
        /// <param name="fieldGraph"></param>
        /// <returns></returns>
        public static Type GetFieldType(FieldGraph fieldGraph)
        {
            // get the sitecore fieldType mapped to this field.
            FieldType fieldType = FieldTypeManager.GetFieldType(fieldGraph.FieldType);

            // Number becomes double?
            if (fieldGraph.FieldType == "Number")
            {
                return typeof(double?);
            }

            //Integer becomes int?
            if (fieldGraph.FieldType == "Integer")
            {
                return typeof(int?);
            }


            // choose type based on Sitecore field types
            if (fieldType != null)
            {
                switch (fieldType.Type.FullName)
                {
                    case "Sitecore.Data.Fields.TextField":
                    case "Sitecore.Data.Fields.HtmlField":
                    case "Sitecore.Data.Fields.ValueLookupField":
                        return typeof(string);
                    case "Sitecore.Data.Fields.DateField":
                        return typeof(Nullable<DateTime>);
                    case "Sitecore.Data.Fields.LookupField":
                    case "Sitecore.Data.Fields.ReferenceField":
                    case "Sitecore.Data.Fields.GroupedDroplinkField":
                    case "Sitecore.Data.Fields.GroupedDroplistField":
                        return typeof(Item);
                    case "Sitecore.Data.Fields.MultilistField":
                        return typeof(IEnumerable<Item>);
                    case "Sitecore.Data.Fields.CheckboxField":
                        return typeof(bool?);
                    default:
                        break;
                }
                return fieldType.Type;
            }
            return typeof(string);
        }

        public static string GetClassNamespace(Item template)
        {
            string output = "";
            ConfigManager mgr = ConfigManager.Instance;


            Item templateFolder = template.Parent;

            List<string> names = new List<string>();
            bool flag = false;
            while (templateFolder.ID != ItemIDs.TemplateRoot)
            {
                FolderNode folderConfigNode = mgr.GetFolderNode(templateFolder.ID);

                if (!string.IsNullOrEmpty(folderConfigNode.Namespace))
                {
                    names.Add(folderConfigNode.Namespace);
                    flag = true;
                    break;
                }
                else
                {
                    names.Add(FormatPublicName(templateFolder.Name));
                }
                templateFolder = templateFolder.Parent;
            }

            if (!flag)
            {
                return ConfigManager.GetClassySCConfig().DefaultNamespace;
            }

            names.Reverse();
            output = StringUtil.Join(names, ".");
            return output;
        }

        
        public static string GetFolderFilePath(Item item)
        {
            ConfigManager mgr = ConfigManager.Instance;


            Item templateFolder = null;
            if (item.TemplateID == TemplateIDs.TemplateFolder)
                templateFolder = item;
            else
                templateFolder = item.Parent;


            List<string> segments = new List<string>();
            bool flag = false;
            while (templateFolder.ID != ItemIDs.TemplateRoot)
            {
                FolderNode folderConfigNode = mgr.GetFolderNode(templateFolder.ID);

                if (!string.IsNullOrEmpty(folderConfigNode.FolderFilePath))
                {
                    segments.Add(folderConfigNode.FolderFilePath);
                    flag = true;
                    break;
                }
                else
                {
                    segments.Add(FormatPublicName(templateFolder.Name));
                }
                templateFolder = templateFolder.Parent;
            }

            if (!flag)
            {
                return ConfigManager.GetClassySCConfig().DefaultFilePath;
            }

            segments.Reverse();

            string output = "";

            foreach (string segment in segments)
            {
                output = FileUtil.MakePath(output, segment);
            }

            return output;
        }

        public static string GetTemplateFilePath(Item item)
        {
            ConfigManager mgr = ConfigManager.Instance;

            TemplateNode node = mgr.GetTemplateNode(item);

            string folderPath = GetFolderFilePath(item.Parent);
            string filename = string.Format("{0}.cs", StringUtil.GetString(node.ClassName, FormatPublicName(item.Name)));


            return FileUtil.MakePath(folderPath, filename);
        }

        public static string GetFullTypeName(ID id)
        {
            Database db = ConfigManager.GetContentDB;
            return GetFullTypeName(db.GetItem(id));
        }

        public static string GetFullTypeName(Item item)
        {
            Assert.IsTrue(item.TemplateID == TemplateIDs.Template, "Not a template item");
            if (item.ID == TemplateIDs.StandardTemplate)
            {
                return "ClassySC.Data.StandardTemplate";
            }
            
            ConfigManager mgr = ConfigManager.Instance;
            string className = mgr.GetTemplateNode(item.ID).ClassName;
            if (string.IsNullOrEmpty(className))
                className = FormatPublicName(item.Name);
            return GetClassNamespace(item) + "." + className;
        }


        /// <summary>
        /// Get CodeTypeReference for FieldGraphs
        /// </summary>
        /// <param name="fieldGraph"></param>
        /// <returns></returns>
        public static CodeTypeReference GetCodeTypeReference(FieldGraph fieldGraph)
        {
            if (!string.IsNullOrEmpty(fieldGraph.PropertyType))
            {
                Regex re = new Regex(@"(\w+)<([\w\.]+)>");
                Match match = re.Match(fieldGraph.PropertyType);
                if (match.Success) // if this is a generic, create inner/outer code refs
                {
                    CodeTypeReference customTypeRef = new CodeTypeReference(match.Groups[1].Value);
                    customTypeRef.TypeArguments.Add(new CodeTypeReference(match.Groups[2].Value));
                    return customTypeRef;
                }
                else
                {
                    return new CodeTypeReference(fieldGraph.PropertyType);
                }
            }

            Type type = GetFieldType(fieldGraph);
            // convert Type to CodeTypeReference
            if (type == typeof(DateTime?))
            {
                CodeTypeReference typeRef = new CodeTypeReference(typeof(Nullable));
                typeRef.TypeArguments.Add(new CodeTypeReference(typeof(DateTime)));
                return typeRef;
            }
            else if (type == typeof(int?))
            {
                CodeTypeReference typeRef = new CodeTypeReference(typeof(Nullable));
                typeRef.TypeArguments.Add(new CodeTypeReference(typeof(int)));
                return typeRef;
            }
            else if (type == typeof(double?))
            {
                CodeTypeReference typeRef = new CodeTypeReference(typeof(Nullable));
                typeRef.TypeArguments.Add(new CodeTypeReference(typeof(double)));
                return typeRef;
            }
            else if (type == typeof(bool?))
            {
                CodeTypeReference typeRef = new CodeTypeReference(typeof(Nullable));
                typeRef.TypeArguments.Add(new CodeTypeReference(typeof(bool)));
                return typeRef;
            }
            else if (type.FullName == "System.String")
            {
                return new CodeTypeReference(typeof(string));
            }
            else if (type.FullName == "Sitecore.Data.Items.Item")
            {
                return new CodeTypeReference("Item");
            }
            return new CodeTypeReference(type);
        }

        /// <summary>
        /// just some visual formatting
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FormatTypeName(Type type)
        {
            if (type.Name == "String")
                return "string";
            if (type.Name == "Int32")
                return "int";
            if (type.Name == "Nullable")
            {
                if (type.FullName.Contains("DateTime"))
                {
                    return "DateTime?";
                }
                if (type.FullName.Contains("boolean"))
                {
                    return "bool?";
                }
            }


            return type.Name;

        }






        public static string GetTypeOutput(CodeTypeReference typeRef)
        {
            string str = string.Empty;
            CodeTypeReference arrayElementType = typeRef;
            while (arrayElementType.ArrayElementType != null)
            {
                arrayElementType = arrayElementType.ArrayElementType;
            }
            str = str + BuilderUtil.GetBaseTypeOutput(arrayElementType);
            while ((typeRef != null) && (typeRef.ArrayRank > 0))
            {
                char[] chArray = new char[typeRef.ArrayRank + 1];
                chArray[0] = '[';
                chArray[typeRef.ArrayRank] = ']';
                for (int i = 1; i < typeRef.ArrayRank; i++)
                {
                    chArray[i] = ',';
                }
                str = str + new string(chArray);
                typeRef = typeRef.ArrayElementType;
            }
            return str;
        }

        public static string GetBaseTypeOutput(CodeTypeReference typeRef)
        {
            string baseType = typeRef.BaseType;
            if (baseType.Length == 0)
            {
                return "void";
            }
            switch (baseType.ToLower(CultureInfo.InvariantCulture))
            {
                case "system.int16":
                    return "short";

                case "system.int32":
                    return "int";

                case "system.int64":
                    return "long";

                case "system.string":
                    return "string";

                case "system.object":
                    return "object";

                case "system.boolean":
                    return "bool";

                case "system.void":
                    return "void";

                case "system.char":
                    return "char";

                case "system.byte":
                    return "byte";

                case "system.uint16":
                    return "ushort";

                case "system.uint32":
                    return "uint";

                case "system.uint64":
                    return "ulong";

                case "system.sbyte":
                    return "sbyte";

                case "system.single":
                    return "float";

                case "system.double":
                    return "double";

                case "system.decimal":
                    return "decimal";
            }
            StringBuilder sb = new StringBuilder(baseType.Length + 10);
            if (typeRef.Options == CodeTypeReferenceOptions.GlobalReference)
            {
                sb.Append("global::");
            }
            string str3 = typeRef.BaseType;
            int startIndex = 0;
            int start = 0;
            for (int i = 0; i < str3.Length; i++)
            {
                switch (str3[i])
                {
                    case '+':
                    case '.':
                        //sb.Append(str3.Substring(startIndex, i - startIndex));
                        //sb.Append('.');
                        i++;
                        startIndex = i;
                        break;

                    case '`':
                        {
                            sb.Append(str3.Substring(startIndex, i - startIndex));
                            i++;
                            int length = 0;
                            while (((i < str3.Length) && (str3[i] >= '0')) && (str3[i] <= '9'))
                            {
                                length = (length * 10) + (str3[i] - '0');
                                i++;
                            }
                            BuilderUtil.GetTypeArgumentsOutput(typeRef.TypeArguments, start, length, sb);
                            start += length;
                            if ((i < str3.Length) && ((str3[i] == '+') || (str3[i] == '.')))
                            {
                                sb.Append('.');
                                i++;
                            }
                            startIndex = i;
                            break;
                        }
                }
            }
            if (startIndex < str3.Length)
            {
                sb.Append(str3.Substring(startIndex));
            }

            string output = sb.ToString();
            Regex nullableReg = new Regex(@"Nullable&lt;(\w+)&gt;");
            Match match = nullableReg.Match(output);
            if (match.Success)
            {
                output = string.Format("{0}?", match.Groups[1].Value);
            }
            return output;
        }



        protected static void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb)
        {
            sb.Append("&lt;");
            bool flag = true;
            for (int i = start; i < (start + length); i++)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    sb.Append(", ");
                }
                if (i < typeArguments.Count)
                {
                    sb.Append(BuilderUtil.GetTypeOutput(typeArguments[i]));
                }
            }
            sb.Append("&gt;");
        }
    }
}

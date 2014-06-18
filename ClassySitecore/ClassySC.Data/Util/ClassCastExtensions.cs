using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Reflection;
using System.Reflection;
using ClassySC.Data;
using ClassySC.Data.Manager;
using Sitecore.Globalization;
using Sitecore.Configuration;

namespace ClassySC.Data
{
    public static class ClassCastExtensions
    {
        public static IEnumerable<T> ToClass<T>(this IEnumerable<Item> items) where T : CustomItemBase
        {
            List<T> output = new List<T>();
            if (items == null)
                return output.AsEnumerable();
            foreach (Item item in items)
            {
                T obj = item.ToClass<T>();
                if (obj != null)
                    output.Add(obj);
            }
            return output.AsEnumerable();

            //return items.Select(itm => itm.ClassCast<T>());
        }

        public static T ToClass<T>(this Item item) where T : CustomItemBase
        {

            if (item == null || (item.Versions.Count == 0 && Settings.GetBoolSetting("ClassySC.AssertVersions", true)))
                return null;
            Type T1 = ResolveType(item, typeof(StandardTemplate));
            var obj = ReflectionUtil.CreateObject(T1, new object[] { item });
            if (obj is T)
                return (T)obj;
            else
                return null;
        }       

        public static T ToClass<T>(this CustomItemBase customItem) where T : CustomItemBase
        {
            if (customItem == null)
                return null;
            Type T1 = ResolveType(customItem.InnerItem, typeof(StandardTemplate));
            var obj = ReflectionUtil.CreateObject(T1, new object[] { customItem.InnerItem });
            if (obj is T)
                return (T)obj;
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">To Type</typeparam>
        /// <typeparam name="U">From Type</typeparam>
        /// <param name="customItems"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToClass<T, U>(this IEnumerable<U> customItems)
            where T : CustomItemBase
            where U : CustomItemBase
        {
            foreach (U customItem in customItems)
            {
                yield return customItem.ToClass<T>();
            }
        }

        public static Type ResolveType(Item item, Type defaultType)
        {
            ID templateId = item.TemplateID;
            Type type = ClassManager.Instance.GetType(templateId);
            if (type != null)
                return type;
            return defaultType;
        }

        public static IEnumerable<T> SelectItems<T>(this Database db, string query) where T : CustomItemBase
        {
            return db.SelectItems(query).ToClass<T>();
        }

        public static T SelectSingleItem<T>(this Database db, string query) where T : CustomItemBase
        {
            return db.SelectSingleItem(query).ToClass<T>();
        }

        public static IEnumerable<T> SelectItems<T>(this ItemAxes axes, string query) where T : CustomItemBase
        {
            return axes.SelectItems(query).ToClass<T>();
        }

        public static T SelectSingleItem<T>(this ItemAxes axes, string query) where T : CustomItemBase
        {
            return axes.SelectSingleItem(query).ToClass<T>();
        }

        public static T GetItem<T>(this Database db, string path) where T : CustomItemBase
        {
            return db.GetItem(path).ToClass<T>();
        }

        public static T GetItem<T>(this Database db, ID id) where T : CustomItemBase
        {
            return db.GetItem(id).ToClass<T>();
        }

        public static T GetItem<T>(this Database db, DataUri uri) where T : CustomItemBase
        {
            return db.GetItem(uri).ToClass<T>();
        }

        public static T GetItem<T>(this Database db, string path, Language language) where T : CustomItemBase
        {
            return db.GetItem(path, language).ToClass<T>();
        }

        public static T GetItem<T>(this Database db, ID id, Language language) where T : CustomItemBase
        {
            return db.GetItem(id, language).ToClass<T>();
        }

        public static T GetItem<T>(this Database db, string path, Language language, Sitecore.Data.Version version) where T : CustomItemBase
        {
            return db.GetItem(path, language, version).ToClass<T>();
        }

        public static T GetItem<T>(this Database db, ID id, Language language, Sitecore.Data.Version version) where T : CustomItemBase
        {
            return db.GetItem(id, language, version).ToClass<T>();
        }




    }
}

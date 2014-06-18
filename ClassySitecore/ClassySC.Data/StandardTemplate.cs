using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using ClassySC.Data;
using Sitecore.Data.Fields;
using Sitecore.Reflection;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Text;

namespace ClassySC.Data
{

    [Template(BaseConst.StandardTemplate.TemplateID)]
    public partial class StandardTemplate : CustomItemBase
    {
        public const string TemplateID = "{1930BBEB-7805-471A-A3BE-4858AC7CF696}";

        public StandardTemplate(Item innerItem)
            : base(innerItem)
        {
        }    

        public static implicit operator StandardTemplate(Item item)
        {
            return new StandardTemplate(item);
        }

        public virtual T GetValue<T>(string fieldID)
        {
            return (T)System.Convert.ChangeType(InnerItem.Fields[new ID(fieldID)].Value, typeof(T));
        }

        public virtual void SetValue<T>(string fieldID, T value)
        {
            InnerItem.Fields[new ID(fieldID)].Value = value.ToString();
        }

        public virtual T GetField<T>(string fieldID) where T : CustomField
        {
            return (T)ReflectionUtil.CreateObject(typeof(T), new object[] { InnerItem.Fields[new ID(fieldID)] });
        }

        public virtual void SetField<T>(string fieldID, T value) where T : CustomField
        {
            if (value == null)
                this.SetString(fieldID, null);
            else
                InnerItem.Fields[new ID(fieldID)].Value = value.Value;
        }

        public virtual string GetString(string fieldID)
        {
            return StringUtil.GetString(InnerItem[new ID(fieldID)]);
        }

        public virtual void SetString(string fieldID, string value)
        {
            Assert.IsNotNull(InnerItem.Fields[new ID(fieldID)], "Invalid Field ID");
            if (value == null)
                InnerItem.Fields[new ID(fieldID)].Reset();
            else
                InnerItem[new ID(fieldID)] = value;
        }

        public virtual bool? GetBool(string fieldID)
        {
            if (string.IsNullOrEmpty(GetString(fieldID)))
                return null;
            return MainUtil.GetBool(InnerItem[new ID(fieldID)], false);
        }

        public virtual void SetBool(string fieldID, bool? value)
        {
            Assert.IsNotNull(InnerItem.Fields[new ID(fieldID)], "Invalid Field ID");
            if (value.HasValue)
                InnerItem[new ID(fieldID)] = MainUtil.BoolToString(value.Value);
            else
                this.SetString(fieldID, null);
        }

        public virtual int? GetInt(string fieldID)
        {
            if (string.IsNullOrEmpty(GetString(fieldID)))
                return null;
            int output;
            if (int.TryParse(GetString(fieldID), out output))
                return output;
            return null;
        }

        public virtual void SetInt(string fieldID, int? value)
        {
            Assert.IsNotNull(InnerItem.Fields[new ID(fieldID)], "Invalid Field ID");
            if (value.HasValue)
                InnerItem[new ID(fieldID)] = value.Value.ToString();
            else
                this.SetString(fieldID, null);
        }

        public virtual double? GetDouble(string fieldID)
        {
            if (string.IsNullOrEmpty(GetString(fieldID)))
                return null;
            double output;
            if (double.TryParse(GetString(fieldID), out output))
                return output;
            return null;
        }

        public virtual void SetDouble(string fieldID, double? value)
        {
            Assert.IsNotNull(InnerItem.Fields[new ID(fieldID)], "Invalid Field ID");
            if (value.HasValue)
                InnerItem[new ID(fieldID)] = value.Value.ToString();
            else
                this.SetString(fieldID, null);
        }

        public virtual DateTime? GetDateTime(string fieldID)
        {
            if (string.IsNullOrEmpty(GetString(fieldID)))
                return null;
            DateField dateField = InnerItem.Fields[new ID(fieldID)];
            return dateField.DateTime;
        }

        public virtual void SetDateTime(string fieldID, DateTime? value)
        {
            Field field = InnerItem.Fields[new ID(fieldID)];
            Assert.IsNotNull(field, "Invalid Field ID");
            DateField dateField = FieldTypeManager.GetField(field) as DateField;
            if (value.HasValue)
                dateField.Value = DateUtil.ToIsoDate(value.Value);
            else
                this.SetString(fieldID, null);
        }

        public virtual Item GetItem(string fieldID)
        {
            Field field = InnerItem.Fields[new ID(fieldID)];
            Assert.IsNotNull(field, "Invalid Field ID");
            if (MainUtil.IsID(field.Value))
            {
                return field.Database.GetItem(field.Value);
            }
            return null;
        }

        public virtual void SetItem(string fieldID, Item value)
        {
            Assert.IsNotNull(InnerItem.Fields[new ID(fieldID)], "Invalid Field ID");
            if (value == null)
                this.SetString(fieldID, null);
            else
                InnerItem[new ID(fieldID)] = value.ID.ToString();
        }

        public virtual IEnumerable<Item> GetItems(string fieldID)
        {
            Field field = InnerItem.Fields[new ID(fieldID)];
            Assert.IsNotNull(field, "Invalid Field ID");
            MultilistField multilistField = FieldTypeManager.GetField(field) as MultilistField;
            if (multilistField == null)
                return Enumerable.Empty<Item>();
            return multilistField.GetItems() as IEnumerable<Item>;
        }

        public virtual ClassyCollection<Item> GetItemsObservable(string fieldID)
        {
            ClassyCollection<Item> output = new ClassyCollection<Item>(GetItems(fieldID));
            this.ObserveItemCollection(fieldID, output);
            return output;
        }

        protected virtual void ObserveItemCollection(string fieldID, ClassyCollection<Item> list)
        {
            Assert.ArgumentNotNull(fieldID, "fieldID");
            Assert.ArgumentNotNull(list, "list");
            list.ListChanged += (object source, ClassyCollection<Item>.ListChangedEventArgs e) =>
            {
                this.SetItems(fieldID, (ClassyCollection<Item>)source);
            };

        }

        public virtual void SetItems(string fieldID, IEnumerable<Item> value)
        {
            Assert.IsNotNull(InnerItem.Fields[new ID(fieldID)], "Invalid Field ID");
            if (value == null)
                this.SetString(fieldID, null);
            else
                InnerItem[new ID(fieldID)] = StringUtil.Join(value.Select(itm => itm.ID.ToString()), "|");
        }

        public virtual T GetClassObject<T>(string fieldID) where T : CustomItemBase
        {
            Item item = this.GetItem(fieldID);
            if (item != null)
                return item.ToClass<T>();
            return null;
        }

        public virtual void SetClassObject<T>(string fieldID, T value) where T : CustomItemBase
        {
            if (value == null)
                this.SetString(fieldID, null);
            else
                this.SetItem(fieldID, value.InnerItem);
        }

        public virtual IEnumerable<T> GetClassObjects<T>(string fieldID) where T : CustomItemBase
        {
            IEnumerable<Item> items = this.GetItems(fieldID);
            if (items != null)
            {
                return items.ToClass<T>();
            }
            return Enumerable.Empty<T>();
        }

        public virtual ClassyCollection<T> GetClassObjectsObservable<T>(string fieldID) where T : CustomItemBase
        {
            IEnumerable<Item> items = this.GetItems(fieldID);
            if (items != null)
            {
                ClassyCollection<T> output = new ClassyCollection<T>(items.ToClass<T>());
                this.ObserveObjectCollection<T>(fieldID, output);
                return output;
            }
            return new ClassyCollection<T>();
        }

        protected virtual void ObserveObjectCollection<T>(string fieldID, ClassyCollection<T> list) where T : CustomItemBase
        {
            list.ListChanged += (object source, ClassyCollection<T>.ListChangedEventArgs e) =>
            {
                this.SetClassObjects<T>(fieldID, (ClassyCollection<T>)source);
            };
        }

        public virtual void SetClassObjects<T>(string fieldID, IEnumerable<T> value) where T : CustomItemBase
        {
            if (value == null)
                this.SetString(fieldID, null);
            else
                this.SetItems(fieldID, value.Select(c => c.InnerItem));
        }

    }
}

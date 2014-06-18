using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Collections;
using Sitecore.Data;
using System.Collections.ObjectModel;

namespace ClassySC.Data
{
    public class ClassyCollection<T> : IList<T>
    {
        private IList<T> internalList;

        public class ListChangedEventArgs : EventArgs
        {
            public int index;
            public T item;
            public ListChangedEventArgs(int index, T item)
            {
                this.index = index;
                this.item = item;
            }
        }

        public delegate void ItemAddedEventHandler(object source, ListChangedEventArgs e);
        public delegate void ItemRemovedEventHandler(object source, ListChangedEventArgs e);
        public delegate void ListChangedEventHandler(object source, ListChangedEventArgs e);
        public delegate void ListClearedEventHandler(object source, EventArgs e);


        private ListChangedEventHandler _listChanged;
        /// <summary>
        /// Fired whenever list item has been changed, added or removed or when list has been cleared
        /// </summary>
        public event ListChangedEventHandler ListChanged
        {
            add
            {
                // prevent double subscription
                if (_listChanged == null || !_listChanged.GetInvocationList().Contains(value))
                {
                    _listChanged += value;
                }
            }
            remove
            {
                _listChanged -= value;
            }
        }
        /// <summary>
        /// Fired when list item has been removed from the list
        /// </summary>
        public event ItemRemovedEventHandler ItemRemoved;
        /// <summary>
        /// Fired when item has been added to the list
        /// </summary>
        public event ItemAddedEventHandler ItemAdded;
        /// <summary>
        /// Fired when list is cleared
        /// </summary>
        public event ListClearedEventHandler ListCleared;

        public ClassyCollection()
        {
            internalList = new List<T>();
        }

        public ClassyCollection(IList<T> list)
        {
            internalList = list;
        }

        public ClassyCollection(IEnumerable<T> collection)
        {
            internalList = new List<T>(collection);
        }

        protected virtual void OnItemAdded(ListChangedEventArgs e)
        {
            if (ItemAdded != null)
                ItemAdded(this, e);
        }

        protected virtual void OnItemRemoved(ListChangedEventArgs e)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, e);
        }

        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            if (_listChanged != null)
                _listChanged(this, e);
        }

        protected virtual void OnListCleared(EventArgs e)
        {
            if (ListCleared != null)
                ListCleared(this, e);
        }

        public int IndexOf(T item)
        {
            return internalList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            internalList.Insert(index, item);
            OnListChanged(new ListChangedEventArgs(index, item));
        }

        public void RemoveAt(int index)
        {
            T item = internalList[index];
            internalList.Remove(item);
            OnListChanged(new ListChangedEventArgs(index, item));
            OnItemRemoved(new ListChangedEventArgs(index, item));
        }

        public T this[int index]
        {
            get { return internalList[index]; }
            set
            {
                internalList[index] = value;
                OnListChanged(new ListChangedEventArgs(index, value));
            }
        }

        public void Add(T item)
        {
            internalList.Add(item);
            OnListChanged(new ListChangedEventArgs(internalList.IndexOf(item), item));
            OnItemAdded(new ListChangedEventArgs(internalList.IndexOf(item), item));
        }

        public void Clear()
        {
            internalList.Clear();
            OnListCleared(new EventArgs());
        }

        public bool Contains(T item)
        {
            return internalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return internalList.Count; }
        }

        public bool IsReadOnly
        {
            get { return IsReadOnly; }
        }

        public bool Remove(T item)
        {
            lock (this)
            {
                int index = internalList.IndexOf(item);
                if (internalList.Remove(item))
                {
                    OnListChanged(new ListChangedEventArgs(index, item));
                    OnItemRemoved(new ListChangedEventArgs(index, item));
                    return true;
                }
                else
                    return false;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)internalList).GetEnumerator();
        }
    }


    //public class ClassyCollectionChangedArgs<T> : EventArgs
    //{
    //    public IEnumerable<T> Collection;

    //    public ClassyCollectionChangedArgs(IEnumerable<T> collection)
    //    {
    //        this.Collection = collection;
    //    }
    //}

    //public class ClassyCollection<T> : Collection<T> where T : CustomItemBase
    //{

    //    public Action ChangeDelegate;

    //    public ClassyCollection(IEnumerable<T> collection):base(collection as IList<T>)
    //    {
            
    //    }

    //    public event EventHandler<ClassyCollectionChangedArgs<T>> CollectionChanged;

    //    protected void OnCollectionChanged()
    //    {
    //        if (ChangeDelegate != null)
    //            ChangeDelegate.Invoke();
    //        //ClassyCollectionChangedArgs<T> e = new ClassyCollectionChangedArgs<T>(this);
    //        //if (this.CollectionChanged != null)
    //        //    this.CollectionChanged(this, e);
    //    }


    //    protected override void SetItem(int index, T item)
    //    {
    //        base.SetItem(index, item);
    //        this.OnCollectionChanged();
    //    }

    //    protected override void ClearItems()
    //    {
    //        base.ClearItems();
    //        this.OnCollectionChanged();
    //    }

    //    protected override void InsertItem(int index, T item)
    //    {
    //        base.InsertItem(index, item);
    //        this.OnCollectionChanged();
    //    }

    //    protected override void RemoveItem(int index)
    //    {
    //        base.RemoveItem(index);
    //        this.OnCollectionChanged();            
    //    }
    //}


    //public class ClassyCollection<T> : IEnumerable<T>, IEnumerable, IList<T>, IList, ICollection<T>, ICollection where T : CustomItemBase
    //{

    //    private List<T> _list;

    //    public ClassyCollection(IEnumerable<T> innerList)
    //    {
    //        this._list = new List<T>(innerList);
    //    }


    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        foreach (T item in _list)
    //        {
    //            yield return item;
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return this._list.GetEnumerator();
    //    }

    //    public int IndexOf(T item)
    //    {
    //        return this._list.IndexOf(item);
    //    }

    //    public void Insert(int index, T item)
    //    {
    //        this._list.Insert(index, item);
    //    }

    //    public void RemoveAt(int index)
    //    {
    //        this._list.RemoveAt(index);
    //    }

    //    public T this[int index]
    //    {
    //        get
    //        {
    //            return this._list[index];
    //        }
    //        set
    //        {
    //            this._list[index] = value;
    //        }
    //    }

    //    public void Add(T item)
    //    {
    //        this._list.Add(item);
    //    }

    //    public void Clear()
    //    {
    //        this._list.Clear();
    //    }

    //    public bool Contains(T item)
    //    {
    //        return this._list.Contains(item);
    //    }

    //    public void CopyTo(T[] array, int arrayIndex)
    //    {
    //        this._list.CopyTo(array, arrayIndex);
    //    }

    //    public int Count
    //    {
    //        get { return this._list.Count; }
    //    }

    //    public bool IsReadOnly
    //    {
    //        get { return false; }
    //    }

    //    public bool Remove(T item)
    //    {
    //        return this._list.Remove(item);
    //    }

    //    public int Add(object value)
    //    {
    //        ClassyCollection<T>.VerifyValueType(value);
    //        this._list.Add((T)value);
    //        return (this._list.Count - 1);
    //    }

    //    public bool Contains(object value)
    //    {
    //        return (ClassyCollection<T>.IsCompatibleObject(value) && this.Contains((T)value));
    //    }

    //    public int IndexOf(object value)
    //    {
    //        if (ClassyCollection<T>.IsCompatibleObject(value))
    //        {
    //            return this.IndexOf((T)value);
    //        }
    //        return -1;
    //    }

    //    public void Insert(int index, object value)
    //    {
    //        ClassyCollection<T>.VerifyValueType(value);
    //        this.Insert(index, (T)value);

    //    }

    //    public bool IsFixedSize
    //    {
    //        get { return false; }
    //    }

    //    public void Remove(object value)
    //    {
    //        if (ClassyCollection<T>.IsCompatibleObject(value))
    //        {
    //            this.Remove((T)value);
    //        }
    //    }

    //    object IList.this[int index]
    //    {
    //        get
    //        {
    //            return this[index];
    //        }
    //        set
    //        {
    //            ClassyCollection<T>.VerifyValueType(value);
    //            this[index] = (T)value;

    //        }
    //    }

    //    public void CopyTo(Array array, int index)
    //    {
    //        /*if ((array != null) && (array.Rank != 1))
    //        {
    //            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
    //        }
    //        try
    //        {
    //            Array.Copy(this._items, 0, array, arrayIndex, this._size);
    //        }
    //        catch (ArrayTypeMismatchException)
    //        {
    //            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
    //        }*/

    //    }

    //    public bool IsSynchronized
    //    {
    //        get { return false; }
    //    }

    //    public object SyncRoot
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    //    {
    //        throw new NotImplementedException();
    //    }



    //    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }



    //    private static bool IsCompatibleObject(object value)
    //    {
    //        if (!(value is T) && ((value != null) || typeof(T).IsValueType))
    //        {
    //            return false;
    //        }
    //        return true;
    //    }

    //    private static void VerifyValueType(object value)
    //    {
    //        if (!ClassyCollection<T>.IsCompatibleObject(value))
    //        {
    //            throw new ArgumentException(string.Format("Value is {0}, needs to be {1}", value.GetType().FullName, typeof(T).FullName), "value");
    //        }
    //    }








    //}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using System.Reflection;
using Sitecore.Reflection;
using ClassySC.Data;
using Sitecore.Configuration;
using Sitecore.Data.Items;

namespace ClassySC.Data.Manager
{
    /// <summary>
    /// This class manages Resolving Class Types from Templates
    /// </summary>
    public class ClassManager
    {

        #region Properties
        /// <summary>
        /// static collection of instances per database name
        /// </summary>
        private static readonly Dictionary<string, ClassManager> _instances = new Dictionary<string, ClassManager>();
        private static readonly object _instancesLock = new object();

        /// <summary>
        /// DB name for this instance
        /// </summary>
        private string _dbName;

        /// <summary>
        /// Cache of Template -> Type results
        /// </summary>
        private ClassTemplateCache _cache;


        private bool _assertItems;

        /// <summary>
        /// Gets the instance of Class Manager to use for the current database
        /// </summary>
        public static ClassManager Instance
        {
            get
            {
                // ensure you get either web db or master db
                Database db = Sitecore.Context.Database ?? Factory.GetDatabase("master");
                if (db.Name != "web" && db.Name != "master")
                    db = Factory.GetDatabase("master");

                // instance already in the collection?
                if (!_instances.ContainsKey(db.Name))
                {
                    lock (_instancesLock)
                    {
                        if (!_instances.ContainsKey(db.Name))
                        {
                            // add instance to instance collection
                            _instances.Add(db.Name, new ClassManager(db));
                        }
                    }
                }
                return _instances[db.Name];
            }
        }

        /// <summary>
        /// Cache of Template -> Type results
        /// </summary>
        private ClassTemplateCache Cache
        {
            get
            {
                // check if cache has been cleared recently
                if (_cache != null && _cache.InnerCache.Count == 0)
                {
                    // initialize if necessary
                    Initialize(Factory.GetDatabase(_dbName));
                }
                return this._cache;
            }
        }


        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Database to use for Templates</param>
        public ClassManager(Database db)
        {
            _dbName = db.Name;
            // spawn new Cache
            _cache = new ClassTemplateCache("ClassTemplateCache[" + this._dbName + "]", StringUtil.ParseSizeString("10MB"));

            _assertItems = Sitecore.Configuration.Settings.GetBoolSetting("ClassySC.AssertItems", true);

            //initialize
            Initialize(db);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Called to populate the ClassTemplateCache with the templates in the database
        /// </summary>
        /// <param name="db">context database</param>
        private void Initialize(Database db)
        {
            try
            {
                Log.Info("Start ClassManager Initialize", this);

                _cache.Clear();

                Assembly classySCDataAssembly = this.GetType().Assembly;
                CacheAssembly(classySCDataAssembly, db);

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetReferencedAssemblies().SingleOrDefault(asm => asm.FullName == classySCDataAssembly.FullName) != null)
                    {
                        CacheAssembly(assembly, db);
                    }
                }

                Log.Info(string.Format("End ClassManager Initialize found {0} classes", _cache.InnerCache.Count), this);
            }
            catch (ReflectionTypeLoadException rex)
            {
                foreach (var item in rex.LoaderExceptions)
                {
                    Log.Error("ClassySC Init Error", item, this);
                }

                foreach (var item in rex.LoaderExceptions)
                {
                    throw item;
                }
            }
        }

        private void CacheAssembly(Assembly assembly, Database db)
        {
            // load each class in the Data assembly
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsInterface)
                {
                    // if the class has the TemplateAttribute, add to cache
                    foreach (TemplateAttribute attr in type.GetCustomAttributes(typeof(TemplateAttribute), false))
                    {

                        if (_assertItems)
                        {
                            // Assert that Template IDs exist in database
                            Assert.IsNotNull(db.GetItem(attr.ID), "Template {0} not found for Class {1}, did you publish?", attr.ID, type.FullName);
                        }

                        _cache.AddClassTemplateRef(attr.ID, type);

                        // Assert that property Field IDs exist in database
                        if (_assertItems)
                        {
                            foreach (PropertyInfo property in type.GetProperties())
                            {
                                foreach (FieldAttribute fieldAttr in property.GetCustomAttributes(typeof(FieldAttribute), false))
                                {
                                    Assert.IsNotNull(db.GetItem(fieldAttr.ID), "Field {0} not found for Property {1}.{2} , did you publish?", fieldAttr.ID, type.FullName, property.Name);
                                }
                            }
                        }
                    }


                }
            }

        }

        /// <summary>
        /// Get a concrete type from a template ID
        /// </summary>
        /// <param name="templateId">template ID</param>
        /// <returns>Type</returns>
        public Type GetType(ID templateId)
        {
            Assert.ArgumentNotNull(templateId, "templateId");
            return this.Cache.GetClassTemplateRef(templateId);
        }

        /// <summary>
        /// Get a concrete type from an Item
        /// </summary>
        /// <param name="item">item</param>
        /// <returns>Type</returns>
        public Type GetType(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return GetType(item.TemplateID);
        }
        #endregion

    }
}

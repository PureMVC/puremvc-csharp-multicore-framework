/* 
 PureMVC C# Multi-Core Port by Tang Khai Phuong <phuong.tang@puremvc.org>, et al.
 PureMVC - Copyright(c) 2006-08 Futurescale, Inc., Some rights reserved. 
 Your reuse is governed by the Creative Commons Attribution 3.0 License 
*/

#region Using

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PureMVC.Interfaces;

#endregion

namespace PureMVC.Core
{
    /// <summary>
    /// A Singleton <c>IModel</c> implementation
    /// </summary>
    /// <remarks>
    ///     <para>In PureMVC, the <c>Model</c> class provides access to model objects (Proxies) by named lookup</para>
    ///     <para>The <c>Model</c> assumes these responsibilities:</para>
    ///     <list type="bullet">
    ///         <item>Maintain a cache of <c>IProxy</c> instances</item>
    ///         <item>Provide methods for registering, retrieving, and removing <c>IProxy</c> instances</item>
    ///     </list>
    ///     <para>
    ///         Your application must register <c>IProxy</c> instances
    ///         with the <c>Model</c>. Typically, you use an 
    ///         <c>ICommand</c> to create and register <c>IProxy</c> 
    ///         instances once the <c>Facade</c> has initialized the Core actors
    ///     </para>
    /// </remarks>
    /// <seealso cref="PureMVC.Patterns.Proxy"/>
    /// <seealso cref="PureMVC.Interfaces.IProxy" />
    public class Model : IModel
    {
        #region Constructors

        /// <summary>
        /// Constructs and initializes a new model
        /// </summary>
        /// <remarks>
        ///     <para>This <c>IModel</c> implementation is a Singleton, so you should not call the constructor directly, but instead call the static Singleton Factory method <c>Model.getInstance()</c></para>
        /// </remarks>
        /// <param name="key">Key of model</param>
        public Model(string key)
        {
            m_multitonKey = key;
            m_proxyMap = new ConcurrentDictionary<string, IProxy>();
            if (m_instanceMap.ContainsKey(key))
                throw new Exception(MULTITON_MSG);
            m_instanceMap[key] = this;
            InitializeModel();
        }

        /// <summary>
        /// Constructs and initializes a new model
        /// </summary>
        /// <remarks>
        ///     <para>This <c>IModel</c> implementation is a Singleton, so you should not call the constructor directly, but instead call the static Singleton Factory method <c>Model.getInstance()</c></para>
        /// </remarks>
        public Model()
            : this(DEFAULT_KEY)
        { }

        #endregion

        #region Public Methods

        #region IModel Members

        /// <summary>
        /// Register an <c>IProxy</c> with the <c>Model</c>
        /// </summary>
        /// <param name="proxy">An <c>IProxy</c> to be held by the <c>Model</c></param>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual void RegisterProxy(IProxy proxy)
        {
            proxy.InitializeNotifier(m_multitonKey);
            m_proxyMap[proxy.ProxyName] = proxy;

            proxy.OnRegister();
        }

        /// <summary>
        /// Retrieve an <c>IProxy</c> from the <c>Model</c>
        /// </summary>
        /// <param name="proxyName">The name of the <c>IProxy</c> to retrieve</param>
        /// <returns>The <c>IProxy</c> instance previously registered with the given <c>proxyName</c></returns>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual IProxy RetrieveProxy(string proxyName)
        {
            if (!m_proxyMap.ContainsKey(proxyName)) return null;

            return m_proxyMap[proxyName];
        }

        /// <summary>
        /// Check if a Proxy is registered
        /// </summary>
        /// <param name="proxyName"></param>
        /// <returns>whether a Proxy is currently registered with the given <c>proxyName</c>.</returns>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual bool HasProxy(string proxyName)
        {
            return m_proxyMap.ContainsKey(proxyName);
        }

        /// <summary>
        /// List all proxy name
        /// </summary>
        public IEnumerable<string> ListProxyNames
        {
            get { return m_proxyMap.Keys; }
        }

        /// <summary>
        /// Remove an <c>IProxy</c> from the <c>Model</c>
        /// </summary>
        /// <param name="proxyName">The name of the <c>IProxy</c> instance to be removed</param>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual IProxy RemoveProxy(string proxyName)
        {
            IProxy proxy = null;

            if (m_proxyMap.ContainsKey(proxyName))
            {
                proxy = RetrieveProxy(proxyName);
                m_proxyMap.Remove(proxyName);
            }

            if (proxy != null) proxy.OnRemove();
            return proxy;
        }

        #endregion

        #endregion

        #region Accessors

        /// <summary>
        /// <c>Model</c> Singleton Factory method.  This method is thread safe.
        /// </summary>
        public static IModel Instance
        {
            get { return GetInstance(DEFAULT_KEY); }
        }

        /// <summary>
        /// <c>Model</c> Singleton Factory method.  This method is thread safe.
        /// </summary>
        public static IModel GetInstance(string key)
        {
            IModel result;
            if (m_instanceMap.TryGetValue(key, out result))
                return result;

            result = new Model(key);
            m_instanceMap[key] = result;
            return result;
        }

        #endregion

        #region Protected & Internal Methods

        /// <summary>
        /// Explicit static constructor to tell C# compiler not to mark type as before field initiate
        /// </summary>
        static Model()
        {
            m_instanceMap = new ConcurrentDictionary<string, IModel>();
        }

        /// <summary>
        /// Initialize the Singleton <c>Model</c> instance.
        /// </summary>
        /// <remarks>
        ///     <para>Called automatically by the constructor, this is your opportunity to initialize the Singleton instance in your subclass without overriding the constructor</para>
        /// </remarks>
        protected virtual void InitializeModel()
        {
        }

        /// <summary>
        /// Remove an IModel instance
        /// </summary>
        /// <param name="key">key of IModel instance to remove</param>
        public static void RemoveModel(string key)
        {
            IModel model;
            if (!m_instanceMap.TryGetValue(key, out model))
                return;

            m_instanceMap.Remove(key);
            model.Dispose();
        }

        /// <summary>
        /// Release and dispose resource of model.
        /// </summary>
        public void Dispose()
        {
            RemoveModel(m_multitonKey);
            m_proxyMap.Clear();
        }

        #endregion

        #region Members

        /// <summary>
        /// The key name of multi-ton model
        /// </summary>
        protected string m_multitonKey;

        /// <summary>
        /// Mapping of proxyNames to <c>IProxy</c> instances
        /// </summary>
        protected IDictionary<string, IProxy> m_proxyMap;

        /// <summary>
        /// Singleton instance
        /// </summary>
        protected static volatile IModel m_instance;

        /// <summary>
        /// IModel lookup table.
        /// </summary>
        protected static readonly IDictionary<string, IModel> m_instanceMap;

        /// <summary>
        /// Default name of model
        /// </summary>
        public const string DEFAULT_KEY = "PureMVC";

        /// <summary>
        /// Exception string for duplicate model
        /// </summary>
        protected const string MULTITON_MSG = "Model instance for this Multiton key already constructed!";

        #endregion
    }
}

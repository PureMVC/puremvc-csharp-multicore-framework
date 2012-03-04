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
using PureMVC.Patterns;

#endregion

namespace PureMVC.Core
{
    /// <summary>
    /// A Singleton <c>IView</c> implementation.
    /// </summary>
    /// <remarks>
    ///     <para>In PureMVC, the <c>View</c> class assumes these responsibilities:</para>
    ///     <list type="bullet">
    ///         <item>Maintain a cache of <c>IMediator</c> instances</item>
    ///         <item>Provide methods for registering, retrieving, and removing <c>IMediators</c></item>
    ///         <item>Managing the observer lists for each <c>INotification</c> in the application</item>
    ///         <item>Providing a method for attaching <c>IObservers</c> to an <c>INotification</c>'s observer list</item>
    ///         <item>Providing a method for broadcasting an <c>INotification</c></item>
    ///         <item>Notifying the <c>IObservers</c> of a given <c>INotification</c> when it broadcast</item>
    ///     </list>
    /// </remarks>
    /// <see cref="PureMVC.Patterns.Mediator"/>
    /// <see cref="PureMVC.Patterns.Observer"/>
    /// <see cref="PureMVC.Patterns.Notification"/>
    public class View : IView
    {
        #region Constructors

        /// <summary>
        /// Constructs and initializes a new view
        /// </summary>
        /// <remarks>
        /// <para>This <c>IView</c> implementation is a Singleton, so you should not call the constructor directly, but instead call the static Singleton Factory method <c>View.Instance</c></para>
        /// </remarks>
        /// <param name="key">Key of view</param>
        protected View(string key)
        {
            m_multitonKey = key;
            m_mediatorMap = new ConcurrentDictionary<string, IMediator>();
            m_observerMap = new ConcurrentDictionary<string, IList<IObserver>>();
            if (m_instanceMap.ContainsKey(key))
                throw new Exception(MULTITON_MSG);
            m_instanceMap[key] = this;
            InitializeView();
        }

        /// <summary>
        /// Constructs and initializes a new view
        /// </summary>
        /// <remarks>
        /// <para>This <c>IView</c> implementation is a Singleton, so you should not call the constructor directly, but instead call the static Singleton Factory method <c>View.Instance</c></para>
        /// </remarks>
        protected View()
            : this(DEFAULT_KEY)
        { }

        #endregion

        #region Public Methods

        #region IView Members

        #region Observer

        /// <summary>
        /// Register an <c>IObserver</c> to be notified of <c>INotifications</c> with a given name
        /// </summary>
        /// <param name="notificationName">The name of the <c>INotifications</c> to notify this <c>IObserver</c> of</param>
        /// <param name="observer">The <c>IObserver</c> to register</param>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual void RegisterObserver(string notificationName, IObserver observer)
        {
            if (!m_observerMap.ContainsKey(notificationName))
            {
                m_observerMap[notificationName] = new List<IObserver>();
            }

            m_observerMap[notificationName].Add(observer);
        }

        /// <summary>
        /// Notify the <c>IObservers</c> for a particular <c>INotification</c>
        /// </summary>
        /// <param name="notification">The <c>INotification</c> to notify <c>IObservers</c> of</param>
        /// <remarks>
        /// <para>All previously attached <c>IObservers</c> for this <c>INotification</c>'s list are notified and are passed a reference to the <c>INotification</c> in the order in which they were registered</para>
        /// </remarks>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual void NotifyObservers(INotification notification)
        {
            IList<IObserver> observers = null;

            if (m_observerMap.ContainsKey(notification.Name))
            {
                // Get a reference to the observers list for this notification name
                IList<IObserver> observers_ref = m_observerMap[notification.Name];
                // Copy observers from reference array to working array, 
                // since the reference array may change during the notification loop
                observers = new List<IObserver>(observers_ref);
            }

            // Notify outside of the lock
            if (observers == null) return;
            // Notify Observers from the working array				
            foreach (IObserver observer in observers)
            {
                observer.NotifyObserver(notification);
            }
        }

        /// <summary>
        /// Remove the observer for a given notifyContext from an observer list for a given Notification name.
        /// </summary>
        /// <param name="notificationName">which observer list to remove from</param>
        /// <param name="notifyContext">remove the observer with this object as its notifyContext</param>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual void RemoveObserver(string notificationName, object notifyContext)
        {
            // the observer list for the notification under inspection
            if (!m_observerMap.ContainsKey(notificationName)) return;
            var observers = m_observerMap[notificationName];

            lock (observers)
            {
                // find the observer for the notifyContext
                for (var i = 0; i < observers.Count; i++)
                {
                    if (!observers[i].CompareNotifyContext(notifyContext)) continue;
                    // there can only be one Observer for a given notifyContext 
                    // in any given Observer list, so remove it and break
                    observers.RemoveAt(i);
                    break;
                }

                // Also, when a Notification's Observer list length falls to 
                // zero, delete the notification key from the observer map
                if (observers.Count == 0)
                    m_observerMap.Remove(notificationName);
            }
        }

        #endregion

        #region Mediator

        /// <summary>
        /// Register an <c>IMediator</c> instance with the <c>View</c>
        /// </summary>
        /// <param name="mediator">A reference to the <c>IMediator</c> instance</param>
        /// <remarks>
        ///     <para>Registers the <c>IMediator</c> so that it can be retrieved by name, and further interrogates the <c>IMediator</c> for its <c>INotification</c> interests</para>
        ///     <para>If the <c>IMediator</c> returns any <c>INotification</c> names to be notified about, an <c>Observer</c> is created encapsulating the <c>IMediator</c> instance's <c>handleNotification</c> method and registering it as an <c>Observer</c> for all <c>INotifications</c> the <c>IMediator</c> is interested in</para>
        /// </remarks>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual void RegisterMediator(IMediator mediator)
        {
            lock (m_mediatorMap)
            {
                // do not allow re-registration (you must to removeMediator fist)
                if (m_mediatorMap.ContainsKey(mediator.MediatorName)) return;

                mediator.InitializeNotifier(m_multitonKey);
                // Register the Mediator for retrieval by name
                m_mediatorMap[mediator.MediatorName] = mediator;

                // Get Notification interests, if any.
                var interests = mediator.ListNotificationInterests;

                // Register Mediator as an observer for each of its notification interests
                // Create Observer
                IObserver observer = new Observer("handleNotification", mediator);

                // Register Mediator as Observer for its list of Notification interests
                foreach (var t in interests)
                {
                    RegisterObserver(t, observer);
                }
            }
            // alert the mediator that it has been registered
            mediator.OnRegister();
        }

        /// <summary>
        /// Retrieve an <c>IMediator</c> from the <c>View</c>
        /// </summary>
        /// <param name="mediatorName">The name of the <c>IMediator</c> instance to retrieve</param>
        /// <returns>The <c>IMediator</c> instance previously registered with the given <c>mediatorName</c></returns>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual IMediator RetrieveMediator(string mediatorName)
        {
            if (!m_mediatorMap.ContainsKey(mediatorName)) return null;
            return m_mediatorMap[mediatorName];
        }

        /// <summary>
        /// Remove an <c>IMediator</c> from the <c>View</c>
        /// </summary>
        /// <param name="mediatorName">The name of the <c>IMediator</c> instance to be removed</param>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual IMediator RemoveMediator(string mediatorName)
        {
            lock (m_mediatorMap)
            {
                // Retrieve the named mediator
                if (!m_mediatorMap.ContainsKey(mediatorName)) return null;
                var mediator = m_mediatorMap[mediatorName];

                // for every notification this mediator is interested in...
                var interests = mediator.ListNotificationInterests;

                foreach (var t in interests)
                {
                    // remove the observer linking the mediator 
                    // to the notification interest
                    RemoveObserver(t, mediator);
                }

                // remove the mediator from the map		
                m_mediatorMap.Remove(mediatorName);

                // alert the mediator that it has been removed
                mediator.OnRemove();
                return mediator;
            }
        }

        /// <summary>
        /// Check if a Mediator is registered or not
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <returns>whether a Mediator is registered with the given <code>mediatorName</code>.</returns>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public virtual bool HasMediator(string mediatorName)
        {
            return m_mediatorMap.ContainsKey(mediatorName);
        }

        /// <summary>
        /// List all mediator name
        /// </summary>
        public IEnumerable<string> ListMediatorNames
        {
            get { return m_mediatorMap.Keys; }
        }

        /// <summary>
        /// Remove an IView instance
        /// </summary>
        /// <param name="key">key of IView instance to remove</param>
        public static void RemoveView(string key)
        {
            IView view;
            if (!m_instanceMap.TryGetValue(key, out view))
                return;

            m_instanceMap.Remove(key);
            view.Dispose();
        }

        /// <summary>
        /// Release and dispose resource of view.
        /// </summary>
        public void Dispose()
        {
            RemoveView(m_multitonKey);
            m_observerMap.Clear();
            m_mediatorMap.Clear();
        }

        #endregion

        #endregion

        #endregion

        #region Accessors

        /// <summary>
        /// View Singleton Factory method.  This method is thread safe.
        /// </summary>
        public static IView Instance
        {
            get { return GetInstance(DEFAULT_KEY); }
        }

        /// <summary>
        /// View Singleton Factory method.  This method is thread safe.
        /// </summary>
        public static IView GetInstance()
        {
            return GetInstance(DEFAULT_KEY);
        }

        /// <summary>
        /// View Singleton Factory method.  This method is thread safe.
        /// </summary>
        public static IView GetInstance(string key)
        {
            IView result;
            if (m_instanceMap.TryGetValue(key, out result))
                return result;

            result = new View(key);
            m_instanceMap[key] = result;
            return result;
        }

        #endregion

        #region Protected & Internal Methods

        /// <summary>
        /// Explicit static constructor to tell C# compiler 
        /// not to mark type as before field initiate
        /// </summary>
        static View()
        {
            m_instanceMap = new ConcurrentDictionary<string, IView>();
        }

        /// <summary>
        /// Initialize the Singleton View instance
        /// </summary>
        /// <remarks>
        /// <para>Called automatically by the constructor, this is your opportunity to initialize the Singleton instance in your subclass without overriding the constructor</para>
        /// </remarks>
        protected virtual void InitializeView()
        {
        }

        #endregion

        #region Members

        /// <summary>
        /// The key name of multi-ton view
        /// </summary>
        protected string m_multitonKey;

        /// <summary>
        /// Mapping of Mediator names to Mediator instances
        /// </summary>
        protected IDictionary<string, IMediator> m_mediatorMap;

        /// <summary>Mapping of Notification names to Observer lists
        /// </summary>
        protected IDictionary<string, IList<IObserver>> m_observerMap;

        /// <summary>
        /// Singleton instance
        /// </summary>
        protected static volatile IView m_instance;

        /// <summary>
        /// View lookup table.
        /// </summary>
        protected static readonly IDictionary<string, IView> m_instanceMap;

        /// <summary>
        /// Default name of view
        /// </summary>
        public const string DEFAULT_KEY = "PureMVC";

        /// <summary>
        /// Exception string for duplicate view
        /// </summary>
        protected const string MULTITON_MSG = "View instance for this Multiton key already constructed!";

        #endregion
    }
}

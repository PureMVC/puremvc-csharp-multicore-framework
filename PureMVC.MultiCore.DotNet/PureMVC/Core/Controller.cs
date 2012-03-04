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
    /// A Singleton <c>IController</c> implementation.
    /// </summary>
    /// <remarks>
    /// 	<para>In PureMVC, the <c>Controller</c> class follows the 'Command and Controller' strategy, and assumes these responsibilities:</para>
    /// 	<list type="bullet">
    /// 		<item>Remembering which <c>ICommand</c>s are intended to handle which <c>INotifications</c>.</item>
    /// 		<item>Registering itself as an <c>IObserver</c> with the <c>View</c> for each <c>INotification</c> that it has an <c>ICommand</c> mapping for.</item>
    /// 		<item>Creating a new instance of the proper <c>ICommand</c> to handle a given <c>INotification</c> when notified by the <c>View</c>.</item>
    /// 		<item>Calling the <c>ICommand</c>'s <c>execute</c> method, passing in the <c>INotification</c>.</item>
    /// 	</list>
    /// 	<para>Your application must register <c>ICommands</c> with the <c>Controller</c>.</para>
    /// 	<para>The simplest way is to subclass <c>Facade</c>, and use its <c>initializeController</c> method to add your registrations.</para>
    /// </remarks>
    /// <see cref="PureMVC.Core.View"/>
    /// <see cref="PureMVC.Patterns.Observer"/>
    /// <see cref="PureMVC.Patterns.Notification"/>
    /// <see cref="PureMVC.Patterns.SimpleCommand"/>
    /// <see cref="PureMVC.Patterns.MacroCommand"/>
    public class Controller : IController
    {
        #region Constructors

        /// <summary>
        /// Constructs and initializes a new controller
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This <c>IController</c> implementation is a Singleton, 
        ///         so you should not call the constructor 
        ///         directly, but instead call the static Singleton
        ///         Factory method <c>Controller.getInstance()</c>
        ///     </para>
        /// </remarks>
        /// <param name="key">Key of controller</param>
        public Controller(string key)
        {
            m_multitonKey = key;
            m_commandMap = new ConcurrentDictionary<string, object>();
            if (m_instanceMap.ContainsKey(key))
                throw new Exception(MULTITON_MSG);
            m_instanceMap[key] = this;
            InitializeController();
        }

        /// <summary>
        /// Constructs and initializes a new controller
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This <c>IController</c> implementation is a Singleton, 
        ///         so you should not call the constructor 
        ///         directly, but instead call the static Singleton
        ///         Factory method <c>Controller.getInstance()</c>
        ///     </para>
        /// </remarks>
        public Controller()
            : this(DEFAULT_KEY)
        { }

        #endregion

        #region Public Methods

        #region IController Members

        /// <summary>
        /// If an <c>ICommand</c> has previously been registered
        /// to handle a the given <c>INotification</c>, then it is executed.
        /// </summary>
        /// <param name="notification">An INotification</param>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public void ExecuteCommand(INotification notification)
        {
            if (!m_commandMap.ContainsKey(notification.Name)) return;
            var commandReference = m_commandMap[notification.Name];

            ICommand command;
            var commandType = commandReference as Type;
            if (commandType != null)
            {
                var commandInstance = Activator.CreateInstance(commandType);
                command = commandInstance as ICommand;
                if (command == null)
                    return;
            }
            else
            {
                command = commandReference as ICommand;
                if (command == null) return;
            }
            command.InitializeNotifier(m_multitonKey);
            command.Execute(notification);
        }

        /// <summary>
        /// Register a particular <c>ICommand</c> class as the handler
        /// for a particular <c>INotification</c>.
        /// </summary>
        /// <param name="notificationName">The name of the <c>INotification</c></param>
        /// <param name="commandType">The <c>Type</c> of the <c>ICommand</c></param>
        /// <remarks>
        ///     <para>
        ///         If an <c>ICommand</c> has already been registered to 
        ///         handle <c>INotification</c>s with this name, it is no longer
        ///         used, the new <c>ICommand</c> is used instead.
        ///     </para>
        /// </remarks> 
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public void RegisterCommand(string notificationName, Type commandType)
        {
            if (!m_commandMap.ContainsKey(notificationName))
            {
                // This call needs to be monitored carefully. Have to make sure that RegisterObserver
                // doesn't call back into the controller, or a dead lock could happen.
                m_view.RegisterObserver(notificationName, new Observer("executeCommand", this));
            }

            m_commandMap[notificationName] = commandType;
        }

        /// <summary>
        /// Register a particular <c>ICommand</c> class as the handler
        /// for a particular <c>INotification</c>.
        /// </summary>
        /// <param name="notificationName">The name of the <c>INotification</c></param>
        /// <param name="command">The <c>ICommand</c></param>
        /// <remarks>
        ///     <para>
        ///         If an <c>ICommand</c> has already been registered to 
        ///         handle <c>INotification</c>s with this name, it is no longer
        ///         used, the new <c>ICommand</c> is used instead.
        ///     </para>
        /// </remarks> 
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public void RegisterCommand(string notificationName, ICommand command)
        {
            if (!m_commandMap.ContainsKey(notificationName))
            {
                // This call needs to be monitored carefully. Have to make sure that RegisterObserver
                // doesn't call back into the controller, or a dead lock could happen.
                m_view.RegisterObserver(notificationName, new Observer("executeCommand", this));
            }
            command.InitializeNotifier(m_multitonKey);
            m_commandMap[notificationName] = command;
        }

        /// <summary>
        /// Check if a Command is registered for a given Notification 
        /// </summary>
        /// <param name="notificationName"></param>
        /// <returns>whether a Command is currently registered for the given <c>notificationName</c>.</returns>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public bool HasCommand(string notificationName)
        {
            return m_commandMap.ContainsKey(notificationName);
        }

        /// <summary>
        /// Remove a previously registered <c>ICommand</c> to <c>INotification</c> mapping.
        /// </summary>
        /// <param name="notificationName">The name of the <c>INotification</c> to remove the <c>ICommand</c> mapping for</param>
        /// <remarks>This method is thread safe and needs to be thread safe in all implementations.</remarks>
        public object RemoveCommand(string notificationName)
        {
            if (!m_commandMap.ContainsKey(notificationName)) return null;
            // remove the observer

            // This call needs to be monitored carefully. Have to make sure that RemoveObserver
            // doesn't call back into the controller, or a dead lock could happen.
            m_view.RemoveObserver(notificationName, this);
            var command = m_commandMap[notificationName];
            m_commandMap.Remove(notificationName);
            return command;
        }

        #endregion

        #endregion

        #region Accessors

        /// <summary>
        /// Singleton Factory method.  This method is thread safe.
        /// </summary>
        /// 
        public static IController Instance
        {
            get { return GetInstance(DEFAULT_KEY); }
        }

        /// <summary>
        /// Facade Singleton Factory method.  This method is thread safe.
        /// </summary>
        public static IController GetInstance()
        {
            return GetInstance(DEFAULT_KEY);
        }

        /// <summary>
        /// Facade Singleton Factory method.  This method is thread safe.
        /// </summary>
        public static IController GetInstance(string key)
        {
            IController result;
            if (m_instanceMap.TryGetValue(key, out result))
                return result;

            result = new Controller(key);
            m_instanceMap[key] = result;
            return result;
        }

        #endregion

        #region Protected & Internal Methods

        /// <summary>
        /// Explicit static constructor to tell C# compiler
        /// not to mark type as before field initiate
        /// </summary>
        static Controller()
        {
            m_instanceMap = new ConcurrentDictionary<string, IController>();
        }

        /// <summary>
        /// Initialize the Singleton <c>Controller</c> instance
        /// </summary>
        /// <remarks>
        ///     <para>Called automatically by the constructor</para>
        ///     
        ///     <para>
        ///         Please aware that if you are using a subclass of <c>View</c>
        ///         in your application, you should also subclass <c>Controller</c>
        ///         and override the <c>initializeController</c> method in the following way:
        ///     </para>
        /// 
        ///     <c>
        ///         // ensure that the Controller is talking to my IView implementation
        ///         public override void initializeController()
        ///         {
        ///             view = MyView.Instance;
        ///         }
        ///     </c>
        /// </remarks>
        private void InitializeController()
        {
            m_view = View.GetInstance(m_multitonKey);
        }

        /// <summary>
        /// List all notification name
        /// </summary>
        public IEnumerable<string> ListNotificationNames
        {
            get { return m_commandMap.Keys; }
        }

        /// <summary>
        /// Release and dispose resource of controller.
        /// </summary>
        public void Dispose()
        {
            RemoveController(m_multitonKey);
            m_commandMap.Clear();
        }

        /// <summary>
        /// Remove an IController instance
        /// </summary>
        /// <param name="key">key of IController instance to remove</param>
        public static void RemoveController(string key)
        {
            IController controller;
            if (!m_instanceMap.TryGetValue(key, out controller))
                return;

            m_instanceMap.Remove(key);
            controller.Dispose();
        }

        #endregion

        #region Members

        /// <summary>
        /// The key name of multi-ton controller
        /// </summary>
        protected string m_multitonKey;

        /// <summary>
        /// Local reference to View
        /// </summary>
        private IView m_view;

        /// <summary>
        /// Mapping of Notification names to Command Class references
        /// </summary>
        private readonly IDictionary<string, object> m_commandMap;

        /// <summary>
        /// Controller lookup table.
        /// </summary>
        protected static readonly IDictionary<string, IController> m_instanceMap;

        /// <summary>
        /// Default name of controller
        /// </summary>
        public const string DEFAULT_KEY = "PureMVC";

        /// <summary>
        /// Exception string for duplicate controller
        /// </summary>
        protected const string MULTITON_MSG = "Controller instance for this Multiton key already constructed!";

        #endregion
    }
}

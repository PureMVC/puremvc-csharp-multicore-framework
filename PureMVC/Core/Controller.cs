//
//  PureMVC C# Multicore
//
//  Copyright(c) 2017 Saad Shams <saad.shams@puremvc.org>
//  Your reuse is governed by the Creative Commons Attribution 3.0 License
//

using System;
using System.Collections.Concurrent;
using PureMVC.Interfaces;
using PureMVC.Patterns.Observer;

namespace PureMVC.Core
{
    /// <summary>
    /// A Multiton <c>IController</c> implementation.
    /// </summary>
    /// <remarks>
    /// 	<para>In PureMVC, the <c>Controller</c> class follows the 
    /// 	'Command and Controller' strategy, and assumes these 
    /// 	responsibilities:</para>
    /// 	<list type="bullet">
    /// 		<item> Remembering which <c>ICommand</c>s 
    /// 		are intended to handle which <c>INotifications</c>.</item>
    /// 		<item> Registering itself as an <c>IObserver</c> with 
    /// 		the <c>View</c> for each <c>INotification</c> 
    /// 		that it has an <c>ICommand</c> mapping for.</item>
    /// 		<item> Creating a new instance of the proper <c>ICommand</c> 
    /// 		to handle a given <c>INotification</c> when notified by the <c>View</c>.</item>
    /// 		<item>Calling the <c>ICommand</c>'s <c>execute</c> 
    /// 		method, passing in the <c>INotification</c>.</item>
    /// 	</list>
    /// 	<para>
    /// 	    Your application must register <c>ICommands</c> with the 
    /// 	    <c>Controller</c>.
    /// 	</para>
    /// 	<para>
    /// 	    The simplest way is to subclass <c>Facade</c>, 
    /// 	    and use its <c>initializeController</c> method to add your 
    /// 	    registrations.
    /// 	</para>
    /// </remarks>
    /// <seealso cref="PureMVC.Core.View"/>
    /// <seealso cref="PureMVC.Patterns.Observer.Observer"/>
    /// <seealso cref="PureMVC.Patterns.Observer.Notification"/>
    /// <seealso cref="PureMVC.Patterns.Command.SimpleCommand"/>
    /// <seealso cref="PureMVC.Patterns.Command.MacroCommand"/>
    public class Controller: IController
    {
        /// <summary>
        /// Constructs and initializes a new controller
        /// </summary>
        /// <remarks>This <c>IController</c> implementation is a Multiton, 
        ///     so you should not call the constructor 
        ///     directly, but instead call the static Multiton
        ///     Factory method <c>Controller.getInstance(multitonKey, () => new Controller(multitonKey))</c>
        /// </remarks>
        /// <param name="key">Key of controller</param>
        /// <exception cref="System.Exception">Thrown if instance for this Multiton key has already been constructed</exception>
        public Controller(string key)
        {
            if (instanceMap.TryGetValue(key, out Lazy<IController> _) && multitonKey != null) throw new Exception(MULTITON_MSG);
            multitonKey = key;
            instanceMap.TryAdd(multitonKey, new Lazy<IController>(() => this));
            commandMap = new ConcurrentDictionary<string, Func<ICommand>>();
            InitializeController();
        }

        /// <summary>
        /// Initialize the Multiton <c>Controller</c> instance
        /// </summary>
        /// <remarks>
        ///     <para>Called automatically by the constructor</para>
        ///     <para>
        ///         Please aware that if you are using a subclass of <c>View</c>
        ///         in your application, you should also subclass <c>Controller</c>
        ///         and override the <c>initializeController</c> method in the following way:
        ///     </para>
        ///     <example>
        ///         <code>
        ///             // ensure that the Controller is talking to my IView implementation
        ///             public override void initializeController()
        ///             {
        ///                 view = MyView.getInstance(multitonKey, () => new MyView(multitonKey));
        ///             }
        ///         </code>
        ///     </example>
        /// </remarks>
        protected virtual void InitializeController()
        {
            view = View.GetInstance(multitonKey, () => new View(multitonKey));
        }

        /// <summary>
        /// <c>Controller</c> Multiton Factory method.
        /// </summary>
        /// <param name="key">Key of controller</param>
        /// <param name="controllerFunc">the <c>FuncDelegate</c> of the <c>IController</c></param>
        /// <returns>the Multiton instance of <c>Controller</c></returns>
        public static IController GetInstance(string key, Func<IController> controllerFunc)
        {
            return instanceMap.GetOrAdd(key, (k) => new Lazy<IController>(controllerFunc)).Value;
        }

        /// <summary>
        /// If an <c>ICommand</c> has previously been registered 
        /// to handle a the given <c>INotification</c>, then it is executed.
        /// </summary>
        /// <param name="notification">note an <c>INotification</c></param>
        public virtual void ExecuteCommand(INotification notification)
        {
            if (commandMap.TryGetValue(notification.Name, out Func<ICommand> commandFunc))
            {
                ICommand commandInstance = commandFunc();
                commandInstance.InitializeNotifier(multitonKey);
                commandInstance.Execute(notification);
            }
        }

        /// <summary>
        /// Register a particular <c>ICommand</c> class as the handler 
        /// for a particular <c>INotification</c>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If a <c>ICommand</c> has already been registered to 
        ///         handle <c>INotification</c>s with this name, it is no longer
        ///         used, the new <c>Func</c> is used instead.
        ///     </para>
        ///     <para>
        ///         The Observer for the new ICommand is only created if this the
        ///         first time an ICommand has been regisered for this Notification name.
        ///     </para>
        /// </remarks>
        /// <param name="notificationName">the name of the <c>INotification</c></param>
        /// <param name="commandFunc">the <c>Func Delegate</c> of the <c>ICommand</c></param>
        public virtual void RegisterCommand(string notificationName, Func<ICommand> commandFunc)
        {
            if (commandMap.TryGetValue(notificationName, out Func<ICommand> _) == false)
            {
                view.RegisterObserver(notificationName, new Observer(ExecuteCommand, this));
            }
            commandMap[notificationName] = commandFunc;
        }

        /// <summary>
        /// Remove a previously registered <c>ICommand</c> to <c>INotification</c> mapping.
        /// </summary>
        /// <param name="notificationName">the name of the <c>INotification</c> to remove the <c>ICommand</c> mapping for</param>
        public virtual void RemoveCommand(string notificationName)
        {
            if (commandMap.TryRemove(notificationName, out Func<ICommand> _))
            {
                view.RemoveObserver(notificationName, this);
            }
        }

        /// <summary>
        /// Check if a Command is registered for a given Notification 
        /// </summary>
        /// <param name="notificationName"></param>
        /// <returns>whether a Command is currently registered for the given <c>notificationName</c>.</returns>
        public virtual bool HasCommand(string notificationName)
        {
            return commandMap.ContainsKey(notificationName);
        }

        /// <summary>
        /// Remove an IController instance
        /// </summary>
        /// <param name="key">multitonKey of IController instance to remove</param>
        public static void RemoveController(string key)
        {
            instanceMap.TryRemove(key, out Lazy<IController> _);
        }

        /// <summary>Local reference to View</summary>
        protected IView view;

        /// <summary>The Multiton Key for this Core</summary>
        protected string multitonKey;

        /// <summary>Mapping of Notification names to Command Class references</summary>
        protected ConcurrentDictionary<string, Func<ICommand>> commandMap;
        
        /// <summary>The Multiton Controller instanceMap.</summary>
        protected static ConcurrentDictionary<string, Lazy<IController>> instanceMap = new ConcurrentDictionary<string, Lazy<IController>>();

        /// <summary>Message Constants</summary>
        protected const string MULTITON_MSG = "Controller instance for this Multiton key already constructed!";
    }
}

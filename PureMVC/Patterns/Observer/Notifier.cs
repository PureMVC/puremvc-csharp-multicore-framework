//
//  PureMVC C# Multicore
//
//  Copyright(c) 2020 Saad Shams <saad.shams@puremvc.org>
//  Your reuse is governed by the Creative Commons Attribution 3.0 License
//

using System;
using PureMVC.Interfaces;

namespace PureMVC.Patterns.Observer
{
    /// <summary>
    /// A Base <c>INotifier</c> implementation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>MacroCommand, Command, Mediator</c> and <c>Proxy</c> 
    ///         all have a need to send <c>Notifications</c>.
    ///     </para>
    ///     <para>
    ///         The <c>INotifier</c> interface provides a common method called
    ///         <c>sendNotification</c> that relieves implementation code of 
    ///         the necessity to actually construct <c>Notifications</c>.
    ///     </para>
    ///     <para>
    ///         The <c>Notifier</c> class, which all of the above mentioned classes
    ///         extend, provides an initialized reference to the <c>Facade</c>
    ///         Multiton, which is required for the convienience method
    ///         for sending <c>Notifications</c>, but also eases implementation as these
    ///         classes have frequent <c>Facade</c> interactions and usually require
    ///         access to the facade anyway.
    ///     </para>
    ///     <para>
    ///         NOTE: In the MultiCore version of the framework, there is one caveat to
    ///         notifiers, they cannot send notifications or reach the facade until they
    ///         have a valid multitonKey.
    ///         The multitonKey is set:
    ///         <list type="bullet">
    ///             <item>on a Command when it is executed by the Controller</item>
    ///             <item>on a Mediator is registered with the View</item>
    ///             <item>on a Proxy is registered with the Model.</item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <seealso cref="PureMVC.Patterns.Proxy.Proxy"/>
    /// <seealso cref="PureMVC.Patterns.Facade.Facade"/>
    /// <seealso cref="PureMVC.Patterns.Mediator.Mediator"/>
    /// <seealso cref="PureMVC.Patterns.Command.MacroCommand"/>
    /// <seealso cref="PureMVC.Patterns.Command.SimpleCommand"/>
    public class Notifier: INotifier
    {
        /// <summary>
        /// Create and send an <c>INotification</c>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Keeps us from having to construct new INotification 
        ///         instances in our implementation code.
        ///     </para>
        /// </remarks>
        /// <param name="notificationName">the name of the notiification to send</param>
        /// <param name="body">the body of the notification (optional)</param>
        /// <param name="type">the type of the notification (optional)</param>
        public virtual void SendNotification(string notificationName, object body = null, string type = null)
        {
            Facade.SendNotification(notificationName, body, type);
        }

        /// <summary>
        /// Initialize this INotifier instance.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is how a Notifier gets its multitonKey. 
        ///         Calls to sendNotification or to access the
        ///         facade will fail until after this method 
        ///         has been called.
        ///     </para>
        ///     <para>
        ///         Mediators, Commands or Proxies may override 
        ///         this method in order to send notifications
        ///         or access the Multiton Facade instance as
        ///         soon as possible. They CANNOT access the facade
        ///         in their constructors, since this method will not
        ///         yet have been called.
        ///     </para>
        /// </remarks>
        /// <param name="key">the multitonKey for this INotifier to use</param>
        public void InitializeNotifier(string key)
        {
            MultitonKey = key;
        }

        /// <summary> Return the Multiton Facade instance</summary>
        protected IFacade Facade
        {
            get {
                if (MultitonKey == null) throw new Exception(MULTITON_MSG);
                return Patterns.Facade.Facade.GetInstance(MultitonKey, key => new Facade.Facade(key));
            }
        }

        /// <summary>The Multiton Key for this app</summary>
        public string MultitonKey { get; protected set; }

        /// <summary>Message Constants</summary>
        protected string MULTITON_MSG = "multitonKey for this Notifier not yet initialized!";
    }
}

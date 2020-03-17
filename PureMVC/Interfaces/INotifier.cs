//
//  PureMVC C# Multicore
//
//  Copyright(c) 2020 Saad Shams <saad.shams@puremvc.org>
//  Your reuse is governed by the Creative Commons Attribution 3.0 License
//

namespace PureMVC.Interfaces
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
    /// </remarks>
    /// <seealso cref="IFacade"/>
    /// <seealso cref="INotification"/>
    public interface INotifier
    {
        /// <summary>
        /// Send a <c>INotification</c>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Convenience method to prevent having to construct new 
        ///         notification instances in our implementation code.
        ///     </para>
        /// </remarks>
        /// <param name="notificationName">the name of the notification to send</param>
        /// <param name="body">the body of the notification (optional)</param>
        /// <param name="type">the type of the notification (optional)</param>
        void SendNotification(string notificationName, object body = null, string type = null);

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
        /// </remarks>
        /// <param name="key">the multitonKey for this INotifier to use</param>
        void InitializeNotifier(string key);
    }
}

/* 
 PureMVC C# Multi-Core Port by Tang Khai Phuong <phuong.tang@puremvc.org>, et al.
 PureMVC - Copyright(c) 2006-08 Futurescale, Inc., Some rights reserved. 
 Your reuse is governed by the Creative Commons Attribution 3.0 License 
*/

#region Using

using System;
using PureMVC.Interfaces;

#endregion

namespace PureMVC.Patterns
{
    /// <summary>
    /// A base <c>INotification</c> implementation
    /// </summary>
    /// <remarks>
    ///     <para>PureMVC does not rely upon underlying event models</para>
    ///     <para>The Observer Pattern as implemented within PureMVC exists to support event-driven communication between the application and the actors of the MVC triad</para>
    ///     <para>Notifications are not meant to be a replacement for Events. Generally, <c>IMediator</c> implementors place event handlers on their view components, which they then handle in the usual way. This may lead to the broadcast of <c>Notification</c>s to trigger <c>ICommand</c>s or to communicate with other <c>IMediators</c>. <c>IProxy</c> and <c>ICommand</c> instances communicate with each other and <c>IMediator</c>s by broadcasting <c>INotification</c>s</para>
    /// </remarks>
    /// <see cref="PureMVC.Patterns.Observer"/>
    [Serializable]
    public class Notification : INotification
    {
        #region Constructors

        /// <summary>
        /// Constructs a new notification with the specified name, default body and type
        /// </summary>
        /// <param name="name">The name of the <c>Notification</c> instance</param>
        public Notification(string name)
            : this(name, null, null)
        { }

        /// <summary>
        /// Constructs a new notification with the specified name and body, with the default type
        /// </summary>
        /// <param name="name">The name of the <c>Notification</c> instance</param>
        /// <param name="body">The <c>Notification</c>s body</param>
        public Notification(string name, object body)
            : this(name, body, null)
        { }

        /// <summary>
        /// Constructs a new notification with the specified name, body and type
        /// </summary>
        /// <param name="name">The name of the <c>Notification</c> instance</param>
        /// <param name="body">The <c>Notification</c>s body</param>
        /// <param name="type">The type of the <c>Notification</c></param>
        public Notification(string name, object body, string type)
        {
            Name = name;
            Body = body;
            Type = type;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the string representation of the <c>Notification instance</c>
        /// </summary>
        /// <returns>The string representation of the <c>Notification</c> instance</returns>
        public override string ToString()
        {
            var msg = "Notification Name: " + Name;
            msg += Environment.NewLine + "Body:" + ((Body == null) ? "null" : Body.ToString());
            msg += Environment.NewLine + "Type:" + (Type ?? "null");
            return msg;
        }

        #endregion

        #region Accessors

        /// <summary>
        /// The name of the <c>Notification</c> instance
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The body of the <c>Notification</c> instance
        /// </summary>
        /// <remarks>This accessor is thread safe</remarks>
        public object Body { get; set; }

        /// <summary>
        /// The type of the <c>Notification</c> instance
        /// </summary>
        /// <remarks>This accessors is thread safe</remarks>
        public string Type { get; set; }

        #endregion

        #region Members

        #endregion
    }
}

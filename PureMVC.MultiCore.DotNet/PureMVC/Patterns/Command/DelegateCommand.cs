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
    /// A base <c>ICommand</c> implementation
    /// </summary>
    /// <remarks>
    ///     <para>Your subclass should override the <c>execute</c> method where your business logic will handle the <c>INotification</c></para>
    /// </remarks>
    /// <see cref="PureMVC.Core.Controller"/>
    /// <see cref="PureMVC.Patterns.Notification"/>
    /// <see cref="PureMVC.Patterns.MacroCommand"/>
    public class DelegateCommand : Notifier, ICommand
    {
        #region Public Methods

        /// <summary>
        /// Construct command with delegate.
        /// </summary>
        /// <param name="action">The delegate method</param>
        public DelegateCommand(Action<INotification> action)
        {
            m_action = action;
        }

        #region ICommand Members

        /// <summary>
        /// Fulfill the use-case initiated by the given <c>INotification</c>
        /// </summary>
        /// <param name="notification">The <c>INotification</c> to handle</param>
        /// <remarks>
        ///     <para>In the Command Pattern, an application use-case typically begins with some user action, which results in an <c>INotification</c> being broadcast, which is handled by business logic in the <c>execute</c> method of an <c>ICommand</c></para>
        /// </remarks>
        public virtual void Execute(INotification notification)
        {
            m_action(notification);
        }

        #endregion

        #endregion

        private readonly Action<INotification> m_action;
    }
}

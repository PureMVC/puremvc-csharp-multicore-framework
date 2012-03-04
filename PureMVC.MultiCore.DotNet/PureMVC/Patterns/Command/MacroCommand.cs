/* 
 PureMVC C# Multi-Core Port by Tang Khai Phuong <phuong.tang@puremvc.org>, et al.
 PureMVC - Copyright(c) 2006-08 Futurescale, Inc., Some rights reserved. 
 Your reuse is governed by the Creative Commons Attribution 3.0 License 
*/

#region Using

using System;
using System.Collections.Generic;
using PureMVC.Interfaces;

#endregion

namespace PureMVC.Patterns
{
    /// <summary>
    /// A base <c>ICommand</c> implementation that executes other <c>ICommand</c>s
    /// </summary>
    /// <remarks>
    ///     <para>A <c>MacroCommand</c> maintains an list of <c>ICommand</c> Class references called <i>SubCommands</i></para>
    ///     <para>When <c>execute</c> is called, the <c>MacroCommand</c> instantiates and calls <c>execute</c> on each of its <i>SubCommands</i> turn. Each <i>SubCommand</i> will be passed a reference to the original <c>INotification</c> that was passed to the <c>MacroCommand</c>'s <c>execute</c> method</para>
    ///     <para>Unlike <c>SimpleCommand</c>, your subclass should not override <c>execute</c>, but instead, should override the <c>initializeMacroCommand</c> method, calling <c>addSubCommand</c> once for each <i>SubCommand</i> to be executed</para>
    /// </remarks>
    /// <see cref="PureMVC.Core.Controller"/>
    /// <see cref="PureMVC.Patterns.Notification"/>
    /// <see cref="PureMVC.Patterns.SimpleCommand"/>
    public class MacroCommand : Notifier, ICommand
    {
        #region Constructors

        /// <summary>
        /// Constructs a new macro command
        /// </summary>
        /// <remarks>
        ///     <para>You should not need to define a constructor, instead, override the <c>initializeMacroCommand</c> method</para>
        ///     <para>If your subclass does define a constructor, be sure to call <c>super()</c></para>
        /// </remarks>
        public MacroCommand()
        {
            m_subCommands = new List<object>();
            InitializeMacroCommand();
        }


        /// <summary>
        /// Constructs a new macro command
        /// </summary>
        /// <remarks>
        ///     <para>You should not need to define a constructor, instead, override the <c>initializeMacroCommand</c> method</para>
        ///     <para>If your subclass does define a constructor, be sure to call <c>super()</c></para>
        /// </remarks>
        /// <param name="types">The list of command type.</param>
        public MacroCommand(IEnumerable<Type> types)
        {
            m_subCommands = new List<object>(types);
            InitializeMacroCommand();
        }

        /// <summary>
        /// Constructs a new macro command
        /// </summary>
        /// <remarks>
        ///     <para>You should not need to define a constructor, instead, override the <c>initializeMacroCommand</c> method</para>
        ///     <para>If your subclass does define a constructor, be sure to call <c>super()</c></para>
        /// </remarks>
        /// <param name="commands">The list of command.</param>
        public MacroCommand(IEnumerable<ICommand> commands)
        {
            m_subCommands = new List<object>(commands);
            InitializeMacroCommand();
        }

        /// <summary>
        /// Constructs a new macro command
        /// </summary>
        /// <remarks>
        ///     <para>You should not need to define a constructor, instead, override the <c>initializeMacroCommand</c> method</para>
        ///     <para>If your subclass does define a constructor, be sure to call <c>super()</c></para>
        /// </remarks>
        /// <param name="commandCollection">The list of command/command type</param>
        public MacroCommand(IEnumerable<object> commandCollection)
        {
            m_subCommands = new List<object>(commandCollection);
            InitializeMacroCommand();
        }

        #endregion

        #region Public Methods

        #region ICommand Members

        /// <summary>
        /// Execute this <c>MacroCommand</c>'s <i>SubCommands</i>
        /// </summary>
        /// <param name="notification">The <c>INotification</c> object to be passsed to each <i>SubCommand</i></param>
        /// <remarks>
        ///     <para>The <i>SubCommands</i> will be called in First In/First Out (FIFO) order</para>
        /// </remarks>
        public void Execute(INotification notification)
        {
            while (m_subCommands.Count > 0)
            {
                var commandType = m_subCommands[0] as Type;
                if (commandType != null)
                {
                    var commandInstance = Activator.CreateInstance(commandType);

                    if (commandInstance is ICommand)
                    {
                        var command = (ICommand)commandInstance;
                        command.InitializeNotifier(MultitonKey);
                        command.Execute(notification);
                    }
                }
                else
                {
                    var command = m_subCommands[0] as ICommand;
                    if (command != null)
                    {
                        command.InitializeNotifier(MultitonKey);
                        command.Execute(notification);
                    }
                }

                m_subCommands.RemoveAt(0);
            }
        }

        #endregion

        #endregion

        #region Protected & Internal Methods

        /// <summary>
        /// Initialize the <c>MacroCommand</c>
        /// </summary>
        /// <remarks>
        ///     <para>In your subclass, override this method to initialize the <c>MacroCommand</c>'s <i>SubCommand</i> list with <c>ICommand</c> class references like this:</para>
        ///     <example>
        ///         <code>
        ///             // Initialize MyMacroCommand
        ///             protected override initializeMacroCommand( )
        ///             {
        ///                 addSubCommand( com.me.myapp.controller.FirstCommand );
        ///                 addSubCommand( com.me.myapp.controller.SecondCommand );
        ///                 addSubCommand( com.me.myapp.controller.ThirdCommand );
        ///             }
        ///         </code>
        ///     </example>
        ///     <para>Please aware that <i>SubCommand</i>s may be any <c>ICommand</c> implementor, <c>MacroCommand</c>s or <c>SimpleCommands</c> are both acceptable</para>
        /// </remarks>
        protected virtual void InitializeMacroCommand()
        {
        }

        /// <summary>
        /// Add a <i>SubCommand</i>
        /// </summary>
        /// <param name="commandType">A a reference to the <c>Type</c> of the <c>ICommand</c></param>
        /// <remarks>
        ///     <para>The <i>SubCommands</i> will be called in First In/First Out (FIFO) order</para>
        /// </remarks>
        protected void AddSubCommand(Type commandType)
        {
            m_subCommands.Add(commandType);
        }

        /// <summary>
        /// Add a <i>SubCommand</i>
        /// </summary>
        /// <param name="command">A a reference to the <c>ICommand</c></param>
        /// <remarks>
        ///     <para>The <i>SubCommands</i> will be called in First In/First Out (FIFO) order</para>
        /// </remarks>
        protected void AddSubCommand(ICommand command)
        {
            m_subCommands.Add(command);
        }

        #endregion

        #region Members

        private readonly IList<object> m_subCommands;

        #endregion
    }
}

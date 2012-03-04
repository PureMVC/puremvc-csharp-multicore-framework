/* 
 PureMVC C# Multi-Core Port by Tang Khai Phuong <phuong.tang@puremvc.org>, et al.
 PureMVC - Copyright(c) 2006-08 Futurescale, Inc., Some rights reserved. 
 Your reuse is governed by the Creative Commons Attribution 3.0 License 
*/

using System.Collections.Generic;

using PureMVC.Interfaces;
using PureMVC.Patterns;

namespace PureMVC.Tests.Core
{
    /**
     * A Mediator class used by ViewTest.
     * 
     * @see org.puremvc.as3.core.view.ViewTest ViewTest
     */
    public class ViewTestMediator6 : Mediator
    {
        /**
         * The Mediator base name
         */
        public new static string NAME = "ViewTestMediator6";

        /**
         * Constructor
         */
        public ViewTestMediator6(string name, object view)
            : base(name, view)
        {
        }

        public override IEnumerable<string> ListNotificationInterests
        {
            get { return new List<string>(new[] { ViewTest.NOTE6 }); }
        }

        public override void HandleNotification(INotification note)
        {
            Facade.RemoveMediator(MediatorName);
        }

        public override void OnRemove()
        {
            ViewTest.counter++;
        }

        public ViewTest ViewTest
        {
            get { return (ViewTest)m_viewComponent; }
        }
    }
}
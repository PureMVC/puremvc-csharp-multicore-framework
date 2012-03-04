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
    public class ViewTestMediator5 : Mediator
    {
        /**
         * The Mediator name
         */
        public new static string NAME = "ViewTestMediator5";

        /**
         * Constructor
         */
        public ViewTestMediator5(object view)
            : base(NAME, view)
        {
        }

        public override IEnumerable<string> ListNotificationInterests
        {
            get { return new List<string>(new[] { ViewTest.NOTE5 }); }
        }

        public override void HandleNotification(INotification note)
        {
            ViewTest.counter++;
        }

        public ViewTest ViewTest
        {
            get { return (ViewTest)m_viewComponent; }
        }
    }
}
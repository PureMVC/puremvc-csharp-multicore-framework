/* 
 PureMVC C# Multi-Core Port by Tang Khai Phuong <phuong.tang@puremvc.org>, et al.
 PureMVC - Copyright(c) 2006-08 Futurescale, Inc., Some rights reserved. 
 Your reuse is governed by the Creative Commons Attribution 3.0 License 
*/

using PureMVC.Patterns;

namespace PureMVC.Tests.Core
{
   	/**
  	 * A Mediator class used by ViewTest.
  	 * 
  	 * @see org.puremvc.as3.core.view.ViewTest ViewTest
  	 */
	public class ViewTestMediator4 : Mediator 
	{
		/**
		 * The Mediator name
		 */
		public new static string NAME = "ViewTestMediator4";
				
		/**
		 * Constructor
		 */
		public ViewTestMediator4(object view)
			: base(NAME, view)
		{
		}

        public ViewTest ViewTest
		{
			get { return (ViewTest) m_viewComponent; }
		}
				
		public override void OnRegister()
		{
			ViewTest.onRegisterCalled = true;
		}
				
		public override  void OnRemove()
		{
			ViewTest.onRemoveCalled = true;
		}
				
				
	}
}
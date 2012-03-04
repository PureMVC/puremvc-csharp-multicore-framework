/* 
 PureMVC C# Multi-Core Port by Tang Khai Phuong <phuong.tang@puremvc.org>, et al.
 PureMVC - Copyright(c) 2006-08 Futurescale, Inc., Some rights reserved. 
 Your reuse is governed by the Creative Commons Attribution 3.0 License 
*/

using PureMVC.Interfaces;
using PureMVC.Patterns;

namespace PureMVC.Tests.Core
{
    /**
	 * A SimpleCommand subclass used by ControllerTest.
	 *
  	 * @see org.puremvc.csharp.core.controller.ControllerTest ControllerTest
  	 * @see org.puremvc.csharp.core.controller.ControllerTestVO ControllerTestVO
	 */
    public class ControllerTestCommand : SimpleCommand
    {
        /**
		 * Constructor.
		 */
        public ControllerTestCommand()
        { }

        /**
		 * Fabricate a result by multiplying the input by 2
		 * 
		 * @param notification the carrying the ControllerTestVO
		 */
        override public void Execute(INotification notification)
		{

            var vo = (ControllerTestVO)notification.Body;
			
			// Fabricate a result
			vo.result = 2 * vo.input;

		}
    }
}

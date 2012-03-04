/* 
 PureMVC C# Multi-Core Port by Tang Khai Phuong <phuong.tang@puremvc.org>, et al.
 PureMVC - Copyright(c) 2006-08 Futurescale, Inc., Some rights reserved. 
 Your reuse is governed by the Creative Commons Attribution 3.0 License 
*/

namespace PureMVC.Tests.Core
{
    /**
  	 * A utility class used by ControllerTest.
  	 * 
  	 * @see org.puremvc.csharp.core.controller.ControllerTest ControllerTest
  	 * @see org.puremvc.csharp.core.controller.ControllerTestCommand ControllerTestCommand
  	 */
    public class ControllerTestVO
    {
        /**
		 * Constructor.
		 * 
		 * @param input the number to be fed to the ControllerTestCommand
		 */
		public ControllerTestVO (int input)
        {
			this.input = input;
		}

		public int input;
		public int result;
    }
}

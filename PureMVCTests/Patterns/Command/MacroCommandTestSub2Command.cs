//
//  PureMVC C# Multicore
//
//  Copyright(c) 2020 Saad Shams <saad.shams@puremvc.org>
//  Your reuse is governed by the Creative Commons Attribution 3.0 License
//

using PureMVC.Interfaces;

namespace PureMVC.Patterns.Command
{
    /// <summary>
    /// A SimpleCommand subclass used by MacroCommandTestCommand.
    /// </summary>
    /// <seealso cref="MacroCommandTest"/>
    /// <seealso cref="MacroCommandTestCommand"/>
    /// <seealso cref="MacroCommandTestVO"/>
    public class MacroCommandTestSub2Command: SimpleCommand
    {
        /// <summary>
        /// Fabricate a result by multiplying the input by itself
        /// </summary>
        /// <param name="notification">notification the <c>INotification</c> carrying the <c>MacroCommandTestVO</c></param>
        public override void Execute(INotification notification)
        {
            var vo = (MacroCommandTestVO)notification.Body;

            // Fabricate a result
            vo.Result2 = vo.Input * vo.Input;
        }
    }
}

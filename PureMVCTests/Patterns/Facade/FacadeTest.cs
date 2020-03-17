//
//  PureMVC C# Multicore
//
//  Copyright(c) 2020 Saad Shams <saad.shams@puremvc.org>
//  Your reuse is governed by the Creative Commons Attribution 3.0 License
//

using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PureMVC.Interfaces;

namespace PureMVC.Patterns.Facade
{
    /// <summary>
    /// Test the PureMVC Facade class.
    /// </summary>
    /// <seealso cref="FacadeTestVO"/>
    /// <seealso cref="FacadeTestCommand"/>
    [TestClass]
    public class FacadeTest
    {
        /// <summary>
        /// Tests the Facade Multiton Factory Method 
        /// </summary>
        [TestMethod]
        public void TestGetInstance()
        {
            // Test Factory Method
            var facade = Facade.GetInstance("FacadeTestKey1", key => new Facade(key));

            // test assertions
            Assert.IsTrue(facade != null, "Expecting instance not null");
            Assert.IsTrue(facade is IFacade, "Expecting instance implements IFacade");
        }
        
        /// <summary>
        /// Tests the Facade Thread Safety
        /// </summary>
        [TestMethod]
        public void TestGetInstanceThreadSafety()
        {
            var instances = new List<IFacade>();
            for (var i = 0; i < 1000; i++)
            {
                new Thread(() =>
                {
                    instances.Add(Facade.GetInstance("ThreadSafety", key => new Facade(key)));
                }).Start();
            }

            // test assertions
            for (int i = 1, count = instances.Count; i < count; i++)
            {
                Assert.AreEqual(instances[0], instances[i]);
            }

            Facade.GetInstance("ThreadSafety", key => new Facade(key));
        }

        /// <summary>
        /// Tests Command registration and execution via the Facade.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This test gets a Multiton Facade instance 
        ///         and registers the FacadeTestCommand class 
        ///         to handle 'FacadeTest' Notifcations.
        ///     </para>
        ///     <para>
        ///         It then sends a notification using the Facade. 
        ///         Success is determined by evaluating 
        ///         a property on an object placed in the body of
        ///         the Notification, which will be modified by the Command.
        ///     </para>
        /// </remarks>
        [TestMethod]
        public void TestRegisterCommandAndSendNotification()
        {
            // Create the Facade, register the FacadeTestCommand to 
            // handle 'FacadeTest' notifications
            var facade = Facade.GetInstance("FacadeTestKey2", key => new Facade(key));
            facade.RegisterCommand("FacadeTestNote", () => new FacadeTestCommand());

            // Send notification. The Command associated with the event
            // (FacadeTestCommand) will be invoked, and will multiply 
            // the vo.input value by 2 and set the result on vo.result
            var vo = new FacadeTestVO(32);
            facade.SendNotification("FacadeTestNote", vo);

            // test assertions
            Assert.IsTrue(vo.result == 64, "Expecting vo.result == 64");
        }

        /// <summary>
        /// Tests Command removal via the Facade.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This test gets a Multiton Facade instance 
        ///         and registers the FacadeTestCommand class 
        ///         to handle 'FacadeTest' Notifcations. Then it removes the command.
        ///     </para>
        ///     <para>
        ///         It then sends a Notification using the Facade. 
        ///         Success is determined by evaluating 
        ///         a property on an object placed in the body of
        ///         the Notification, which will NOT be modified by the Command.
        ///     </para>
        /// </remarks>
        [TestMethod]
        public void TestRegisterAndRemoveCommandAndSendNotification()
        {
            // Create the Facade, register the FacadeTestCommand to 
            // handle 'FacadeTest' events
            var facade = Facade.GetInstance("FacadeTestKey3", key => new Facade(key));
            facade.RegisterCommand("FacadeTestNote", () => new FacadeTestCommand());
            facade.RemoveCommand("FacadeTestNote");

            // Send notification. The Command associated with the event
            // (FacadeTestCommand) will NOT be invoked, and will NOT multiply 
            // the vo.input value by 2 
            var vo = new FacadeTestVO(32);
            facade.SendNotification("FacadeTestNote", vo);

            // test assertions 
            Assert.IsTrue(vo.result != 64, "Expecting vo.result != 64");
        }

        /// <summary>
        /// Tests the regsitering and retrieving Model proxies via the Facade.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Tests <c>registerProxy</c> and <c>retrieveProxy</c> in the same test.
        ///         These methods cannot currently be tested separately
        ///         in any meaningful way other than to show that the
        ///         methods do not throw exception when called.
        ///     </para>
        /// </remarks>
        [TestMethod]
        public void TestRegisterAndRetrieveProxy()
        {
            // register a proxy and retrieve it.
            var facade = Facade.GetInstance("FacadeTestKey4", key => new Facade(key));
            facade.RegisterProxy(new Proxy.Proxy("colors", new [] { "red", "green", "blue" }));
            var proxy = facade.RetrieveProxy("colors");

            // test assertions
            Assert.IsTrue(proxy != null, "Expecting proxy != null");

            // retrieve data from proxy
            var data = (string[])proxy.Data;

            // test assertions
            Assert.IsNotNull(data, "Expecting data not null");
            Assert.IsTrue(data.Length == 3, "Expecting data.Length == 3");
            Assert.IsTrue(data[0] == "red", "Expected data[0] == 'red'");
            Assert.IsTrue(data[1] == "green", "Expecting data[1] == 'green'");
            Assert.IsTrue(data[2] == "blue", "Expecting data[2] == 'blue'");
        }

        /// <summary>
        /// Tests the removing Proxies via the Facade.
        /// </summary>
        [TestMethod]
        public void TestRegisterAndRemoveProxy()
        {
            // register a proxy, remove it, then try to retrieve it
            var facade = Facade.GetInstance("FacadeTestKey5", key => new Facade(key));
            IProxy proxy = new Proxy.Proxy("sizes", new [] { "7", "13", "21" });
            facade.RegisterProxy(proxy);

            // remove the proxy
            var removedProxy = facade.RemoveProxy("sizes");

            // assert that we removed the appropriate proxy
            Assert.IsTrue(removedProxy.ProxyName == "sizes", "Expecting removedProxy.ProxyName == 'sizes'");

            // make sure we can no longer retrieve the proxy from the model
            proxy = facade.RemoveProxy("sizes");

            // test assertions
            Assert.IsNull(proxy, "Expecting proxy is null");
        }

        /// <summary>
        /// Tests registering, retrieving and removing Mediators via the Facade.
        /// </summary>
        [TestMethod]
        public void TestRegisterRetrieveAndRemoveMediator()
        {
            // register a mediator, remove it, then try to retrieve it
            var facade = Facade.GetInstance("FacadeTestKey6", key => new Facade(key));
            facade.RegisterMediator(new Mediator.Mediator(Mediator.Mediator.NAME, new object()));

            // retrieve the mediator
            Assert.IsNotNull(facade.RetrieveMediator(Mediator.Mediator.NAME));

            // remove the mediator
            var removedMediator = facade.RemoveMediator(Mediator.Mediator.NAME);

            // assert that we have removed the appropriate mediator
            Assert.IsTrue(removedMediator.MediatorName == Mediator.Mediator.NAME, "Expecting removedMediator.MediatorName == Mediator.NAME");
        }

        /// <summary>
        /// Tests the hasProxy Method
        /// </summary>
        [TestMethod]
        public void TestHasProxy()
        {
            // register a Proxy
            var facade = Facade.GetInstance("FacadeTestKey7", key => new Facade(key));
            facade.RegisterProxy(new Proxy.Proxy("hasProxyTest", new [] { 1, 2, 3 }));

            // assert that the model.hasProxy method returns true
            // for that proxy name
            Assert.IsTrue(facade.HasProxy("hasProxyTest"), "Expecting facade.hasProxy('hasProxyTest') == true");
        }

        /// <summary>
        /// Tests the hasMediator Method
        /// </summary>
        [TestMethod]
        public void TestHasMediator()
        {
            // register a Mediator
            var facade = Facade.GetInstance("FacadeTestKey8", key => new Facade(key));
            facade.RegisterMediator(new Mediator.Mediator("facadeHasMediatorTest", new object()));

            // assert that the facade.hasMediator method returns true
            // for that mediator name
            Assert.IsTrue(facade.HasMediator("facadeHasMediatorTest") == true, "Expecting facade.HasMediator('facadeHasMediatorTest') == true");

            facade.RemoveMediator("facadeHasMediatorTest");

            // assert that the facade.hasMediator method returns false
            // for that mediator name
            Assert.IsFalse(facade.HasMediator("facadeHasMediatoTest"), "Expecting facade.HasMediator('facadeHasMediatorTest') == false");
        }

        /// <summary>
        /// Test hasCommand method.
        /// </summary>
        [TestMethod]
        public void TestHasCommand()
        {
            // register the ControllerTestCommand to handle 'hasCommandTest' notes
            var facade = Facade.GetInstance("FacadTestKey10", key => new Facade(key));
            facade.RegisterCommand("facadeHasCommandTest", () => new FacadeTestCommand());

            // test that hasCommand returns true for hasCommandTest notifications 
            Assert.IsTrue(facade.HasCommand("facadeHasCommandTest"), "Expecting facade.HasCommand('facadeHasCommandTest') == true");

            // Remove the Command from the Controller
            facade.RemoveCommand("facadeHasCommandTest");

            // test that hasCommand returns false for hasCommandTest notifications 
            Assert.IsFalse(facade.HasCommand("facadeHasCommandTest"), "facade.HasCommand('facadeHasCommandTest')");
        }

        /// <summary>
        /// Tests the hasCore and removeCore methods
        /// </summary>
        [TestMethod]
        public void TestHasCoreAndRemoveCore()
        {
            // assert that the Facade.hasCore method returns false first
            Assert.IsFalse(Facade.HasCore("FacadeTestKey11"), "Expecting Facade.HasCore('FacadeTestKey11') == false");

            // register a Core
            Facade.GetInstance("FacadeTestKey11", key => new Facade(key));

            // assert that the Facade.hasCore method returns true now that a Core is registered
            Assert.IsTrue(Facade.HasCore("FacadeTestKey11"), "Expecting Facade.HasCore('FacadeTestKey11') == true");

            // remove the Core
            Facade.RemoveCore("FacadeTestKey11");

            // assert that the Facade.hasCore method returns false now that the core has been removed.
            Assert.IsFalse(Facade.HasCore("FacadeTestKey11"), "Expecting Facade.HasCore('FacadeTestKey11') == false");
        }

        /// <summary>
        /// Tests the multiton instances
        /// </summary>
        [TestMethod]
        public void TestMultitons()
        {
            var temp1 = Facade.GetInstance("A", k => new Facade(k));
            var temp2 = Facade.GetInstance("A", k => new Facade(k));

            Assert.IsTrue(temp1 == temp2);

            temp2 = Facade.GetInstance("B", k => new Facade(k));
            
            Assert.IsFalse(temp1 == temp2);
        }
    }
}

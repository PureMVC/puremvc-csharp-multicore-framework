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
using PureMVC.Patterns.Proxy;

namespace PureMVC.Core
{
    /// <summary>
    /// Test the PureMVC Model class.
    /// </summary>
    [TestClass]
    public class ModelTest
    {
        /// <summary>
        /// Tests the Model Multiton Factory Method 
        /// </summary>
        [TestMethod]
        public void TestGetInstance()
        {
            // Test Factory Method
            var model = Model.GetInstance("ModelTestKey", key => new Model(key));

            // test assertions
            Assert.IsNotNull(model, "Expecting instance not null");
            Assert.IsTrue(model != null, "Expecting instance implements IModel");
        }

        /// <summary>
        /// Tests the Model Thread Safety
        /// </summary>
        [TestMethod]
        public void TestGetInstanceThreadSafety()
        {
            var instances = new List<IModel>();
            for (var i = 0; i < 1000; i++)
            {
                new Thread(() =>
                {
                    instances.Add(Model.GetInstance("ThreadSafety", key => new Model(key)));
                }).Start();
            }

            // test assertions
            for (int i = 1, count = instances.Count; i < count; i++)
            {
                Assert.AreEqual(instances[0], instances[i]);
            }

            Model.GetInstance("ThreadSafety", key => new Model(key));
        }

        /// <summary>
        /// Tests the proxy registration and retrieval methods.
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
            var model = Model.GetInstance("ModelTestKey2", key => new Model(key));
            model.RegisterProxy(new Proxy("colors", new string[3]{ "red", "green", "blue" }));
            var proxy = model.RetrieveProxy("colors");
            var data = (string[])proxy.Data;

            // test assertions
            Assert.IsNotNull(data, "Expecting data not null");
            Assert.IsTrue(data != null, "Expecting data type is string[]");
            Assert.IsTrue(data.Length == 3, "Expecting data.length == 3");
            Assert.IsTrue(data[0].Equals("red"), "Expecting data[0].Equals('red')");
            Assert.IsTrue(data[1].Equals("green"), "Expecting data[1].Equals('green')");
            Assert.IsTrue(data[2].Equals("blue"), "Expecting data[2].Equals('blue')");
        }

        /// <summary>
        /// Tests the proxy removal method.
        /// </summary>
        [TestMethod]
        public void TestRegisterAndRemoveProxy()
        {
            // register a proxy, remove it, then try to retrieve it
            var model = Model.GetInstance("ModelTestKey3", key => new Model(key));
            model.RegisterProxy(new Proxy("sizes", new int[3]{ 7, 13, 21 }));
            var removedProxy = model.RemoveProxy("sizes");

            Assert.IsTrue(removedProxy.ProxyName == "sizes", "Expecting removedProxy.ProxyName == 'sizes'");
            var proxy = model.RetrieveProxy("sizes");

            // test assertions
            Assert.IsNull(proxy, "Expecting proxy is null");
        }

        /// <summary>
        /// Tests the hasProxy Method
        /// </summary>
        [TestMethod]
        public void TestHasProxy()
        {
            // register a proxy
            var model = Model.GetInstance("ModelTestKey4", key => new Model(key));
            var proxy = new Proxy("aces", new string[] { "clubs", "spades", "hearts", "diamonds" });
            model.RegisterProxy(proxy);

            // assert that the model.hasProxy method returns true
            // for that proxy name
            Assert.IsTrue(model.HasProxy("aces"), "Expecting model.hasProxy('aces') == true");

            // remove the proxy
            model.RemoveProxy("aces");

            // assert that the model.hasProxy method returns false
            // for that proxy name
            Assert.IsTrue(model.HasProxy("aces") == false, "Expecting model.hasProxy('aces') == false");
        }

        /// <summary>
        /// Tests that the Model calls the onRegister and onRemove methods
        /// </summary>
        [TestMethod]
        public void TestOnRegisterAndOnRemove()
        {
            // Get the Multiton View instance
            var model = Model.GetInstance("ModelTestKey5", key => new Model(key));

            // Create and register the test mediator
            IProxy proxy = new ModelTestProxy();
            model.RegisterProxy(proxy);

            // assert that onRegsiter was called, and the proxy responded by setting its data accordingly
            Assert.IsTrue(proxy.Data.ToString() == ModelTestProxy.ON_REGISTER_CALLED, "Expecting proxy.Data.ToString() == ModelTestProxy.ON_REGISTER_CALLED");

            // Remove the component
            model.RemoveProxy(ModelTestProxy.NAME);

            // assert that onRemove was called, and the proxy responded by setting its data accordingly
            Assert.IsTrue(proxy.Data.ToString() == ModelTestProxy.ON_REMOVE_CALLED, "Expecting proxy.Data.ToString() == ModelTestProxy.ON_REMOVE_CALLED");
        }

        /// <summary>
        /// Tests the multiton instances
        /// </summary>
        [TestMethod]
        public void TestMultitons()
        {
            var temp1 = Model.GetInstance("A", k => new Model(k));
            var temp2 = Model.GetInstance("A", k => new Model(k));
            
            Assert.IsTrue(temp1 == temp2);

            temp2 = Model.GetInstance("B", k => new Model(k));
            
            Assert.IsFalse(temp1 == temp2);
        }
    }
}

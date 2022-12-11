using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.OperationModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpcHelper.Tests
{
    /// <summary>
    /// https://opclabs.doc-that.com/files/onlinedocs/QuickOpc/Latest/User's%20Guide%20and%20Reference-QuickOPC/OpcLabs.EasyOpcUA~OpcLabs.EasyOpc.UA.EasyUAClient.html#ExampleBookmark
    /// </summary>
    public class KepserverEx6ClientTest
    {
        private KepserverEx6Client _client;

        [SetUp]
        public void Setup()
        {
            _client = new KepserverEx6Client();
        }

        [TestCase("opc.tcp://localhost:49320", "ns=2;s=Channel1.BIENTAN.ap_suat")]
        public void ReadNode(string linkServer,
            string linkNode)
        {
            var value = _client.ReadNode<int>(linkServer, linkNode);
            Assert.That(value.Value, Is.GreaterThanOrEqualTo(0));
        }

        //[Test]
        //public async bool SubscribeChanges<T>(string linkServer,
        //    string linkNode,
        //    Action<T> callbackOnData,
        //    CancellationToken token,
        //    out string msgError)
        //{

        //}
    }
}

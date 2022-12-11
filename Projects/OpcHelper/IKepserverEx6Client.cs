using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OpcHelper
{
    /// <summary>
    /// https://opclabs.doc-that.com/files/onlinedocs/QuickOpc/Latest/User's%20Guide%20and%20Reference-QuickOPC/OpcLabs.EasyOpcUA~OpcLabs.EasyOpc.UA.EasyUAClient.html#ExampleBookmark
    /// </summary>
    public interface IKepserverEx6Client
    {
        /// <summary>
        /// Read one node
        /// </summary>
        /// <param name="linkServer"></param>
        /// <param name="linkNode"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        KepserverEx6Result<T> ReadNode<T>(
            string linkServer,
            string linkNode);

        IBaseResult<Guid> SubscribeChanges(
            string linkServer,
            string linkNode,
            Action<KepserverEx6Result<object>> callbackOnData);
    }
}

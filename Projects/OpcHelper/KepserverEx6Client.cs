using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.OperationModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpcHelper
{
    /// <summary>
    /// https://opclabs.doc-that.com/files/onlinedocs/QuickOpc/Latest/User's%20Guide%20and%20Reference-QuickOPC/OpcLabs.EasyOpcUA~OpcLabs.EasyOpc.UA.EasyUAClient.html#ExampleBookmark
    /// </summary>
    public class KepserverEx6Client : IKepserverEx6Client, IDisposable
    {
        public void Dispose()
        {
            lock (_objLock)
            {
                foreach (var item in _mapCLientSubscribe)
                {
                    item.Value.UAClient.UnsubscribeAllMonitoredItems();
                }
                _mapCLientSubscribe.Clear();
            }
        }

        public KepserverEx6Result<T> ReadNode<T>(string linkServer, string linkNode)
        {
            try
            {
                using (var client = new EasyUAClient())
                {
                    var attributeData = client.Read(linkServer, linkNode);
                    // Display results
                    Debug.WriteLine($"ServerTimestamp: {attributeData?.ServerTimestamp}");
                    var value = Convert.ChangeType(attributeData.Value, typeof(T));
                    if (value is T r)
                    {
                        return new KepserverEx6Result<T>
                        {
                            Time = DateTime.Now,
                            IsSuccess = true,
                            Message = null,
                            Status = attributeData.StatusCode.StatusInfo.ToStatusInfoKepex(),
                            Value = r,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new KepserverEx6Result<T>
                {
                    Time = DateTime.Now,
                    IsSuccess = false,
                    Message = ex.Message,
                    Status = StatusInfoKepex.Error,
                    Value = default,
                };
            }
            return default;
        }


        class MySubscribeItem
        {
            public Guid ID { get; set; }
            public Action<KepserverEx6Result<object>> CallbackOnData { get; set; }
        }

        class MySubscribe
        {
            public List<MySubscribeItem> Callbacks { get; set; } = new List<MySubscribeItem>();
            public EasyUAClient UAClient { get; set; }
        }

        static readonly object _objLock = new object();

        static Dictionary<string, MySubscribe> _mapCLientSubscribe = new Dictionary<string, MySubscribe>();


        /// <summary>
        /// UnsubscribeAllMonitoredItems
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="linkServer"></param>
        /// <param name="linkNode"></param>
        /// <param name="callbackOnData"></param>
        /// <param name="token"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        public IBaseResult<Guid> SubscribeChanges(
            string linkServer,
            string linkNode,
            Action<KepserverEx6Result<object>> callbackOnData)
        {
            return BaseResult<Guid>.From(() =>
            {
                var key = $"{linkServer}|{linkNode}";
                var mySubscribeItem = new MySubscribeItem
                {
                    CallbackOnData = callbackOnData,
                    ID = Guid.NewGuid(),
                };
                lock (_objLock)
                {
                    if (!_mapCLientSubscribe.ContainsKey(key))
                    {
                        var subscribe = new MySubscribe
                        {
                            Callbacks = new List<MySubscribeItem>(),
                            UAClient = new EasyUAClient(),
                        };
                        _mapCLientSubscribe.Add(key, subscribe);
                    }
                    _mapCLientSubscribe[key].Callbacks.Add(mySubscribeItem);
                }

                _mapCLientSubscribe[key].UAClient.DataChangeNotification += (s, e) =>
                {
                    try
                    {
                        var r = new KepserverEx6Result<object>();
                        r.IsSuccess = false;
                        if (e.Succeeded)
                        {
                            r.IsSuccess = e.Succeeded;
                            r.Value = e.AttributeData.Value;
                            r.Time = e.AttributeData.HasServerTimestamp ? e.AttributeData.ServerTimestamp : DateTime.Now;
                            r.Status = e.StatusInfo.ToStatusInfoKepex();
                            Parallel.ForEach(_mapCLientSubscribe[key].Callbacks, callback => callback?.CallbackOnData?.Invoke(r));
                        }
                        else
                        {
                            r.Message = e.ErrorMessage;
                            Debug.WriteLine(r.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                };

                Debug.WriteLine($"Subscribing {linkNode}...");
                _mapCLientSubscribe[key].UAClient.SubscribeDataChange(linkServer, linkNode, 1000);
                return mySubscribeItem.ID;
            });
        }
    }
}

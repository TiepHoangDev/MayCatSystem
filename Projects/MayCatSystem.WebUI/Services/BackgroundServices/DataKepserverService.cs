using OpcHelper;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace MayCatSystem.WebUI.Services.BackgroundServices
{
    public interface IDataKepserver
    {

    }

    public class DataKepserverService : BackgroundService, IDataKepserver
    {
        private readonly object _lock_lastData = new object();
        public ConcurrentDictionary<string, DataKepserver> LastDataKepserver { get; private set; } = new ConcurrentDictionary<string, DataKepserver>();

        public event EventHandler<bool>? HaschangeData;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //1. read config
                    var fileConfig = "_data/configs.json";
                    var configs = new List<ConfigKepserver>();
                    if (File.Exists(fileConfig))
                    {
                        try
                        {
                            var allText = File.ReadAllText(fileConfig);
                            configs = JsonSerializer.Deserialize<List<ConfigKepserver>>(allText);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    else
                    {
                        var sampleData = new List<ConfigKepserver> {
                            new ConfigKepserver
                            {
                                LinkServer= "opc.tcp://localhost:49320",
                                LinkNode= "ns=2;s=Channel1.BIENTAN.ap_suat",
                                FixValue = "",
                                LabelText = "Sample",
                                OrderNo = 1,
                            }
                        };
                        var json = JsonSerializer.Serialize(sampleData);
                        File.WriteAllText(fileConfig, json);
                    }

                    //2. register event
                    UpdateLastData(() =>
                    {
                        var client = new KepserverEx6Client();
                        LastDataKepserver.Clear();
                        foreach (var item in configs)
                        {
                            var key = GetKeyConfig(item);
                            if (LastDataKepserver.ContainsKey(key))
                            {
                                Debug.WriteLine($"data duplicate item {item}");
                            }
                            else
                            {
                                LastDataKepserver.TryAdd(key, new DataKepserver()
                                {
                                    LinkNode = item.LinkNode,
                                    LinkServer = item.LinkServer,
                                    FixValue = item.FixValue,
                                    LabelText = item.LabelText,
                                    OrderNo = item.OrderNo,
                                    LastData = client.ReadNode<object>(item.LinkServer, item.LinkNode),
                                });
                            }
                        }
                        return true;
                    });

                    var channelKep = configs?.Where(q => string.IsNullOrWhiteSpace(q.FixValue)).ToList();
                    if (channelKep?.Any() == true)
                    {
                        var client = new KepserverEx6Client();
                        foreach (var item in channelKep)
                        {
                            try
                            {
                                client.SubscribeChanges(item.LinkServer, item.LinkNode, d =>
                                {
                                    UpdateLastData(() =>
                                    {
                                        var key = GetKeyConfig(item);
                                        if (LastDataKepserver.ContainsKey(key))
                                        {
                                            var lastData = LastDataKepserver[key];
                                            lastData.LastData = d;
                                            Debug.WriteLine($"Set new value {lastData}");
                                            return true;
                                        }
                                        return false;
                                    });
                                });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"SubscribeChanges {item} error {ex}");
                            }
                        }
                        var timeReset = TimeSpan.FromDays(1);
                        await Task.Delay(timeReset, stoppingToken);
                    }

                    if (configs?.Any() != true)
                    {
                        Debug.WriteLine($"Configs file {fileConfig} is empty config!!!");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                await Task.Delay(3000, stoppingToken);
            }
        }

        private string GetKeyConfig(ConfigKepserver item)
        {
            return $"KEY_#{item.OrderNo}_{item.LinkServer}_{item.LinkNode}";
        }

        private void UpdateLastData(Func<bool> callUpdate)
        {
            lock (_lock_lastData)
            {
                var haveUpdate = callUpdate?.Invoke() == true;
                HaschangeData?.Invoke(this, haveUpdate);
            }
        }
    }

    public record ConfigKepserver
    {
        public int? OrderNo { get; set; }
        public string? LabelText { get; set; }
        public string? LinkNode { get; set; }
        public string? LinkServer { get; set; }
        public string? FixValue { get; set; }

        public override string ToString()
        {
            return $"LinkNode=[{LinkNode}] \tLinkServer=[{LinkServer}]";
        }
    }



    public record DataKepserver : ConfigKepserver
    {
        public KepserverEx6Result<object>? LastData { get; set; }

        public override string ToString()
        {
            return base.ToString() + $" \t{LastData}";
        }
    }
}

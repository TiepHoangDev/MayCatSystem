#define TEST

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
#if TEST
                                    LastData = new KepserverEx6Result<object>
                                    {
                                        IsSuccess = true,
                                        Status = StatusInfoKepex.Normal,
                                        Time = DateTime.Now,
                                        Value = double.TryParse(item.FixValue, out double f) ? f : null
                                    },
#else
                                    LastData = string.IsNullOrWhiteSpace(item.FixValue) ? client.ReadNode<object>(item.LinkServer, item.LinkNode) : null,
#endif
                                });
                            }
                        }
                        return true;
                    });

                    var channelKep = configs?.Where(q => q.OrderNo < 20
                    && double.TryParse(q.FixValue, out double d)
                    && d > 40
                    && d != 50).ToList();
                    if (channelKep?.Any() == true)
                    {
#if TEST
                        //#elif cannhayso
                        var random = new Random();
                        while (!stoppingToken.IsCancellationRequested)
                        {
                            UpdateLastData(() =>
                            {
                                foreach (var item in channelKep)
                                {
                                    var key = GetKeyConfig(item);
                                    if (LastDataKepserver.ContainsKey(key))
                                    {
                                        var lastData = LastDataKepserver[key];
                                        lastData.LastData = new KepserverEx6Result<object>
                                        {
                                            IsSuccess = true,
                                            Status = StatusInfoKepex.Normal,
                                            Time = DateTime.Now,
                                            Value = Math.Floor(Convert.ToDouble(lastData.LastData?.Value)) + random.Next(0, 99) / 100D
                                        };
                                        Debug.WriteLine($"Set new value {lastData}");
                                    }
                                }
                                return true;
                            });
                            await Task.Delay(5000, stoppingToken);
                        }
#else
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
                        var timeReset = TimeSpan.FromHours(1);
                        await Task.Delay(timeReset, stoppingToken);
#endif
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

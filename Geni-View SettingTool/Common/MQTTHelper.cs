using Geni_View_SettingTool.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geni_View_SettingTool.Common
{
    public class MQTTHelper : IDisposable
    {
        private static readonly Lazy<MQTTHelper> _instance = new Lazy<MQTTHelper>(() => new MQTTHelper());
        public static MQTTHelper Instance => _instance.Value;
        public bool _connectStatus = false;
        public bool _reConnect = false;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private string broker = Global._setting.BrokerIP;
        private int port = 1883;
        private string clientId = "app";

        private string userName = Global._setting.Account;
        private string psw = Global._setting.Password;

        private bool isDispose = false;
        private List<string> topics = new List<string>() { };

        public IMqttClient _client;
        private MqttClientOptions _options;
        public MqttClientOptions Options { get => _options; set => _options = value; }

        //public event PropertyChangedEventHandler PropertyChanged;

        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        #region public functions
        public MQTTHelper()
        {
            try
            {
                // Create a new MQTT client.
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                // Configure options for the client.
                Options = new MqttClientOptionsBuilder()
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                .WithClientId(clientId)
                .WithCredentials(userName, psw)
                .WithTcpServer(broker, port) // Use TCP connection.
                .WithSessionExpiryInterval(uint.MaxValue)
                .WithCleanSession(false)    // Very important, it will affect the QoS receive message flow.
                //.WithKeepAlivePeriod(new TimeSpan(0, 0, 120))
                //.WithTimeout(new TimeSpan(0, 0, 30))
                .WithWillTopic($"device/status/{clientId}")
                .WithWillPayload(GetLastWill())
                .WithWillRetain(true)
                .Build();


                // Setup connecting event handler.
                _client.ConnectingAsync += mqttClient_ConnectingAsync;

                // Setup connected event handler.
                _client.ConnectedAsync += mqttClient_ConnectedAsync;

                // Setup disconnected event handler.
                _client.DisconnectedAsync += mqttClient_DisconnectedAsync;

                // Setup receive message event handler.
                _client.ApplicationMessageReceivedAsync += mqttClient_MessageReceivedAsync;

            }
            catch (AggregateException aggEx)
            {
                foreach (var ex in aggEx.InnerExceptions)
                {
                    _logger.Error($"AggregateException : MQTT Client create failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($" MQTT Client create failed: {ex.Message}");

            }
        }

        public void Setting(string IP, int portVal, string client, string username, string pswVal, List<string> topicVal)
        {
            broker   = IP;
            port     = portVal;
            clientId = client;
            userName = username;
            psw      = pswVal;
            topics   = topicVal;

            Options = new MqttClientOptionsBuilder()
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                .WithClientId(clientId)
                .WithCredentials(userName, psw)
                .WithTcpServer(broker, port) // Use TCP connection.
                .WithSessionExpiryInterval(uint.MaxValue)
                .WithCleanSession(false)    // Very important, it will affect the QoS receive message flow.
                                            //.WithKeepAlivePeriod(new TimeSpan(0, 0, 60))
                                            //.WithTimeout(new TimeSpan(0, 0, 30))
                .WithWillTopic($"device/status/{clientId}")
                .WithWillPayload(GetLastWill())
                .WithWillRetain(true)
                .Build();
        }

        public bool Connect()
        {
            try
            {
                if (_client.IsConnected == false)
                {
                    // Connect to the MQTT broker.
                    var ret = _client.ConnectAsync(Options);

                    ret.Wait();
                }
                else
                {
                    _logger.Info($"Connect already.");

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.CollectInnerException);

                throw;
            }

            return _client.IsConnected;
        }

        public bool Disconnect()
        {
            if (_client.IsConnected == true)
            {
                DeviceStatus deviceStatus = new DeviceStatus()
                {
                    ID = clientId,
                    Type = "SettingTool",
                    Connection = "Disconnect"
                };


                Publish($"{ Global._setting.Battery.status}{clientId}", JsonConvert.SerializeObject(deviceStatus));

                //Disconnect from the MQTT broker.
                _client.DisconnectAsync().Wait();
            }

            _reConnect = false;

            return _client.IsConnected;
        }

        public void Subscribe(string topic, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (!_client.IsConnected)
            {
                _logger.Warn("Client Subscribe : Client does not connect to broker");

                return;
            }
            else
            {
                // Subscribe topic from the MQTT broker.
                _logger.Info($"Client Subscribe : Client Subscribe topic({topic}) from the MQTT broker");
                _client.SubscribeAsync(topic, qosLevel);
            }
        }

        public void Unsubscribe(string topic)
        {
            if (!_client.IsConnected)
            {
                _logger.Warn("Client Unsubscribe : Client does not connect to broker");
                return;
            }
            else
            {
                // Unsubscribe topic from the MQTT broker.
                _logger.Info($"Client Unsubscribe : Client Unsubscribe topic({topic}) from the MQTT broker");
                _client.UnsubscribeAsync(topic);
            }
        }

        public void Publish(string topic, string data, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtMostOnce, bool retain = true)
        {
            if (!_client.IsConnected)
            {
                _logger.Warn("Client Publish : Client does not connect to broker");
                return;
            }
            else
            {
                // Publish data to the MQTT broker.
                _logger.Trace($"Client Publish : Topic={topic}, Payload={data} to broker");
                _client.PublishStringAsync(topic, data, qosLevel, retain);
            }

        }

        //public async Task Publish(string topic, string data, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtMostOnce)
        public async Task<MqttClientPublishResult> PublishAsync(string topic, string data, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtMostOnce,bool retain = true)

        {
            var result = await _client.PublishStringAsync(topic, data, qosLevel, retain);
            _logger.Trace($"Client Publish : Topic={topic}, Payload={data} to broker");
            return result;
        }

        public void Dispose()
        {
            if (_client.IsConnected)
            {
                isDispose = true;
                _connectStatus = false;

                _client.Dispose();
            }
            _logger.Warn($"Client dispose");

        }
        #endregion

        #region event
        private Task mqttClient_ConnectingAsync(MqttClientConnectingEventArgs arg)
        {
            _logger.Info("Client Connecting MQTT broker");
            return Task.CompletedTask;
        }

        private Task mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            _logger.Info("Client Connected MQTT broker");

            _connectStatus = true;
            Global._appViewModel.IsConnected = true;


            foreach (var topic in topics)
            {
                _logger.Info($"Client Subscribe topic {topic}");

                try
                {
                    var ret = _client.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtLeastOnce).Result;
                }
                catch (Exception ex)
                {
                    string msg = ex.CollectInnerException();
                    _logger.Error(JsonConvert.SerializeObject(msg));


                    throw;
                }
            }

            DeviceStatus deviceStatus = new DeviceStatus()
            {
                ID = clientId,
                Type = "SettingTool",
                Connection = "Online"
            };

            Publish($"{ Global._setting.Battery.status}{clientId}", JsonConvert.SerializeObject(deviceStatus), retain : true);

            return Task.CompletedTask;
        }

        private async Task<Task> mqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            _logger.Warn("Client Disconnected MQTT broker");
            _connectStatus = false;
            Global._appViewModel.IsConnected = false;

            try
            {
                if (isDispose == false && _reConnect == true)
                {
                    _logger.Warn("Client Reconnecting MQTT broker");

                    await Task.Delay(new TimeSpan(0, 0, 10));  // Code delay for testing disconnect 10 seconds then reconnect.
                    var retry = _client.ConnectAsync(Options);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Mqtt reconnecting failed", ex);
            }

            return Task.CompletedTask;
        }

        private Task mqttClient_MessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            //When recycling will stop enqueue.
            string topic = arg.ApplicationMessage.Topic;
            var topic2 = arg.ApplicationMessage.Topic.Split('/').ToList();

            string msg = "";

            try
            {
                var array = arg.ApplicationMessage.PayloadSegment;// Use new official property

                if (array != null && array.Array != null)
                {
                    msg = Encoding.UTF8.GetString(array.Array).Replace("\t", "").Replace("\n", "");

                    var json = JToken.Parse(msg);

                    if (json.Type == JTokenType.Object)
                    {
                        var result = JsonConvert.DeserializeObject<CommandResult>(msg);
                        Device device = new Device();

                        device.SN = result.ID;
                        device.Result = result.Result;
                        device.CreateTime = result.DateTimeUTC;

                        switch (result.Cmd)
                        {
                            case "LocalSettingResult":
                                //Global._dashboard.AddOrUpdate(device.SN, device);

                                if (Global._devices.ContainsKey(device.SN))
                                {
                                    Global._devices[device.SN].Broker     = result.Broker;
                                    Global._devices[device.SN].SSID       = result.SSID;
                                    Global._devices[device.SN].Result     = result.Result;
                                    Global._devices[device.SN].CreateTime = result.DateTimeUTC;
                                }
                                else
                                {
                                    device.Broker     = result.Broker;
                                    device.SSID       = result.SSID;

                                    Global._devices.TryAdd(device.SN, device);
                                }

                                _logger.Trace($"Receive localSettingResult id={device.SN} ");

                                break;

                            case "LogRate":

                                _logger.Trace($"Receive lograte id={device.SN} ");
                                break;

                            case "Status":

                                if (result.Type == "Battery")
                                {
                                    if (Global._devices.ContainsKey(device.SN))
                                    {
                                        //Global._devices[device.SN] = device;
                                        Global._devices[device.SN].Status = result.Connection;
                                        //Global._devices[device.SN].CreateTime = result.DateTimeUTC;
                                    }
                                    else
                                    {
                                        device.Status = result.Connection;
                                        Global._devices.TryAdd(device.SN, device);
                                    }
                                }

                                _logger.Trace($"Receive status id={device.SN} ");
                                break;

                            default:
                                _logger.Info(msg);
                                break;
                        }
                    }
                    else
                    {
                        
                    }
                }

                Global._dashboard.AddAndClear(Global._devices.Values.OrderBy(x=>x.SN).ToList());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }

            return Task.CompletedTask;
        }

        private string GetLastWill()
        {
            string result = "";

            DeviceStatus deviceStatus = new DeviceStatus() 
            {
                ID = clientId,
                Type = "SettingTool",
                Connection = "Offline"
            };

            result = JsonConvert.SerializeObject(deviceStatus);


            return result;
        }
        #endregion

        

    }
}
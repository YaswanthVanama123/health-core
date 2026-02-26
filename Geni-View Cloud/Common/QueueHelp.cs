using MQTTnet;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GeniView.Cloud.Common.Queue
{
    public class MQTTData
    {

        public string Topic { get; set; }
        public string Payload { get; set; }

        public MQTTData() { }

        public MQTTData(string topic, string payload) 
        {
            Topic = topic;
            Payload = payload;
        }

        public MQTTData(MqttApplicationMessage mqttMsg)
        {
            Topic = mqttMsg.Topic;
            Payload = mqttMsg.ConvertPayloadToString();
        }
    }
    public class QueueHelp
    {
        public ConcurrentQueue<MQTTData> _queue = new ConcurrentQueue<MQTTData>();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public QueueHelp() { }


        public void Enqueue(MqttApplicationMessage mqttMsg)
        {
            _queue.Enqueue(new MQTTData(mqttMsg));
        }

        public void Enqueue(MQTTData mqttMsg)
        {
            _queue.Enqueue(mqttMsg);
        }

        public bool Dequeue(out  MQTTData data)
        {
            var result = _queue.TryDequeue(out  data);
            return result;
        }

        public List<MQTTData> DequeueWhile(CancellationToken ct)
        {
            List<MQTTData> result = new List<MQTTData>();
            bool success = true;

            _logger.Trace($"Start QTY:{_queue.Count}");

            while (success == true)
            {
                success = _queue.TryDequeue(out MQTTData data);

                if (success == true)
                {
                    result.Add(data);

                    _logger.Trace($"Sucess:{success} ct:{ct.IsCancellationRequested} {data.Payload}");
                }

                //Thread.Sleep(500);
            }

            _logger.Trace($"End QTY:{_queue.Count}");

            return result;
        }
    }
}
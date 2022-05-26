using BalanceService.GraphQL;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Database.Models;
using Newtonsoft.Json;
using System.Net;

namespace BalanceService.Setting
{
    public class KafkaHelper
    {
        public static async Task<bool> SendMessage(KafkaSettings settings, string topic, string key, string val)
        {
            var succeed = false;
            var config = new ProducerConfig
            {
                BootstrapServers = settings.Server,
                ClientId = Dns.GetHostName(),

            };
            using (var adminClient = new AdminClientBuilder(config).Build())
            {
                try
                {
                    await adminClient.CreateTopicsAsync(new List<TopicSpecification> {
                        new TopicSpecification {
                            Name = topic,
                            NumPartitions = settings.NumPartitions,
                            ReplicationFactor = settings.ReplicationFactor } });
                }
                catch (CreateTopicsException e)
                {
                    if (e.Results[0].Error.Code != ErrorCode.TopicAlreadyExists)
                    {
                        Console.WriteLine($"An error occured creating topic {topic}: {e.Results[0].Error.Reason}");
                    }
                    else
                    {
                        Console.WriteLine("Topic already exists");
                    }
                }
            }
            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                producer.Produce(topic, new Message<string, string>
                {
                    Key = key,
                    Value = val
                }, (deliveryReport) =>
                {
                    if (deliveryReport.Error.Code != ErrorCode.NoError)
                    {
                        Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                    }
                    else
                    {
                        Console.WriteLine($"Produced message to: {deliveryReport.TopicPartitionOffset}");
                        succeed = true;
                    }
                });
                producer.Flush(TimeSpan.FromSeconds(10));
            }

            return await Task.FromResult(succeed);
        }

        internal static Task SendMessage(object value, string v, string key, string val)
        {
            throw new NotImplementedException();
        }
    }
}

using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System.Net;

namespace BillProcessor.Settings
{
    public class KafkaHelper
    {
        /*--------------------------------------------- KAFKA SETTING ------------------------------------------------*/
        /*Console.WriteLine("-------Kafka-------");

        var config = new ConsumerConfig
        {
            BootstrapServers = builder.Configuration.GetSection("KafkaSettings").GetSection("Server").Value,
            GroupId = "tester",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var topic = "simpleOrder";
        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };

        using (var consumer = new ConsumerBuilder<string, string>(config).Build())
        {
            Console.WriteLine("Connected");
            consumer.Subscribe(topic);
            try
            {
                while (true)
                {
                    var cr = consumer.Consume(cts.Token); // blocking
                    Console.WriteLine($"Consumed record with key: {cr.Message.Key} and value: {cr.Message.Value}");

                    // EF
                    ReceiveKafkaBill receiveKafkaBill = JsonConvert.DeserializeObject<ReceiveKafkaBill>(cr.Message.Value);
                    using (var context = new BankDotnetDbContext())
                    {
                        Bill bill = new Bill();
                        bill.VirtualAccount = receiveKafkaBill.Virtualaccount;
                        bill.TotalBill = Convert.ToDouble(receiveKafkaBill.Bills);
                        bill.PaymentStatus = receiveKafkaBill.PaymentStatus;
                        bill.Type = "Pembayaran OPO";

                        context.Bills.Add(bill);
                        context.SaveChanges();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ctrl-C was pressed.
            }
            finally
            {
                consumer.Close();
            }

        }*/
        /*--------------------------------------------- END ------------------------------------------------*/
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

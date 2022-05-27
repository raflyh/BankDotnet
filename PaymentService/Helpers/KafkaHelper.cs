using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Database.Models;
using Newtonsoft.Json;
using PaymentService.GraphQL;
using PaymentService.Settings;
using System.Net;

namespace PaymentService.Helpers
{
    public class KafkaHelper
    {
        public static async Task<int> AcceptBills(KafkaSettings settings, BankDotnetDbContext context)
        {
            var Serverconfig = new ConsumerConfig
            {
                BootstrapServers = settings.Server,
                GroupId = "Batch2",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            var billId = 0;
            using (var consumer = new ConsumerBuilder<string, string>(Serverconfig).Build())
            {
                /* string[] topics = { "BankTravika", "BankSolaka" , "simpleOrder" };
                 foreach(string topic in topics)
                 {
                     consumer.Subscribe(topic);
                 }*/
                consumer.Subscribe("simpleOrder");
                Console.WriteLine("==============Accepting Bills================");
                
                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);
                        Console.WriteLine($"Consumed record with key: {cr.Message.Key} and value: {cr.Message.Value}");
                        if (cr.Topic == "simpleOrder")
                        {
                            ReceiveBill receiveBill = JsonConvert.DeserializeObject<ReceiveBill>(cr.Message.Value);
                            var bill = new Bill
                            {
                                VirtualAccount = receiveBill.Virtualaccount,
                                TotalBill = Convert.ToDouble(receiveBill.Bills),
                                PaymentStatus = "Accepted",
                                BillTransactionId = receiveBill.TransactionId
                            };
                            context.Bills.Add(bill);
                            await context.SaveChangesAsync();
                            billId = bill.Id;
                        }
                        if(cr.Topic == "BankSolaka")
                        {
                            ReceiveBill receiveBill = JsonConvert.DeserializeObject<ReceiveBill>(cr.Message.Value);
                            var bill = new Bill
                            {
                                VirtualAccount = receiveBill.Virtualaccount,
                                TotalBill = Convert.ToDouble(receiveBill.Bills),
                                PaymentStatus = "Accepted",
                                BillTransactionId = receiveBill.TransactionId
                            };
                            context.Bills.Add(bill);
                            await context.SaveChangesAsync();
                            billId = bill.Id;
                        }
                        Console.WriteLine("=========Bill Saved Into Database=========");
                        Console.WriteLine("=========Bill Accepted==========");
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
            }
            return await Task.FromResult(billId);
        }

        public static async Task<bool> SendPaymentStatus(KafkaSettings settings, string topic, string key, string val)
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
    }
}

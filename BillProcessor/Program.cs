// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Database.Models;
using BillProcessor.Settings;

Console.WriteLine("Bill Procesor App");

IConfiguration configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", true, true)
      .Build();

var config = new ConsumerConfig
{
    BootstrapServers = configuration.GetSection("KafkaSettings").GetSection("Server").Value,
    GroupId = "tester",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

var topic = "Bank";
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
            ReceiveKafkaBill receiveBill = JsonConvert.DeserializeObject<ReceiveKafkaBill>(cr.Message.Value);
            using (var context = new BankDbContext())
            {
                double total = Convert.ToDouble(receiveBill.Bills);
                var bill = new Bill
                {
                    BillTransactionId = receiveBill.TransactionId,
                    VirtualAccount = receiveBill.Virtualaccount,
                    TotalBill = total,
                    PaymentStatus = "Accepted"
                };
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

}

using BalanceService.GraphQL;
using BalanceService.Setting;
using Confluent.Kafka;
using Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var conString = builder.Configuration.GetConnectionString("MyDatabase");
builder.Services.AddDbContext<BankDotnetDbContext>(options =>
     options.UseSqlServer(conString)
);

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddAuthorization();
builder.Services.AddControllers();
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
// role-based identity
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
            ValidateIssuer = true,
            ValidAudience = builder.Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),
            ValidateIssuerSigningKey = true
        };

    });
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
 
/*--------------------------------------------- KAFKA SETTING ------------------------------------------------*/
Console.WriteLine("-------Kafka-------");

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
                bill.Type= "Pembayaran OPO";

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
/*--------------------------------------------- END ------------------------------------------------*/


var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGraphQL();
app.MapGet("/", () => "Hello World!");

app.Run();

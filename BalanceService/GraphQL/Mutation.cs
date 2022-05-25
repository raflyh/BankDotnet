using BalanceService.Setting;
using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;

namespace BalanceService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public async Task<ResponseOutput> AddBalanceAsync(
            BalanceInput input,
            [Service] BankDotnetDbContext context)
        {
            var nasabah = context.Balances.Where(x => x.AccountNumber == input.AccountNumber).FirstOrDefault();
            var user = context.Users.Where(o=>o.Id == nasabah.UserId).FirstOrDefault();
            if (nasabah != null)
            {
                nasabah.TotalBalance = (nasabah.TotalBalance + input.TotalBalance);
                nasabah.CreatedDate = DateTime.Now;

                var ret = context.Balances.Update(nasabah);
                await context.SaveChangesAsync();
                
                return new ResponseOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = $"Berhasil Menambah Saldo ke No Rekening: {nasabah.AccountNumber}, Nama: {user.FullName}",
                };
            }
            return new ResponseOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = $"Data Nasabah Dengan No Rekening : {nasabah.AccountNumber} Tidak Ditemukan",
            };
        }
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public async Task<TransferOutput> AddTransferAsync(
            TransferBalance input,
            [Service] BankDotnetDbContext context)
        {
            var recipient = context.Balances.Where(x => x.AccountNumber == input.RecipientAccountNumber).FirstOrDefault();
            var userRecipient = context.Users.Where(o => o.Id == recipient.UserId).FirstOrDefault();
            var sender = context.Balances.Where(s => s.AccountNumber == input.SenderAccountNumber).FirstOrDefault();
            var userSender = context.Users.Where(o => o.Id == sender.UserId).FirstOrDefault();

            if (sender == null)
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Penerima Tidak Ditemukan"
                };
            if (recipient == null)
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message ="Pengirim Tidak Ditemukan"
                };
            if (recipient != null && sender != null)
            {
                if (sender.TotalBalance > input.Total)
                {
                    var transaksi = new Transaction
                    {
                        RecipientBalanceId = recipient.Id,
                        SenderBalanceId= sender.Id,
                        Total= input.Total,
                        TransactionDate = DateTime.Now,
                        Description = input.Description
                    };
                    context.Transactions.Add(transaksi);

                    sender.TotalBalance = (sender.TotalBalance - input.Total);
                    context.Balances.Update(sender);

                    recipient.TotalBalance = (recipient.TotalBalance + input.Total);
                    context.Balances.Update(recipient);
                    await context.SaveChangesAsync();

                    return new TransferOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Transfer Berhasil",
                        SenderAccount = $"{userSender.FullName}-{sender.AccountNumber}",
                        RecipientAccount = $"{userRecipient.FullName}-{recipient.AccountNumber}",
                        Description = input.Description,
                    };

                }
                else
                {
                    return new TransferOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Saldo Tidak Mencukupi"
                    };
                }
            }
            else
            {
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Error"
                };
            }
        }
        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<TopupOutput> AddRedeemCodeAsync(
            TopupOpo input,
            ClaimsPrincipal claimsPrincipal,
            [Service] BankDotnetDbContext context, [Service] IOptions<KafkaSettings> settings)
        {
            const string src = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 16;
            var sb = new StringBuilder();
            Random rdm = new Random();
            for (var i = 0; i < length; i++)
            {
                var c = src[rdm.Next(0, src.Length)];
                sb.Append(c);
            }
            Console.WriteLine(sb.ToString());
            input.Code = sb.ToString();
            input.Amount = input.Amount;

            var dts = DateTime.Now.ToString();
            var key = "TopupOPO-" + dts;
            var val = JObject.FromObject(input).ToString(Formatting.None);/*JsonConvert.SerializeObject(input);*/
            var result = await KafkaHelper.SendMessage(settings.Value, "Latihan4", key, val);

            TopupOutput resp = new TopupOutput
            {
                TransactionDate = dts,
                Message = "Create redeem code successful"
            };
            if (!result)
                resp.Message = "Failed to submit data";
            return await Task.FromResult(resp);
            
        }
    }
}

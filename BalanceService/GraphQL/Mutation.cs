using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace BalanceService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public async Task<ResponseOutput> AddBalanceAsync(
            BalanceInput input,
            [Service] BankDotnetDbContext context)
        {
            // EF
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
            // EF
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
                    sender.TotalBalance = (sender.TotalBalance - input.Total);
                    context.Balances.Update(sender);
                    context.SaveChangesAsync();

                    recipient.TotalBalance = (recipient.TotalBalance + input.Total);
                    context.Balances.Update(recipient);
                    context.SaveChangesAsync();

                    return new TransferOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Tranfer Berhasil",
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

    }
}

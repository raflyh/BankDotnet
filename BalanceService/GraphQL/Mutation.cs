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
    }
}

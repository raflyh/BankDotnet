using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CreditService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public IQueryable<Credit> GetCredits([Service] BankDotnetDbContext context) =>
            context.Credits;

    }
}

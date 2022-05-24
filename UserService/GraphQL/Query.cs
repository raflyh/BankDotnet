using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UserService.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<UserData> GetUsers([Service] BankDotnetDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            // check admin role ?
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var users = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (users != null)
            {
                if (adminRole.Value == "ADMIN")
                {
                    return context.Users.Include(b => b.Balances).Include(o => o.UserRoles).ThenInclude(r => r.Role).Select(p => new UserData()
                    {
                        Id = p.Id,
                        FullName = p.FullName,
                        Username = p.Username,
                        PhoneNumber = p.PhoneNumber,
                        Address = p.Address,
                        CreatedDate = p.CreatedDate,
                        UserRoles = p.UserRoles,
                        Balances = p.Balances
                    });
                }
                return context.Users.Where(o => o.Id == users.Id).Include(o => o.UserRoles).ThenInclude(r => r.Role).Select(p => new UserData()
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    Username = p.Username,
                    PhoneNumber = p.PhoneNumber,
                    Address = p.Address,
                    CreatedDate = p.CreatedDate,
                    UserRoles = p.UserRoles,
                });
            }
            return new List<UserData>().AsQueryable();
        }
    }
}

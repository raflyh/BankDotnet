using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UserService.Setting;
using Database.Models;

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
                    return context.Users.Include(b=>b.Balances).Include(o => o.UserRoles).ThenInclude(r => r.Role).Select(p => new UserData()
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

        [Authorize]
        public IQueryable<UserRole> GetUserRoleNasabah([Service] BankDotnetDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var users = context.UserRoles.Include(u => u.User).Include(r=>r.Role).Where(o => o.RoleId == 2);
            if (users != null)
            {
                if (adminRole.Value == "MANAGER" || adminRole.Value == "ADMIN" || adminRole.Value == "CUSTOMER SERVICE")
                {
                    return users;
                }
            }
            return new List<UserRole>().AsQueryable();
        }

    }
}

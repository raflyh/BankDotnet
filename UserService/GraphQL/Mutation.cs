using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Settings;

namespace UserService.GraphQL
{
    public class Mutation
    {
        public async Task<UserData> RegisterNasabahAsync(
            RegisterUser input,
            [Service] BankDotnetDbContext context)
        {
            var role = context.Roles.Where(m => m.Name == "NASABAH").FirstOrDefault();
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();

            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                PhoneNumber = input.PhoneNumber,
                Address = input.Address,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
                CreatedDate = DateTime.Now,
            };

            if (role == null) throw new Exception("Role Null");
            var userRole = new UserRole
            {
                RoleId = role.Id,
                UserId = newUser.Id
            };

            var balances = context.Balances.Where(m => m.UserId == newUser.Id).FirstOrDefault();

            if (balances != null) throw new Exception("Balance sudah ada");
            Random rnd = new Random();
            var random = Convert.ToString(rnd.Next());
            var balance = new Balance
            {
                UserId = newUser.Id,
                AccountNumber = $"{random}" + $"{Convert.ToString(rnd.Next(10, 20))}",
                TotalBalance = 0,
                CreatedDate = DateTime.Now,
            };

            newUser.Balances.Add(balance);
            newUser.UserRoles.Add(userRole);
            var ret = context.Users.Add(newUser);

            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                FullName = newUser.FullName,
                PhoneNumber = newUser.PhoneNumber,
                Address = newUser.Address,
                CreatedDate = newUser.CreatedDate
            });
        }

        //Login User
        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] BankDotnetDbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.Id == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims,
                    signingCredentials: credentials
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }
    }
}

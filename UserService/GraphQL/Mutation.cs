using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UserService.Setting;
using Database.Models;

namespace UserService.GraphQL
{
    public class Mutation
    {
        //ADMIN
        //Register User Admin
        public async Task<UserData> RegisterAdminAsync(
            RegisterUser input,
            [Service] BankDotnetDbContext context)
        {
            var role = context.Roles.Where(m => m.Name == "ADMIN").FirstOrDefault();
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData
                {
                    Id = 0,
                    Username = "User sudah Ada",
                    FullName = "User sudah Ada",
                    PhoneNumber = "User sudah Ada",
                    Address = "User sudah Ada",
                    CreatedDate = DateTime.Now,
                });
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

            if (role == null) throw new Exception("Invalid Role");

            var userRole = new UserRole
            {
                RoleId = role.Id,
                UserId = newUser.Id
            };

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
        //NASABAH
        //Register User Nasabah
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public async Task<UserData> RegisterNasabahAsync(
            RegisterUser input,
            [Service] BankDotnetDbContext context)
        {
            var role = context.Roles.Where(m => m.Name == "NASABAH").FirstOrDefault();
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData
                {
                    Id = 0,
                    Username = "User sudah Ada",
                    FullName = "User sudah Ada",
                    PhoneNumber = "User sudah Ada",
                    Address = "User sudah Ada",
                    CreatedDate = DateTime.Now,
                });
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


            if ( balances != null) throw new Exception("Balance sudah ada");
            Random rnd = new Random();
            var random = Convert.ToString(rnd.Next(1000000000,2000000000));
            var balance = new Balance
            {
                UserId = newUser.Id,
                AccountNumber = $"{random}" + $"{ Convert.ToString(rnd.Next(10,20)) }",
                TotalBalance = 0,
                CreatedDate = DateTime.Now,
            };

            var savings = context.Savings.Where(m => m.BalanceId == balance.Id).FirstOrDefault();

            if (savings != null) throw new Exception("saving sudah ada");
            
            var saving = new Saving
            {
                BalanceId = balance.Id,
                TotalSaving = 0,
                TotalGold =0,
                Date = DateTime.Now
            };

            newUser.Balances.Add(balance);
            balance.Savings.Add(saving);
            newUser.UserRoles.Add(userRole);
            var ret = context.Users.Add(newUser);

            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData {
                Id = newUser.Id,
                Username = newUser.Username,
                FullName = newUser.FullName,
                PhoneNumber = newUser.PhoneNumber,
                Address = newUser.Address,
                CreatedDate = newUser.CreatedDate
            });
        }

        //OFFICER
        //Register User Manager dan CS
        public async Task<UserData> RegisterOfficerAsync(
            RegisterUser input,
            [Service] BankDotnetDbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData
                {
                    Id = 0,
                    Username = "User sudah Ada",
                    FullName = "User sudah Ada",
                    PhoneNumber = "User sudah Ada",
                    Address = "User sudah Ada",
                    CreatedDate = DateTime.Now,
                });
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

        //Update Profile User 
        [Authorize]
        public async Task<UserData> UpdateProfileAsync(
            UpdateUser input,
            [Service] BankDotnetDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            
            if (user != null)
            {
                user.Username = input.Username;
                user.FullName = input.FullName;
                user.PhoneNumber = input.PhoneNumber;
                user.Address = input.Address;

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(new UserData
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                CreatedDate = user.CreatedDate
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

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList();
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

        //Change Password
        public async Task<User> ChangePasswordAsync(
            ChangePassword input,
            [Service] BankDotnetDbContext context)
        {

            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                bool valid = BCrypt.Net.BCrypt.Verify(input.OldPassword, user.Password);
                if (valid)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);

                    context.Users.Update(user);
                    await context.SaveChangesAsync();
                    return await Task.FromResult(new User { Username = "Password",Password = "Berhasil di ganti" });
                }
                else
                {
                    return await Task.FromResult(new User { Username = "Salah", Password = "Salah" });
                }
            }

            return await Task.FromResult(user);

        }

        //Add User Role
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<UserRole> AddUserRoleAsync(
          InputUserRole input,
          [Service] BankDotnetDbContext context)
        {
            var user = context.UserRoles.Where(o => o.UserId == input.UserId).FirstOrDefault();
            if (user != null) return new UserRole();
            var userRole = new UserRole
            {
                UserId = input.UserId,
                RoleId = input.RoleId,
            };
            var ret = context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        //Change User Role
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<UserRole> ChangeUserRoleAsync(
          InputUserRole input,
          [Service] BankDotnetDbContext context)
        {
            var user = context.UserRoles.Where(o => o.UserId == input.UserId).FirstOrDefault();
            if (user == null) return new UserRole();
           
            user.RoleId = input.RoleId;

            var ret = context.UserRoles.Update(user);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        //Delete user
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> DeleteUserByIdAsync(
        int id,
        [Service] BankDotnetDbContext context)
        {
            var user = context.Users.Where(o => o.Id == id).Include(u => u.UserRoles).FirstOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(user);
        }

        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<Bill> CreateBillAsync(
            BillPayment input,
            ClaimsPrincipal claimsPrincipal,
            [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName)
                .Include(b=>b.Balances).Include(c=>c.Credits).FirstOrDefault();

            if (user != null)
            {
                Random rnd = new Random();
                var random = Convert.ToString(rnd.Next(1000000000, 2000000000));
                if (user.Username.Contains("PLN"))
                {
                    var newBill = new Bill
                    {
                        VirtualAccount = "007" + $"{random}",
                        TotalBill = input.TotalBill,
                        PaymentStatus = input.PaymentStatus,
                    };
                    var ret = context.Bills.Add(newBill);
                    await context.SaveChangesAsync();
                    return await Task.FromResult(newBill);
                }
                if (user.Username.Contains("PDAM"))
                {
                    var newBill = new Bill
                    {
                        VirtualAccount = "008" + $"{random}",
                        TotalBill = input.TotalBill,
                        PaymentStatus = input.PaymentStatus,
                    };
                    var ret = context.Bills.Add(newBill);
                    await context.SaveChangesAsync();
                    return await Task.FromResult(newBill);
                }
            }
            return new Bill();
        }
    }
}

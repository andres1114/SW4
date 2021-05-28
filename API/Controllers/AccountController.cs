using API.Data;
using API.Entities;
using API.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API.Interface;

namespace API.Controllers {
    public class AccountController : BaseApiController {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        public AccountController(DataContext context, ITokenService tokenService) {
            this.context = context;
            this.tokenService = tokenService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO) {
            if (await UserExists(registerDTO.Username)) return BadRequest("Username already exists in DB");

            using var hmac = new HMACSHA512();
            var user = new AppUser {
                UserName = registerDTO.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };

            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();

            return new UserDTO {
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user)
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) {
            var user = await this.context.Users.SingleOrDefaultAsync(user => user.UserName == loginDTO.Username);
            if (user == null) return Unauthorized("User invalid or not found");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for (int x = 0; x < computedHash.Length; x++) {
                if (computedHash[x] != user.PasswordHash[x]) return Unauthorized("Password does not match"); 
            }

            return new UserDTO {
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user)
            };
        }
        private async Task<bool> UserExists(string username) {
            return await this.context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}

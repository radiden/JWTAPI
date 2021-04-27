using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using jwtapi.Models;
using jwtapi.Data;

namespace jwtapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly RefreshContext _context;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(IConfiguration config, RefreshContext context, ILogger<AuthorizationController> logger)
        {
            _config = config;
            _context = context;
            _logger = logger;
        }
        
        [HttpGet]
        [Route("GetToken")]
        public IActionResult Get()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("JWTSecret")));
            var token = new JwtSecurityToken(
                issuer: "jwtapi",
                audience: "jwtapiuser",
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            var writtenToken = new JwtSecurityTokenHandler().WriteToken(token);
            var hmac = new HMACSHA256();
            RefreshToken refreshToken = new RefreshToken
            {
                JWT = writtenToken,
                Expiration = DateTime.Now.AddDays(7),
                Signature = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(writtenToken + DateTime.Now.AddDays(7).ToString() + _config.GetValue<string>("RefreshSecret")))).Replace("-", "")
            };
            string encodedRefresh = RefreshToB64(refreshToken);
            _context.Add(new Refresh
            {
                JWT = writtenToken,
                RefreshToken = encodedRefresh
            });
            _context.SaveChanges();
            return new OkObjectResult(new TokenDetails
                {
                    Token = writtenToken,
                    TokenExpirationDate = DateTime.Now.AddMinutes(5),
                    RefreshToken = encodedRefresh,
                    RefreshExpirationDate = DateTime.Now.AddDays(7)
                });
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken(TokenDetails token)
        {
            if (ModelState.IsValid)
            {
                if (ValidateJWT(token.Token)) {
                    _logger.LogInformation("Refreshing token...");

                    var tokens = from val in _context.Refresh select val;

                    var FilteredToken = tokens.Where(val => val.JWT == token.Token);

                    if (FilteredToken.Any() && await FilteredToken.CountAsync() == 1)
                    {
                        var finaltoken = await FilteredToken.FirstAsync();

                        if (finaltoken.RefreshToken == token.RefreshToken) {
                            _context.Refresh.Remove(finaltoken);
                            _logger.LogInformation("Successfuly removed token, redirecting...");
                            
                            return Get();
                        } else {
                            return new BadRequestObjectResult("The refresh token does not match.");    
                        }
                    } else {
                        return new BadRequestObjectResult("Could not find such token.");
                    }

                } else {
                    return new BadRequestObjectResult("The provided token was invalid.");
                }
            }
            return new BadRequestObjectResult("The model is invalid.");
        }
        private string RefreshToB64(RefreshToken token)
        {
            return Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes<RefreshToken>(token));
        }

        private bool ValidateJWT(string token)
        {
            var TokenHandler = new JwtSecurityTokenHandler();

            try
            {
                TokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidIssuer = "jwtapi",
                    ValidAudience = "jwtapiuser",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("JWTSecret"))),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        private readonly UserManager<UserModel> _userManager;

        public AuthorizationController(IConfiguration config, RefreshContext context,
            ILogger<AuthorizationController> logger, UserManager<UserModel> userManager)
        {
            _config = config;
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody]LoginModel info)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (String.IsNullOrWhiteSpace(info.UserName))
            {
                return new BadRequestObjectResult("Please provide a username!");
            }

            var user = await _userManager.FindByNameAsync(info.UserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, info.Password))
            {
                return new BadRequestObjectResult($"Wrong credentials!");
            }

            var token = await GenerateNewJwt(user);

            return new OkObjectResult(token);
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]RegistrationModel info)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (String.IsNullOrWhiteSpace(info.UserName))
            {
                return new BadRequestObjectResult("Please provide a username!");
            }
            
            if (await _userManager.FindByNameAsync(info.UserName) is not null)
            {
                return new BadRequestObjectResult("Username is taken.");
            }

            var user = new UserModel
            {
                UserName = info.UserName,
                FullName = info.FullName
            };

            var userCreated = await _userManager.CreateAsync(user, info.Password);
            if (!userCreated.Succeeded)
            {
                return new BadRequestObjectResult("Failed to create user.");
            }

            return new OkObjectResult(user);
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody]TokenDetails token)
        {
            if (ModelState.IsValid)
            {
                if (ValidateJwt(token.Token)) {
                    _logger.LogInformation("Refreshing token...");

                    var tokens = from val in _context.Refresh select val;

                    var filteredToken = tokens.Where(val => val.JWT == token.Token);

                    if (await filteredToken.CountAsync() == 1)
                    {
                        var finalToken = await filteredToken.FirstAsync();

                        if (finalToken.RefreshToken == token.RefreshToken)
                        {
                            if (ValidateToken(token.RefreshToken))
                            {
                                _context.Refresh.Remove(finalToken);
                                _logger.LogInformation("Successfully removed token, redirecting...");

                                var subject = GetUserFromJwt(token.Token);

                                var userInfo = _userManager.Users.First(user => user.UserName == subject);
                                
                                return new OkObjectResult(GenerateNewJwt(userInfo));
                            } else {
                                return new BadRequestObjectResult("The refresh token has expired.");
                            }
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
        
        [HttpPost]
        [Route("AddToRole")]
        public async Task<IActionResult> AddToRole([FromBody]AddRoleModel info)
        {
            if (String.IsNullOrWhiteSpace(info.Username))
            {
                return new BadRequestObjectResult("Please provide a username!");
            }

            var user = await _userManager.FindByNameAsync(info.Username);

            if (user == null)
            {
                return new BadRequestObjectResult("Couldn't find that user.");
            }
            await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, info.Role));
            
            return new OkObjectResult($"Added user {info.Username} to role {info.Role}!");
        }

        private string GetUserFromJwt(string jwt)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(jwt);
            
            return token.Claims.First(claim => claim.Type == Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub).Value;
        }
        
        private async Task<TokenDetails> GenerateNewJwt(UserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetClaimsAsync(user);
            
            claims.AddRange(roles.Select(role => role));
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("JWTSecret")));
            var token = new JwtSecurityToken(
                issuer: "jwtapi",
                audience: "jwtapiuser",
                expires: DateTime.UtcNow.AddMinutes(1),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            var writtenToken = new JwtSecurityTokenHandler().WriteToken(token);
            var hmac = new HMACSHA256();
            var refreshToken = new RefreshToken
            {
                JWT = writtenToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                Signature = BitConverter
                    .ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(writtenToken + DateTime.UtcNow.AddDays(7) +
                                                                      _config.GetValue<string>("RefreshSecret"))))
                    .Replace("-", "")
            };
            var encodedRefresh = RefreshToB64(refreshToken);
            _context.Add(new Refresh
            {
                JWT = writtenToken,
                RefreshToken = encodedRefresh
            });
            await _context.SaveChangesAsync();
            return new TokenDetails
            {
                Token = writtenToken,
                TokenExpirationDate = DateTime.UtcNow.AddMinutes(1),
                RefreshToken = encodedRefresh,
                RefreshExpirationDate = DateTime.UtcNow.AddDays(7)
            };
        }

        private static string RefreshToB64(RefreshToken token)
        {
            return Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(token));
        }

        private bool ValidateJwt(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidIssuer = "jwtapi",
                    ValidAudience = "jwtapiuser",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("JWTSecret"))),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken _);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool ValidateToken(string token)
        {
            var tokenJson = JsonSerializer.Deserialize<RefreshToken>(Convert.FromBase64String(token));
            return tokenJson.Expiration >= DateTime.UtcNow;
        }
    }
}

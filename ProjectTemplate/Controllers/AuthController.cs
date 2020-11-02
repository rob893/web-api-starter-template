using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Linq;
using ProjectTemplate.Models.DTOs;
using ProjectTemplate.Models.Domain;
using ProjectTemplate.Models.Settings;
using ProjectTemplate.Core;
using ProjectTemplate.Data.Repositories;

namespace ProjectTemplate.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository userRepository;
        private readonly AuthenticationSettings authSettings;
        private readonly IMapper mapper;


        public AuthController(UserRepository userRepository, IOptions<AuthenticationSettings> authSettings, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.authSettings = authSettings.Value;
            this.mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserForReturnDto>> RegisterAsync([FromBody] UserForRegisterDto userForRegisterDto)
        {
            var userToCreate = mapper.Map<User>(userForRegisterDto);

            var result = await userRepository.CreateUserWithPasswordAsync(userToCreate, userForRegisterDto.Password);

            var userToReturn = mapper.Map<UserForReturnDto>(userToCreate);

            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetailsWithErrors(result.Errors.Select(e => e.Description).ToList(), 400, Request));
            }

            return CreatedAtRoute("GetUserAsync", new { controller = "Users", id = userToCreate.Id }, userToReturn);
        }

        /// <summary>
        /// Logs the user in
        /// </summary>
        /// <param name="userForLoginDto"></param>
        /// <returns>200 with user object on success. 401 on failure.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginForReturnDto>> LoginAsync([FromBody] UserForLoginDto userForLoginDto)
        {
            var user = await userRepository.GetByUsernameAsync(userForLoginDto.Username, user => user.RefreshToken);

            if (user == null)
            {
                return Unauthorized(new ProblemDetailsWithErrors("Invalid username or password.", 401, Request));
            }

            var result = await userRepository.CheckPasswordAsync(user, userForLoginDto.Password);

            if (!result)
            {
                return Unauthorized(new ProblemDetailsWithErrors("Invalid username or password.", 401, Request));
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = new RefreshToken
            {
                Token = refreshToken,
                Expiration = DateTimeOffset.UtcNow.AddMinutes(authSettings.RefreshTokenExpirationTimeInMinutes)
            };

            await userRepository.SaveAllAsync();

            var userToReturn = mapper.Map<UserForReturnDto>(user);

            return Ok(new LoginForReturnDto
            {
                Token = token,
                RefreshToken = refreshToken,
                User = userToReturn
            });
        }

        [HttpPost("refreshToken")]
        public async Task<ActionResult> RefreshTokenAsync([FromBody] RefreshTokenDto refreshTokenDto)
        {
            // Still validate the passed in token, but ignore its expiration date by setting validate lifetime to false
            var validationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authSettings.APISecrect)),
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidateLifetime = false,
                ValidAudience = authSettings.TokenAudience,
                ValidIssuer = authSettings.TokenIssuer
            };

            ClaimsPrincipal tokenClaims;

            try
            {
                tokenClaims = new JwtSecurityTokenHandler().ValidateToken(refreshTokenDto.Token, validationParameters, out var rawValidatedToken);
            }
            catch (Exception e)
            {
                return Unauthorized(new ProblemDetailsWithErrors(e.Message, 401, Request));
            }

            var userIdClaim = tokenClaims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ProblemDetailsWithErrors("Invalid token.", 401, Request));
            }

            var user = await userRepository.GetByIdAsync(userId, user => user.RefreshToken);

            if (user == null)
            {
                return Unauthorized(new ProblemDetailsWithErrors("Invalid token.", 401, Request));
            }

            if (user.RefreshToken == null || user.RefreshToken.Token != refreshTokenDto.RefreshToken || DateTimeOffset.UtcNow > user.RefreshToken.Expiration)
            {
                return Unauthorized(new ProblemDetailsWithErrors("Invalid token.", 401, Request));
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = new RefreshToken
            {
                Token = refreshToken,
                Expiration = DateTimeOffset.UtcNow.AddMinutes(authSettings.RefreshTokenExpirationTimeInMinutes)
            };

            await userRepository.SaveAllAsync();

            return Ok(new
            {
                token,
                refreshToken
            });
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            if (user.UserRoles != null)
            {
                foreach (string role in user.UserRoles.Select(r => r.Role.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.APISecrect));

            if (key.KeySize < 128)
            {
                throw new Exception("API Secret must be longer");
            }

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(authSettings.TokenExpirationTimeInMinutes),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = creds,
                Audience = authSettings.TokenAudience,
                Issuer = authSettings.TokenIssuer
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.Api.Data;
using DatingApp.Api.Dtos;
using DatingApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //ApiController is needed to infer the input parameters to the respective API entity and also its needed for validation purpose
    //otherwise we have to go manual validation !ModelState.IsValid
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            _config = config;
            _authRepository = authRepository;
        }

        // [HttpPost("register")]
        // public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegisterDto)
        // {
        //     if(!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }

        //     userForRegisterDto.Username =  userForRegisterDto.Username.ToLower();

        //     if (await _authRepository.UserExists(userForRegisterDto.Username))
        //         return BadRequest("User already exists!");

        //     var user = new User(){
        //         Username = userForRegisterDto.Username
        //     };

        //     var createdUser = await _authRepository.Register(user, userForRegisterDto.Password);

        //     return StatusCode(201);
        // }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _authRepository.UserExists(userForRegisterDto.Username))
                return BadRequest("User already exists!");

            var user = new User()
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = await _authRepository.Register(user, userForRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            // Build a token
            // Token contains a UserId and Username
            // this token can be validated without making a database call
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            //Create a security key that will be used to sign the security token that receives from the client to the server
            //Using this key server signs the token when send to Client
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //security token descriptor
            //passing claims as a subject
            //expiring date as a day
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            // Create the token handler to create token descriptor
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Send the token as an object
            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}
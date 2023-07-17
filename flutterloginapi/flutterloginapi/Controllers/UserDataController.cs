using flutterloginapi.Data;
using flutterloginapi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace flutterloginapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDataController : ControllerBase
    {
        private readonly IUserData _userData;
        IConfiguration _configuration;
        private readonly IOtp _otp;
        public UserDataController(IUserData userData, IOtp otp, IConfiguration configuration)
        {
            _userData = userData;
            _otp = otp;
            _configuration = configuration;
        }
        [HttpGet]
        [Route("~/api/GetUser/{token}")]
        public async Task<IActionResult> GetUser(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var user = await _userData.GetUser(token);
                if(user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            return BadRequest();
        }
        [HttpGet]
        [Route("~/api/GetUserDetails/{username}")]
        public async Task<IActionResult> GetUserDetails(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _userData.GetUserDetails(username);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            return BadRequest();
        }
        [HttpPost]
        [Route("~/api/CheckUser")] 
        public async Task<IActionResult> CheckUser(UserDataModel model)
        {
            var checkUser = await _userData.CheckUser(model);
            if(checkUser.StatusCode == 200)
            {
               var status = await _userData.SendOtp(model.Email);
                string code = status.StatusMessage.ToString();
                await _userData.AddOtp(model.Email, code);
                status.StatusMessage = code;
                return Ok(status);
            }
            return Ok(checkUser);
        }
        [HttpPost]
        [Route("~/api/CreateUser")]
        public async Task<IActionResult> CreateUser(UserDataModel model, string code)
        {
            var createUser = await _userData.CreateUser(model, code);
            await _userData.DeleteOtp(model.Email);
            return Ok(createUser);
        }
        [HttpPost]
        [Route("~/api/LoginUser")]
        public async Task<IActionResult> LoginUser(LoginModel model)
        {
            IActionResult response = Unauthorized();
            var LoginUser = await _userData.LoginUser(model);
            if(LoginUser.StatusCode == 200 || LoginUser.StatusCode == 199)
            {
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var signingCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
                var subject = new ClaimsIdentity(new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, model.username),
                new Claim(JwtRegisteredClaimNames.Email, model.username),
            });
                var issued = DateTime.Now;
                var expires = DateTime.UtcNow.AddMinutes(10);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = subject,
                    Expires = expires,
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = signingCredentials
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                await _userData.ManageToken(jwtToken, issued, expires, model.username);
                Status status = new Status();
                status.StatusCode = LoginUser.StatusCode;
                status.StatusMessage = jwtToken;
                return Ok(status);
            }
            return response;
        }
        [HttpPost]
        [Route("~/api/Logout")]
        public async Task<IActionResult> Logout(string token)
        {
            var status = await _userData.Logout(token);
            return Ok(status);
        }
        [HttpPost]
        [Route("~/api/UpdateUser")]
        public async Task<IActionResult> UpdateResult(UserDataModel1 model, string username)
        {
            Status status = await _userData.UpdateUser(model, username);
            return Ok(status);
        }
    }
}

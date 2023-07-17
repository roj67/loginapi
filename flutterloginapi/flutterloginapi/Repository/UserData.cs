using Dapper;
using flutterloginapi.Data;
using Microsoft.AspNetCore.Http;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.OpenApi.Any;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace flutterloginapi.Repository
{
    
    public class UserData : IUserData
    {
        private readonly DapperContext _context;
        private readonly IEncodeDecode _encodeDecode;
        private readonly IOtp _otp;
        public UserData(IConfiguration configuration, IEncodeDecode encodeDecode, IOtp otp)
        {
            _context = new DapperContext(configuration);
            _encodeDecode = encodeDecode;
            _otp = otp;
        }

        public async Task<Status> CheckUser(UserDataModel model)
        {
            string Email = model.Email;
            string Username = model.Username;
            var query1 = $"SELECT * FROM usertable WHERE Username = '{Username}'";
            var query2 = $"SELECT * FROM usertable WHERE Email = '{Email}'";
            using (var connection = _context.CreateConnection())
            {
                var checkUsername = await connection.QueryFirstOrDefaultAsync(query1);
                var checkEmail = await connection.QueryFirstOrDefaultAsync(query2);
                Status status = new Status();
                if (checkUsername != null)
                {
                    status.StatusCode = 201;
                    status.StatusMessage = "Username already taken";
                    return status;
                }
                if (checkEmail != null)
                {
                    status.StatusCode = 202;
                    status.StatusMessage = "Email already taken";
                    return status;
                }
                status.StatusCode = 200;
                status.StatusMessage = "Success";
                return status;
            }
        }

        public async Task<Status> AddOtp(string email, string code)
        {
            var query = $"INSERT into userotptable(email, otp) Values('{email}','{code}')";
            using(var connection = _context.CreateConnection())
            {
                Status status = new Status();
                await connection.ExecuteAsync(query);
                status.StatusCode = 200;
                status.StatusMessage = "otp added";
                return status;
            }
        }

        public async Task<Status> DeleteOtp(string email)
        {
            var query = $"delete from userotptable where email = '{email}'";
            using (var connection = _context.CreateConnection())
            {
                Status status = new Status();
                await connection.ExecuteAsync(query);
                status.StatusCode = 200;
                status.StatusMessage = "otp added";
                return status;
            }
        }

        public async Task<Status> CreateUser(UserDataModel model, string code)
        {
            string Firstname = model.FirstName;
            string Lastname = model.LastName;
            string Email = model.Email;
            string Username = model.Username;
            string Password = _encodeDecode.EncodePassword(model.Password);
            var query = $"INSERT INTO usertable(FirstName,LastName,Email,Username,Passwords) VALUES ('{Firstname}','{Lastname}','{Email}','{Username}','{Password}')";
            var query1 = $"SELECT otp from userotptable Where email = '{Email}'";
            using (var connection = _context.CreateConnection())
            {
                Status status = new Status();
                string code1 = await connection.QueryFirstOrDefaultAsync<String>(query1);
                if(code1 == code)
                {
                    await connection.ExecuteAsync(query);
                    status.StatusCode = 200;
                    status.StatusMessage = "Success";
                    return status;
                }
                status.StatusCode = 201;
                status.StatusMessage = "Error creating the user";
                return status;
            }
        }

        public async Task<UserDataModel> GetUser(string token)
        {    
            using(var connection = _context.CreateConnection())
            {
                var userIdquery = $"SELECT user_id FROM usertokentable Where token = '{token}'";
                var userid = await connection.QueryFirstOrDefaultAsync(userIdquery);
                var query1 = $"SELECT * From usertable where id = '{userid.user_id}'";
                Console.WriteLine(userid);
                var data = await connection.QuerySingleOrDefaultAsync<UserDataModel>(query1);
                return data;
            }
        }

        public async Task<UserDataModel> GetUserDetails(string username)
        {
            using (var connection = _context.CreateConnection())
            {
                var query1 = $"SELECT * From usertable where username = '{username}'";
                var data = await connection.QuerySingleOrDefaultAsync<UserDataModel>(query1);
                return data;
            }
        }

        public async Task<Status> LoginUser(LoginModel model)
        {
            string password = _encodeDecode.EncodePassword(model.password);
            var query = $"SELECT * FROM usertable Where Username = '{model.username}' and Passwords = '{password}'";
            var query1 = $"SELECT UserType FROM usertable Where Username = '{model.username}'";
            Status status = new Status();
            using (var connection = _context.CreateConnection())
            {
                var data = await connection.QuerySingleOrDefaultAsync(query);
                if (data == null)
                {
                    status.StatusCode = 201;
                    status.StatusMessage = "User doesnot exist";
                    return status;
                }
                string data1 = await connection.QuerySingleOrDefaultAsync<string>(query1);
                if(data1 != null)
                {
                    status.StatusCode = 199;
                    status.StatusMessage = data1;
                    return status;
                }
                status.StatusCode = 200;
                status.StatusMessage = "Success";
                return status;
            }
        }

        public async Task<Status> SendOtp(string email)
        {
            var code = _encodeDecode.Generate_otp();
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress("Confirmation", email));
            message.From.Add(new MailboxAddress("SignUp Login System", "tamangroj56@gmail.com"));
            message.Subject = "Confirm your account";
            message.Body = new TextPart("Html")
            {
                Text = $"Enter the given otp in the follow up Page \n<center><strong>{code}</strong></center>"
            };
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("tamangroj56@gmail.com", "fdvqhftublpvfkdu");
                client.Send(message);
                client.Disconnect(true);
            }
            Status status = new Status
            {
                StatusCode = 200,
                StatusMessage = code
            };
            return status;
        }

        public async Task ManageToken(string jwtToken, DateTime issued, DateTime expires, string username)
        {
            var userIDquery = $"Select Id from usertable where username = '{username}'";       
            using(var connection = _context.CreateConnection())
            {
                var isLoggedIn = $"update usertable set isLoggedIn = '1' where username='{username}'";
                await connection.ExecuteAsync(isLoggedIn);
                var userId = await connection.QueryFirstOrDefaultAsync(userIDquery);
                var query = $"Insert into usertokentable (token,issuedDate,expiredDate,isExpired,isLoggedIn,user_id) values ('{jwtToken}','{issued}','{expires}','0','1','{userId.Id}')";
                await connection.ExecuteAsync(query);
            }
        }

        public async Task<Status> Logout(string token)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = $"Delete from usertokentable where token = '{token}'";
                var query1 = $"Select user_id from usertokentable where token = '{token}'";
                var tokenid = await connection.QueryFirstOrDefaultAsync(query1);
                var query2 = $"update usertable set isLoggedIn='0' where Id='{tokenid.user_id}'";
                await connection.ExecuteAsync(query2);
                await connection.ExecuteAsync(query);
                Status status = new Status();
                status.StatusCode = 200;
                status.StatusMessage = "Success";
                return status;
            }
        }

        public async Task<Status> UpdateUser(UserDataModel1 model, string username)
        {
            string Email = model.Email;
            string Username = model.Username;
            var query1 = $"SELECT * FROM usertable WHERE Username = '{Username}'";
            var query2 = $"SELECT * FROM usertable WHERE Email = '{Email}'";
            var query3 = $"update usertable set FirstName = '{model.FirstName}', LastName = '{model.LastName}', Email = '{Email}', Username = '{Username}' where Username = '{username}'";
            using (var connection = _context.CreateConnection())
            {
                var checkUsername = await connection.QueryFirstOrDefaultAsync(query1);
                var checkEmail = await connection.QueryFirstOrDefaultAsync(query2);
                Status status = new Status();
                if (checkUsername != null)
                {
                    status.StatusCode = 201;
                    status.StatusMessage = "Username already taken";
                    return status;
                }
                if (checkEmail != null)
                {
                    status.StatusCode = 202;
                    status.StatusMessage = "Email already taken";
                    return status;
                }
                await connection.ExecuteAsync(query3);
                status.StatusCode = 200;
                status.StatusMessage = "Success";
                return status;
            }
        }
    }
}


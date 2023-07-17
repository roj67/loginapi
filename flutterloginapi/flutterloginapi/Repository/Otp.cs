using MailKit.Net.Smtp;
using MimeKit;

namespace flutterloginapi.Repository
{
    public class Otp : IOtp
    {
        private readonly IEncodeDecode _encodeDecode;
        public Otp(IEncodeDecode encodeDecode)
        {
            _encodeDecode = encodeDecode;
        }

        public Status SendOtp(string email)
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
                client.Authenticate("tamangroj56@gmail.com", "kdzfgppfvirawoyj");
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
    }
}

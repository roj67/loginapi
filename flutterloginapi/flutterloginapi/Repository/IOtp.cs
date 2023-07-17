namespace flutterloginapi.Repository
{
    public interface IOtp
    {
        public Status SendOtp(string email);
    }
}

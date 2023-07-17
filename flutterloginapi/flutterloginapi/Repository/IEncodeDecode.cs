namespace flutterloginapi.Repository
{
    public interface IEncodeDecode
    {
        public string EncodePassword(string password);
        public string DecodePassword(string encodedData);
        public string Generate_otp();
    }
}

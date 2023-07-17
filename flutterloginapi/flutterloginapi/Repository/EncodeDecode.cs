
namespace flutterloginapi.Repository
{
    public class EncodeDecode : IEncodeDecode
    {
        public string DecodePassword(string encodedData)
        {
            
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new string(decoded_char);
            return result;
        }

        public string EncodePassword(string password)
        {
            try
            {
                byte[] encodeData_byte = new byte[password.Length];
                encodeData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encodeData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }

        public string Generate_otp()
        {
            char[] charArr = "0123456789".ToCharArray();
            string strrandom = string.Empty;
            Random objran = new Random();
            for(int i = 0; i< 4; i++)
            {
                int pos = objran.Next(0, 9);
                strrandom += charArr.GetValue(pos);
            }
            return strrandom;
        }
    }
}


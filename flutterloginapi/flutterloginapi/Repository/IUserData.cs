namespace flutterloginapi.Repository
{
    public interface IUserData
    {
        public Task<UserDataModel> GetUser(string token);
        public Task<UserDataModel> GetUserDetails(string token);
        public Task<Status> DeleteOtp(string email);
        public Task<Status> AddOtp(string email, string code);
        public Task<Status> CreateUser(UserDataModel model, string code);
        public Task<Status> CheckUser(UserDataModel model);
        public Task<Status> LoginUser(LoginModel model);
        public Task<Status> SendOtp(string email);
        public Task ManageToken(string jwtToken,DateTime issued,DateTime expires,string username);
        public Task<Status> Logout(string token);
        public Task<Status> UpdateUser(UserDataModel1 model, string username);
        
    }
}

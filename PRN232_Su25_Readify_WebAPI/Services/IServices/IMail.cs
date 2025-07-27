using PRN232_Su25_Readify_WebAPI.Dtos;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IMail
    {
        public Task SendEmail(Message message);
        public Task SendMailConfirmAsync(string email, string urlUser);
        public Task SendMailForgotPassWord(string email, string token);
        Task SendRoyaltyApprovedMailAsync(string email, string authorName, decimal amount);

        Task SendRoyaltyRejectedMailAsync(string email, string authorName, string rejectionReason);
    }
}

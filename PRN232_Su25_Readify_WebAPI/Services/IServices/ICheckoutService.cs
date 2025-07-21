using Microsoft.AspNetCore.Mvc;

namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface ICheckoutService
    {
        Task<bool> Checkout();
    }
}

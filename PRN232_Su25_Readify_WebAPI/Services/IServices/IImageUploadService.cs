namespace PRN232_Su25_Readify_WebAPI.Services.IServices
{
    public interface IImageUploadService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder, string? publicId = null);
    }
}

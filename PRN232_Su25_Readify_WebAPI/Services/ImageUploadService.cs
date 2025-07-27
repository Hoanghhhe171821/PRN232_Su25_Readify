using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;

        public ImageUploadService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folder, string? publicId = null)
        {
            if (file == null || file.Length == 0)
                return null;

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                PublicId = publicId,
                Transformation = new Transformation().Width(300).Height(300).Crop("fill").Gravity("face")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return result.SecureUrl.ToString();
            }

            return null;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Octokit;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    public class ChaptersController : ControllerBase
    {
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repo;
        public ChaptersController(IConfiguration configuration)
        {
            var token = configuration["GitHub:Token"];
            _owner = configuration["GitHub:Owner"];
            _repo = configuration["GitHub:Repo"];   

            _client = new GitHubClient(new ProductHeaderValue("PRN232_Su25_Readify_WebAPI"));
            _client.Credentials = new Credentials(token);
        }
        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Ví dụ: lấy thông tin repo
                var repo = await _client.Repository.Get(_owner, _repo);
                return Ok(new
                {
                    repo.FullName,
                    repo.Private,
                    repo.Description
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

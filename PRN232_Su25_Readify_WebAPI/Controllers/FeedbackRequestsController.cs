using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.DbContext;
using PRN232_Su25_Readify_WebAPI.Dtos.FeedbackRequest;
using PRN232_Su25_Readify_WebAPI.Models;
using System.Security.Claims;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackRequestsController : ControllerBase
    {
        private readonly ReadifyDbContext _context;

        public FeedbackRequestsController(ReadifyDbContext context)
        {
            _context = context; 
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] FeedbackAddRequest request)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("hehe");
            }

            FeedbackRequest model = new FeedbackRequest ()
            {
                UserId = userId,
                Subject = request.Subject,
                Message = request.Message,
                Response = "",
                ResponseUserId = userId,
                CreateDate = DateTime.UtcNow,
            };
            _context.FeedbackRequests.Add(model);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.FeedbackRequests.Include(fr => fr.User)
                .Select( fr => new FeedbackRequestResponse
                {
                    Id = fr.Id,
                    Subject = fr.Subject,
                    Message = fr.Message,
                    UserId = fr.UserId,
                    UserName = fr.User.UserName,
                    Response = fr.Response,
                    CreateDate = fr.CreateDate,
                    UpdateDate = fr.UpdateDate,
                }).ToListAsync());
        }

        [HttpPut("response")]
        public async Task<IActionResult> ResponseFeedback([FromBody] FeedbackAnswerRequest request)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }
            var feedbackRequest = _context.FeedbackRequests.FirstOrDefault(fr => fr.Id == request.Id);
            if (feedbackRequest == null)
            {
                return BadRequest("Not found feedback request");
            }
            feedbackRequest.Response = request.Response;
            feedbackRequest.UpdateDate = DateTime.Now;
            _context.SaveChanges();
            return Ok();
        }

    }
}

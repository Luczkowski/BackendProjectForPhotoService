using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PTBackend2024.Models;
using System.Security.Claims;

namespace PTBackend2024.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class photosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;

        public photosController(ApplicationDbContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos()
        {
            var photos = await _context.Photos
                .Select(p => new
                {
                    p.PhotoId,
                    p.Uri,
                    p.Title,
                    p.Description,
                    p.CreatedDate,
                    p.UserId,
                    likes = p.Likes.Select(l => new
                    {
                        l.User.Username
                    }).ToList(),
                    comments = p.Comments.Select(c => new
                    {
                        c.CommentId,
                        c.Content,
                        c.User.Username,
                        c.User.ProfilePhotoUri
                    }).ToList()
                }).ToListAsync();

            return Ok(photos);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadPhoto([FromForm] PhotoDto newPhoto)
        {
            if (newPhoto.File == null || newPhoto.File.Length == 0)
                return BadRequest("No file uploaded");

            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);

            // Connection strings
            var containerClient = _blobServiceClient.GetBlobContainerClient("photos");
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newPhoto.File.FileName);
            // Connect with storage
            var blobClient = containerClient.GetBlobClient(fileName);
            await using var stream = newPhoto.File.OpenReadStream();
            // Upload photo
            await blobClient.UploadAsync(stream);

            var photo = new Photo
            {
                FileName = fileName,
                Uri = blobClient.Uri.ToString(),
                Title = newPhoto.Title,
                Description = newPhoto.Description,
                CreatedDate = DateTime.Now,
                UserId = userID
            };

            // Save info to database
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(photo);
        }

        [HttpGet("{photoID}")]
        public async Task<IActionResult> GetPhoto(int photoID)
        {
            var photo = await _context.Photos
                .Select(p => new
                {
                    p.PhotoId,
                    p.Uri,
                    p.Title,
                    p.Description,
                    p.CreatedDate,
                    p.UserId,
                    likes = p.Likes.Select(l => new
                    {
                        l.User.Username
                    }).ToList(),
                    comments = p.Comments.Select(c => new
                    {
                        c.CommentId,
                        c.Content,
                        c.User.Username,
                        c.User.ProfilePhotoUri
                    }).ToList()
                }).FirstOrDefaultAsync(p => p.PhotoId == photoID);
            if (photo == null)
            {
                return NotFound("Photo not found");
            }

            return Ok(photo);
        }

        //[HttpGet("{photoID}/likes")]
        //public async Task<ActionResult<IEnumerable<Photo>>> GetLikes(int photoID)
        //{
        //    var likes = await _context.Likes
        //        .Where(l => l.PhotoId == photoID)
        //        .Select(l => new
        //        {
        //            l.LikeId,
        //            l.UserId
        //        }).ToListAsync();

        //    return Ok(likes);
        //}

        [HttpPost("{photoID}/like")]
        [Authorize]
        public async Task<IActionResult> LikePhoto(int photoID)
        {
            var photo = await _context.Photos.FindAsync(photoID);
            if (photo == null)
            {
                return NotFound("Photo not found");
            }

            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var user = await _context.Users.FindAsync(userID);

            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PhotoId == photoID && l.UserId == userID);
            if (like != null)
            {
                return BadRequest();
            }

            like = new Like
            {
                PhotoId = photoID,
                UserId = userID
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{photoID}/unlike")]
        [Authorize]
        public async Task<IActionResult> UnlikePhoto(int photoID)
        {
            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PhotoId == photoID && l.UserId == userID);
            if (like == null)
            {
                return NotFound("Like not found");
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return Ok();
        }

        //[HttpGet("{photoID}/comments")]
        //public async Task<ActionResult<IEnumerable<Photo>>> GetComments(int photoID)
        //{
        //    var likes = await _context.Comments
        //        .Where(c => c.PhotoId == photoID)
        //        .Select(c => new
        //        {
        //            c.CommentId,
        //            c.Content,
        //            c.CreadesDate,
        //            c.UserId
        //        }).ToListAsync();

        //    return Ok(likes);
        //}

        [HttpPost("{photoID}/comment")]
        [Authorize]
        public async Task<IActionResult> AddComment(int photoID, [FromBody] CommentDto newComment)
        {
            var photo = await _context.Photos.FindAsync(photoID);
            if (photo == null)
            {
                return NotFound("Photo not found");
            }

            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var user = await _context.Users.FindAsync(userID);

            var comment = new Comment
            {
                Content = newComment.Content,
                CreadesDate = DateTime.Now,
                PhotoId = photoID,
                UserId = userID
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);

        }

        [HttpDelete("comment/{commentID}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentID)
        {
            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);

            var comment = await _context.Comments.FindAsync(commentID);
            if (comment == null || comment.UserId != userID)
            {
                return BadRequest("Comment not found or des not belong to user");
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{photoID}")]
        [Authorize]
        public async Task<IActionResult> DeletePhoto(int photoID)
        {
            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);

            var photo = await _context.Photos.FindAsync(photoID);
            if (photo == null || photo.UserId != userID)
            {
                return NotFound("Photo not found");
            }

            // Connection stings
            var containerClient = _blobServiceClient.GetBlobContainerClient("photos");
            // Connect with storage
            var blobClient = containerClient.GetBlobClient(photo.FileName);
            // Delete photo
            await blobClient.DeleteIfExistsAsync();

            // Delete info from database
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return Ok("Photo deleted");
        }
    }
}

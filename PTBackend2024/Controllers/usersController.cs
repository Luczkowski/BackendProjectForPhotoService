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
    public class usersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;

        public usersController(ApplicationDbContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Photos)
                .Include(u => u.Likes)
                .Include(u => u.Comments)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.ProfilePhotoUri,
                    Photos = u.Photos.Select(p => new
                    {
                        p.PhotoId,
                        p.Uri,
                        p.Title,
                        p.Description,
                        p.CreatedDate,
                        likes = p.Likes.Select(l => new
                        {
                            l.User.UserId,
                            l.User.Username
                        }).ToList(),
                        comments = p.Comments.Select(c => new
                        {
                            c.CommentId,
                            c.Content,
                            c.User.Username,
                            c.User.ProfilePhotoUri
                        }).ToList()
                    }).ToList()
                }).ToListAsync();

            return Ok(users);
        }

        //[HttpPost]
        //public async Task<ActionResult<User>> PostUser(User newUser)
        //{
        //    if (_context.Users.Any(u => u.Username == newUser.Username))
        //    {
        //        return BadRequest("Nazwa jest już zajęta");
        //    }

        //    if (_context.Users.Any(u => u.Email == newUser.Email))
        //    {
        //        return BadRequest("Istnieje konto na ten email");
        //    }

        //    _context.Users.Add(newUser);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(PostUser), newUser);
        //}

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<User>> GetMe()
        {
            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);

            var user = await _context.Users
                .Include(u => u.Photos)
                .Include(u => u.Likes)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.ProfilePhotoUri,
                    Photos = u.Photos.Select(p => new
                    {
                        p.PhotoId,
                        p.Uri,
                        p.Title,
                        p.Description,
                        p.CreatedDate,
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
                        })
                    }).ToList()
                }).FirstOrDefaultAsync(u => u.UserId == userID);

            return Ok(user);
        }

        [HttpGet("{userID}")]
        public async Task<ActionResult<User>> GetUser(int userID)
        {
            var user = await _context.Users
                .Include(u => u.Photos)
                .Include(u => u.Likes)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.ProfilePhotoUri,
                    Photos = u.Photos.Select(p => new
                    {
                        p.PhotoId,
                        p.Uri,
                        p.Title,
                        p.Description,
                        p.CreatedDate,
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
                        })
                    }).ToList()
                }).FirstOrDefaultAsync(u => u.UserId == userID);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("me/photos")]
        public async Task<ActionResult<IEnumerable<Photo>>> GetMyPhotos()
        {
            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);

            var userPhotos = await _context.Photos
                .Where(p => p.UserId == userID)
                .Select(p => new
                {
                    p.PhotoId,
                    p.Uri,
                    p.Title,
                    p.Description,
                    p.CreatedDate,
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

            return Ok(userPhotos);
        }

        [HttpGet("{userID}/photos")]
        public async Task<ActionResult<IEnumerable<Photo>>> GetUserPhotos(int userID)
        {
            var userPhotos = await _context.Photos
                .Where(p => p.UserId == userID)
                .Select(p => new
                {
                    p.PhotoId,
                    p.Uri,
                    p.Title,
                    p.Description,
                    p.CreatedDate,
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
            if (userPhotos == null)
            {
                return NotFound();
            }

            return Ok(userPhotos);
        }

        [HttpPut("me/profile_photo/{photoID}")]
        [Authorize]
        public async Task<IActionResult> SetProfilePhoto(int photoID)
        {
            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var user = await _context.Users.FindAsync(userID);

            var photo = await _context.Photos.FindAsync(photoID);
            if (photo == null || photo.UserId != userID)
            {
                return BadRequest("Photo not found or does not belong to user");
            }

            user.ProfilePhotoUri = photo.Uri;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            var userID = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var user = await _context.Users
                .Include(u => u.Photos)
                    .ThenInclude(p => p.Likes)
                .Include(u => u.Photos)
                    .ThenInclude(p => p.Comments)
                .Include(u => u.Likes)
                .Include(u => u.Comments)
                .FirstOrDefaultAsync(u => u.UserId == userID);

            _context.Likes.RemoveRange(user.Likes);
            _context.Comments.RemoveRange(user.Comments);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

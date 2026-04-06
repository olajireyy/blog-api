using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // sets base route to api/posts
    public class PostsController : ControllerBase
    {
        // ControllerBase is used for APIs — no view support
        private readonly AppDbContext _db;

        public PostsController(AppDbContext db)
        {
            _db = db; // injected automatically by ASP.NET dependency injection
        }

        // GET /api/posts — returns all posts as JSON array, newest first
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _db.Posts
                .OrderByDescending(p => p.CreatedAt) // newest first
                .ToListAsync();

            return Ok(posts); // 200 OK + serializes posts to JSON automatically
        }

        // GET /api/posts/5 — returns a single post by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _db.Posts.FindAsync(id); // optimized primary key lookup

            if (post == null)
                return NotFound(new { message = $"Post {id} not found" }); // 404

            return Ok(post); // 200 + post as JSON
        }

        // GET /api/posts/me — returns the logged in user's ID from their JWT token
        // React uses this to compare with post.userId to show/hide edit and delete buttons
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            // Read the user's ID embedded in the JWT claims — no DB lookup needed
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            return Ok(new { userId = int.Parse(userIdClaim.Value) });
        }

        // POST /api/posts — creates a new post, requires JWT token
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Post post)
        {
            // [FromBody] reads the request body as JSON and maps it to Post
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // 400 + validation errors

            // Read the logged in user's ID from the JWT token
            // This is stored in the token at login — no DB lookup needed
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // 401 — token missing or invalid

            post.UserId = int.Parse(userIdClaim.Value); // attach owner to the post
            post.CreatedAt = DateTime.Now;               // always set timestamp server-side

            _db.Posts.Add(post);          // stage the new post
            await _db.SaveChangesAsync(); // commit to DB

            // 201 Created + new post as JSON + Location header pointing to GET /api/posts/{id}
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        // PUT /api/posts/5 — updates an existing post, requires JWT token
        // Only the post owner can update — returns 403 if someone else tries
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Post post)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // 400 + validation errors

            var existing = await _db.Posts.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Post {id} not found" }); // 404

            // Read the logged in user's ID from the JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // 401 — not logged in

            var userId = int.Parse(userIdClaim.Value);

            // 403 Forbidden — logged in but not the owner of this post
            // Different from 401 which means not logged in at all
            if (existing.UserId != userId)
                return StatusCode(403, new { message = "You can only edit your own posts." });

            // Only update the fields we allow — never let client change Id, UserId, CreatedAt
            existing.Title = post.Title;
            existing.Content = post.Content;

            await _db.SaveChangesAsync(); // commit changes

            return Ok(existing); // 200 + updated post as JSON
        }

        // DELETE /api/posts/5 — deletes a post, requires JWT token
        // Only the post owner can delete — returns 403 if someone else tries
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _db.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { message = $"Post {id} not found" }); // 404

            // Read the logged in user's ID from the JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(); // 401 — not logged in

            var userId = int.Parse(userIdClaim.Value);

            // 403 Forbidden — logged in but not the owner of this post
            if (post.UserId != userId)
                return StatusCode(403, new { message = "You can only delete your own posts." });

            _db.Posts.Remove(post);       // stage the delete
            await _db.SaveChangesAsync(); // commit

            return NoContent(); // 204 — success but nothing to return
        }
    }
}
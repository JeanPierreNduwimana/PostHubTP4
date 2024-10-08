using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Formats.Asn1;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NuGet.Protocol.Plugins;
using PostHubAPI.Data;
using PostHubAPI.Models;
using PostHubAPI.Models.DTOs;
using PostHubAPI.Services;
using SixLabors.ImageSharp;

namespace PostHubAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly HubService _hubService;
        private readonly UserManager<User> _userManager;
        private readonly PictureService _pictureService;
        private readonly PostService _postService;

        public CommentsController(PictureService pictureService, PostService postService, HubService hubService, UserManager<User> userManager, CommentService commentService)
        {
            _pictureService = pictureService;
            _postService = postService;
            _hubService = hubService;
            _userManager = userManager;
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<ActionResult<List<Picture>>> AddPictures()
        {
            IFormCollection formcollection = await Request.ReadFormAsync();
            List<Picture> pictures = new List<Picture>();
            int index = 0;
            while (formcollection.Files.GetFile(index.ToString()) != null)
            {
                Picture picture = new Picture();
                IFormFile? file = formcollection.Files.GetFile(index.ToString());
                if (file != null)
                {

                    Image image = Image.Load(file.OpenReadStream());
                    picture.FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    picture.MimeType = file.ContentType;

                    await _pictureService.EditPicture(picture, file, image);
                    await _pictureService.AjoutPhoto(picture);
                    pictures.Add(picture);

                    index++;
                }

            }

            return pictures;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetPicture(int id)
        {
            if (_pictureService.IsContextNull())
            {
                return NotFound();
            }

            Picture? picture = await _pictureService.FindPicture(id);
            if (picture == null || picture.FileName == null || picture.MimeType == null) { return NotFound(new { Message = "Cette image n'existe pas" }); }

            byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);

            return File(bytes, picture.MimeType);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> GetPictureBig(int id)
        {
            if (_pictureService.IsContextNull())
            {
                return NotFound();
            }

            Picture? picture = await _pictureService.FindPicture(id);
            if (picture == null || picture.FileName == null || picture.MimeType == null) { return NotFound(new { Message = "Cette image n'existe pas" }); }

            byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/full/" + picture.FileName);

            return File(bytes, picture.MimeType);
        }

        [HttpPost]
        public async Task<ActionResult> DeletePicture(List<Picture> pictures)
        {
            await _pictureService.DeletePictures(pictures);
            return NoContent();
        }
        [HttpPost("{hubId}")]
        [Authorize]
        public async Task<ActionResult<PostDisplayDTO>> PostPost(int hubId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null) return Unauthorized();

            Hub? hub = await _hubService.GetHub(hubId);
            if (hub == null) return NotFound();

            PostDTO postDTO = new PostDTO();
            List<Picture> pictures = new List<Picture>();

            try

            {
                IFormCollection formcollection = await Request.ReadFormAsync();
                string? fileTitle = Request.Form["title"];
                string? fileText = Request.Form["text"];


                if (fileTitle != null && fileText != null)
                {
                    postDTO.Title = fileTitle;
                    postDTO.Text = fileText;
                }
                else
                {
                    return BadRequest(new { Message = "Il faut un titre et un text pour le post." });
                }
                int index = 0;

                pictures = await Recuperaptiondesphotos();

                if(formcollection.Files.Count < pictures.Count)
                {
                    return BadRequest(new { Message = "Il y a un problème avec les images ajoutés" });
                }
                

                
            }
            catch (Exception)
            {
                throw;
            }

            Comment? mainComment = await _commentService.CreateComment(user, postDTO.Text, null, pictures);

            
            if(mainComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            Post? post = await _postService.CreatePost(postDTO.Title, hub, mainComment);
            if(post == null) return StatusCode(StatusCodes.Status500InternalServerError);

            bool voteToggleSuccess = await _commentService.UpvoteComment(mainComment.Id, user);
            if(!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new PostDisplayDTO(post, true, user));
        }


        //Méthodes qui chargent les images recues

        private async Task<List<Picture>> Recuperaptiondesphotos()
        {
            List<Picture> pictures = new List<Picture>();

            IFormCollection formCollection = await Request.ReadFormAsync();
            int i = 0;

            while (formCollection.Files.GetFile(i.ToString()) != null)
            {

                IFormFile? file = formCollection.Files.GetFile(i.ToString());

                if (file != null)
                {
                    Image image = Image.Load(file.OpenReadStream());

                    Picture picture = new Picture()
                    {
                        Id = 0,
                        FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName),
                        MimeType = file.ContentType
                    };

                    await _pictureService.EditPicture(picture, file, image);
                    await _pictureService.AjoutPhoto(picture);

                    pictures.Add(picture);
                }


                i++;
            }

            return pictures;

        }

        [HttpPost("{parentCommentId}")]
        [Authorize]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<CommentDisplayDTO>> PostComment(int parentCommentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null) return Unauthorized();

            List<Picture> pictures = new List<Picture>();
            try
            {
                pictures = await Recuperaptiondesphotos();
            }
            catch (Exception)
            {
                return BadRequest(new { Message = "Il y a un problème avec les images ajoutés" });
            }
            



            Comment? parentComment = await _commentService.GetComment(parentCommentId);
            if (parentComment == null || parentComment.User == null) return BadRequest();

            string texteCommentaire = Request.Form["textComment"]; 
            Comment? newComment = await _commentService.CreateComment(user, texteCommentaire, parentComment, pictures);
            if (newComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            bool voteToggleSuccess = await _commentService.UpvoteComment(newComment.Id, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new CommentDisplayDTO(newComment, false, user));
        }

        [HttpGet("{tabName}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> GetPosts(string tabName, string sorting)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Post> posts = new List<Post>();
            IEnumerable<Hub>? hubs;

            if (tabName == "myHubs" && user != null && user.Hubs != null)
            {
                hubs = user.Hubs; 
            }
            else
            {
                hubs = await _hubService.GetAllHubs();
                if(hubs == null) return StatusCode(StatusCodes.Status500InternalServerError);
            }

            int postPerHub = (int)Math.Ceiling(10.0 / hubs.Count());

            foreach (Hub h in hubs)
            {
                if (sorting == "popular") posts.AddRange(GetPopularPosts(h, postPerHub));
                else posts.AddRange(GetRecentPosts(h, postPerHub));
            }

            if (sorting == "popular")
                posts = posts.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).ToList();
            else
                posts = posts.OrderByDescending(p => p.MainComment?.Date).ToList();

            return Ok(posts.Select(p => new PostDisplayDTO(p, false, null)));
        }

        [HttpGet("{searchText}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> SearchPosts(string searchText, string sorting)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Post> posts = new List<Post>();
            IEnumerable<Hub>? hubs = await _hubService.GetAllHubs();
            if (hubs == null) return StatusCode(StatusCodes.Status500InternalServerError);

            foreach (Hub h in hubs)
            {
                h.Posts ??= new List<Post>();
                posts.AddRange(h.Posts.Where(p => p.MainComment!.Text.ToUpper().Contains(searchText.ToUpper()) || p.Title.ToUpper().Contains(searchText.ToUpper())));
            }

            if (sorting == "popular")
                posts = posts.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).ToList();
            else
                posts = posts.OrderByDescending(p => p.MainComment?.Date).ToList();

            return Ok(posts.Select(p => new PostDisplayDTO(p, false, null)));
        }

        [HttpGet("{hubId}/{sorting}")]
        public async Task<ActionResult<IEnumerable<PostDisplayDTO>>> GetHubPosts(int hubId, string sorting)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Hub? hub = await _hubService.GetHub(hubId);
            if (hub == null) return NotFound();

            IEnumerable<PostDisplayDTO>? posts = hub.Posts?.Select(p => new PostDisplayDTO(p, false, user));
            if (sorting == "popular") posts = posts?.OrderByDescending(p => p.MainComment.Upvotes - p.MainComment.Downvotes);
            else { posts = posts?.OrderByDescending(p => p.MainComment.Date); }
            
            return Ok(posts);
        }

        [HttpGet("{postId}/{sorting}")]
        public async Task<ActionResult<PostDisplayDTO>> GetFullPost(int postId, string sorting)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Post? post = await _postService.GetPost(postId);
            if(post == null) return NotFound();

            PostDisplayDTO postDisplayDTO = new PostDisplayDTO(post, true, user);
            if (sorting == "popular") 
                postDisplayDTO.MainComment.SubComments = postDisplayDTO.MainComment!.SubComments!.OrderByDescending(c => c.Upvotes - c.Downvotes).ToList();
            else
                postDisplayDTO.MainComment.SubComments = postDisplayDTO.MainComment!.SubComments!.OrderByDescending(c => c.Date).ToList();

            

            return Ok(postDisplayDTO);
        }

        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDisplayDTO>> PutComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Picture> pictures = new List<Picture>();
            try
            {
                pictures = await Recuperaptiondesphotos();
            }
            catch (Exception)
            {
                return BadRequest(new { Message = "Il y a un problème avec les images ajoutés" });
            }

            Comment? comment = await _commentService.GetComment(commentId);
            if (comment == null) return NotFound();

            if (user == null || comment.User != user) return Unauthorized();

            string texteCommentaire = Request.Form["text"];

            Comment? editedComment = await _commentService.EditComment(comment, texteCommentaire, pictures);
            if(editedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new CommentDisplayDTO(editedComment, true, user));
        }

        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult> UpvoteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if(user == null) return BadRequest();

            bool voteToggleSuccess = await _commentService.UpvoteComment(commentId, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new { Message = "Vote complété." });
        }

        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<ActionResult> DownvoteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null) return BadRequest();

            bool voteToggleSuccess = await _commentService.DownvoteComment(commentId, user);
            if (!voteToggleSuccess) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new { Message = "Vote complété." });
        }

        [HttpDelete("{CommentId}/{PictureId}")]
        [Authorize]
        public async Task<ActionResult> RemovePicture(int CommentId, int PictureId)
        {
            Comment? comment = await _commentService.GetComment(CommentId);
            if (comment == null) return NotFound();

            Picture? picture = await _pictureService.FindPicture(PictureId);
            if (picture == null) return NotFound();

            foreach (var p in comment.Pictures)
            {
                if (p.Id == picture.Id)
                {
                    comment.Pictures.Remove(p);
                    await _pictureService.DeleteOnePicture(picture);
                    return Ok(new { Message = "Suppresion de la photo réussi" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "Suppresion de la photo n'a pas réussi" });
        }

        [HttpDelete("{CommentId}/{PictureId}")]
        [Authorize]
        public async Task<ActionResult> RemovePicturePost(int CommentId, int PictureId)
        {
            Post? post = await _postService.GetPost(CommentId);
            if (post == null) return NotFound();

            Picture? picture = await _pictureService.FindPicture(PictureId);
            if (picture == null) return NotFound();

            foreach (var p in post.MainComment.Pictures)
            {
                if (p.Id == picture.Id)
                {
                    post.MainComment.Pictures.Remove(p);
                    await _pictureService.DeleteOnePicture(picture);
                    return Ok(new { Message = "Suppresion de la photo réussi" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "Suppresion de la photo n'a pas réussi" });
        }

        [HttpDelete("{commentId}")]
        [Authorize(Roles = "moderator")]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Comment? comment = await _commentService.GetComment(commentId);
            if (comment == null) return NotFound();
            if (user == null || (!await _userManager.IsInRoleAsync(user, "moderator") && comment.User != user))
            {
                return Unauthorized();
            }

            await _pictureService.DeletePictures(comment.Pictures);



            do
            {
                comment.SubComments ??= new List<Comment>();

                Comment? parentComment = comment.ParentComment;

                if (comment.MainCommentOf != null && comment.GetSubCommentTotal() == 0)
                {
                    Post? deletedPost = await _postService.DeletePost(comment.MainCommentOf);
                    if (deletedPost == null) return StatusCode(StatusCodes.Status500InternalServerError);
                }

                if(comment.GetSubCommentTotal() == 0)
                {
                    Comment? deletedComment = await _commentService.HardDeleteComment(comment);
                    if (deletedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);
                    await _pictureService.DeletePictures(deletedComment.Pictures);
                }
                else
                {
                    Comment? deletedComment = await _commentService.SoftDeleteComment(comment);
                    if (deletedComment == null) return StatusCode(StatusCodes.Status500InternalServerError);
                    //_postService.DeletePictures(comment.Pictures);
                    break;
                }

                comment = parentComment;

            } while (comment != null && comment.User == null && comment.GetSubCommentTotal() == 0);

            return Ok(new { Message = "Commentaire supprimé." });
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Signalement(int id) 
        {
            await _commentService.reportComment(id);
            return Ok(new { Message = "Commentaire signalé."});
        }

        [HttpGet]
        [DisableRequestSizeLimit]
        [Authorize(Roles = "moderator")]
        public async Task<IActionResult> GetReported() 
        {
            List<Comment> commentsSignale = await _commentService.GetReportedComment();
            List<CommentDisplayDTO> comments = new List<CommentDisplayDTO>();

            foreach (Comment comment in commentsSignale) {
                CommentDisplayDTO monCommentaire = new CommentDisplayDTO(comment, false, null);
                comments.Add(monCommentaire);
            }

            return Ok(comments);
        }


        private static IEnumerable<Post> GetPopularPosts(Hub hub, int qty)
        {
            return hub.Posts!.OrderByDescending(p => p.MainComment?.Upvoters?.Count - p.MainComment?.Downvoters?.Count).Take(qty);
        }

        private static IEnumerable<Post> GetRecentPosts(Hub hub, int qty)
        {
            return hub.Posts!.OrderByDescending(p => p.MainComment?.Date).Take(qty);
        }
    }
}

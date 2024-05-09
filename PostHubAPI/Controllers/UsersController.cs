using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Plugins;
using PostHubAPI.Models;
using PostHubAPI.Models.DTOs;
using PostHubAPI.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PostHubAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly PictureService _pictureService;
        readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager, PictureService pictureService)
        {
            _userManager = userManager;
            _pictureService = pictureService;
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterDTO register)
        {
            if (register.Password != register.PasswordConfirm)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { Message = "Les deux mots de passe spécifiés sont différents." });
            }
            User user = new User()
            {
                UserName = register.Username,
                Email = register.Email
            };
            IdentityResult identityResult = await _userManager.CreateAsync(user, register.Password);
            if (!identityResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "La création de l'utilisateur a échoué." });
            }
            return Ok(new { Message = "Inscription réussie ! 🥳" });
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginDTO login)
        {
            User? user = await TrouverUser(login);

            if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);
                List<Claim> authClaims = new List<Claim>();
                foreach (string role in roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }
                authClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes("LooOOongue Phrase SiNoN Ça ne Marchera PaAaAAAaAas !"));
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: "http://localhost:7007",
                    audience: "http://localhost:4200",
                    claims: authClaims,
                    expires: DateTime.Now.AddMinutes(300),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                    );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    validTo = token.ValidTo,
                    username = user.UserName // Ceci sert déjà à afficher / cacher certains boutons côté Angular
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { Message = "Le nom d'utilisateur ou le mot de passe est invalide." });
            }
        }

        private async Task<User?> TrouverUser(LoginDTO loginuser)
        {
            User user = await _userManager.FindByNameAsync(loginuser.Username);
            User useremail = await _userManager.FindByEmailAsync(loginuser.Username);

            if(user != null)
            {
                return user;
            }

            if(useremail != null)
            {
                return useremail;
            }

            return null;
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> ChangeAvatar(String username)
        {
            User user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "L'utilisateur n'à pas pue être trouver!" });
            }

            IFormCollection formcollection = await Request.ReadFormAsync();
            IFormFile? file = formcollection.Files.GetFile("UserNewAvatar");
            Picture picture = new Picture();

            try
            {
                Image image = Image.Load(file.OpenReadStream());
                if (file != null)
                {
                    picture.Id = 0;
                    picture.FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    picture.MimeType = file.ContentType;
                    user.FileName = picture.FileName;
                    user.MimeType = picture.MimeType;
                }
                await _pictureService.EditAvatar(picture, image);
                await _pictureService.AjoutPhoto(picture);

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                   new { Message = "L'image n'à pas pue être enregistrer!" });
            }

            return Ok(new { Message = "L'avatar à été changer avec succès!" });
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetAvatar(String username)
        {
            User user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/avatar/default.png");
                return File(bytes, "image/png");
            }

            if (user.FileName != null && user.MimeType != null)
            {
                Picture picture = new Picture();
                picture.FileName = user.FileName;
                picture.MimeType = user.MimeType;
                byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/avatar/" + picture.FileName);
                return File(bytes, picture.MimeType);
            }
            else
            {
                if (user.FileName == null && user.MimeType == null)
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/avatar/default.png");
                    return File(bytes, "image/png");
                }else
                {
                    return StatusCode(StatusCodes.Status404NotFound,
                                       new { Message = "Custom picture not found. Please upload a profile picture." });
                }
            }
        }

        [HttpPost()]
        public async Task<IActionResult> ChangerMotDePasse()
        {
            string? mdpActuelle = Request.Form["oldPassword"];
            string? mdpNouveau = Request.Form["newPassword"];

            if (mdpActuelle == null || mdpNouveau == null)
            {
                return BadRequest();
            }

            User? user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null)
            {
                return Unauthorized();
            }

            if(await _userManager.CheckPasswordAsync(user, mdpActuelle))
            {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, mdpNouveau);
            }

            return Ok(new { Message = "Changement de mot de passe reussi" });

        }

        [HttpPost()]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> MakeAdmin(string username)
        {

            User? user = await _userManager.FindByIdAsync(username);

            if (user == null)
            {
                return NotFound(new { Message = "Cet utilisateur n'existe pas." });
            }

            await _userManager.AddToRoleAsync(user, "admin");
            return Ok(new { Message = user + " est maintenant admin!" });
        }

        [HttpPost("{username}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> MakeModerator(string username)
        {
            User? user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound(new { Message = "Cet utilisateur n'existe pas." });
            }

            await _userManager.AddToRoleAsync(user, "moderator");
            return Ok(new { Message = user + " est maintenant moderateur!" });
        }


        [HttpGet("{username}")]
        public async Task<bool> IsUserAdmin(string username)
        {
            User? user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, "admin");
        }

        [HttpGet("{username}")]
        public async Task<bool> IsUserModerator(string username)
        {
            User? user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, "moderator");
        }
    }
}
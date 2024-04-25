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
            User user = await _userManager.FindByNameAsync(login.Username);
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

        [HttpPost]
        public async Task<IActionResult> ChangeAvatar(String username)
        {
            User user = await _userManager.FindByNameAsync(username);

            if (user == null) {
                return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "L'utilisateur n'à pas pue être trouver!" });
            }

            IFormCollection formcollection = await Request.ReadFormAsync();
            IFormFile? file = formcollection.Files.GetFile("UserNewAvatar");
            Picture picture = new Picture();

            try{
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
                return StatusCode(StatusCodes.Status500InternalServerError,
                                   new { Message = "L'utilisateur n'à pas pue être trouver!" });
            }

            Picture picture = new Picture();
            if (user.FileName != null && user.MimeType != null)
            {
                picture.FileName = user.FileName;
                picture.MimeType = user.MimeType;
                byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/avatar/" + picture.FileName);
                return File(bytes, picture.MimeType);
            }
            else {
                byte[] bytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/images/avatar/default.png");
                return File(bytes, picture.FileName);
            }
        }   
    }
}

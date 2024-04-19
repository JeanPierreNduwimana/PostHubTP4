using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Data;
using PostHubAPI.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;

namespace PostHubAPI.Services
{
    public class PictureService
    {
        private readonly PostHubAPIContext _context;

        public PictureService(PostHubAPIContext context) 
        {
            _context = context;
        }

        public async Task AjoutPhoto(Picture picture)
        {
            if(!IsContextNull())
            {
                await _context.Pictures.AddAsync(picture);
                await _context.SaveChangesAsync();
            }
            
        }

        public async Task<List<Picture>> ListPhoto()
        {
           return await _context.Pictures.ToListAsync();
        }

        public async Task<Picture?> FindPicture(int id)
        {
            Picture? picture = await _context.Pictures.FindAsync(id);

            if(picture == null)
            {
                return null;
            }
            else
            {
                return picture;
            }
            
        }

        public bool IsContextNull() => _context.Pictures == null;
    }
}

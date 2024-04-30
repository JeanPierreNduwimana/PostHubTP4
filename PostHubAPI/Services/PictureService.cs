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

        public async Task DeletePictures(List<Picture>  pictures)
        {
            if (pictures == null)
            {
                return;
            }

            foreach (Picture picture in pictures)
            {
                _context.Pictures.Remove(picture);
                System.IO.File.Delete(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);
                System.IO.File.Delete(Directory.GetCurrentDirectory() + "/images/full/" + picture.FileName);

            }
            await _context.SaveChangesAsync();


        }


        public async Task<Picture[]> EditPicture(Picture picture, IFormFile file, Image image) {

            List<Picture> pictures = new List<Picture>();

            image.Save(Directory.GetCurrentDirectory() + "/images/full/" + picture.FileName);

                
            image.Mutate(i => i.Resize(new ResizeOptions()
            {
                    Mode = ResizeMode.Min,
                    Size = new Size() { Width = 320}
                })
            );

            image.Save(Directory.GetCurrentDirectory() + "/images/thumbnail/" + picture.FileName);
            pictures.Add(picture);

            return pictures.ToArray();
        }

        public async Task<Picture> EditAvatar(Picture picture, Image image)
        {
            image.Save(Directory.GetCurrentDirectory() + "/images/avatar/" + picture.FileName);
            return picture;
        }

        public async Task AjoutPhoto(Picture picture)
        {

            if (!IsContextNull())
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Models;

namespace PostHubAPI.Data
{
    public class PostHubAPIContext : IdentityDbContext<User>
    {
        public PostHubAPIContext (DbContextOptions<PostHubAPIContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Nécessaire pour la relation One-To-One entre Post et Comment : EF veut savoir quelle table contiendra la clé étrangère
            modelBuilder.Entity<Post>().HasOne(p => p.MainComment).WithOne(c => c.MainCommentOf).HasForeignKey<Post>(p => p.MainCommentId);


            modelBuilder.Entity<IdentityRole>().HasData(

                new IdentityRole { Id = "1", Name = "admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "moderator", NormalizedName = "MODERATOR" }

            );

            //Ajout de l'admin
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            User u1 = new User
            {
                Id = "11111111-1111-1111-1111-111111111111",
                UserName = "UserAdmin",
                NormalizedUserName = "USERADMIN",
                Email = "a@a.a",
                NormalizedEmail = "A@A.A"
            };
            u1.PasswordHash = passwordHasher.HashPassword(u1, "Salut1!");

            modelBuilder.Entity<User>().HasData(u1);

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(

                new IdentityUserRole<string> { UserId = u1.Id, RoleId = "1" }
            );

            //Ajout du Modo
            PasswordHasher<User> passwordHasherModo = new PasswordHasher<User>();
            User modo1 = new User
            {
                Id = "22222222-2222-2222-2222-222222222222",
                UserName = "UserModo",
                NormalizedUserName = "USERMODO",
                Email = "m@m.m",
                NormalizedEmail = "M@M.M"
            };
            modo1.PasswordHash = passwordHasherModo.HashPassword(modo1, "Salut1!");
            modelBuilder.Entity<User>().HasData(modo1);
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(

                new IdentityUserRole<string> { UserId = modo1.Id, RoleId = "2"}
            );
        }

        public DbSet<Hub> Hubs { get; set; } = default!;
        public DbSet<Post> Posts { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Picture> Pictures { get; set; } = default!;
    }
}

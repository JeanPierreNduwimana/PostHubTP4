﻿namespace PostHubAPI.Models.DTOs
{
    public class CommentDTO
    {
        public string Text { get; set; } = null!;
        public List<Picture>? pictures { get; set; }
    }
}

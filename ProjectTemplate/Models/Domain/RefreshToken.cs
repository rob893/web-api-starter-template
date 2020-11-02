using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectTemplate.Models.Domain
{
    public class RefreshToken
    {
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        [MaxLength(255)]
        public string Token { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
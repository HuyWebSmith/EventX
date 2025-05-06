using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EventX.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public string? RolePreference { get; set; } // Attendee, Speaker, Sponsor, Organizer
        public string? InterestArea { get; set; }   // Tech, Business, Music...
        public string? Bio { get; set; }
        public string? LinkedInProfile { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true; // Quản lý trạng thái hoạt động của người dùng
        public DateTime? LastLogin { get; set; }
        public string? Address { get; set; }
        public bool IsBlocked { get; set; }
    }
}

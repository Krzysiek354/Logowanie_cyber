using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cyber_zad_1_.Models
{
    public class User : IdentityUser
    {
        public User() {
            FullName = null; 
            IsBlocked = false; 
            IsPasswordComplexityEnabled = true; 
            IsFirstLogin = true;
        }

        public User(string userName, string passwordHash, string fullName)
        {
            this.UserName = userName; 
            this.PasswordHash = passwordHash;
            this.FullName = fullName;
        }

        public string? FullName { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsPasswordComplexityEnabled { get; set; }
        public bool IsFirstLogin { get; set; }

        public DateTime PasswordChangedDate { get; set; }
    }
}
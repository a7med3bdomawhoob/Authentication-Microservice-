using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DAL.Entities
{
    public class AppUser: IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }


        public AppUser(string userName, string email) : base(userName)
        {
            Email = email;
            FirstName = string.Empty; // Default value
            LastName = string.Empty;  // Default value
        }


    }
}

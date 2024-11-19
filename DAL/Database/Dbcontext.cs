using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Database
{
    public class Dbcontext : IdentityDbContext<AppUser>
    {
        public Dbcontext(DbContextOptions<Dbcontext> options)
       : base(options)
        {
        }

    }
}

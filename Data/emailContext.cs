using EmailDemo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailDemo.Data
{
    public class emailContext : DbContext
    {
        public emailContext(DbContextOptions<emailContext> options) : base(options)
        {

        }

        public emailContext()
        {

        }

        public DbSet<register> register { get; set; }
    }

    
}

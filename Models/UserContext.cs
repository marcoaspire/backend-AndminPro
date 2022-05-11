using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _04_API_HospitalAPP.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            builder.Entity<User>(entity =>
            {
                entity.ToTable(name: "tbl_users");
            });
            builder.Entity<Hospital>(entity =>
            {
                entity.ToTable(name: "tbl_hopitals");
            });
            builder.Entity<Doctor>(entity =>
            {
                entity.ToTable(name: "tbl_doctors");
            });
        }
        public DbSet<User> Users { get; set; }
        
        public DbSet<Hospital> Hospitals { get; set; }
        
        public DbSet<Doctor> Doctors { get; set; }
        
    }
}

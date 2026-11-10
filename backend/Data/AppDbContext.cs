using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Database tables
        public DbSet<SchoolClass> Classes { get; set; }
        public DbSet<SeatHold> Holds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Creating indexes is highly recommended for efficient concurrency locking.
            modelBuilder.Entity<SeatHold>()
                .HasIndex(h => h.ClassId);

            modelBuilder.Entity<SeatHold>()
                .HasIndex(h => h.StudentId);
        }
    }
}
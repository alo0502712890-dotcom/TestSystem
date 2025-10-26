using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestSystem.Models;
using static System.Net.Mime.MediaTypeNames;

namespace TestSystem.Data
{
    public partial class TestSystemContext : DbContext
    {
        public TestSystemContext() { }

        public TestSystemContext(DbContextOptions<TestSystemContext> options)
            : base(options) { }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<TestSession> TestSessions { get; set; }
        public virtual DbSet<UserAnswer> UserAnswers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=Beneduk\\SQLEXPRESS;Database=TestSystem;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestSession>()
                .HasKey(ts => ts.SessionID); 

            base.OnModelCreating(modelBuilder);
        }
    }
}

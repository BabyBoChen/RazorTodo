using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RazorTodo.Abstraction.Models;
using RazorTodo.Abstraction.Services;

#nullable disable

namespace RazorTodo.DAL
{
    public partial class RazorTodoContext : DbContext, IDbContext
    {
        public RazorTodoContext()
        {
        }

        public RazorTodoContext(DbContextOptions<RazorTodoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<GovernmentCalendar> GovernmentCalendars { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<TblUid> TblUids { get; set; }
        public virtual DbSet<Todo> Todos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=RazorTodo.db");
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GovernmentCalendar>(entity =>
            {
                entity.ToTable("GovernmentCalendar");
            });

            modelBuilder.Entity<Photo>(entity =>
            {
                entity.ToTable("Photo");

                entity.Property(e => e.FileId).IsRequired();

                entity.Property(e => e.Uid).IsRequired();

                entity.HasOne(d => d.Todo)
                    .WithMany(p => p.Photos)
                    .HasForeignKey(d => d.TodoId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TblUid>(entity =>
            {
                entity.HasKey(e => e.Uid);

                entity.ToTable("TblUid");
            });

            modelBuilder.Entity<Todo>(entity =>
            {
                entity.ToTable("todo");

                entity.Property(e => e.CreatedDate)
                    .IsRequired()
                    .HasDefaultValueSql("date('now', 'localtime')");

                entity.Property(e => e.LineOrder).HasDefaultValueSql("0");

                entity.Property(e => e.TodoName)
                    .IsRequired()
                    .HasDefaultValueSql("'Untitled'");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public void SetProxyEnabled(bool isEnabled)
        {
            this.ChangeTracker.LazyLoadingEnabled = isEnabled;
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace RazorTodo.DAL
{
    public partial class RazorTodoContext : DbContext
    {
        public RazorTodoContext()
        {
        }

        public RazorTodoContext(DbContextOptions<RazorTodoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<GovernmentCalendar> GovernmentCalendars { get; set; }
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
    }
}

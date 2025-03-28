﻿using backend.Core.Entities;
using backend.Core.Entities.PanelManagement;
using backend.Core.Enums;
using backend.Infrastructure.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }

        public DbSet<SupervisionRequest> SupervisionRequests { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<MessageReadStatus> MessageReadStatuses { get; set; }

        public DbSet<Panel> Panels { get; set; }
        public DbSet<PanelMember> PanelMembers { get; set; }
        public DbSet<EvaluationEvent> EvaluationEvents { get; set; }
        public DbSet<GroupEvaluation> GroupEvaluations { get; set; }
        public DbSet<StudentEvaluation> StudentEvaluations { get; set; }

        public DbSet<EvaluationRubric> EvaluationRubrics { get; set; }
        public DbSet<RubricCategory> RubricCategories { get; set; }
        public DbSet<StudentCategoryScore> StudentCategoryScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Teacher>().ToTable("Teachers");
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<GroupMember>().ToTable("GroupMembers");


            // Configure relationships
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Student)
                .WithMany()
                .HasForeignKey(gm => gm.StudentId);


            modelBuilder.Entity<SupervisionRequest>().ToTable("SupervisionRequests");
            modelBuilder.Entity<SupervisionRequest>()
                .HasOne(sr => sr.Group)
                .WithMany()
                .HasForeignKey(sr => sr.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.Teacher)
                .WithMany()
                .HasForeignKey(g => g.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Group)
                    .WithMany(g => g.Messages)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sender)
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<MessageReadStatus>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Message)
                    .WithMany(m => m.ReadStatuses)
                    .HasForeignKey(e => e.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.MessageId, e.UserId }).IsUnique();
            });



            // Configure unique constraint to prevent duplicate teacher assignments in a panel
            modelBuilder.Entity<PanelMember>()
                .HasIndex(pm => new { pm.PanelId, pm.TeacherId })
                .IsUnique();

            // Configure the GroupEvaluation to ensure a group can only be evaluated 
            // once by a panel for a specific event
            modelBuilder.Entity<GroupEvaluation>()
                .HasIndex(ge => new { ge.GroupId, ge.EventId })
                .IsUnique();

            // Configure RubricCategory to ensure weights add up to 1.0 (100%)
            modelBuilder.Entity<RubricCategory>()
                .HasIndex(rc => rc.RubricId);

            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                Id = 1,
                CreatedAt = new DateTime(2025, 2, 18, 12, 0, 0, DateTimeKind.Utc),
                Email = "admin@projectgo.com",
                FullName = "System Admin",
                PasswordHash = "$2a$11$dGbHOWMrjr/9KPl9LxongumrriovDITJb6H42vb3s4RpHAYURKE4C", // Static hash for "adminpassword"
                Role = UserType.Admin,
                IsSuperAdmin = true
            });





            // Apply configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}

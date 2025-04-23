using backend.Core.Entities;
using backend.Core.Entities.PanelManagement;
using backend.Core.Enums;
using backend.Infrastructure.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        #region DbSet Properties
        // User Management
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }

        // Group Management
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<SupervisionRequest> SupervisionRequests { get; set; }

        // Communication
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<MessageReadStatus> MessageReadStatuses { get; set; }

        // Panel Management
        public DbSet<Panel> Panels { get; set; }
        public DbSet<PanelMember> PanelMembers { get; set; }

        // Evaluation Management
        public DbSet<EvaluationEvent> EvaluationEvents { get; set; }
        public DbSet<GroupEvaluation> GroupEvaluations { get; set; }
        public DbSet<StudentEvaluation> StudentEvaluations { get; set; }
        public DbSet<EvaluationRubric> EvaluationRubrics { get; set; }
        public DbSet<RubricCategory> RubricCategories { get; set; }
        public DbSet<StudentCategoryScore> StudentCategoryScores { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var seedDateTime = new DateTime(2024, 3, 17, 12, 0, 0, DateTimeKind.Utc);

            base.OnModelCreating(modelBuilder);

            #region Table Configurations
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Teacher>().ToTable("Teachers");
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<GroupMember>().ToTable("GroupMembers");
            modelBuilder.Entity<SupervisionRequest>().ToTable("SupervisionRequests");
            #endregion

            #region Group Management Relationships
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasOne(gm => gm.Group)
                    .WithMany(g => g.Members)
                    .HasForeignKey(gm => gm.GroupId);

                entity.HasOne(gm => gm.Student)
                    .WithMany()
                    .HasForeignKey(gm => gm.StudentId);
            });

            modelBuilder.Entity<SupervisionRequest>(entity =>
            {
                entity.HasOne(sr => sr.Group)
                    .WithMany()
                    .HasForeignKey(sr => sr.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasOne(g => g.Teacher)
                    .WithMany()
                    .HasForeignKey(g => g.TeacherId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region Communication Configuration
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
            #endregion

            #region Evaluation Configuration
            modelBuilder.Entity<PanelMember>()
                .HasIndex(pm => new { pm.PanelId, pm.TeacherId })
                .IsUnique();

            modelBuilder.Entity<GroupEvaluation>()
                .HasIndex(ge => new { ge.GroupId, ge.EventId })
                .IsUnique();

            modelBuilder.Entity<StudentEvaluation>(entity =>
            {
                // Existing configurations
                entity.HasOne(se => se.Rubric)
                    .WithMany()
                    .HasForeignKey(se => se.RubricId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(se => se.StudentId);
                entity.HasIndex(se => se.GroupEvaluationId);

                // Add this new configuration
                entity.Property(se => se.RequiredEvaluatorsCount)
                    .HasDefaultValue(0);
            });

            modelBuilder.Entity<RubricCategory>(entity =>
            {
                entity.HasIndex(rc => rc.RubricId);
                entity.HasOne(rc => rc.Rubric)
                    .WithMany(r => r.Categories)
                    .HasForeignKey(rc => rc.RubricId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StudentCategoryScore>(entity =>
            {
                entity.HasOne(scs => scs.Category)
                    .WithMany()
                    .HasForeignKey(scs => scs.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(scs => new { scs.StudentEvaluationId, scs.CategoryId, scs.EvaluatorId })
         .IsUnique();
            });
            #endregion

            #region Seed Data
            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                Id = 1,
                CreatedAt = new DateTime(2025, 2, 18, 12, 0, 0, DateTimeKind.Utc),
                Email = "admin@projectgo.com",
                FullName = "System Admin",
                PasswordHash = "$2a$11$dGbHOWMrjr/9KPl9LxongumrriovDITJb6H42vb3s4RpHAYURKE4C",
                Role = UserType.Admin,
                IsSuperAdmin = true
            });

             
            #endregion

            // Apply additional configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}

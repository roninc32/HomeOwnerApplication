using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HomeOwnerApplication.Models;

namespace HomeOwnerApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly DateTime _currentTime = DateTime.Parse("2025-03-20 22:19:26");
        private const string CurrentUser = "roninc32";

        public virtual DbSet<UserActivity> UserActivities { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Set default schema
            builder.HasDefaultSchema("identity");

            // Configure ASP.NET Identity tables with schema prefix
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");

                // Indexes
                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                entity.HasIndex(u => u.UserName)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_UserName");

                entity.HasIndex(u => u.PhoneNumber)
                    .HasDatabaseName("IX_Users_PhoneNumber");

                entity.HasIndex(u => u.CreatedAt)
                    .HasDatabaseName("IX_Users_CreatedAt");

                // Properties
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                entity.Property(u => u.PropertyAddress)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                entity.Property(u => u.CreatedAt)
                    .IsRequired()
                    .HasColumnType("datetime2")
                    .HasDefaultValue(_currentTime)
                    .HasComment("Date and time when the user was created");

                entity.Property(u => u.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)")
                    .HasDefaultValue(CurrentUser)
                    .HasComment("User who created the record");

                entity.Property(u => u.LastLoginAt)
                    .HasColumnType("datetime2")
                    .HasComment("Date and time of the user's last login");

                entity.Property(u => u.ModifiedAt)
                    .IsRequired()
                    .HasColumnType("datetime2")
                    .HasDefaultValue(_currentTime)
                    .HasComment("Last modified timestamp");

                entity.Property(u => u.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)")
                    .HasDefaultValue(CurrentUser)
                    .HasComment("User who last modified the record");

                entity.Property(u => u.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true)
                    .HasComment("Indicates if the user account is active");

                entity.Property(u => u.EmergencyContactName)
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)")
                    .HasComment("Name of emergency contact");

                entity.Property(u => u.EmergencyContactPhone)
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)")
                    .HasComment("Phone number of emergency contact");

                // Query filter for soft delete
                entity.HasQueryFilter(u => u.IsActive);
            });

            // Identity tables
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Configure UserActivity
            builder.Entity<UserActivity>(entity =>
            {
                entity.ToTable("UserActivities");

                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_UserActivities_UserId");
                entity.HasIndex(e => e.ActivityTime)
                    .HasDatabaseName("IX_UserActivities_ActivityTime");
                entity.HasIndex(e => e.ActivityType)
                    .HasDatabaseName("IX_UserActivities_ActivityType");

                // Properties
                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)")
                    .HasComment("Type of activity performed");

                entity.Property(e => e.ActivityTime)
                    .IsRequired()
                    .HasColumnType("datetime2")
                    .HasComment("Time when the activity occurred");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)")
                    .HasDefaultValue(CurrentUser);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("datetime2")
                    .HasDefaultValue(_currentTime);

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)")
                    .HasDefaultValue(CurrentUser);

                entity.Property(e => e.ModifiedAt)
                    .IsRequired()
                    .HasColumnType("datetime2")
                    .HasDefaultValue(_currentTime);

                // Relationships
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_UserActivities_Users_UserId");
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        private void UpdateAuditFields()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is ApplicationUser || entry.Entity is UserActivity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Property("CreatedAt").CurrentValue = _currentTime;
                            entry.Property("CreatedBy").CurrentValue = CurrentUser;
                            entry.Property("ModifiedAt").CurrentValue = _currentTime;
                            entry.Property("ModifiedBy").CurrentValue = CurrentUser;
                            break;
                        case EntityState.Modified:
                            entry.Property("ModifiedAt").CurrentValue = _currentTime;
                            entry.Property("ModifiedBy").CurrentValue = CurrentUser;
                            break;
                    }
                }
            }
        }
    }
}
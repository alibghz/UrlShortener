using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions options
            ) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ConfigureEntities(builder);
            base.OnModelCreating(builder);
        }

        private static void ConfigureEntities(ModelBuilder builder)
        {
            builder.Entity<ShortenedUrl>()
                .HasIndex(r => r.Code)
                .IsUnique();

            builder.Entity<ShortenedUrl>()
                .HasIndex(r => r.Url)
                .IsUnique();

        }

        public override int SaveChanges()
        {
            SetTimeStamps();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetTimeStamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimeStamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetTimeStamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetTimeStamps()
        {
            foreach (var entry in ChangeTracker.Entries<IBaseEntity>())
            {
                var now = DateTime.UtcNow;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.CurrentValues["CreatedAtUtc"] = now;
                        entry.Property("CreatedAtUtc").IsModified = true;
                        break;
                }
            }
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
    }
}

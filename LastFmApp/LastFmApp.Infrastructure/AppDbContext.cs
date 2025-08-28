using LastFmApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace LastFmApp.Infrastructure
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Artist> Artists => Set<Artist>();
        public DbSet<Album> Albums => Set<Album>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<ArtistTag> ArtistTags => Set<ArtistTag>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Artist>(e =>
            {
                e.HasIndex(x => x.Name);
                e.Property(x => x.Name).IsRequired().HasMaxLength(300);
                e.Property(x => x.Mbid).HasMaxLength(64);
                e.Property(x => x.Url).HasMaxLength(1024);
            });

            b.Entity<Album>(e =>
            {
                e.HasIndex(x => new { x.ArtistId, x.Title }).IsUnique();
                e.Property(x => x.Title).IsRequired().HasMaxLength(300);
                e.Property(x => x.Mbid).HasMaxLength(64);
                e.Property(x => x.Url).HasMaxLength(1024);
                e.Property(x => x.ImageUrl).HasMaxLength(1024);
            });

            b.Entity<Tag>(e =>
            {
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            });

            b.Entity<ArtistTag>().HasKey(x => new { x.ArtistId, x.TagId });
            b.Entity<ArtistTag>()
                .HasOne(at => at.Artist).WithMany(a => a.ArtistTags).HasForeignKey(at => at.ArtistId);
            b.Entity<ArtistTag>()
                .HasOne(at => at.Tag).WithMany(t => t.ArtistTags).HasForeignKey(at => at.TagId);
        }
    }
}


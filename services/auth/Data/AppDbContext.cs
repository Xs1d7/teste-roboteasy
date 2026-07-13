using Auth.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.HasKey(x => x.Id);
        user.HasIndex(x => x.Username).IsUnique();
        user.Property(x => x.Username).HasMaxLength(64).IsRequired();
        user.Property(x => x.AvatarKey).HasMaxLength(255).IsRequired(false);
        user.Property(x => x.PasswordHash).IsRequired();
    }
}

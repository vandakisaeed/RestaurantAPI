
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();

    // for seeding on Migration - more limited in use. 
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //   base.OnModelCreating(modelBuilder);

    //   var userAliceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    //   var userBobId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    //   var post1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    //   var post2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

    //   modelBuilder.Entity<User>().HasData(
    //      new User { Id = userAliceId, Name = "Alice", Email = "alice@mail", CreatedAt = new DateTimeOffset(2025, 11, 3, 12, 10, 30, new TimeSpan(1, 0, 0)) },
    //      new User { Id = userBobId, Name = "Bob", Email = "bob@mail", CreatedAt = new DateTimeOffset(2025, 11, 3, 12, 10, 30, new TimeSpan(1, 0, 0)) }
    //  );

    //   modelBuilder.Entity<Post>().HasData(
    //       new Post { Id = post1Id, UserId = userAliceId, Title = "Hello World", Content = "First post", PublishedAt = new DateTimeOffset(2025, 11, 3, 12, 10, 30, new TimeSpan(1, 0, 0)) },
    //       new Post { Id = post2Id, UserId = userBobId, Title = "Second", Content = "Another post", PublishedAt = null }
    //   );
    // }

    // Only for demonstration - not recommended unless needed and we won't need it
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //   // // Optional: apply configurations from this assembly if you add IEntityTypeConfiguration<T> types
    //   // modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationContext).Assembly);

    //   // // Example explicit configuration (only needed if conventions are not enough):
    //   // modelBuilder.Entity<Post>()
    //   //     .HasOne(p => p.User)
    //   //     .WithMany(u => u.Posts)
    //   //     .HasForeignKey(p => p.UserId)
    //   //     .OnDelete(DeleteBehavior.Cascade);
    // }
}
// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using InterviewHub.Domain;

namespace InterviewHub.Data;

public class AppDbContext : DbContext
{
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);

            // приватний бекінг-філд _questions — EF Core вміє з ним працювати напряму
            entity.Metadata.FindNavigation(nameof(Category.Questions))!
                .SetPropertyAccessMode(Microsoft.EntityFrameworkCore.PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Text).IsRequired();
            entity.Property(q => q.Answer).IsRequired();
            entity.Property(q => q.Difficulty).HasConversion<string>(); // зберігати enum як текст, не int

            entity.HasOne(q => q.Category)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u=> u.Email).IsUnique();
            entity.Property(u=> u.Email).IsRequired();
            entity.Property(u=> u.PasswordHash).IsRequired();
            entity.Property(u=> u.Role).HasConversion<string>();
        });
    }
}
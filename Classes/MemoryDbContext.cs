using Microsoft.EntityFrameworkCore;

//public class MemoryDbContext : DbContext
//{
//    public DbSet<Memory> Memories { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//    {
//        optionsBuilder.UseSqlite("Data Source=memory.db");
//    }
//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<Memory>().HasData(
//            new Memory { Id = 1, Key = "favorite_color", Value = "blue" }
//        );
//    }

//}


// Memory database context and model
//public class MemoryDbContext : DbContext
//{
//    public DbSet<mamaliMemory> Memories { get; set; } = null!;

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//    {
//        // Use an in-memory database for now
//        optionsBuilder.UseInMemoryDatabase("MamaliMemoryDb");
//    }
//}
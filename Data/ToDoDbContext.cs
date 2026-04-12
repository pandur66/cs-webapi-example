using Microsoft.EntityFrameworkCore;
using webapi_demo.Models;
namespace webapi_demo.Data;

public class ToDoDbContext : DbContext
{
    // Constructor for dependency injection
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options) { }

    // DbSet properties for each entity - These properties represent the tables in the database
    public DbSet<ToDoItem> ToDoItems { get; set; }

    // Optional: Configure model relationships or rules
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ToDoItem>().HasKey(t => t.Id); // Example: Defining a primary key
    }
}

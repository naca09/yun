using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteProduct> NoteProducts { get; set; }

        public DbSet<Category> Category { get; set; }

        public DbSet<Customer> Customers { get; set; }
            
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mô tả mối quan hệ một-nhiều giữa Note và NoteProduct
            modelBuilder.Entity<Note>()
                .HasMany(n => n.NoteProducts)
                .WithOne(np => np.Note)
                .HasForeignKey(np => np.NoteId);

            // Mô tả mối quan hệ một-nhiều giữa NoteProduct và Product
            modelBuilder.Entity<NoteProduct>()
                .HasOne(np => np.Product)
                .WithMany(p => p.NoteProducts)
                .HasForeignKey(np => np.ProductID);
        }
    }
    }


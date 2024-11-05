using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Entity
{
    public class Model1 : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<BarcodeScan> BarcodeScans { get; set; }
        public DbSet<Product> Products { get; set; }
        public Model1()
            : base("name=Model1")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Mối quan hệ giữa BarcodeScan và Account
            modelBuilder.Entity<BarcodeScan>()
                .HasRequired(b => b.Account)
                .WithMany(a => a.BarcodeScans)
                .HasForeignKey(b => b.AccountId);

            // Mối quan hệ giữa BarcodeScan và Product
            modelBuilder.Entity<BarcodeScan>()
                .HasRequired(b => b.Product)
                .WithMany(p => p.BarcodeScans)
                .HasForeignKey(b => b.ProductId);
        }
    }

    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<BarcodeScan> BarcodeScans { get; set; }
    }
    // Product.cs
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public DateTime CreatedDate { get; set; }

        // Liên kết với BarcodeScan (Một sản phẩm có thể có nhiều BarcodeScan)
        public virtual ICollection<BarcodeScan> BarcodeScans { get; set; }
    }


    // BarcodeScan.cs
    public class BarcodeScan
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public DateTime ScanTime { get; set; }

        // Liên kết với Account (Ai đã quét mã)
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }

        // Liên kết với Product (Mã barcode của sản phẩm nào)
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }


}
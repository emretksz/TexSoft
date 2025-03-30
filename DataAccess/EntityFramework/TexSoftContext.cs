using DataAccess.Repositories;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EntityFramework
{
    public class TexSoftContext : DbContext
    {
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Shippings> Shippings { get; set; }
        public virtual DbSet<Stock> Stocks { get; set; }
        public virtual DbSet<Temp> Temps { get; set; }
        public virtual DbSet<Tenant> Tenants { get; set; }
        public virtual DbSet<AppUser> AppUsers { get; set; }   
        public virtual DbSet<AppRole> AppRole { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<OrderDate> OrderDates { get; set; }
        public virtual DbSet<MagazaStock> MagazaStocks { get; set; }
        public virtual DbSet<Tekliler> Teklilers { get; set; }
        public virtual DbSet<ShippingDetails> ShippingDetails { get; set; }
        public virtual DbSet<ProductAges> ProductAges { get; set; }
        public virtual DbSet<AllStockList> AllStockLists { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
       optionsBuilder.UseSqlServer(@"");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

           // modelBuilder.Entity<Temp>().HasOne(x => x.Shippings).WithOne()))).HasForeignKey<Role>(x => x.RoleId);
            //modelBuilder.Entity<Temp>().HasMany(q => q.Shippings).WithOne(a => a.Temp);
            modelBuilder.Entity<Product>().HasMany(q => q.Colors).WithMany(a => a.Products);
            modelBuilder.Entity<Tenant>().HasMany(q => q.Shippings).WithOne(a => a.Tenant);
           // modelBuilder.Entity<Tenant>().HasMany(q => q.AppUsers).WithOne(a => a.Tenant);
            //modelBuilder.Entity<FavoriProduct>().HasMany(x => x.User).WithMany(a => a.FavoriProduct);
            //modelBuilder.Entity<FavoriProduct>().HasMany(x => x.Product).WithMany(a => a.FavoriProduct);
            //modelBuilder.Entity<Orders>().HasMany(x => x.User).WithMany(a => a.Order);
            //modelBuilder.Entity<Orders>().HasMany(x => x.Product).WithMany(a => a.Order);
          
        }
    }
    }

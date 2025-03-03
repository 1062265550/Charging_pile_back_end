using Microsoft.EntityFrameworkCore;
using ChargingPile.API.Models.Entities;

namespace ChargingPile.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChargingStation> ChargingStations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置充电站的空间索引
            modelBuilder.Entity<ChargingStation>()
                .HasIndex(e => new { e.Latitude, e.Longitude })
                .HasDatabaseName("idx_location");

            modelBuilder.Entity<ChargingStation>()
                .Property(e => e.Location)
                .HasColumnType("point")
                .IsRequired();

            // 配置用户OpenId唯一索引
            modelBuilder.Entity<User>()
                .HasIndex(e => e.OpenId)
                .IsUnique();

            // 配置订单的用户ID和状态联合索引
            modelBuilder.Entity<Order>()
                .HasIndex(e => new { e.UserId, e.Status });
        }
    }
} 
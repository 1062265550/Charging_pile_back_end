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
        public DbSet<Models.Entities.ChargingPile> ChargingPiles { get; set; }
        public DbSet<ChargingPort> ChargingPorts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置实体与表名的映射
            modelBuilder.Entity<ChargingPort>().ToTable("charging_ports");
            modelBuilder.Entity<Models.Entities.ChargingPile>().ToTable("charging_piles");

            // 配置充电站的空间索引
            modelBuilder.Entity<ChargingStation>()
                .HasIndex(e => new { e.Latitude, e.Longitude })
                .HasDatabaseName("idx_location");

            modelBuilder.Entity<ChargingStation>()
                .Property(e => e.Location)
                .HasColumnType("point")
                .IsRequired();

            // 配置订单和充电口的关系
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ChargingPort)
                .WithMany()
                .HasForeignKey(o => o.PortId)
                .OnDelete(DeleteBehavior.Restrict);

            // 配置用户OpenId唯一索引
            modelBuilder.Entity<User>()
                .HasIndex(e => e.OpenId)
                .IsUnique();

            // 配置订单号唯一索引
            modelBuilder.Entity<Order>()
                .HasIndex(e => e.OrderNo)
                .IsUnique()
                .HasDatabaseName("uk_order_no");

            // 配置订单的用户ID和状态联合索引
            modelBuilder.Entity<Order>()
                .HasIndex(e => new { e.UserId, e.Status });

            // 配置充电桩编号唯一索引
            modelBuilder.Entity<Models.Entities.ChargingPile>()
                .HasIndex(e => e.PileNo)
                .IsUnique()
                .HasDatabaseName("uk_pile_no");

            // 配置充电口编号唯一索引
            modelBuilder.Entity<ChargingPort>()
                .HasIndex(e => e.PortNo)
                .IsUnique()
                .HasDatabaseName("uk_port_no");

            // 配置充电口与订单的关系
            modelBuilder.Entity<ChargingPort>()
                .HasOne(p => p.CurrentOrder)
                .WithMany()
                .HasForeignKey(p => p.CurrentOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChargingPort>()
                .HasOne(p => p.LastOrder)
                .WithMany()
                .HasForeignKey(p => p.LastOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 
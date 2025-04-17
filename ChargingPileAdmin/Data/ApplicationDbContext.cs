using ChargingPileAdmin.Models;
using Microsoft.EntityFrameworkCore;

namespace ChargingPileAdmin.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 充电站
        public DbSet<ChargingStation> ChargingStations { get; set; }
        
        // 充电桩
        public DbSet<ChargingPile> ChargingPiles { get; set; }
        
        // 充电端口
        public DbSet<ChargingPort> ChargingPorts { get; set; }
        
        // 用户
        public DbSet<User> Users { get; set; }
        
        // 订单
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置表名
            modelBuilder.Entity<ChargingStation>().ToTable("charging_stations");
            modelBuilder.Entity<ChargingPile>().ToTable("charging_piles");
            modelBuilder.Entity<ChargingPort>().ToTable("charging_ports");
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Order>().ToTable("orders");

            // 配置关系
            modelBuilder.Entity<ChargingPile>()
                .HasOne(p => p.ChargingStation)
                .WithMany(s => s.ChargingPiles)
                .HasForeignKey(p => p.StationId);

            modelBuilder.Entity<ChargingPort>()
                .HasOne(p => p.ChargingPile)
                .WithMany(p => p.ChargingPorts)
                .HasForeignKey(p => p.PileId);

            // 配置所有decimal类型的列，避免数据截断警告
            // ChargingPile 实体的decimal属性
            modelBuilder.Entity<ChargingPile>()
                .Property(p => p.PowerRate)
                .HasColumnType("decimal(10, 2)");
            
            modelBuilder.Entity<ChargingPile>()
                .Property(p => p.FloatingPower)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<ChargingPile>()
                .Property(p => p.Temperature)
                .HasColumnType("decimal(5, 1)");

            // ChargingPort 实体的decimal属性
            modelBuilder.Entity<ChargingPort>()
                .Property(p => p.Power)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<ChargingPort>()
                .Property(p => p.Voltage)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<ChargingPort>()
                .Property(p => p.CurrentAmpere)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<ChargingPort>()
                .Property(p => p.Electricity)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<ChargingPort>()
                .Property(p => p.Temperature)
                .HasColumnType("decimal(5, 1)");
                
            modelBuilder.Entity<ChargingPort>()
                .Property(p => p.TotalPowerConsumption)
                .HasColumnType("decimal(12, 2)");

            // Order 实体的decimal属性
            modelBuilder.Entity<Order>()
                .Property(p => p.Power)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.PowerConsumption)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.ServiceFee)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.TotalAmount)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.PeakElectricity)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.PeakAmount)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.ValleyElectricity)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.ValleyAmount)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.FlatElectricity)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.FlatAmount)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.SharpElectricity)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.SharpAmount)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.DeepValleyElectricity)
                .HasColumnType("decimal(10, 2)");
                
            modelBuilder.Entity<Order>()
                .Property(p => p.DeepValleyAmount)
                .HasColumnType("decimal(10, 2)");
            
            // User 实体的decimal属性
            modelBuilder.Entity<User>()
                .Property(p => p.Balance)
                .HasColumnType("decimal(10, 2)");

            base.OnModelCreating(modelBuilder);
        }
    }
}

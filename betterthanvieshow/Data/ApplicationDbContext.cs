using betterthanvieshow.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Data;

/// <summary>
/// 應用程式資料庫上下文
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 使用者資料集
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 影廳資料集
    /// </summary>
    public DbSet<Theater> Theaters { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 實體配置
        modelBuilder.Entity<User>(entity =>
        {
            // 主鍵
            entity.HasKey(e => e.Id);

            // Email 唯一索引
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_User_Email");

            // 角色預設值
            entity.Property(e => e.Role)
                .HasDefaultValue("Customer");

            // 建立時間預設值
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // 角色檢查約束
            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_User_Role",
                "[Role] IN ('Customer', 'Admin')"
            ));
        });

        // Theater 實體配置
        modelBuilder.Entity<Theater>(entity =>
        {
            // 主鍵
            entity.HasKey(e => e.Id);

            // 檢查約束：排數必須大於 0
            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Theater_RowCount",
                "[RowCount] > 0"
            ));

            // 檢查約束：列數必須大於 0
            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Theater_ColumnCount",
                "[ColumnCount] > 0"
            ));

            // 檢查約束：座位總數必須大於 0
            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Theater_TotalSeats",
                "[TotalSeats] > 0"
            ));
        });

    }
}

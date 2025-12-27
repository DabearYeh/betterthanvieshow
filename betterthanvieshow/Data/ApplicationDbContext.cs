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

    /// <summary>
    /// 座位資料集
    /// </summary>
    public DbSet<Seat> Seats { get; set; }

    /// <summary>
    /// 電影資料集
    /// </summary>
    public DbSet<Movie> Movies { get; set; }

    /// <summary>
    /// 場次資料集
    /// </summary>
    public DbSet<MovieShowTime> MovieShowTimes { get; set; }

    /// <summary>
    /// 每日時刻表資料集
    /// </summary>
    public DbSet<DailySchedule> DailySchedules { get; set; }

    /// <summary>
    /// 票券資料集
    /// </summary>
    public DbSet<Ticket> Tickets { get; set; }


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

        // Seat 實體配置
        modelBuilder.Entity<Seat>(entity =>
        {
            // 主鍵
            entity.HasKey(e => e.Id);

            // 外鍵設定
            entity.HasOne(e => e.Theater)
                .WithMany(t => t.Seats)
                .HasForeignKey(e => e.TheaterId)
                .OnDelete(DeleteBehavior.Cascade);

            // 唯一約束：同一影廳內 (RowName, ColumnNumber) 必須唯一
            entity.HasIndex(e => new { e.TheaterId, e.RowName, e.ColumnNumber })
                .IsUnique()
                .HasDatabaseName("IX_Seat_Theater_Row_Column");
        });

        // Movie 實體配置
        modelBuilder.Entity<Movie>(entity =>
        {
            // 主鍵
            entity.HasKey(e => e.Id);

            // 建立時間預設值
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // 檢查約束：時長必須大於 0
            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Movie_Duration",
                "[Duration] > 0"
            ));

            // 檢查約束：下映日期必須大於等於上映日期
            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Movie_EndDate",
                "[EndDate] >= [ReleaseDate]"
            ));
        });

        // MovieShowTime 實體配置
        modelBuilder.Entity<MovieShowTime>(entity =>
        {
            // 主鍵
            entity.HasKey(e => e.Id);

            // 外鍵設定 - 電影
            entity.HasOne(e => e.Movie)
                .WithMany()
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            // 外鍵設定 - 影廳
            entity.HasOne(e => e.Theater)
                .WithMany()
                .HasForeignKey(e => e.TheaterId)
                .OnDelete(DeleteBehavior.Restrict);

            // 建立時間預設值
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // 索引：影廳 + 日期（用於查詢某日某廳的所有場次）
            entity.HasIndex(e => new { e.TheaterId, e.ShowDate })
                .HasDatabaseName("IX_MovieShowTime_Theater_Date");
        });

        // DailySchedule 實體配置
        modelBuilder.Entity<DailySchedule>(entity =>
        {
            // 主鍵
            entity.HasKey(e => e.Id);

            // 日期唯一索引
            entity.HasIndex(e => e.ScheduleDate)
                .IsUnique()
                .HasDatabaseName("IX_DailySchedule_Date");

            // 狀態預設值
            entity.Property(e => e.Status)
                .HasDefaultValue("Draft");

            // 建立時間預設值
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // 更新時間預設值
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            // 狀態檢查約束
            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_DailySchedule_Status",
                "[Status] IN ('Draft', 'OnSale')"
            ));
        });

        // Ticket 實體配置
        modelBuilder.Entity<Ticket>(entity =>
        {
            // 主鍵
            entity.HasKey(e => e.Id);

            // 票券編號唯一索引
            entity.HasIndex(e => e.TicketNumber)
                .IsUnique()
                .HasDatabaseName("IX_Ticket_TicketNumber");

            // 外鍵設定 - 場次
            entity.HasOne(e => e.ShowTime)
                .WithMany()
                .HasForeignKey(e => e.ShowTimeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 外鍵設定 - 座位
            entity.HasOne(e => e.Seat)
                .WithMany()
                .HasForeignKey(e => e.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // 狀態預設值
            entity.Property(e => e.Status)
                .HasDefaultValue("待支付");

            // 狀態檢查約束
            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Ticket_Status",
                "[Status] IN ('待支付', '未使用', '已使用', '已過期')"
            ));

            // 唯一約束：同一場次同一座位只能有一張有效票券
            // 注意：這個約束可能需要在應用層實作，因為「有效」的定義包含多個狀態
            entity.HasIndex(e => new { e.ShowTimeId, e.SeatId })
                .HasDatabaseName("IX_Ticket_ShowTime_Seat");
        });

    }
}

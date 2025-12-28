using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace betterthanvieshow.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderEntityAndForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ShowTimeId = table.Column<int>(type: "int", nullable: false),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "未付款"),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TicketCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.CheckConstraint("CHK_Order_Status", "[Status] IN ('未付款', '已付款', '已取消')");
                    table.CheckConstraint("CHK_Order_TicketCount", "[TicketCount] >= 1 AND [TicketCount] <= 6");
                    table.CheckConstraint("CHK_Order_TotalPrice", "[TotalPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_Order_MovieShowTime_ShowTimeId",
                        column: x => x.ShowTimeId,
                        principalTable: "MovieShowTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 確保索引不存在（如果之前執行失敗可能已部分創建）
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ticket_OrderId' AND object_id = OBJECT_ID('Ticket'))
                BEGIN
                    DROP INDEX IX_Ticket_OrderId ON Ticket;
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_OrderId",
                table: "Ticket",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrderNumber",
                table: "Order",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_ShowTimeId",
                table: "Order",
                column: "ShowTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_UserId",
                table: "Order",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Order_OrderId",
                table: "Ticket",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Order_OrderId",
                table: "Ticket");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_OrderId",
                table: "Ticket");
        }
    }
}

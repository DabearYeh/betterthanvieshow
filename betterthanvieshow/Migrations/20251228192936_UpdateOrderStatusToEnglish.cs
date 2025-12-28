using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace betterthanvieshow.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderStatusToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Order_Status",
                table: "Order");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Order",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "未付款");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Order_Status",
                table: "Order",
                sql: "[Status] IN ('Pending', 'Paid', 'Cancelled')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Order_Status",
                table: "Order");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Order",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "未付款",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Order_Status",
                table: "Order",
                sql: "[Status] IN ('未付款', '已付款', '已取消')");
        }
    }
}

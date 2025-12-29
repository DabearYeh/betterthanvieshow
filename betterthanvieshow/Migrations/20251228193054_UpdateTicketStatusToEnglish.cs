using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace betterthanvieshow.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketStatusToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Ticket_Status",
                table: "Ticket");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Ticket",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "待支付");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Ticket_Status",
                table: "Ticket",
                sql: "[Status] IN ('Pending', 'Unused', 'Used', 'Expired')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Ticket_Status",
                table: "Ticket");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Ticket",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "待支付",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Ticket_Status",
                table: "Ticket",
                sql: "[Status] IN ('待支付', '未使用', '已使用', '已過期')");
        }
    }
}

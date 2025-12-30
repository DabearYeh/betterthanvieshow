using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace betterthanvieshow.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketValidateLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketValidateLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    ValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ValidatedBy = table.Column<int>(type: "int", nullable: false),
                    ValidationResult = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketValidateLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketValidateLog_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketValidateLog_User_ValidatedBy",
                        column: x => x.ValidatedBy,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidateLog_TicketId",
                table: "TicketValidateLog",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketValidateLog_ValidatedBy",
                table: "TicketValidateLog",
                column: "ValidatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketValidateLog");
        }
    }
}

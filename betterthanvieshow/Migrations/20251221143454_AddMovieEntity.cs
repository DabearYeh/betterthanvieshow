using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace betterthanvieshow.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Director = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cast = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PosterUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TrailerUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CanCarousel = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movie", x => x.Id);
                    table.CheckConstraint("CHK_Movie_Duration", "[Duration] > 0");
                    table.CheckConstraint("CHK_Movie_EndDate", "[EndDate] >= [ReleaseDate]");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Movie");
        }
    }
}

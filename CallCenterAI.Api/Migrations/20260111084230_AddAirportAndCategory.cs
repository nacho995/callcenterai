using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallCenterAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAirportAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AirportId",
                table: "Calls",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Calls",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calls_AirportId",
                table: "Calls",
                column: "AirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_CategoryId",
                table: "Calls",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_Airports_AirportId",
                table: "Calls",
                column: "AirportId",
                principalTable: "Airports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_Categories_CategoryId",
                table: "Calls",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calls_Airports_AirportId",
                table: "Calls");

            migrationBuilder.DropForeignKey(
                name: "FK_Calls_Categories_CategoryId",
                table: "Calls");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Calls_AirportId",
                table: "Calls");

            migrationBuilder.DropIndex(
                name: "IX_Calls_CategoryId",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "AirportId",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Calls");
        }
    }
}

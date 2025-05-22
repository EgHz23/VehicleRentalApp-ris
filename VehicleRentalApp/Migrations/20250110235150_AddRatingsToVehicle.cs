using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRentalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingsToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleId1",
                table: "Ratings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_VehicleId1",
                table: "Ratings",
                column: "VehicleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Vehicles_VehicleId1",
                table: "Ratings",
                column: "VehicleId1",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Vehicles_VehicleId1",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_VehicleId1",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "VehicleId1",
                table: "Ratings");
        }
    }
}

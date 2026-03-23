using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StuffTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeIdToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HomeId",
                table: "Locations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Locations_HomeId",
                table: "Locations",
                column: "HomeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Locations_HomeId",
                table: "Locations",
                column: "HomeId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Locations_HomeId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_HomeId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "HomeId",
                table: "Locations");
        }
    }
}

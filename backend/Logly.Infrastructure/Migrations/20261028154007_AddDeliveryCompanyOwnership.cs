using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryCompanyOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeliveryCompanies_Name",
                table: "DeliveryCompanies");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "DeliveryCompanies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_DeliveryCompanies_Owner_Name",
                table: "DeliveryCompanies",
                columns: new[] { "OwnerId", "Name" },
                unique: true,
                filter: "\"OwnerId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_DeliveryCompanies_Shared_Name",
                table: "DeliveryCompanies",
                column: "Name",
                unique: true,
                filter: "\"OwnerId\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryCompanies_Users_OwnerId",
                table: "DeliveryCompanies",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryCompanies_Users_OwnerId",
                table: "DeliveryCompanies");

            migrationBuilder.DropIndex(
                name: "UX_DeliveryCompanies_Owner_Name",
                table: "DeliveryCompanies");

            migrationBuilder.DropIndex(
                name: "UX_DeliveryCompanies_Shared_Name",
                table: "DeliveryCompanies");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "DeliveryCompanies");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryCompanies_Name",
                table: "DeliveryCompanies",
                column: "Name",
                unique: true);
        }
    }
}

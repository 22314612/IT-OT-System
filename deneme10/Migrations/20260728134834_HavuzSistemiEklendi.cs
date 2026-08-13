using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deneme10.Migrations
{
    /// <inheritdoc />
    public partial class HavuzSistemiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                table: "Records",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserName",
                table: "Records",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "Records",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "AssignedToUserName",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "Records");
        }
    }
}

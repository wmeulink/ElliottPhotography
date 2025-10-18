using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElliottPhotography.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThumbnailImage",
                table: "Landscapes",
                newName: "ThumbnailData");

            migrationBuilder.RenameColumn(
                name: "FullImage",
                table: "Landscapes",
                newName: "ImageData");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Photos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "Photos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "Photos");

            migrationBuilder.RenameColumn(
                name: "ThumbnailData",
                table: "Landscapes",
                newName: "ThumbnailImage");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "Landscapes",
                newName: "FullImage");
        }
    }
}

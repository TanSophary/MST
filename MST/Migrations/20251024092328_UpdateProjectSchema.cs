using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MST.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Projects",
                newName: "ThumbnailPath");

            migrationBuilder.AddColumn<string>(
                name: "GalleryPaths",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GalleryPaths",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "ThumbnailPath",
                table: "Projects",
                newName: "ImagePath");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MST.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMultipleImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePaths",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "Thumbnail",
                table: "Projects",
                newName: "ImagePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Projects",
                newName: "Thumbnail");

            migrationBuilder.AddColumn<string>(
                name: "ImagePaths",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

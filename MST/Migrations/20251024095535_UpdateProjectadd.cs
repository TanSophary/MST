using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MST.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThumbnailPath",
                table: "Projects",
                newName: "Thumbnail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Thumbnail",
                table: "Projects",
                newName: "ThumbnailPath");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MST.Migrations
{
    /// <inheritdoc />
    public partial class addImagelisttoProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageListJson",
                table: "Projects",
                newName: "ImageList");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageList",
                table: "Projects",
                newName: "ImageListJson");
        }
    }
}

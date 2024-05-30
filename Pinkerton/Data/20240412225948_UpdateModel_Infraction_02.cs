using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pinkerton.Data
{
    /// <inheritdoc />
    public partial class UpdateModel_Infraction_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServerId",
                table: "Infraction",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "Infraction");
        }
    }
}

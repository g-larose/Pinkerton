using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pinkerton.Data
{
    /// <inheritdoc />
    public partial class UpdateModel_ServerMember_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long[]>(
                name: "Roles",
                table: "Members",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "text[]",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string[]>(
                name: "Roles",
                table: "Members",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(long[]),
                oldType: "bigint[]",
                oldNullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pinkerton.Data
{
    /// <inheritdoc />
    public partial class UpdateModel_ServerMember_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infraction_Members_CreatedById",
                table: "Infraction");

            migrationBuilder.DropIndex(
                name: "IX_Infraction_CreatedById",
                table: "Infraction");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Infraction");

            migrationBuilder.AddColumn<string>(
                name: "MemberId",
                table: "Members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Roles",
                table: "Members",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServerMemberId",
                table: "Infraction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Infraction_ServerMemberId",
                table: "Infraction",
                column: "ServerMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infraction_Members_ServerMemberId",
                table: "Infraction",
                column: "ServerMemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infraction_Members_ServerMemberId",
                table: "Infraction");

            migrationBuilder.DropIndex(
                name: "IX_Infraction_ServerMemberId",
                table: "Infraction");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ServerMemberId",
                table: "Infraction");

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Infraction",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infraction_CreatedById",
                table: "Infraction",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Infraction_Members_CreatedById",
                table: "Infraction",
                column: "CreatedById",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}

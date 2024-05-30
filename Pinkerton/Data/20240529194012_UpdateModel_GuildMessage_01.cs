using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pinkerton.Data
{
    /// <inheritdoc />
    public partial class UpdateModel_GuildMessage_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChannelId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string[]>(
                name: "Roles",
                table: "Members",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(long[]),
                oldType: "bigint[]",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "Messages");

            migrationBuilder.AlterColumn<long[]>(
                name: "Roles",
                table: "Members",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "text[]",
                oldNullable: true);
        }
    }
}

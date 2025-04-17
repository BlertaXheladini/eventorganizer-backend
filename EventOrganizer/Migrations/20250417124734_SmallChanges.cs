using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventOrganizer.Migrations
{
    /// <inheritdoc />
    public partial class SmallChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Roles_RolesId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_RolesId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RolesId",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RolesId",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RolesId",
                table: "User",
                column: "RolesId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Roles_RolesId",
                table: "User",
                column: "RolesId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizingEvents.Migrations
{
    /// <inheritdoc />
    public partial class StaffChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FistName",
                table: "Staff",
                newName: "FirstName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Staff",
                newName: "FistName");
        }
    }
}

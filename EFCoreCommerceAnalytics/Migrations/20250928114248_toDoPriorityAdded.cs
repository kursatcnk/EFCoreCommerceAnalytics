using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreCommerceAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class toDoPriorityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ToDos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ToDos");
        }
    }
}

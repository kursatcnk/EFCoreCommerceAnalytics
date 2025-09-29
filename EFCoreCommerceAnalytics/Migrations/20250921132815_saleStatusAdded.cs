using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreCommerceAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class saleStatusAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SaleStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaleStatus",
                table: "Orders");
        }
    }
}

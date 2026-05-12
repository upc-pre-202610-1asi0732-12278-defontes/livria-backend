using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivriaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingPrice",
                table: "orders",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingPrice",
                table: "orders");
        }
    }
}

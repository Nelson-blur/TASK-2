using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenFieldWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixingDiscounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Discounts_DiscountsId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Discounts_DiscountsId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Products_DiscountsId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DiscountsId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountsId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountsId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UsedDiscount",
                table: "Orders");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountApplied",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountApplied",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "DiscountsId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountsId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UsedDiscount",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    DiscountsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscountCode = table.Column<int>(type: "int", nullable: false),
                    DiscountName = table.Column<int>(type: "int", nullable: false),
                    DiscountPercentage = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.DiscountsId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_DiscountsId",
                table: "Products",
                column: "DiscountsId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DiscountsId",
                table: "Orders",
                column: "DiscountsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Discounts_DiscountsId",
                table: "Orders",
                column: "DiscountsId",
                principalTable: "Discounts",
                principalColumn: "DiscountsId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Discounts_DiscountsId",
                table: "Products",
                column: "DiscountsId",
                principalTable: "Discounts",
                principalColumn: "DiscountsId");
        }
    }
}

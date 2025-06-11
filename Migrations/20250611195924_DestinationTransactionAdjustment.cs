using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletAPI.Migrations
{
    /// <inheritdoc />
    public partial class DestinationTransactionAdjustment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DestinationWalletId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DestinationWalletId",
                table: "Transactions",
                column: "DestinationWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_DestinationWalletId",
                table: "Transactions",
                column: "DestinationWalletId",
                principalTable: "Wallets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_DestinationWalletId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_DestinationWalletId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DestinationWalletId",
                table: "Transactions");

            migrationBuilder.UpdateData(
                table: "Transactions",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}

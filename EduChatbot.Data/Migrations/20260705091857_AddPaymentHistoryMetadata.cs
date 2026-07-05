using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduChatbot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentHistoryMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "provider_transaction_code",
                table: "payment_transactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_reason",
                table: "payment_transactions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_created_at",
                table: "payment_transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_paid_at",
                table: "payment_transactions",
                column: "paid_at");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_status",
                table: "payment_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_user_id_created_at",
                table: "payment_transactions",
                columns: new[] { "user_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payment_transactions_created_at",
                table: "payment_transactions");

            migrationBuilder.DropIndex(
                name: "IX_payment_transactions_paid_at",
                table: "payment_transactions");

            migrationBuilder.DropIndex(
                name: "IX_payment_transactions_status",
                table: "payment_transactions");

            migrationBuilder.DropIndex(
                name: "IX_payment_transactions_user_id_created_at",
                table: "payment_transactions");

            migrationBuilder.DropColumn(
                name: "provider_transaction_code",
                table: "payment_transactions");

            migrationBuilder.DropColumn(
                name: "status_reason",
                table: "payment_transactions");
        }
    }
}

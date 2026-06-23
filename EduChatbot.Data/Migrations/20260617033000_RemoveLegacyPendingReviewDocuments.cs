using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduChatbot.Data.Migrations
{
    [Migration("20260617033000_RemoveLegacyPendingReviewDocuments")]
    public partial class RemoveLegacyPendingReviewDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE documents
                SET status = 'Approved',
                    review_note = NULL,
                    reviewed_at = NULL,
                    reviewed_by_id = NULL
                WHERE status = 'PendingReview';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}

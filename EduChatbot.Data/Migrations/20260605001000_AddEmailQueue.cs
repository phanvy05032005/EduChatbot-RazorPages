using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduChatbot.Data.Migrations
{
    /// <summary>
    /// Thêm bảng email_queue để lưu email queue. (Generated to match EF conventions)
    /// </summary>
    [Migration("20260605001000_AddEmailQueueV2")]
    public partial class AddEmailQueueV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: email_queue is already created by 20260605000000_AddEmailQueue.
            // Keep this migration so existing migration ordering remains stable.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: the previous AddEmailQueue migration owns the email_queue table.
        }
    }
}

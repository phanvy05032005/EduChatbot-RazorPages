using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduChatbot.Data.Migrations
{
    [Migration("20260617030000_AddUniqueLecturerCourseAssignments")]
    public partial class AddUniqueLecturerCourseAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lecturer_courses_course_id",
                table: "lecturer_courses");

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_courses_course_id",
                table: "lecturer_courses",
                column: "course_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_courses_lecturer_id",
                table: "lecturer_courses",
                column: "lecturer_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lecturer_courses_lecturer_id",
                table: "lecturer_courses");

            migrationBuilder.DropIndex(
                name: "IX_lecturer_courses_course_id",
                table: "lecturer_courses");

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_courses_course_id",
                table: "lecturer_courses",
                column: "course_id");
        }
    }
}

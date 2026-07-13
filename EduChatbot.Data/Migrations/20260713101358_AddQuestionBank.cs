using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EduChatbot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "source_question_bank_item_id",
                table: "quiz_questions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "question_bank_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    document_id = table.Column<int>(type: "integer", nullable: true),
                    source_chunk_id = table.Column<int>(type: "integer", nullable: true),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    question_text_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    question_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by_lecturer_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tags = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_bank_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_question_bank_items_AspNetUsers_created_by_lecturer_id",
                        column: x => x.created_by_lecturer_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_question_bank_items_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_question_bank_items_document_chunks_source_chunk_id",
                        column: x => x.source_chunk_id,
                        principalTable: "document_chunks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_question_bank_items_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "question_bank_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_bank_item_id = table.Column<int>(type: "integer", nullable: false),
                    option_order = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    option_text = table.Column<string>(type: "text", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_bank_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_question_bank_options_question_bank_items_question_bank_ite~",
                        column: x => x.question_bank_item_id,
                        principalTable: "question_bank_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_source_question_bank_item_id",
                table: "quiz_questions",
                column: "source_question_bank_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_items_course_id",
                table: "question_bank_items",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_items_created_by_lecturer_id",
                table: "question_bank_items",
                column: "created_by_lecturer_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_items_document_id",
                table: "question_bank_items",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_items_source_chunk_id",
                table: "question_bank_items",
                column: "source_chunk_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_options_question_bank_item_id",
                table: "question_bank_options",
                column: "question_bank_item_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quiz_questions_question_bank_items_source_question_bank_ite~",
                table: "quiz_questions",
                column: "source_question_bank_item_id",
                principalTable: "question_bank_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quiz_questions_question_bank_items_source_question_bank_ite~",
                table: "quiz_questions");

            migrationBuilder.DropTable(
                name: "question_bank_options");

            migrationBuilder.DropTable(
                name: "question_bank_items");

            migrationBuilder.DropIndex(
                name: "IX_quiz_questions_source_question_bank_item_id",
                table: "quiz_questions");

            migrationBuilder.DropColumn(
                name: "source_question_bank_item_id",
                table: "quiz_questions");
        }
    }
}

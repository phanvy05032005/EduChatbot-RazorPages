using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace EduChatbot.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<LecturerCourse> LecturerCourses => Set<LecturerCourse>();

    public DbSet<EmailQueue> EmailQueues => Set<EmailQueue>();

    public DbSet<Quiz> Quizzes => Set<Quiz>();

    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();

    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();

    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();

    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(document => document.Id);

            // Dùng tên bảng/cột lowercase để thao tác PostgreSQL trong DBeaver dễ hơn.
            entity.Property(document => document.Id).HasColumnName("id");
            entity.Property(document => document.FileName).HasColumnName("file_name").IsRequired().HasMaxLength(255);
            entity.Property(document => document.StoredFileName).HasColumnName("stored_file_name").IsRequired().HasMaxLength(255);
            entity.Property(document => document.FilePath).HasColumnName("file_path").IsRequired().HasMaxLength(500);
            entity.Property(document => document.UploadedBy).HasColumnName("uploaded_by").IsRequired().HasMaxLength(100);
            entity.Property(document => document.UploadedById).HasColumnName("uploaded_by_id").HasMaxLength(450);
            entity.Property(document => document.ContentType).HasColumnName("content_type").IsRequired().HasMaxLength(100);
            entity.Property(document => document.FileSize).HasColumnName("file_size");
            entity.Property(document => document.ExtractedText).HasColumnName("extracted_text");
            entity.Property(document => document.ChunkCount).HasColumnName("chunk_count");
            entity.Property(document => document.EmbeddingPreview).HasColumnName("embedding_preview").HasMaxLength(500);
            entity.Property(document => document.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(document => document.UploadedAt)
                .HasColumnName("uploaded_at")
                .HasColumnType("timestamp with time zone");
            entity.Property(document => document.CourseId).HasColumnName("course_id");
            entity.Property(document => document.SubjectCode).HasColumnName("subject_code").HasMaxLength(50);
            entity.Property(document => document.SubjectName).HasColumnName("subject_name").HasMaxLength(255);
            entity.Property(document => document.MatchScore).HasColumnName("match_score");
            entity.Property(document => document.ValidationResult).HasColumnName("validation_result");
            entity.Property(document => document.ReviewedById).HasColumnName("reviewed_by_id").HasMaxLength(450);
            entity.Property(document => document.ReviewedAt)
                .HasColumnName("reviewed_at")
                .HasColumnType("timestamp with time zone");
            entity.Property(document => document.ReviewNote).HasColumnName("review_note");

            entity.HasMany(document => document.Chunks)
                .WithOne(chunk => chunk.Document)
                .HasForeignKey(chunk => chunk.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(document => document.Course)
                .WithMany(course => course.Documents)
                .HasForeignKey(document => document.CourseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.ToTable("document_chunks");
            entity.HasKey(chunk => chunk.Id);

            // Bảng chunk lưu text nhỏ và vector embedding thật cho workflow RAG.
            entity.Property(chunk => chunk.Id).HasColumnName("id");
            entity.Property(chunk => chunk.DocumentId).HasColumnName("document_id");
            entity.Property(chunk => chunk.ChunkIndex).HasColumnName("chunk_index");
            entity.Property(chunk => chunk.Content).HasColumnName("content").IsRequired();
            entity.Property(chunk => chunk.Embedding).HasColumnName("embedding").HasColumnType("vector(1536)");
            entity.Property(chunk => chunk.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(chunk => new { chunk.DocumentId, chunk.ChunkIndex })
                .IsUnique();
        });

        modelBuilder.Entity<ChatConversation>(entity =>
        {
            entity.ToTable("chat_conversations");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(450);
            entity.Property(c => c.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");
            entity.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");
            entity.Property(c => c.CourseId).HasColumnName("course_id");

            entity.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(c => c.UserId);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id).HasColumnName("id");
            entity.Property(m => m.ConversationId).HasColumnName("conversation_id");
            entity.Property(m => m.Role).HasColumnName("role").IsRequired().HasMaxLength(10);
            entity.Property(m => m.Content).HasColumnName("content").IsRequired();
            entity.Property(m => m.SourceChunks).HasColumnName("source_chunks");
            entity.Property(m => m.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(m => m.ConversationId);
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName).HasMaxLength(100);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            entity.Property(c => c.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(c => c.Description).HasColumnName("description");
            entity.HasIndex(c => c.Code).IsUnique();
        });

        modelBuilder.Entity<LecturerCourse>(entity =>
        {
            entity.ToTable("lecturer_courses");
            entity.HasKey(lc => new { lc.LecturerId, lc.CourseId });
            entity.Property(lc => lc.LecturerId).HasColumnName("lecturer_id");
            entity.Property(lc => lc.CourseId).HasColumnName("course_id");
            entity.HasIndex(lc => lc.LecturerId).IsUnique();
            entity.HasIndex(lc => lc.CourseId).IsUnique();

            entity.HasOne(lc => lc.Lecturer)
                .WithMany()
                .HasForeignKey(lc => lc.LecturerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lc => lc.Course)
                .WithMany(c => c.LecturerCourses)
                .HasForeignKey(lc => lc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EmailQueue>(entity =>
        {
            entity.ToTable("email_queue");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ToEmail).HasColumnName("to_email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Subject).HasColumnName("subject").IsRequired().HasMaxLength(500);
            entity.Property(e => e.Body).HasColumnName("body").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.SentAt)
                .HasColumnName("sent_at")
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.ToTable("quizzes");
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Id).HasColumnName("id");
            entity.Property(q => q.CourseId).HasColumnName("course_id");
            entity.Property(q => q.DocumentId).HasColumnName("document_id");
            entity.Property(q => q.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(q => q.Difficulty).HasColumnName("difficulty").IsRequired().HasMaxLength(50);
            entity.Property(q => q.AdditionalInstruction).HasColumnName("additional_instruction");
            entity.Property(q => q.NumberOfQuestions).HasColumnName("number_of_questions");
            entity.Property(q => q.TimeLimitMinutes).HasColumnName("time_limit_minutes");
            entity.Property(q => q.MaxAttempts).HasColumnName("max_attempts");
            entity.Property(q => q.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(q => q.CreatedByLecturerId).HasColumnName("created_by_lecturer_id").IsRequired().HasMaxLength(450);
            entity.Property(q => q.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(q => q.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(q => q.PublishedAt).HasColumnName("published_at").HasColumnType("timestamp with time zone");

            entity.HasOne(q => q.Course)
                .WithMany()
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(q => q.Document)
                .WithMany()
                .HasForeignKey(q => q.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(q => q.CreatedByLecturer)
                .WithMany()
                .HasForeignKey(q => q.CreatedByLecturerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.ToTable("quiz_questions");
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Id).HasColumnName("id");
            entity.Property(q => q.QuizId).HasColumnName("quiz_id");
            entity.Property(q => q.QuestionOrder).HasColumnName("question_order");
            entity.Property(q => q.QuestionText).HasColumnName("question_text").IsRequired();
            entity.Property(q => q.Explanation).HasColumnName("explanation");
            entity.Property(q => q.SourceChunkId).HasColumnName("source_chunk_id");

            entity.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(q => q.SourceChunk)
                .WithMany()
                .HasForeignKey(q => q.SourceChunkId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<QuizOption>(entity =>
        {
            entity.ToTable("quiz_options");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("id");
            entity.Property(o => o.QuizQuestionId).HasColumnName("quiz_question_id");
            entity.Property(o => o.OptionOrder).HasColumnName("option_order");
            entity.Property(o => o.Label).HasColumnName("label").IsRequired().HasMaxLength(10);
            entity.Property(o => o.OptionText).HasColumnName("option_text").IsRequired();
            entity.Property(o => o.IsCorrect).HasColumnName("is_correct");

            entity.HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.ToTable("quiz_attempts");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.QuizId).HasColumnName("quiz_id");
            entity.Property(a => a.StudentId).HasColumnName("student_id").IsRequired().HasMaxLength(450);
            entity.Property(a => a.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(a => a.StartedAt).HasColumnName("started_at").HasColumnType("timestamp with time zone");
            entity.Property(a => a.SubmittedAt).HasColumnName("submitted_at").HasColumnType("timestamp with time zone");
            entity.Property(a => a.TotalQuestions).HasColumnName("total_questions");
            entity.Property(a => a.CorrectCount).HasColumnName("correct_count");
            entity.Property(a => a.Score).HasColumnName("score");

            entity.HasOne(a => a.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttemptAnswer>(entity =>
        {
            entity.ToTable("quiz_attempt_answers");
            entity.HasKey(aa => aa.Id);
            entity.Property(aa => aa.Id).HasColumnName("id");
            entity.Property(aa => aa.QuizAttemptId).HasColumnName("quiz_attempt_id");
            entity.Property(aa => aa.QuizQuestionId).HasColumnName("quiz_question_id");
            entity.Property(aa => aa.SelectedOptionId).HasColumnName("selected_option_id");
            entity.Property(aa => aa.IsCorrect).HasColumnName("is_correct");

            entity.HasOne(aa => aa.Attempt)
                .WithMany(a => a.Answers)
                .HasForeignKey(aa => aa.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(aa => aa.Question)
                .WithMany()
                .HasForeignKey(aa => aa.QuizQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(aa => aa.SelectedOption)
                .WithMany()
                .HasForeignKey(aa => aa.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

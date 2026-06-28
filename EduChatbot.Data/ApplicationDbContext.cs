using EduChatbot.Models;
using EduChatbot.Models.Enums;
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

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

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
            entity.Property(user => user.SubscriptionType)
                .HasColumnName("subscription_type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(SubscriptionType.BASIC);

            entity.Property(user => user.AccountTitle)
                .HasColumnName("account_title")
                .HasMaxLength(50)
                .HasDefaultValue("Basic");

            entity.Property(user => user.TokenLimit)
                .HasColumnName("token_limit")
                .HasDefaultValue(5000);

            entity.Property(user => user.UsedTokens)
                .HasColumnName("used_tokens")
                .HasDefaultValue(0);

            entity.Ignore(user => user.IsQuizUnlocked);
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("payment_transactions");
            entity.HasKey(pt => pt.Id);

            entity.Property(pt => pt.Id).HasColumnName("id");
            entity.Property(pt => pt.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(450);
            entity.Property(pt => pt.OrderCode).HasColumnName("order_code");
            entity.Property(pt => pt.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            entity.Property(pt => pt.Currency).HasColumnName("currency").HasMaxLength(10).HasDefaultValue("VND");
            entity.Property(pt => pt.Provider)
                .HasColumnName("provider")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(PaymentProvider.PAYOS);
            entity.Property(pt => pt.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(PaymentStatus.PENDING);
            entity.Property(pt => pt.CheckoutUrl).HasColumnName("checkout_url").HasMaxLength(1000);
            entity.Property(pt => pt.PayOSPaymentLinkId).HasColumnName("payos_payment_link_id").HasMaxLength(255);
            entity.Property(pt => pt.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone");
            entity.Property(pt => pt.PaidAt)
                .HasColumnName("paid_at")
                .HasColumnType("timestamp with time zone");
            entity.Property(pt => pt.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");
            entity.Property(pt => pt.SubscriptionId)
                .HasColumnName("subscription_id");

            entity.HasIndex(pt => pt.OrderCode).IsUnique();
            entity.HasIndex(pt => pt.UserId);
            entity.HasIndex(pt => pt.SubscriptionId);

            entity.HasOne(pt => pt.User)
                .WithMany()
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pt => pt.Subscription)
                .WithMany()
                .HasForeignKey(pt => pt.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("subscription_plans");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
            entity.Property(p => p.DurationDays).HasColumnName("duration_days");
            entity.Property(p => p.RequestLimit).HasColumnName("request_limit");
            entity.Property(p => p.RefreshIntervalMinutes).HasColumnName("refresh_interval_minutes");
            entity.Property(p => p.AllowChat).HasColumnName("allow_chat");
            entity.Property(p => p.AllowQuiz).HasColumnName("allow_quiz");
            entity.Property(p => p.TokenLimit).HasColumnName("token_limit");
            entity.Property(p => p.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id).HasColumnName("id");
            entity.Property(s => s.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(450);
            entity.Property(s => s.SubscriptionPlanId).HasColumnName("subscription_plan_id");
            entity.Property(s => s.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(SubscriptionStatus.PENDING);
            entity.Property(s => s.StartDate).HasColumnName("start_date").HasColumnType("timestamp with time zone");
            entity.Property(s => s.EndDate).HasColumnName("end_date").HasColumnType("timestamp with time zone");
            entity.Property(s => s.RemainingRequests).HasColumnName("remaining_requests");
            entity.Property(s => s.RequestWindowStart).HasColumnName("request_window_start").HasColumnType("timestamp with time zone");
            entity.Property(s => s.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

            entity.HasIndex(s => new { s.UserId, s.Status });
            entity.HasIndex(s => new { s.UserId, s.SubscriptionPlanId, s.Status });

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Plan)
                .WithMany()
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
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

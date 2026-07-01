/* ========================================================
   EduChatbot — i18n (English ↔ Vietnamese)
   Default language: English (all HTML text is in English).
   Vietnamese translations are applied via JS when user toggles.
   ======================================================== */

(function () {
    'use strict';

    /* ---------- Vietnamese translations ---------- */
    const VI = {
        /* ---- Shared / Layout ---- */
        'nav.chat': 'Chat',
        'nav.profile': 'Hồ sơ',
        'nav.logout': 'Đăng xuất',
        'nav.login': 'Đăng nhập',
        'nav.register': 'Đăng ký',
        'nav.dashboard': 'Bảng điều khiển',
        'nav.documents': 'Tài liệu',
        'nav.upload': 'Tải lên',
        'nav.learningMaterials': 'Tài liệu học tập',
        'nav.uploadDocument': 'Tải lên tài liệu',
        'nav.myCourses': 'Môn học của tôi',
        'nav.allCourses': 'Tất cả môn học',
        'student.sidebar.quizzes': 'Bài trắc nghiệm',
        'student.sidebar.subscription': 'Gói dịch vụ',
        'nav.quizzesManagement': 'Quản lý trắc nghiệm',
        'nav.quizzes': 'Trắc nghiệm',

        /* ---- Quizzes Shared ---- */
        'quizzes.title': 'Đề trắc nghiệm AI',
        'quizzes.status.draft': 'Bản nháp',
        'quizzes.status.published': 'Đã xuất bản',
        'quizzes.status.archived': 'Đã lưu trữ',
        'quizzes.course': 'Môn học',
        'quizzes.documentSource': 'Tài liệu nguồn',
        'quizzes.questions': 'Số câu hỏi',
        'quizzes.timeLimit': 'Thời gian',
        'quizzes.maxAttempts': 'Số lượt thi tối đa',
        'quizzes.status': 'Trạng thái',
        'quizzes.createdAt': 'Ngày tạo',
        'quizzes.actions': 'Thao tác',
        'quizzes.difficulty': 'Độ khó',
        'quizzes.minutes': 'phút',

        /* ---- Lecturer Quizzes ---- */
        'quizzes.lecturer.kicker': 'Đánh giá học tập',
        'quizzes.lecturer.desc': 'Tạo bài trắc nghiệm bằng AI từ tài liệu môn học, kiểm duyệt câu hỏi và xem lịch sử thi.',
        'quizzes.lecturer.totalQuizzes': 'Tổng số đề thi',
        'quizzes.lecturer.draftQuizzes': 'Đề thi nháp',
        'quizzes.lecturer.publishedQuizzes': 'Đề thi đã xuất bản',
        'quizzes.lecturer.archivedQuizzes': 'Đề thi đã lưu trữ',

        /* ---- Lecturer Dashboard ---- */
        'docs.dashboard.kicker': 'Không gian giảng viên',
        'docs.dashboard.desc': 'Quản lý các môn học được phân công và tài liệu học tập.',
        'docs.dashboard.assignedCourses': 'Môn học được phân công',
        'docs.dashboard.totalDocs': 'Tổng tài liệu',
        'docs.dashboard.indexedDocs': 'Tài liệu đã lập chỉ mục',
        'docs.dashboard.docStats': 'Thống kê tài liệu',
        'docs.dashboard.total': 'Tổng số',
        'docs.dashboard.indexed': 'Đã lập chỉ mục',
        'docs.dashboard.rejected': 'Yêu cầu xử lý / Lỗi',
        'docs.dashboard.totalChunks': 'Tổng số phân đoạn',
        'docs.dashboard.quickActions': 'Thao tác nhanh',
        'docs.dashboard.reviewIndexed': 'Duyệt tài liệu đã lập chỉ mục',
        'docs.dashboard.uploadNew': 'Tải lên tài liệu mới',
        'docs.dashboard.noAssignedCourses': 'Chưa có môn học được phân công.',
        'docs.dashboard.noAssignedCoursesDesc': 'Vui lòng liên hệ quản trị viên để được phân công môn học.',

        /* ---- Lecturer Courses ---- */
        'docs.courses.kicker': 'Môn học',
        'docs.courses.title': 'Môn học & Phân công',
        'docs.courses.desc': 'Xem danh sách môn học được phân công, các tài liệu đã tải lên và phân công giảng dạy trong hệ thống.',
        'docs.courses.totalCourses': 'Tổng số môn học',
        'docs.courses.assignedCourses': 'Môn học đảm nhiệm',
        'docs.courses.documentsUploaded': 'Tài liệu đã tải lên',
        'docs.courses.totalQuizzes': 'Tổng số đề thi',
        'docs.courses.adminOverview': 'Tổng quan quản lý',
        'docs.courses.you': '(Bạn)',
        'docs.courses.noCourses': 'Không tìm thấy môn học nào trong hệ thống.',

        /* ---- Lecturer Documents ---- */
        'docs.index.title': 'Tài liệu học tập',
        'docs.index.desc': 'Tìm kiếm, kiểm tra trạng thái lập chỉ mục và quản lý các tệp tài liệu học tập.',
        'docs.index.searchPlaceholder': 'Tìm kiếm tên tài liệu, giảng viên, trạng thái...',
        'docs.index.reset': 'Thiết lập lại',
        'docs.index.allDocs': '← Tất cả tài liệu',
        'docs.index.backDashboard': '← Quay lại Bảng điều khiển',
        'docs.index.totalDocuments': 'Tổng số tài liệu',
        'docs.index.approvedDocuments': 'Tài liệu đã duyệt',
        'docs.index.processingDocuments': 'Tài liệu đang xử lý',
        'docs.index.failedDocuments': 'Tài liệu lỗi/từ chối',
        'docs.index.noDocuments': 'Chưa có tài liệu học tập nào.',
        'docs.index.uploadHint': 'Tải lên tệp PDF hoặc DOCX để phân tách và lập chỉ mục cho Chatbot học tập.',

        'quizzes.lecturer.generateBtn': 'Tạo đề thi bằng AI',
        'quizzes.lecturer.reviewEdit': 'Chỉnh sửa & Kiểm duyệt',
        'quizzes.lecturer.viewDetails': 'Xem chi tiết',
        'quizzes.lecturer.attempts': 'Lượt thi của sinh viên',
        'quizzes.lecturer.noQuizzes': 'Chưa có bài thi nào được tạo.',
        'quizzes.lecturer.noQuizzesDesc': 'Hãy sinh đề thi trắc nghiệm bằng AI từ một tài liệu môn học đã được phê duyệt.',
        'quizzes.lecturer.showing': 'Đang hiển thị',
        'quizzes.lecturer.quizzesCount': 'bài thi trắc nghiệm',

        /* ---- Lecturer Generate ---- */
        'quizzes.lecturer.gen.title': 'Tạo đề thi trắc nghiệm bằng AI',
        'quizzes.lecturer.gen.desc': 'Chọn môn học và tài liệu học tập đã duyệt để tự động sinh đề thi trắc nghiệm bản nháp.',
        'quizzes.lecturer.gen.course': 'Môn học',
        'quizzes.lecturer.gen.selectCourse': '-- Chọn môn học --',
        'quizzes.lecturer.gen.document': 'Tài liệu nguồn (Đã duyệt)',
        'quizzes.lecturer.gen.selectDoc': '-- Chọn tài liệu --',
        'quizzes.lecturer.gen.quizTitle': 'Tiêu đề đề thi',
        'quizzes.lecturer.gen.difficulty': 'Độ khó',
        'quizzes.lecturer.gen.easy': 'Dễ',
        'quizzes.lecturer.gen.medium': 'Trung bình',
        'quizzes.lecturer.gen.hard': 'Khó',
        'quizzes.lecturer.gen.numQuestions': 'Số lượng câu hỏi',
        'quizzes.lecturer.gen.timeLimit': 'Giới hạn thời gian (Phút)',
        'quizzes.lecturer.gen.maxAttempts': 'Số lượt thi tối đa',
        'quizzes.lecturer.gen.additionalInst': 'Hướng dẫn bổ sung cho AI (Tùy chọn)',
        'quizzes.lecturer.gen.btn': 'Tạo đề thi',
        'quizzes.lecturer.gen.cancel': 'Hủy bỏ',
        'quizzes.lecturer.gen.loadingTitle': 'Hệ thống AI đang phân tích tài liệu...',
        'quizzes.lecturer.gen.loadingDesc': 'Đang sinh danh sách câu hỏi và các phương án trả lời. Việc này có thể mất tới một phút.',

        /* ---- Lecturer Review ---- */
        'quizzes.lecturer.rev.title': 'Kiểm duyệt đề thi',
        'quizzes.lecturer.rev.questionsList': 'Danh sách câu hỏi',
        'quizzes.lecturer.rev.noQuestions': 'Đề thi chưa có câu hỏi nào.',
        'quizzes.lecturer.rev.explanation': 'Giải thích',
        'quizzes.lecturer.rev.publishConfirm': 'Bạn có chắc chắn muốn xuất bản đề thi này? Sau khi xuất bản, câu hỏi và đáp án sẽ bị khóa và không thể chỉnh sửa.',
        'quizzes.lecturer.rev.publishBtn': 'Xuất bản đề thi',
        'quizzes.lecturer.rev.archiveConfirm': 'Bạn có chắc chắn muốn lưu trữ đề thi này? Sinh viên sẽ không thể bắt đầu lượt thi mới, nhưng lịch sử và kết quả thi cũ vẫn được giữ nguyên.',
        'quizzes.lecturer.rev.archiveBtn': 'Lưu trữ đề thi',
        'quizzes.lecturer.rev.viewAttempts': 'Xem lượt thi',
        'quizzes.lecturer.rev.addQuestion': 'Thêm câu hỏi thủ công',
        'quizzes.lecturer.rev.generateMore': 'Sinh thêm bằng AI',
        'quizzes.lecturer.rev.genMoreTitle': 'Sinh thêm câu hỏi bằng AI',
        'quizzes.lecturer.rev.genMoreCount': 'Số lượng câu sinh thêm',
        'quizzes.lecturer.rev.genMoreInstructions': 'Hướng dẫn bổ sung cho AI (Tùy chọn)',
        'quizzes.lecturer.rev.genMoreBtn': 'Sinh thêm',
        'quizzes.lecturer.rev.addModalTitle': 'Thêm câu hỏi thủ công',
        'quizzes.lecturer.rev.editModalTitle': 'Chỉnh sửa câu hỏi',
        'quizzes.lecturer.rev.questionText': 'Nội dung câu hỏi',
        'quizzes.lecturer.rev.optionsLabel': 'Các phương án trả lời (Chọn đúng 1 đáp án đúng)',
        'quizzes.lecturer.rev.correctLabel': 'Đúng',
        'quizzes.lecturer.rev.closeBtn': 'Đóng',
        'quizzes.lecturer.rev.saveChanges': 'Lưu thay đổi',
        'quizzes.lecturer.rev.deleteConfirm': 'Bạn có chắc chắn muốn xóa câu hỏi này?',

        /* ---- Lecturer Attempts ---- */
        'quizzes.lecturer.att.title': 'Báo cáo lượt làm bài',
        'quizzes.lecturer.att.totalCount': 'Tổng số lượt làm bài',
        'quizzes.lecturer.att.noAttempts': 'Chưa có sinh viên nào thực hiện bài thi này.',
        'quizzes.lecturer.att.student': 'Sinh viên',
        'quizzes.lecturer.att.startedAt': 'Bắt đầu lúc',
        'quizzes.lecturer.att.submittedAt': 'Nộp bài lúc',
        'quizzes.lecturer.att.correctCount': 'Số câu đúng',
        'quizzes.lecturer.att.score': 'Điểm số',

        /* ---- Student Quizzes ---- */
        'quizzes.student.libraryTitle': 'Thư viện đề thi',
        'quizzes.student.libraryDesc': 'Kiểm tra kiến thức của bạn với các bài trắc nghiệm được sinh tự động từ tài liệu học tập.',
        'quizzes.student.myHistory': 'Lịch sử của tôi',
        'quizzes.student.noQuizzes': 'Không có bài thi trắc nghiệm nào',
        'quizzes.student.noQuizzesDesc': 'Hiện tại chưa có đề thi trắc nghiệm nào được xuất bản cho các môn học của bạn.',
        'quizzes.student.continueInProgress': 'Tiếp tục làm bài (Đang thực hiện)',
        'quizzes.student.startQuiz': 'Bắt đầu làm bài',
        'quizzes.student.limitReached': 'Đã hết lượt thi',
        'quizzes.student.premiumRequired': 'Yêu cầu Premium',
        'quizzes.student.alertTitle': 'Yêu cầu gói Premium',
        'quizzes.student.alertText': 'Tính năng làm bài trắc nghiệm yêu cầu tài khoản của bạn phải ở gói Premium. Vui lòng nâng cấp gói để tiếp tục.',
        'quizzes.student.alertConfirm': 'Xem gói dịch vụ',
        'quizzes.student.alertCancel': 'Đóng',

        /* ---- Student Take ---- */
        'quizzes.student.take.desc': 'Vui lòng hoàn thành tất cả câu hỏi trước khi nộp bài.',
        'quizzes.student.take.remaining': 'Thời gian còn lại',
        'quizzes.student.take.autoSubmitWarning': 'Bài thi sẽ tự động nộp khi đồng hồ đếm ngược về không.',
        'quizzes.student.take.submitBtn': 'Nộp bài thi',
        'quizzes.student.take.confirmSubmit': 'Bạn có chắc chắn muốn nộp bài thi này?',

        /* ---- Student Result ---- */
        'quizzes.student.res.title': 'Kết quả thi',
        'quizzes.student.res.desc': 'Chi tiết kết quả bài làm trắc nghiệm của bạn.',
        'quizzes.student.res.passed': 'ĐẠT',
        'quizzes.student.res.failed': 'CHƯA ĐẠT',
        'quizzes.student.res.answeredCorrectly': 'Bạn đã trả lời đúng {0} trên tổng số {1} câu hỏi.',
        'quizzes.student.res.completedAt': 'Hoàn thành lúc',
        'quizzes.student.res.reviewTitle': 'Đánh giá câu hỏi',
        'quizzes.student.res.correctOption': 'Đáp án đúng',
        'quizzes.student.res.yourAnswer': 'Bài làm của bạn',
        'quizzes.student.res.correctBadge': 'Đúng',
        'quizzes.student.res.incorrectBadge': 'Sai',
        'student.result.answered1': 'Bạn đã trả lời đúng',
        'student.result.answered2': 'trên tổng số',
        'student.result.answered3': 'câu hỏi.',
        'student.result.completedAt': 'Hoàn thành lúc:',
        'student.result.questionsReview': 'Xem lại câu hỏi',

        /* ---- Student History ---- */
        'quizzes.student.his.title': 'Lịch sử làm bài trắc nghiệm',
        'quizzes.student.his.desc': 'Theo dõi và xem lại tất cả các lượt làm bài thi trắc nghiệm trước đây của bạn.',
        'quizzes.student.his.noAttempts': 'Không có lịch sử làm bài',
        'quizzes.student.his.noAttemptsDesc': 'Bạn chưa thực hiện bất kỳ bài trắc nghiệm nào.',
        'quizzes.student.his.inProgress': 'Đang làm',
        'quizzes.student.his.inprogress': 'Đang làm',
        'quizzes.student.his.submitted': 'Đã nộp',
        'quizzes.student.his.continue': 'Tiếp tục',
        'quizzes.student.his.viewResult': 'Xem kết quả',

        /* ---- Admin Sidebar ---- */
        'admin.sidebar.dashboard': 'Bảng điều khiển',
        'admin.sidebar.users': 'Quản lý người dùng',
        'admin.sidebar.studentAccounts': 'Tài khoản sinh viên',
        'admin.sidebar.lecturerAccounts': 'Tài khoản giảng viên',
        'admin.sidebar.coursesAssignments': 'Môn học & Phân công',
        'admin.sidebar.pendingReview': 'Chờ duyệt',
        'admin.sidebar.roleManagement': 'Quản lý vai trò',
        'admin.sidebar.systemManagement': 'Quản lý hệ thống',
        'admin.sidebar.statistics': 'Thống kê',
        'admin.sidebar.logout': 'Đăng xuất',

        /* ---- Admin Dashboard ---- */
        'admin.dashboard.kicker': 'Tổng quan hệ thống',
        'admin.dashboard.title': 'Tổng quan hệ thống',
        'admin.dashboard.subtitle': 'Theo dõi số liệu và hoạt động trên toàn hệ thống EduChatbot.',
        'admin.dashboard.refresh': 'Làm mới',
        'admin.dashboard.recentActivities': 'Hoạt động gần đây',
        'admin.dashboard.live': 'Trực tiếp',
        'admin.dashboard.justNow': 'Vừa cập nhật · Hệ thống',
        'admin.dashboard.noRecentActivity': 'Không có hoạt động gần đây.',
        'admin.dashboard.totalQuestions': 'Tổng câu hỏi',
        'admin.dashboard.last24h': '24 giờ qua',
        'admin.dashboard.exportReport': 'Xuất báo cáo',
        'admin.dashboard.syncActive': 'Đồng bộ hàng ngày',
        'admin.dashboard.quickActions': 'Thao tác nhanh',
        'admin.dashboard.healthTitle': 'Sức khỏe hệ thống',
        'admin.dashboard.healthSubtitle': 'Băng thông và độ trễ phản hồi thời gian thực trên các phòng ban.',
        'admin.dashboard.systemStatusTitle': 'Trạng thái dịch vụ hệ thống',
        'admin.dashboard.allOperational': 'Tất cả dịch vụ hoạt động tốt',
        'admin.dashboard.serviceName': 'Dịch vụ hệ thống phụ',
        'admin.dashboard.serviceLatency': 'Phản hồi trung bình',
        'admin.dashboard.serviceLoad': 'Tải CPU',
        'admin.dashboard.serviceStatus': 'Trạng thái',
        'admin.dashboard.optimal': 'Tối ưu',
        'admin.dashboard.active': 'Hoạt động',

        /* ---- Admin Accounts ---- */
        'admin.accounts.manage': 'Quản lý',
        'admin.accounts.createAccount': 'Tạo tài khoản',
        'admin.accounts.viewSearchDesc': 'Xem, tìm kiếm, khóa, mở khóa và quản lý tài khoản',
        'admin.accounts.importStudentExcel': 'Nhập tài khoản sinh viên bằng Excel',
        'admin.accounts.importLecturerExcel': 'Nhập tài khoản giảng viên bằng Excel',
        'admin.accounts.uploadImport': 'Tải lên & Nhập',
        'admin.accounts.requireStudentExcel': 'Yêu cầu file Excel (.xlsx) có chứa các cột: Email và FullName. Hệ thống sẽ tự động gửi thông tin tài khoản đến email sinh viên được import.',
        'admin.accounts.requireLecturerExcel': 'Yêu cầu file Excel (.xlsx) có chứa các cột: FullName, Email, CourseCodes.',
        'admin.accounts.exampleCodes': 'Ví dụ CourseCodes: PRN222 hoặc PRN222,PRM392. Email sẽ được đưa vào hàng đợi gửi nền.',
        'admin.accounts.searchPlaceholder': 'Tìm theo ID, họ tên hoặc email',
        'admin.accounts.list': 'Danh sách',
        'admin.accounts.accounts': 'tài khoản',
        'admin.accounts.lecturerId': 'ID Giảng viên',
        'admin.accounts.studentId': 'ID Sinh viên',
        'admin.accounts.fullName': 'Họ tên',
        'admin.accounts.email': 'Email',
        'admin.accounts.department': 'Khoa',
        'admin.accounts.status': 'Trạng thái',
        'admin.accounts.actions': 'Thao tác',
        'admin.accounts.edit': 'Sửa',
        'admin.accounts.lock': 'Khóa',
        'admin.accounts.unlock': 'Mở khóa',
        'admin.accounts.delete': 'Xóa',
        'admin.accounts.noMatching': 'Không tìm thấy tài khoản phù hợp.',
        'admin.accounts.search': 'Tìm kiếm',
        'admin.accounts.confirmDelete': 'Xóa tài khoản này?',
        'admin.accounts.id': 'ID',
        'admin.accounts.emptyStudents': 'Không tìm thấy tài khoản sinh viên nào.',
        'admin.accounts.emptyLecturers': 'Không tìm thấy tài khoản giảng viên nào.',
        'admin.accounts.importStudentsDesc': 'Tải lên tệp Excel (.xlsx) để tạo hàng loạt tài khoản sinh viên.',
        'admin.accounts.importLecturersDesc': 'Tải lên tệp Excel (.xlsx) để tạo hàng loạt tài khoản giảng viên.',
        'admin.accounts.clickUpload': 'Click để chọn file hoặc kéo thả vào đây',
        'admin.accounts.statsTitle': 'Thống kê tài khoản',
        'admin.accounts.statsActive': 'Tổng hoạt động',
        'admin.accounts.statsLocked': 'Tài khoản bị khóa',
        'admin.accounts.statsTotal': 'Tổng tài khoản',
        'admin.accounts.statsUtilization': 'Hiệu suất',
        'admin.accounts.importExcelBtn': 'Import Excel',
        'admin.accounts.policyLock': 'Tự động khóa sau 5 lần nhập sai',
        'admin.accounts.policyMfa': 'Bảo mật MFA tùy chọn cho Sinh viên',
        'admin.accounts.account': 'Tài khoản',
        'admin.accounts.role': 'Vai trò',
        'admin.accounts.securityTitle': 'Bảo mật tài khoản',
        'admin.accounts.securityDesc1': 'Tài khoản trên toàn hệ thống được bảo vệ bởi các quy tắc Identity. Hiện đang quản lý ',
        'admin.accounts.securityDesc2': ' giảng viên và ',
        'admin.accounts.securityDesc3': ' sinh viên một cách bảo mật.',
        'admin.accounts.policyTitle': 'Chính sách bảo mật',
        'admin.accounts.statsStudentLabel': 'Sinh viên',
        'admin.accounts.statsLecturerLabel': 'Giảng viên',
        'admin.accounts.statsDetails': 'Chi tiết',

        /* ---- Admin Account Form ---- */
        'admin.form.edit': 'Chỉnh sửa',
        'admin.form.create': 'Tạo mới',
        'admin.form.account': 'tài khoản',
        'admin.form.maintainDesc': 'Quản lý thông tin danh tính',
        'admin.form.fullName': 'Họ tên',
        'admin.form.password': 'Mật khẩu',
        'admin.form.assignedCourses': 'Môn học phụ trách',
        'admin.form.searchCourses': 'Tìm theo mã hoặc tên môn (ví dụ: PRN222, .NET...)',
        'admin.form.noCourses': 'Chưa có môn học nào được tạo. Hãy tạo môn học trước.',
        'admin.form.holdToSelect': 'Giữ phím Cmd (macOS) / Ctrl (Windows) để chọn nhiều môn.',
        'admin.form.autoSendEmail': 'Hệ thống sẽ tự động gửi thông tin tài khoản đến email người dùng sau khi tạo thành công.',
        'admin.form.save': 'Lưu tài khoản',
        'admin.form.cancel': 'Hủy',

        /* ---- Admin Courses ---- */
        'admin.courses.kicker': 'Môn học',
        'admin.courses.title': 'Quản lý',
        'admin.courses.titleSerif': 'Môn học & Phân công',
        'admin.courses.subtitle': 'Tạo môn học mới và phân công quyền đăng tải tài liệu cho từng giảng viên.',
        'admin.courses.createNew': 'Tạo Môn học mới',
        'admin.courses.courseCode': 'Mã môn học',
        'admin.courses.courseCodePlaceholder': 'Ví dụ: PRN222',
        'admin.courses.courseName': 'Tên môn học',
        'admin.courses.courseNamePlaceholder': 'Ví dụ: C# & .NET Cloud',
        'admin.courses.courseDesc': 'Mô tả môn học',
        'admin.courses.courseDescPlaceholder': 'Ví dụ: ASP.NET Core, Razor Pages, Entity Framework Core, Repository Pattern, Dependency Injection, Authentication, Authorization',
        'admin.courses.createCourse': 'Tạo môn học',
        'admin.courses.createSubtitle': 'Tạo môn học mới và tùy chọn phân công giảng viên sau.',
        'admin.courses.courseCodeHelper': 'Sử dụng mã môn học ngắn gọn và duy nhất.',
        'admin.courses.assignedLecturer': 'Giảng viên phụ trách',
        'admin.courses.assignLater': '-- Gán sau --',
        'admin.courses.assignedLecturerHelper': 'Mỗi môn học chỉ có một giảng viên chính. Một giảng viên có thể dạy nhiều môn học.',
        'admin.courses.importExcel': 'Nhập Môn học bằng Excel',
        'admin.courses.importCourses': 'Nhập môn học',
        'admin.courses.downloadTemplate': 'Tải mẫu Excel',
        'admin.courses.requireExcel': 'Yêu cầu file Excel (.xlsx) chứa các cột: Code (Mã môn học), Name (Tên môn học), và Description (Mô tả).',
        'admin.courses.courseList': 'Danh sách môn học',
        'admin.courses.courses': 'môn học',
        'admin.courses.code': 'Mã Môn',
        'admin.courses.name': 'Tên Môn',
        'admin.courses.assignedLecturers': 'Giảng viên phụ trách',
        'admin.courses.noDesc': 'Chưa có mô tả môn học.',
        'admin.courses.noLecturer': 'Chưa phân công giảng viên',
        'admin.courses.selectLecturer': '-- Chọn giảng viên --',
        'admin.courses.assign': 'Phân công',
        'admin.courses.confirmDelete': 'Xóa môn học này sẽ làm mất liên kết với các tài liệu đã tải lên?',
        'admin.courses.totalSummary': 'Tổng môn học',
        'admin.courses.assignedSummary': 'Môn đã phân công',
        'admin.courses.unassignedSummary': 'Môn chưa phân công',
        'admin.courses.unassignedBadge': 'Chưa phân công',
        'admin.courses.actions': 'Thao tác',

        /* ---- Admin Roles ---- */
        'admin.roles.title': 'Phân',
        'admin.roles.titleSerif': 'quyền',
        'admin.roles.subtitle': 'Các vai trò hệ thống và phạm vi quyền hạn tương ứng. Quản lý mức độ truy cập trên toàn bộ nền tảng để đảm bảo tính toàn vẹn và bảo mật dữ liệu.',
        'admin.roles.student': 'Sinh viên',
        'admin.roles.lecturer': 'Giảng viên',
        'admin.roles.admin': 'Quản trị viên',
        'admin.roles.studentDesc': 'Vai trò người học cơ bản được gán cho mọi sinh viên. Cho phép tương tác cơ bản và truy cập lịch sử chat.',
        'admin.roles.lecturerDesc': 'Vai trò giảng viên với các chức năng quản lý tài liệu, tải tài liệu lên và kiểm thử đánh giá chatbot.',
        'admin.roles.adminDesc': 'Mức truy cập cao nhất. Toàn quyền quản trị tài khoản, phân công giảng dạy, cài đặt hệ thống và cấu trúc chatbot.',
        'admin.roles.permissions': 'Quyền hạn',
        'admin.roles.kicker': 'Cấu hình hệ thống',
        'admin.roles.exportLog': 'Xuất lịch sử',
        'admin.roles.createNew': 'Tạo vai trò mới',
        'admin.roles.activePermissions': 'Quyền hạn hoạt động',
        'admin.roles.readOnly': 'Chỉ đọc',
        'admin.roles.superAdmin': 'Quản trị hệ thống',
        'admin.roles.inheritanceTitle': 'Chính sách thừa kế',
        'admin.roles.inheritanceDesc': 'Các vai trò được thiết kế phân tầng. Vai trò ở tầng cao hơn sẽ tự động kế thừa các quyền tương tác cơ bản từ vai trò thấp hơn. Quản trị viên có thể ghi đè các chỉ định cụ thể qua hệ thống phân quyền.',
        'admin.roles.recentChangesTitle': 'Thay đổi gần đây',
        'admin.roles.noChanges': 'Không có thay đổi vai trò nào gần đây.',
        'admin.roles.securityNoteTitle': 'Chính sách bảo mật',
        'admin.roles.securityNoteDesc': 'Các vai trò hệ thống được áp dụng thông qua Authorization Policies trên toàn bộ các phân vùng Admin, Giảng viên và Sinh viên. Việc thay đổi có thể ảnh hưởng đến khả năng truy cập các trang được bảo vệ.',
        
        /* ---- Dynamic Permissions Mapping ---- */
        'permission.Login': 'Đăng nhập hệ thống',
        'permission.Ask Chatbot': 'Hỏi đáp Chatbot AI',
        'permission.View Own Chat History': 'Xem lịch sử chat cá nhân',
        'permission.Upload Documents': 'Tải tài liệu học tập',
        'permission.Manage Own Documents': 'Quản lý tài liệu đã tải',
        'permission.Run Evaluation': 'Chạy đánh giá kiểm thử',
        'permission.Manage Accounts': 'Quản lý tài khoản',
        'permission.Manage Roles': 'Quản lý phân quyền',
        'permission.Manage System': 'Cài đặt hệ thống',
        'permission.View Statistics': 'Xem thống kê báo cáo',

        /* ---- Admin System ---- */
        'admin.system.title': 'Quản lý',
        'admin.system.titleSerif': 'hệ thống',
        'admin.system.subtitle': 'Trạng thái các dịch vụ và cấu hình hệ thống.',
        'admin.system.refreshStatus': 'Làm mới trạng thái',
        'admin.system.backupSystem': 'Sao lưu hệ thống',
        'admin.system.dbStatus': 'Trạng thái Database',
        'admin.system.appStatus': 'Trạng thái ứng dụng',
        'admin.system.storageUsage': 'Dung lượng lưu trữ',
        'admin.system.sysVersion': 'Phiên bản hệ thống',
        'admin.system.dbDetail': 'Kết nối PostgreSQL',
        'admin.system.appDetail': 'ASP.NET Core Razor Pages',
        'admin.system.storageDetail': 'Tài liệu đã upload',
        'admin.system.versionDetail': 'Assembly hiện tại',
        'admin.system.aiConfig': 'Cấu hình AI',
        'admin.system.runtime': 'Chạy thực tế',
        'admin.system.storage': 'Bộ nhớ lưu trữ',
        'admin.system.model': 'Mô hình AI',
        'admin.system.temp': 'Độ sáng tạo (Temp)',
        'admin.system.maxTokens': 'Token tối đa',
        'admin.system.embedding': 'Embedding / Vector',
        'admin.system.docPipeline': 'Xử lý tài liệu',
        'admin.system.chunkSize': 'Kích thước đoạn (Chunk)',
        'admin.system.overlap': 'Độ trùng lặp (Overlap)',
        'admin.system.formats': 'Định dạng hỗ trợ',
        'admin.system.database': 'Cơ sở dữ liệu',
        'admin.system.connected': 'Đã kết nối',
        'admin.system.disconnected': 'Mất kết nối',
        'admin.system.openrouterConfig': 'Đã cấu hình OpenRouter',
        'admin.system.vectorEnabled': 'Đã bật tìm kiếm Vector',
        'admin.system.chunkSizeDetail': '512 token',
        'admin.system.overlapDetail': '64 token',
        'admin.system.formatsDetail': 'PDF, DOCX',
        'admin.system.kicker': 'Trạng thái hệ thống',

        /* ---- Admin Statistics ---- */
        'admin.stats.title': 'Thống kê',
        'admin.stats.titleSerif': 'hệ thống',
        'admin.stats.subtitle': 'Hoạt động sử dụng và xu hướng dữ liệu tổng quan.',
        'admin.stats.totals': 'Số liệu tổng quan',
        'admin.stats.snapshot': 'Xem nhanh hiện tại',
        'admin.stats.topTopics': 'Chủ đề hàng đầu',
        'admin.stats.relativeActivity': 'Tỷ lệ hoạt động',
        'admin.stats.totalQuestionsAsked': 'Tổng số câu hỏi đã hỏi',
        'admin.stats.totalDocuments': 'Tổng số tài liệu',
        'admin.stats.totalStudents': 'Tổng số sinh viên',
        'admin.stats.totalLecturers': 'Tổng số giảng viên',
        'admin.stats.kicker': 'Phân tích dữ liệu',

        /* ---- Login ---- */
        'login.hero.line1': 'Học tập thông minh',
        'login.hero.line2': 'cùng tài liệu của bạn.',
        'login.hero.desc': 'Hỏi đáp tức thì về tài liệu giảng viên đã upload, kèm trích dẫn nguồn rõ ràng theo từng đoạn.',
        'login.stat.documents': 'Tài liệu',
        'login.stat.qa': 'Hỏi đáp',
        'login.stat.citations': 'Trích nguồn',
        'login.welcome': 'Chào mừng quay lại',

        /* ---- Chat ---- */
        'chat.welcome': 'Hôm nay bạn muốn học gì?',
        'chat.suggestion1': 'ASP.NET Core Razor Pages là gì?',
        'chat.suggestion2': 'Giải thích Entity Framework Core',
        'chat.suggestion3': 'Repository Pattern hoạt động ra sao?',
        'chat.suggestion4': 'So sánh SQL Server và PostgreSQL',
        'chat.inputPlaceholder': 'Hỏi về tài liệu học tập...',
        'chat.aiDisclaimer': 'AI có thể mắc lỗi. Hãy đối chiếu với tài liệu gốc.',
        'chat.conversations': 'Cuộc trò chuyện',
        'chat.heroSubtitle': 'Học tập, khám phá, với tài liệu của bạn.',
        'chat.startNew': 'Bắt đầu Chat mới',
        'chat.selectCourseDesc': 'Chọn môn học để giới hạn phạm vi thảo luận của AI đúng tài liệu học tập bên trong môn đó.',
        'chat.selectCourse': '-- Chọn Môn học --',
        'chat.createChat': 'Tạo Chat',
        'chat.noConversations': 'Bạn chưa có cuộc trò chuyện nào. Hãy tạo cuộc trò chuyện mới để bắt đầu khám phá tài liệu.',
        'chat.noConversationsShort': 'Chưa có cuộc trò chuyện nào.',
        'chat.errorSending': 'Xin lỗi, đã xảy ra lỗi khi gửi tin nhắn. Vui lòng thử lại.',
        'chat.searchingDocs': 'Đang tìm kiếm tài liệu...',

        /* ---- Documents (Lecturer) ---- */
        'docs.dashboard.viewFull': 'Xem danh sách đầy đủ, kiểm tra chunk data và quản lý học liệu đã upload.',
        'docs.dashboard.addNew': 'Thêm tài liệu mới',
        'docs.dashboard.fileFormats': 'PDF, DOCX, tối đa 10 MB',
        'docs.details.course': 'Môn học',
        'docs.details.matchScore': 'Kết quả Match Score:',
        'docs.details.noChunks': 'Document này chưa có chunk data.',
        'docs.index.noDocuments': 'Chưa có tài liệu nào được upload.',
        'docs.index.uploadHint': 'Upload file PDF hoặc DOCX để hệ thống extract, chunk và tạo embedding.',
        'docs.index.course': 'Môn học',
        'docs.upload.course': 'Môn học',
        'docs.upload.selectCourse': '-- Chọn môn học --',
        'docs.upload.fileTooLarge': 'Dung lượng tệp tin không được vượt quá 50 MB.',
        'docs.upload.title': 'Tải lên tài liệu',
        'docs.upload.desc': 'Tải lên tệp PDF hoặc DOCX để trích xuất nội dung, phân tách câu hỏi và nạp vào cơ sở dữ liệu tri thức của chatbot.',
        'docs.upload.selectCourseContext': 'Chọn ngữ cảnh môn học',
        'docs.upload.chooseCourseHelp': 'Vui lòng chọn môn học liên quan...',
        'docs.upload.fileAttachment': 'Đính kèm tệp tin tài liệu',
        'docs.upload.chooseFileHelp': 'Chọn tệp PDF hoặc DOCX',
        'docs.upload.dragDropHelp': 'Kéo thả tệp tin tại đây hoặc nhấp để duyệt.',
        'docs.upload.maxSize': 'Dung lượng tối đa 50MB',
        'docs.upload.startUpload': 'Bắt đầu tải lên',
        'docs.upload.cancel': 'Hủy bỏ',
        'docs.upload.noAssignedCourses': 'Không có môn học được phân công đảm nhiệm để tải lên tài liệu.',

        /* ---- Profile ---- */
        'profile.kicker': 'HỒ SƠ CÁ NHÂN',
        'profile.title': 'Hồ sơ tài khoản',
        'profile.desc': 'Quản lý thông tin tài khoản cá nhân và cấu hình bảo mật mật khẩu của bạn.',
        'profile.back': '← Quay lại',
        'profile.updateSuccess': 'Cập nhật hồ sơ thành công.',
        'profile.personalInfo': 'Thông tin cá nhân',
        'profile.changePassword': 'Đổi mật khẩu',
        'profile.lecturerAccount': 'Tài khoản Giảng viên',
        'profile.studentAccount': 'Tài khoản Sinh viên',
        'profile.adminAccount': 'Tài khoản Quản trị',
        'profile.backDashboard': 'Quay lại bảng điều khiển',
        'profile.backChat': 'Quay lại phòng chat',
        'profile.fullName': 'Họ và tên',
        'profile.fullNamePlaceholder': 'Nhập họ và tên của bạn',
        'profile.email': 'Địa chỉ email đăng nhập',
        'profile.currentPassword': 'Mật khẩu hiện tại',
        'profile.newPassword': 'Mật khẩu mới',
        'profile.confirmPassword': 'Xác nhận mật khẩu mới',
        'profile.saveChanges': 'Lưu thay đổi',
        'profile.updatePassword': 'Cập nhật mật khẩu',
        'profile.updateProfile': 'Lưu hồ sơ',
        'profile.changePasswordBtn': 'Đổi mật khẩu',
        'profile.requirementsTitle': 'Yêu cầu về mật khẩu',
        'profile.reqLength': 'Độ dài tối thiểu 6 ký tự',
        'profile.reqSpecial': 'Chứa ít nhất 1 chữ số và 1 ký tự đặc biệt',
        'profile.reqCase': 'Có cả chữ hoa và chữ thường',

        /* ---- Pending Review ---- */
        'pending.title': 'Chờ duyệt',
        'admin.pending.title': 'Duyệt tài liệu',
        'admin.pending.titleSerif': 'Chờ duyệt',
        'admin.pending.subtitle': 'Duyệt các tài liệu có điểm khớp môn học dưới ngưỡng tự động duyệt.',
        'admin.pending.empty': 'Không có tài liệu nào đang chờ duyệt.',
        'admin.pending.heading': 'Tài liệu đang chờ quyết định',
        'admin.pending.pendingCount': 'đang chờ',
        'admin.pending.fileName': 'Tên file',
        'admin.pending.lecturer': 'Giảng viên',
        'admin.pending.subject': 'Môn học',
        'admin.pending.matchScore': 'Match Score',
        'admin.pending.uploadDate': 'Ngày tải lên',
        'admin.pending.reason': 'Lý do',
        'admin.pending.approve': 'Duyệt',
        'admin.pending.reject': 'Từ chối',

        /* ---- Password Change Modal & Banner ---- */
        'pwd.modal.title': 'Thay đổi mật khẩu của bạn',
        'pwd.modal.desc': 'Tài khoản của bạn đã được quản trị viên tạo. Vui lòng thay đổi mật khẩu của bạn để tiếp tục an toàn.',
        'pwd.modal.currentPassword': 'Mật khẩu hiện tại',
        'pwd.modal.newPassword': 'Mật khẩu mới',
        'pwd.modal.confirmPassword': 'Xác nhận mật khẩu mới',
        'pwd.modal.cancel': 'Hủy',
        'pwd.modal.save': 'Đổi mật khẩu',
        'pwd.modal.errorFillAll': 'Vui lòng điền đầy đủ các thông tin.',
        'pwd.modal.errorMatch': 'Mật khẩu mới và mật khẩu xác nhận không khớp.',
        'pwd.banner.title': 'Yêu cầu đổi mật khẩu',
        'pwd.banner.desc': 'Bạn chưa đổi mật khẩu mặc định. Nhấp vào đây để đổi mật khẩu ngay.',
        'pwd.banner.action': 'Đổi ngay',

        /* ---- Student Sidebar & Topbar ---- */
        'student.sidebar.dashboard': 'Bảng điều khiển',
        'student.sidebar.newchat': 'Chat mới',
        'student.sidebar.chathistory': 'Lịch sử Chat',
        'student.sidebar.courses': 'Môn học',
        'student.sidebar.documents': 'Tài liệu',
        'student.sidebar.subscription': 'Gói dịch vụ',
        'student.sidebar.logout': 'Đăng xuất',
        'student.sidebar.profile': 'Hồ sơ của tôi',
        'student.sidebar.changepassword': 'Đổi mật khẩu',
        'student.topbar.profile': 'Hồ sơ của tôi',
        'student.topbar.changepassword': 'Đổi mật khẩu',
        'student.topbar.startchat': 'Bắt đầu Chat mới',
        'student.topbar.search_placeholder': 'Tìm môn học, tài liệu...',

        /* ---- Subscription ---- */
        'subscription.kicker': 'Gói dịch vụ',
        'subscription.plans.title': 'Gói dịch vụ',
        'subscription.plans.subtitle': 'Chọn gói để mở khóa tính năng học tập và duy trì tiến độ học.',
        'subscription.plans.viewMine': 'Gói của tôi',
        'subscription.price.free': 'Miễn phí',
        'subscription.feature.requests': 'lượt hỏi',
        'subscription.feature.chat': 'Hỗ trợ học tập qua Chat AI',
        'subscription.feature.quizUnlocked': 'Mở khóa Quiz',
        'subscription.feature.quizLocked': 'Quiz bị khóa',
        'subscription.badge.current': 'Gói hiện tại',
        'subscription.badge.premiumCurrent': 'Premium đang hoạt động',
        'subscription.action.upgrade': 'Nâng cấp Premium',
        'subscription.action.backChat': 'Quay lại Chat',
        'subscription.action.backDashboard': 'Quay lại bảng điều khiển',
        'subscription.action.viewMine': 'Xem gói của tôi',
        'subscription.action.retry': 'Thử lại',
        'subscription.me.kicker': 'Gói của tôi',
        'subscription.me.title': 'Gói dịch vụ của bạn',
        'subscription.me.subtitle': 'Xem gói hiện tại, lượt hỏi còn lại và các tính năng học tập đã mở khóa.',
        'subscription.me.currentPlan': 'Gói hiện tại',
        'subscription.me.remaining': 'Lượt hỏi còn lại',
        'subscription.me.used': 'Lượt hỏi đã dùng',
        'subscription.me.details': 'Chi tiết gói dịch vụ',
        'subscription.me.planType': 'Loại gói',
        'subscription.me.planName': 'Tên gói',
        'subscription.me.limit': 'Hạn mức lượt hỏi',
        'subscription.me.nextReset': 'Thời gian reset tiếp theo',
        'subscription.me.quizUnlocked': 'Đã mở khóa',
        'subscription.me.quizLocked': 'Bị khóa - Cần Premium',
        'subscription.callback.successTitle': 'Thanh toán thành công',
        'subscription.callback.failedTitle': 'Thanh toán thất bại',

        /* ---- Student Dashboard ---- */
        'student.dashboard.welcome': 'Chào mừng quay lại',
        'student.dashboard.welcome_subtitle': 'Dưới đây là tổng quan về trạng thái học tập và tài liệu của bạn.',
        'student.dashboard.totalcourses': 'Tổng môn học',
        'student.dashboard.docs': 'Tài liệu hiện có',
        'student.dashboard.chats': 'Phiên trò chuyện',
        'student.dashboard.current_plan': 'Gói hiện tại',
        'student.dashboard.free_plan': 'Gói miễn phí',
        'student.dashboard.upgrade_disabled': 'Chức năng nâng cấp đã tắt',
        'student.dashboard.my_courses': 'Môn học của tôi',
        'student.dashboard.view_all': 'Xem tất cả',
        'student.dashboard.no_courses_title': 'Không có môn học nào',
        'student.dashboard.no_courses_desc': 'Hiện tại chưa có môn học nào được phân công hoặc đăng ký.',
        'student.dashboard.required': 'Bắt buộc',
        'student.dashboard.docs_count': 'tài liệu',
        'student.dashboard.view_course': 'Xem môn học',
        'student.dashboard.recent_docs': 'Tài liệu gần đây',
        'student.dashboard.view_all_library': 'Xem toàn bộ thư viện',
        'student.dashboard.no_docs_title': 'Không tìm thấy tài liệu',
        'student.dashboard.no_docs_desc': 'Chưa có tài liệu học tập nào được phê duyệt để hiển thị.',
        'student.dashboard.table_doc_name': 'Tên tài liệu',
        'student.dashboard.table_course': 'Môn học',
        'student.dashboard.table_action': 'Thao tác',
        'student.dashboard.btn_view': 'Xem',
        'student.dashboard.btn_ask_ai': 'Hỏi AI',
        'student.dashboard.recent_chats': 'Trò chuyện gần đây',
        'student.dashboard.view_history': 'Xem lịch sử',
        'student.dashboard.no_chats_title': 'Chưa có lịch sử trò chuyện',
        'student.dashboard.start_new_chat': 'Bắt đầu Chat mới',
        'student.dashboard.no_messages': 'Chưa có tin nhắn trong cuộc hội thoại này.',
        'student.dashboard.btn_continue': 'Tiếp tục',

        /* ---- Student Chat ---- */
        'student.chat.current_course': 'Môn học hiện tại',
        'student.chat.select_course': '-- Chọn môn học --',
        'student.chat.active_docs': 'Tài liệu hoạt động',
        'student.chat.select_course_hint': 'Chọn môn học để xem tài liệu tương ứng.',
        'student.chat.no_docs_approved': 'Không có tài liệu nào được phê duyệt cho môn học này.',
        'student.chat.new_session': 'Phiên mới',
        'student.chat.welcome_title': 'Xin chào! Tôi là EduBot',
        'student.chat.welcome_desc': 'Tôi có thể trả lời câu hỏi dựa trên tài liệu học tập được phê duyệt. Hãy chọn một môn học và đặt câu hỏi!',
        'student.chat.suggest_summarize': 'Tóm tắt tài liệu này',
        'student.chat.suggest_explain': 'Giải thích chủ đề này',
        'student.chat.suggest_outcomes': 'Danh sách kết quả học tập',
        'student.chat.input_placeholder': 'Hỏi EduBot một câu hỏi về môn học...',
        'student.chat.sources': 'Nguồn trích dẫn:',
        'student.chat.chunk': 'Đoạn',
        'student.chat.thinking': 'EduBot đang suy nghĩ...',

        /* ---- Student Chat History ---- */
        'student.chathistory.title': 'Lịch sử trò chuyện',
        'student.chathistory.subtitle': 'Xem lại các cuộc trò chuyện trước đây của bạn với trợ lý AI.',
        'student.chathistory.search_placeholder': 'Tìm kiếm cuộc trò chuyện...',
        'student.chathistory.no_chats_title': 'Chưa có lịch sử trò chuyện',
        'student.chathistory.no_chats_desc': 'Bạn chưa có phiên trò chuyện nào trước đây. Hãy bắt đầu ngay!',
        'student.chathistory.general': 'Chung',
        'student.chathistory.messages_count': 'tin nhắn',
        'student.chathistory.btn_continue': 'Tiếp tục',
        'student.chathistory.delete': 'Xoá',
        'student.chathistory.deleteConfirm': 'Bạn có chắc muốn xoá đoạn chat này không?',
        'student.chathistory.deleteSuccess': 'Đã xoá đoạn chat.',
        'student.chathistory.deleteFailed': 'Không thể xoá đoạn chat.',
        'student.chathistory.deleteModalTitle': 'Xoá cuộc trò chuyện',
        'student.chathistory.selectAll': 'Chọn tất cả',
        'student.chathistory.selectedCountLabel': 'được chọn',
        'student.chathistory.deleteSelected': 'Xoá mục đã chọn',
        'student.chathistory.bulkDeleteModalTitle': 'Xoá các cuộc trò chuyện',
        'student.chathistory.bulkDeleteConfirm': 'Bạn có chắc muốn xoá tất cả các cuộc trò chuyện đã chọn không?',
        'student.chathistory.deleteMultipleSuccess': 'Đã xoá thành công các cuộc trò chuyện đã chọn.',
        'student.chathistory.noChatsSelected': 'Chưa chọn cuộc trò chuyện nào để xoá.',

        /* ---- Student Courses ---- */
        'student.courses.title': 'Thư viện môn học',
        'student.courses.subtitle': 'Khám phá môn học và sử dụng trợ lý ảo AI để học tập tốt hơn.',
        'student.courses.search_placeholder': 'Tìm môn học theo mã hoặc tên...',
        'student.courses.no_courses_title': 'Không có môn học nào',
        'student.courses.no_courses_desc': 'Hiện chưa có môn học nào được tạo trong hệ thống.',
        'student.courses.core': 'Chính khóa',
        'student.courses.docs_count': 'tài liệu',
        'student.courses.btn_view': 'Xem chi tiết',

        /* ---- Student Course Detail ---- */
        'student.coursedetail.breadcrumb_courses': 'Môn học',
        'student.coursedetail.core_syllabus': 'Đề cương chính',
        'student.coursedetail.btn_start_chat': 'Bắt đầu Chat với Môn học',
        'student.coursedetail.section_title': 'Đề cương & Tài liệu',
        'student.coursedetail.table_file_name': 'Tên tệp',
        'student.coursedetail.table_uploaded_date': 'Ngày tải lên',
        'student.coursedetail.table_size': 'Kích thước',
        'student.coursedetail.table_actions': 'Thao tác',
        'student.coursedetail.course_not_found': 'Không tìm thấy môn học',
        'student.coursedetail.course_not_found_desc': 'Môn học yêu cầu không tồn tại hoặc đã bị xóa.',
        'student.coursedetail.btn_back': 'Quay lại thư viện',

        /* ---- Student Documents ---- */
        'student.documents.title': 'Thư viện tài liệu',
        'student.documents.subtitle': 'Truy cập tất cả giáo trình, tài liệu học tập hướng dẫn đã được duyệt.',
        'student.documents.total_files': 'Tổng số tệp',
        'student.documents.space_used': 'Dung lượng đã dùng',
        'student.documents.search_placeholder': 'Tìm tài liệu theo tên...',
        'student.documents.all_courses': '-- Tất cả môn học --',
        'student.documents.btn_apply': 'Áp dụng lọc',
        'student.chat.newConversation': 'Cuộc trò chuyện mới',
        'student.chat.newSession': 'Phiên mới',
        'student.chat.summarizeDocument': 'Tóm tắt tài liệu này',
        'student.chat.explainTopic': 'Giải thích chủ đề này',
        'student.chat.listLearningOutcomes': 'Danh sách kết quả học tập',
        'student.chat.typing': 'EduBot đang suy nghĩ...',
        'student.chat.inputPlaceholder': 'Hỏi EduBot một câu hỏi về môn học...',
        'student.chat.sources': 'Nguồn trích dẫn:',
        'student.chat.chunk': 'Đoạn',
        'student.chat.match': 'độ khớp',
        'student.chat.sourcePreview': 'Xem nguồn trích dẫn',
        'student.chat.openDocument': 'Mở tài liệu',
        'student.chat.quotaLabel': 'Lượt hỏi',
        'student.chat.quotaExceeded': 'Bạn đã sử dụng hết lượt hỏi của gói hiện tại. Vui lòng chờ đến kỳ reset tiếp theo hoặc nâng cấp gói.',
        'student.chat.quotaExceededShort': 'Hết lượt hỏi',
        'student.chat.upgradePlan': 'Nâng cấp gói',
        'student.chat.noRequestsPlaceholder': 'Bạn đã hết lượt hỏi. Vui lòng nâng cấp gói hoặc chờ reset.',
        'student.chat.waitForReset': 'Vui lòng chờ đến kỳ reset tiếp theo.',
        'student.chat.viewSubscriptionPlans': 'Xem gói dịch vụ',
        'documents.deleteConfirm': 'Bạn có chắc chắn muốn xóa tài liệu này cùng với tất cả các phân đoạn (chunk) của nó?',
    };

    /* ---------- English translations for JS-generated text ---------- */
    const EN = {
        'student.sidebar.subscription': 'Subscription',
        'student.chat.sources': 'Sources:',
        'student.chat.chunk': 'Chunk',
        'student.chat.match': 'match',
        'student.chat.sourcePreview': 'Source preview',
        'student.chat.openDocument': 'Open document',
        'student.chat.newConversation': 'New Conversation',
        'student.chat.newSession': 'New Session',
        'student.chat.new_session': 'New Session',
        'student.chat.summarizeDocument': 'Summarize this document',
        'student.chat.suggest_summarize': 'Summarize this document',
        'student.chat.explainTopic': 'Explain this topic',
        'student.chat.suggest_explain': 'Explain this topic',
        'student.chat.listLearningOutcomes': 'List learning outcomes',
        'student.chat.suggest_outcomes': 'List learning outcomes',
        'student.chat.typing': 'EduBot is thinking...',
        'student.chat.thinking': 'EduBot is thinking...',
        'student.chat.inputPlaceholder': 'Ask EduBot a question about courses...',
        'student.chat.input_placeholder': 'Ask EduBot a question about courses...',
        'student.chat.quotaLabel': 'Requests',
        'student.chat.quotaExceeded': 'You have used all requests in your current plan. Please wait until the next reset period or upgrade your plan.',
        'student.chat.quotaExceededShort': 'Quota Exceeded',
        'student.chat.upgradePlan': 'Upgrade Plan',
        'student.chat.noRequestsPlaceholder': 'You have no requests remaining. Please upgrade or wait for reset.',
        'student.chat.waitForReset': 'Please wait until the next reset period.',
        'student.chat.viewSubscriptionPlans': 'View Subscription Plans',
        'documents.deleteConfirm': 'Are you sure you want to delete this document and all its chunks?',
        'chat.searchingDocs': 'Searching documents...',
        'chat.errorSending': 'Sorry, an error occurred while sending the message. Please try again.',
        'student.chathistory.delete': 'Delete',
        'student.chathistory.deleteConfirm': 'Are you sure you want to delete this chat?',
        'student.chathistory.deleteSuccess': 'Chat deleted.',
        'student.chathistory.deleteFailed': 'Unable to delete chat.',
        'student.chathistory.deleteModalTitle': 'Delete Conversation',
        'student.chathistory.selectAll': 'Select All',
        'student.chathistory.selectedCountLabel': 'selected',
        'student.chathistory.deleteSelected': 'Delete Selected',
        'student.chathistory.bulkDeleteModalTitle': 'Delete Conversations',
        'student.chathistory.bulkDeleteConfirm': 'Are you sure you want to delete all selected chats?',
        'student.chathistory.deleteMultipleSuccess': 'Successfully deleted the selected chat conversations.',
        'student.chathistory.noChatsSelected': 'No conversations selected for deletion.',

        /* ---- Subscription ---- */
        'subscription.kicker': 'Subscription',
        'subscription.plans.title': 'Subscription Plans',
        'subscription.plans.subtitle': 'Choose a plan to unlock learning features and keep your study flow moving.',
        'subscription.plans.viewMine': 'My Subscription',
        'subscription.price.free': 'Free',
        'subscription.feature.requests': 'requests',
        'subscription.feature.chat': 'AI learning chat',
        'subscription.feature.quizUnlocked': 'Quiz unlocked',
        'subscription.feature.quizLocked': 'Quiz locked',
        'subscription.badge.current': 'Current plan',
        'subscription.badge.premiumCurrent': 'Premium active',
        'subscription.action.upgrade': 'Upgrade Premium',
        'subscription.action.backChat': 'Back to Chat',
        'subscription.action.backDashboard': 'Back to Dashboard',
        'subscription.action.viewMine': 'View My Subscription',
        'subscription.action.retry': 'Try Again',
        'subscription.me.kicker': 'My Subscription',
        'subscription.me.title': 'My Subscription',
        'subscription.me.subtitle': 'Review your current plan, request quota, and unlocked learning features.',
        'subscription.me.currentPlan': 'Current Plan',
        'subscription.me.remaining': 'Remaining Requests',
        'subscription.me.used': 'Used Requests',
        'subscription.me.details': 'Plan Details',
        'subscription.me.planType': 'Plan Type',
        'subscription.me.planName': 'Plan Name',
        'subscription.me.limit': 'Request Limit',
        'subscription.me.nextReset': 'Next Reset Time',
        'subscription.me.quizUnlocked': 'Unlocked',
        'subscription.me.quizLocked': 'Locked — Premium required',
        'subscription.callback.successTitle': 'Payment Successful',
        'subscription.callback.failedTitle': 'Payment Failed',
        'subscription.callback.successDesc': 'Your account has been upgraded to Premium successfully.',
        'subscription.callback.cancelledDesc': 'The payment was cancelled. You are still on the Basic plan.',
        'subscription.callback.failedDesc': 'The payment failed. Please try again.',
        'subscription.callback.errorDesc': 'Invalid order information or error verifying payment.',
        'quizzes.student.premiumRequired': 'Premium Required',
        'quizzes.student.alertTitle': 'Premium Plan Required',
        'quizzes.student.alertText': 'The quiz taking feature requires a Premium account subscription. Please upgrade your plan to continue.',
        'quizzes.student.alertConfirm': 'View Subscription Plans',
        'quizzes.student.alertCancel': 'Close',
        'profile.adminAccount': 'Admin Account',
        'profile.lecturerAccount': 'Lecturer Account',
        'profile.studentAccount': 'Student Account'
    };

    /* ---------- Core logic ---------- */
    const STORAGE_KEY = 'eduChatbot.lang';
    const COOKIE_NAME = 'edu_lang';

    function getLang() {
        return localStorage.getItem(STORAGE_KEY) || 'en';
    }

    function setLang(lang) {
        localStorage.setItem(STORAGE_KEY, lang);
        document.cookie = COOKIE_NAME + '=' + lang + ';path=/;max-age=31536000';
        applyLang(lang);
    }

    function applyLang(lang) {
        document.documentElement.lang = lang;

        // Translate data-i18n elements
        document.querySelectorAll('[data-i18n]').forEach(function (el) {
            var key = el.getAttribute('data-i18n');
            if (lang === 'vi' && VI[key]) {
                // Store original English text
                if (!el.hasAttribute('data-i18n-en')) {
                    el.setAttribute('data-i18n-en', el.textContent);
                }
                el.textContent = VI[key];
            } else {
                // Restore English
                var en = el.getAttribute('data-i18n-en');
                if (en !== null) {
                    el.textContent = en;
                }
            }
        });

        // Translate data-i18n-placeholder
        document.querySelectorAll('[data-i18n-placeholder]').forEach(function (el) {
            var key = el.getAttribute('data-i18n-placeholder');
            if (lang === 'vi' && VI[key]) {
                if (!el.hasAttribute('data-i18n-placeholder-en')) {
                    el.setAttribute('data-i18n-placeholder-en', el.placeholder);
                }
                el.placeholder = VI[key];
            } else {
                var en = el.getAttribute('data-i18n-placeholder-en');
                if (en !== null) {
                    el.placeholder = en;
                }
            }
        });

        // Setup modern SweetAlert2 confirm handler
        document.querySelectorAll('[data-i18n-confirm]').forEach(function (el) {
            // Remove any inline onclick attributes to prevent native dialogs
            el.removeAttribute('onclick');

            // Avoid adding multiple click event listeners if toggle is called multiple times
            if (el.dataset.swalHooked) return;
            el.dataset.swalHooked = "true";
            
            el.addEventListener('click', function(e) {
                // If this click is triggered programmatically by our code, let it proceed!
                if (el.dataset.swalSubmitting === 'true') {
                    return;
                }

                e.preventDefault();
                var currentLang = EduI18n.getLang();
                var key = el.getAttribute('data-i18n-confirm');
                
                var text = "";
                if (currentLang === 'vi' && VI[key]) {
                    text = VI[key];
                } else if (currentLang === 'en' && EN[key]) {
                    text = EN[key];
                } else {
                    text = el.getAttribute('data-i18n-confirm-en') || el.getAttribute('data-i18n-confirm') || '';
                }
                
                // Show modern SweetAlert2 confirm dialog
                if (window.Swal) {
                    Swal.fire({
                        title: currentLang === 'vi' ? 'Xác nhận' : 'Confirm',
                        text: text,
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33',
                        confirmButtonText: currentLang === 'vi' ? 'Đồng ý' : 'Yes',
                        cancelButtonText: currentLang === 'vi' ? 'Hủy' : 'Cancel',
                        background: '#181c26',
                        color: '#fff',
                        customClass: {
                            popup: 'swal2-dark-custom shadow border'
                        }
                    }).then(function(result) {
                        if (result.isConfirmed) {
                            var form = el.closest('form');
                            if (form && el.type === 'submit') {
                                // Programmatically submit the form
                                el.dataset.swalSubmitting = 'true';
                                el.click(); // Click again, which will trigger default behavior since swalSubmitting is true
                                el.dataset.swalSubmitting = 'false';
                            } else if (el.tagName === 'A') {
                                window.location.href = el.href;
                            } else if (form) {
                                form.submit();
                            }
                        }
                    });
                } else {
                    // Fallback to native confirm if SweetAlert2 is not loaded
                    if (confirm(text)) {
                        var form = el.closest('form');
                        if (form) {
                            form.submit();
                        } else if (el.tagName === 'A') {
                            window.location.href = el.href;
                        }
                    }
                }
            });
        });

        // Update toggle button text
        var toggleBtns = document.querySelectorAll('.lang-toggle-btn');
        toggleBtns.forEach(function (btn) {
            btn.textContent = lang === 'vi' ? 'EN' : 'VI';
            btn.title = lang === 'vi' ? 'Switch to English' : 'Chuyển sang Tiếng Việt';
        });
    }

    /* ---------- Public API ---------- */
    window.EduI18n = {
        getLang: getLang,
        setLang: setLang,
        toggle: function () {
            setLang(getLang() === 'en' ? 'vi' : 'en');
        },
        t: function (key) {
            var lang = getLang();
            if (lang === 'vi' && VI[key]) return VI[key];
            if (lang === 'en' && EN[key]) return EN[key];
            if (EN[key]) return EN[key];
            if (VI[key]) return VI[key];

            // Safe clean fallback
            if (typeof key === 'string' && key.startsWith('student.chat.')) {
                var last = key.split('.').pop() || '';
                return last
                    .replace(/_/g, ' ')
                    .replace(/([A-Z])/g, ' $1')
                    .replace(/^./, function (str) { return str.toUpperCase(); })
                    .trim();
            }
            return key; // fallback
        }
    };

    /* ---------- Auto-apply on load ---------- */
    document.addEventListener('DOMContentLoaded', function () {
        applyLang(getLang());
    });
})();

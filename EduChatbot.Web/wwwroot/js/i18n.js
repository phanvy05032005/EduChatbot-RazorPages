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
        'admin.dashboard.title': 'Tổng quan',
        'admin.dashboard.titleSerif': 'hệ thống',
        'admin.dashboard.subtitle': 'Theo dõi số liệu và hoạt động trên toàn hệ thống EduChatbot.',
        'admin.dashboard.refresh': 'Làm mới',
        'admin.dashboard.recentActivities': 'Hoạt động gần đây',
        'admin.dashboard.live': 'Trực tiếp',
        'admin.dashboard.updatedJustNow': 'Vừa cập nhật · Quản trị',
        'admin.dashboard.questionsLast7': 'Câu hỏi · 7 ngày qua',
        'admin.dashboard.totalQuestions': 'Tổng câu hỏi',
        'admin.dashboard.totalStudents': 'Tổng sinh viên',
        'admin.dashboard.totalLecturers': 'Tổng giảng viên',
        'admin.dashboard.documentsInSystem': 'Tài liệu trong hệ thống',
        'admin.dashboard.totalChatQuestions': 'Tổng câu hỏi Chat',

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

        /* ---- Admin Roles ---- */
        'admin.roles.title': 'Phân',
        'admin.roles.titleSerif': 'quyền',
        'admin.roles.subtitle': 'Các vai trò trong hệ thống và phạm vi quyền hạn tương ứng.',
        'admin.roles.student': 'Sinh viên',
        'admin.roles.lecturer': 'Giảng viên',
        'admin.roles.admin': 'Quản trị viên',
        'admin.roles.studentDesc': 'Hỏi đáp về tài liệu đã upload, xem lịch sử trò chuyện cá nhân.',
        'admin.roles.lecturerDesc': 'Upload, quản lý và chỉnh sửa tài liệu giảng dạy.',
        'admin.roles.adminDesc': 'Toàn quyền quản trị hệ thống và tài khoản.',
        'admin.roles.permissions': 'Quyền hạn',

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
        'docs.upload.fileTooLarge': 'File không được vượt quá 10 MB.',

        /* ---- Profile ---- */
        'profile.kicker': 'HỒ SƠ CÁ NHÂN',
        'profile.title': 'Hồ sơ của bạn',
        'profile.back': '← Quay lại Chat',
        'profile.updateSuccess': 'Cập nhật hồ sơ thành công.',
        'profile.personalInfo': 'Thông tin cá nhân',
        'profile.changePassword': 'Đổi mật khẩu',
        'profile.lecturerAccount': 'Tài khoản giảng viên',
        'profile.studentAccount': 'Tài khoản sinh viên',
        'profile.backDashboard': 'Quay lại bảng điều khiển',
        'profile.backChat': 'Quay lại Chat',
        'profile.fullName': 'Họ tên',
        'profile.fullNamePlaceholder': 'Nhập họ tên của bạn',
        'profile.email': 'Email',
        'profile.currentPassword': 'Mật khẩu hiện tại',
        'profile.newPassword': 'Mật khẩu mới',
        'profile.updateProfile': 'Cập nhật hồ sơ',
        'profile.changePasswordBtn': 'Đổi mật khẩu',
        'profile.confirmPassword': 'Xác nhận mật khẩu mới',

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
        'student.sidebar.logout': 'Đăng xuất',
        'student.sidebar.profile': 'Hồ sơ của tôi',
        'student.sidebar.changepassword': 'Đổi mật khẩu',
        'student.topbar.profile': 'Hồ sơ của tôi',
        'student.topbar.changepassword': 'Đổi mật khẩu',
        'student.topbar.startchat': 'Bắt đầu Chat mới',
        'student.topbar.search_placeholder': 'Tìm môn học, tài liệu...',

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
    };

    /* ---------- English translations for JS-generated text ---------- */
    const EN = {
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
        'student.chathistory.noChatsSelected': 'No conversations selected for deletion.'
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

        // Translate data-i18n-confirm (onclick confirm dialogs)
        document.querySelectorAll('[data-i18n-confirm]').forEach(function (el) {
            var key = el.getAttribute('data-i18n-confirm');
            if (lang === 'vi' && VI[key]) {
                el.setAttribute('onclick', "return confirm('" + VI[key].replace(/'/g, "\\'") + "');");
            } else {
                var en = el.getAttribute('data-i18n-confirm-en');
                if (en) {
                    el.setAttribute('onclick', "return confirm('" + en.replace(/'/g, "\\'") + "');");
                }
            }
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

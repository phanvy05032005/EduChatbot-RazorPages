(function () {
    // Check if SignalR is loaded
    if (typeof signalR === 'undefined') {
        console.warn('SignalR library not loaded.');
        return;
    }

    // Create toast container
    let toastContainer = document.querySelector('.realtime-toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'realtime-toast-container';
        document.body.appendChild(toastContainer);
    }

    function showRealtimeToast(title, message) {
        const toast = document.createElement('div');
        toast.className = 'realtime-toast';
        toast.innerHTML = `
            <div class="realtime-toast-icon">⚡</div>
            <div class="realtime-toast-content">
                <div class="realtime-toast-title">${title}</div>
                <div class="realtime-toast-message">${message}</div>
            </div>
            <button type="button" class="realtime-toast-close">&times;</button>
        `;

        toastContainer.appendChild(toast);

        // Trigger show animation on next frame
        requestAnimationFrame(() => {
            toast.classList.add('show');
        });

        // Close button handler
        toast.querySelector('.realtime-toast-close').addEventListener('click', () => {
            closeToast(toast);
        });

        // Auto close after 5 seconds
        setTimeout(() => {
            closeToast(toast);
        }, 5000);
    }

    function closeToast(toast) {
        if (!toast.classList.contains('fade-out')) {
            toast.classList.add('fade-out');
            toast.addEventListener('transitionend', () => {
                toast.remove();
            });
        }
    }

    // Initialize Connection to Student Hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    // Listen to CourseCreated
    connection.on("CourseCreated", (payload) => {
        console.log("CourseCreated event received:", payload);
        if (!payload) return;

        const isVi = typeof EduI18n !== 'undefined' && EduI18n.getLang() === 'vi';
        const title = isVi ? 'MÔN HỌC MỚI' : 'NEW COURSE';
        const message = isVi 
            ? `Môn học mới ${payload.courseCode} - ${payload.courseName} vừa được tạo!`
            : `New course ${payload.courseCode} - ${payload.courseName} has been created!`;

        showRealtimeToast(title, message);

        // Check if page needs reloading
        const pathLower = window.location.pathname.toLowerCase();
        const isDashboard = pathLower === '/student' || pathLower === '/student/' || pathLower.includes('/student/index');
        const isCoursesPage = pathLower.includes('/student/courses');

        if (isDashboard || isCoursesPage) {
            setTimeout(() => {
                location.reload();
            }, 1500);
        }
    });

    // Listen to DocumentAvailable
    connection.on("DocumentAvailable", (payload) => {
        console.log("DocumentAvailable event received:", payload);
        if (!payload) return;

        const isVi = typeof EduI18n !== 'undefined' && EduI18n.getLang() === 'vi';
        const title = isVi ? 'TÀI LIỆU MỚI' : 'NEW DOCUMENT';
        const message = isVi
            ? `Tài liệu mới "${payload.fileName}" cho môn ${payload.courseCode} vừa được tải lên!`
            : `New document "${payload.fileName}" for course ${payload.courseCode} is available!`;

        showRealtimeToast(title, message);

        // Check if page needs reloading
        const pathLower = window.location.pathname.toLowerCase();
        const isDashboard = pathLower === '/student' || pathLower === '/student/' || pathLower.includes('/student/index');
        const isDocumentsPage = pathLower.includes('/student/documents');
        const isCourseDetailPage = pathLower.includes('/student/coursedetail');

        let shouldReload = isDashboard || isDocumentsPage;

        if (isCourseDetailPage) {
            const params = new URLSearchParams(window.location.search);
            const courseIdParam = params.get('id');
            if (courseIdParam === String(payload.courseId)) {
                shouldReload = true;
            }
        }

        if (shouldReload) {
            setTimeout(() => {
                location.reload();
            }, 1500);
        }
    });

    // Start Connection
    connection.start()
        .then(() => console.log("SignalR Student notification connection established."))
        .catch(err => console.error("SignalR Student Connection Error: ", err));

})();

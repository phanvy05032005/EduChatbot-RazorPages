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

        // Auto close after 4 seconds
        setTimeout(() => {
            closeToast(toast);
        }, 4000);
    }

    function closeToast(toast) {
        if (!toast.classList.contains('fade-out')) {
            toast.classList.add('fade-out');
            toast.addEventListener('transitionend', () => {
                toast.remove();
            });
        }
    }

    // Initialize Connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/adminHub")
        .withAutomaticReconnect()
        .build();

    // Re-highlight new elements or rows after a refresh
    function highlightElements(selector) {
        document.querySelectorAll(selector).forEach(el => {
            el.classList.add('realtime-highlight');
        });
    }

    function refreshSection(targetId, notifyTitle, notifyMsg, callback) {
        const targetElement = document.getElementById(targetId);
        if (!targetElement) return;

        // Fetch current page content
        fetch(window.location.href)
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.text();
            })
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newContent = doc.getElementById(targetId);
                if (newContent) {
                    targetElement.innerHTML = newContent.innerHTML;
                    if (notifyTitle && notifyMsg) {
                        showRealtimeToast(notifyTitle, notifyMsg);
                    }
                    if (callback) callback();
                }
            })
            .catch(err => console.error('Error refreshing content:', err));
    }

    // Listen to Account Changes
    connection.on("ReceiveAccountChange", (action, role) => {
        console.log(`Account change received: ${action} for role ${role}`);
        
        // Translate actions for Vietnamese locale if language is VI
        const isVi = typeof EduI18n !== 'undefined' && EduI18n.getLang() === 'vi';
        
        let actionStr = action === 'Create' ? (isVi ? 'được tạo mới' : 'created') :
                        action === 'Update' ? (isVi ? 'được cập nhật' : 'updated') :
                        action === 'Delete' ? (isVi ? 'đã bị xóa' : 'deleted') :
                        action === 'StatusChange' ? (isVi ? 'thay đổi trạng thái' : 'status changed') :
                        (isVi ? 'được import thành công' : 'imported successfully');

        let roleStr = role === 'Student' ? (isVi ? 'Sinh viên' : 'Student') : (isVi ? 'Giảng viên' : 'Lecturer');
        let notifyTitle = isVi ? 'CẬP NHẬT HỆ THỐNG' : 'SYSTEM UPDATE';
        let notifyMsg = isVi ? `Tài khoản ${roleStr} ${actionStr}!` : `${roleStr} account ${actionStr}!`;

        // 1. If currently on Dashboard
        if (document.getElementById('dashboard-container')) {
            refreshSection('dashboard-container', notifyTitle, notifyMsg);
        }
        
        // 2. If on Users page (unified students/lecturers management)
        const pathLower = window.location.pathname.toLowerCase();
        const isUsersPage = pathLower.includes('/admin/users');
        
        if (isUsersPage) {
            const targetCardId = role === 'Student' ? 'accounts-table-card-students' : 'accounts-table-card-lecturers';
            refreshSection(targetCardId, notifyTitle, notifyMsg, () => {
                // Highlight new rows inside the updated table card
                highlightElements(`#${targetCardId} .admin-table tbody tr`);
            });
        }
    });

    // Listen to Course Changes
    connection.on("ReceiveCourseChange", (action, courseCode) => {
        console.log(`Course change received: ${action} for ${courseCode}`);
        
        const isVi = typeof EduI18n !== 'undefined' && EduI18n.getLang() === 'vi';
        
        let actionStr = action === 'Create' ? (isVi ? 'được tạo mới' : 'created') :
                        action === 'Delete' ? (isVi ? 'đã bị xóa' : 'deleted') :
                        action === 'Assign' ? (isVi ? 'phân công giảng viên' : 'lecturer assigned') :
                        action === 'Remove' ? (isVi ? 'gỡ phân công giảng viên' : 'lecturer removed') :
                        (isVi ? 'được import' : 'imported');

        let notifyTitle = isVi ? 'CẬP NHẬT MÔN HỌC' : 'COURSE UPDATE';
        let notifyMsg = isVi ? `Môn học ${courseCode} ${actionStr}!` : `Course ${courseCode} ${actionStr}!`;
        if (action === 'Import') {
            notifyMsg = isVi ? 'Danh sách môn học được import!' : 'Courses list was imported!';
        }

        // 1. If currently on Dashboard
        if (document.getElementById('dashboard-container')) {
            refreshSection('dashboard-container', notifyTitle, notifyMsg);
        }

        // 2. If on Courses page
        if (window.location.pathname.toLowerCase().includes('/admin/courses')) {
            refreshSection('courses-table-card', notifyTitle, notifyMsg, () => {
                highlightElements('.admin-table tbody tr');
            });
        }
    });

    // Listen to Material Changes
    connection.on("ReceiveMaterialChange", (action, lecturerId, lecturerName, fileName) => {
        console.log(`Material change received: ${action} by ${lecturerName} for file ${fileName}`);
        
        const isVi = typeof EduI18n !== 'undefined' && EduI18n.getLang() === 'vi';
        
        let actionStr = action === 'Create' ? (isVi ? 'tải lên tài liệu mới' : 'uploaded a new material') :
                        action === 'Update' ? (isVi ? 'cập nhật tài liệu' : 'updated a material') :
                        (isVi ? 'xóa tài liệu' : 'deleted a material');

        let notifyTitle = isVi ? 'CẬP NHẬT TÀI LIỆU' : 'MATERIAL UPDATE';
        let notifyMsg = isVi 
            ? `Giảng viên ${lecturerName} vừa ${actionStr}: "${fileName}"` 
            : `Lecturer ${lecturerName} ${actionStr}: "${fileName}"`;

        // 1. Show Toast notification on ALL admin pages
        showRealtimeToast(notifyTitle, notifyMsg);

        // 2. If on Lecturer Detail page and matches, trigger refresh
        if (typeof window.refreshLecturerMaterials === 'function') {
            window.refreshLecturerMaterials(lecturerId, notifyTitle, notifyMsg);
        }
        
        // 3. If on Dashboard, refresh stats
        if (document.getElementById('dashboard-container')) {
            refreshSection('dashboard-container', null, null);
        }
    });

    // Start Connection
    connection.start()
        .then(() => console.log("SignalR Admin connection established."))
        .catch(err => console.error("SignalR Connection Error: ", err));

})();

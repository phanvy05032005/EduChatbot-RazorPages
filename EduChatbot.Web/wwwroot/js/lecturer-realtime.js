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

    function showRealtimeToast(title, message, redirectUrl) {
        const toast = document.createElement('div');
        toast.className = 'realtime-toast';
        toast.style.cursor = 'pointer'; // Make it look clickable
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
        toast.querySelector('.realtime-toast-close').addEventListener('click', (e) => {
            e.stopPropagation();
            closeToast(toast);
        });

        // Click handler to redirect
        if (redirectUrl) {
            toast.addEventListener('click', (e) => {
                if (e.target.classList.contains('realtime-toast-close')) return;
                window.location.href = redirectUrl;
            });
        }

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

    // Initialize Connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/adminHub")
        .withAutomaticReconnect()
        .build();

    function highlightElements(selector) {
        document.querySelectorAll(selector).forEach(el => {
            el.classList.add('realtime-highlight');
        });
    }

    function refreshSection(targetId, notifyTitle, notifyMsg, redirectUrl, callback) {
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
                        showRealtimeToast(notifyTitle, notifyMsg, redirectUrl);
                    }
                    // Reapply internationalization since DOM changed
                    if (typeof EduI18n !== 'undefined' && typeof EduI18n.setLang === 'function') {
                        // Apply current language translations to new elements
                        const currentLang = localStorage.getItem('eduChatbot.lang') || 'en';
                        document.documentElement.lang = currentLang;
                        targetElement.querySelectorAll('[data-i18n]').forEach(el => {
                            const key = el.getAttribute('data-i18n');
                            const translations = EduI18n.t(key);
                            if (translations !== key) {
                                el.textContent = translations;
                            }
                        });
                    }
                    if (callback) callback();
                }
            })
            .catch(err => console.error('Error refreshing content:', err));
    }

    // Listen to Course Changes
    connection.on("ReceiveCourseChange", (action, courseCode) => {
        console.log(`Lecturer received course change: ${action} for ${courseCode}`);

        // Only interested in Create (creation) and Import (excel importing)
        if (action !== 'Create' && action !== 'Import') {
            return;
        }

        const isVi = typeof EduI18n !== 'undefined' && EduI18n.getLang() === 'vi';
        const notifyTitle = isVi ? 'CẬP NHẬT MÔN HỌC' : 'COURSE UPDATE';

        let notifyMsg = "";
        if (action === 'Create') {
            notifyMsg = isVi
                ? `Hệ thống đã thêm môn học mới [${courseCode}]! Bấm vào đây để xem.`
                : `The system has added a new course [${courseCode}]! Click here to view.`;
        } else if (action === 'Import') {
            notifyMsg = isVi
                ? 'Hệ thống đã cập nhật danh sách môn học mới! Bấm vào đây để xem.'
                : 'The system has imported a new list of courses! Click here to view.';
        }

        const redirectUrl = '/Documents/Courses';

        // 1. If currently on the Lecturer's Courses page, refresh the list dynamically
        if (window.location.pathname.toLowerCase().includes('/documents/courses')) {
            refreshSection('courses-table-card', notifyTitle, notifyMsg, null, () => {
                highlightElements('.role-table tbody tr');
            });
        } else {
            // 2. Otherwise, just show the clickable toast notification
            showRealtimeToast(notifyTitle, notifyMsg, redirectUrl);
        }
    });

    // Start Connection
    connection.start()
        .then(() => console.log("SignalR Lecturer connection established."))
        .catch(err => console.error("SignalR Connection Error: ", err));

})();

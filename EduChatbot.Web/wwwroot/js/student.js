/* ========================================================
   EduChatbot — Student UI Client Interactions
   ======================================================== */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initChatbox();
        initClientSearch();
        initMobileSidebar();
    });

    /* ---------- Mobile Sidebar Offcanvas Drawer ---------- */
    function initMobileSidebar() {
        const sidebarToggle = document.getElementById('sidebarToggle');
        const sidebarClose = document.getElementById('sidebarClose');
        const container = document.querySelector('.student-container');

        if (!container) return;

        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function (e) {
                e.stopPropagation();
                container.classList.add('sidebar-open');
            });
        }

        if (sidebarClose) {
            sidebarClose.addEventListener('click', function () {
                container.classList.remove('sidebar-open');
            });
        }

        // Close when clicking outside of the sidebar on tablet/mobile
        document.addEventListener('click', function (e) {
            if (window.innerWidth < 992 && container.classList.contains('sidebar-open')) {
                const sidebar = document.querySelector('.student-sidebar');
                if (sidebar && !sidebar.contains(e.target) && sidebarToggle && !sidebarToggle.contains(e.target)) {
                    container.classList.remove('sidebar-open');
                }
            }
        });
    }

    /* ---------- AI Chatbox composition & SSE Stream ---------- */
    function initChatbox() {
        const messagesContainer = document.getElementById('student-chat-messages');
        const chatForm = document.getElementById('student-chat-form');
        const chatInput = document.getElementById('student-chat-input');
        const sendBtn = document.getElementById('student-chat-send-btn');
        const conversationId = document.getElementById('conversation-id')?.value;
        const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        if (!messagesContainer || !chatForm || !chatInput || !sendBtn) return;

        const streamUrl = chatForm.dataset.streamUrl || '/Student/Chat?handler=SendMessageStream';
        let isSending = false;
        let isComposing = false;

        function updateSendBtn() {
            sendBtn.disabled = isSending || !chatInput.value.trim();
        }

        function scrollToBottom() {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }

        chatInput.addEventListener('input', function () {
            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 160) + 'px';
            updateSendBtn();
        });

        chatInput.addEventListener('compositionstart', function () {
            isComposing = true;
        });

        chatInput.addEventListener('compositionend', function () {
            isComposing = false;
            updateSendBtn();
        });

        chatInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey && !isComposing) {
                e.preventDefault();
                submitMessage();
            }
        });

        chatForm.addEventListener('submit', function (e) {
            e.preventDefault();
            submitMessage();
        });

        // Click suggestion chips
        document.querySelectorAll('.suggestion-chip').forEach(function (chip) {
            chip.addEventListener('click', function () {
                if (isSending) return;
                chatInput.value = this.textContent.trim();
                chatInput.dispatchEvent(new Event('input', { bubbles: true }));
                submitMessage();
            });
        });

        function submitMessage() {
            const text = chatInput.value.trim();
            if (!text || isSending) return;

            isSending = true;
            updateSendBtn();

            // Clear empty panel if exists
            const emptyPanel = messagesContainer.querySelector('.empty-state-panel');
            if (emptyPanel) emptyPanel.remove();

            // Append User message
            appendMessage('user', text);

            // Clear input and reset height immediately
            chatInput.value = '';
            chatInput.style.height = '';
            chatInput.dispatchEvent(new Event('input', { bubbles: true }));
            chatInput.focus();

            scrollToBottom();

            // Create streaming AI message bubble template
            const streamRow = document.createElement('div');
            streamRow.className = 'chat-message assistant d-flex align-self-start gap-2 mb-3 align-items-start w-100';
            
            const avatar = document.createElement('div');
            avatar.className = 'topbar-user-avatar bg-secondary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle flex-shrink-0';
            avatar.style.width = '32px';
            avatar.style.height = '32px';
            avatar.style.fontSize = '0.75rem';
            avatar.textContent = 'AI';
            streamRow.appendChild(avatar);

            const contentDiv = document.createElement('div');
            contentDiv.className = 'd-flex flex-column gap-1';

            const bubble = document.createElement('div');
            bubble.className = 'chat-bubble ai';
            bubble.innerHTML = '<span class="typing-cursor"></span>';
            contentDiv.appendChild(bubble);
            streamRow.appendChild(contentDiv);
            messagesContainer.appendChild(streamRow);
            scrollToBottom();

            let accumulated = '';

            fetch(streamUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': antiForgeryToken
                },
                body: 'conversationId=' + encodeURIComponent(conversationId) +
                    '&message=' + encodeURIComponent(text) +
                    '&__RequestVerificationToken=' + encodeURIComponent(antiForgeryToken)
            })
            .then(function (res) {
                const reader = res.body.getReader();
                const decoder = new TextDecoder();
                let buffer = '';

                function processChunk(result) {
                    if (result.done) {
                        finishStream(bubble, accumulated);
                        return;
                    }

                    buffer += decoder.decode(result.value, { stream: true });
                    const lines = buffer.split('\n');
                    buffer = lines.pop() || '';

                    for (let i = 0; i < lines.length; i++) {
                        const line = lines[i].trim();
                        if (!line.startsWith('data: ')) continue;
                        const jsonStr = line.substring(6);
                        try {
                            const data = JSON.parse(jsonStr);

                            if (data.token) {
                                accumulated += data.token;
                                bubble.innerHTML = renderMarkdown(accumulated) + '<span class="typing-cursor"></span>';
                                scrollToBottom();
                            } else if (data.outOfScope) {
                                accumulated = data.content;
                                avatar.textContent = '⚠';
                                avatar.classList.add('bg-warning');
                                bubble.innerHTML = renderMarkdown(accumulated);
                                scrollToBottom();
                            } else if (data.sources && data.sources.length > 0) {
                                renderStreamSources(contentDiv, data.sources);
                                scrollToBottom();
                            } else if (data.done) {
                                finishStream(bubble, accumulated);
                            }
                        } catch (e) { /* ignore JSON parsing error */ }
                    }

                    return reader.read().then(processChunk);
                }

                return reader.read().then(processChunk);
            })
            .catch(function () {
                bubble.innerHTML = renderMarkdown(window.EduI18n ? EduI18n.t('chat.errorSending') : 'Sorry, an error occurred. Please try again.');
                scrollToBottom();
            })
            .finally(function () {
                isSending = false;
                updateSendBtn();
                chatInput.focus();
            });
        }

        function finishStream(bubble, content) {
            bubble.classList.remove('streaming-bubble');
            if (content) {
                bubble.innerHTML = renderMarkdown(content);
            }
        }

        function renderStreamSources(contentDiv, sources) {
            const sourcesDiv = document.createElement('div');
            sourcesDiv.className = 'd-flex flex-wrap gap-2 mt-2';
            const sourcesLabel = window.EduI18n ? EduI18n.t('student.chat.sources') : 'Sources:';
            sourcesDiv.innerHTML = '<div class="w-100 text-muted" style="font-size: 0.75rem;">' + sourcesLabel + '</div>';

            const chunkLabel = window.EduI18n ? EduI18n.t('student.chat.chunk') : 'Chunk';

            sources.forEach(function (s) {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'source-chip';
                btn.dataset.documentId = s.documentId;
                btn.dataset.chunkIndex = s.chunk;
                
                // Parse score: convert similarity score from 0-1 to 0-100 range
                let scorePercent = 0;
                if (s.score !== undefined && s.score !== null) {
                    scorePercent = s.score <= 1.0 ? Math.round(s.score * 100) : Math.round(s.score);
                }
                btn.dataset.score = scorePercent || '';
                btn.setAttribute('data-bs-toggle', 'modal');
                btn.setAttribute('data-bs-target', '#sourcePreviewModal');
                btn.title = s.chunkPreview || '';

                let inner = '<i class="bi bi-file-earmark-text"></i>' +
                    '<span class="source-file">' + escapeHtml(s.doc) + '</span>' +
                    '<span class="source-meta">· ' + chunkLabel + '</span> ' +
                    '<span class="source-chunk">' + s.chunk + '</span>';

                if (scorePercent > 0) {
                    const matchLabel = window.EduI18n ? EduI18n.t('student.chat.match') : 'match';
                    inner += '<span class="source-score">· ' + scorePercent + '% ' + matchLabel + '</span>';
                }
                
                btn.innerHTML = inner;
                sourcesDiv.appendChild(btn);
            });
            contentDiv.appendChild(sourcesDiv);
        }

        // Bootstrap Source Preview Modal Handler
        const sourceModal = document.getElementById('sourcePreviewModal');
        if (sourceModal) {
            sourceModal.addEventListener('show.bs.modal', function (event) {
                const button = event.relatedTarget;
                const docId = button.dataset.documentId;
                const chunkIdx = button.dataset.chunkIndex;
                const score = button.dataset.score;

                // Bind basic metadata instantly
                document.getElementById('modal-doc-name').textContent = button.querySelector('.source-file')?.textContent || '';
                document.getElementById('modal-chunk-index').textContent = chunkIdx;

                const scoreWrapper = document.getElementById('modal-score-wrapper');
                const scoreVal = document.getElementById('modal-score-value');
                if (score) {
                    scoreWrapper.style.display = 'block';
                    const matchLabel = window.EduI18n ? EduI18n.t('student.chat.match') : 'match';
                    scoreVal.textContent = score + '% ' + matchLabel;
                } else {
                    scoreWrapper.style.display = 'none';
                }

                // Temporary loading message
                const chunkTextDiv = document.getElementById('modal-chunk-text');
                chunkTextDiv.textContent = window.EduI18n ? EduI18n.t('chat.searchingDocs') : 'Loading source content...';

                // Hide open button initially
                const openDocBtn = document.getElementById('modal-open-doc-btn');
                if (openDocBtn) {
                    openDocBtn.style.display = 'none';
                    openDocBtn.href = '#';
                }

                // AJAX Secure query
                fetch('/Student/Chat?handler=GetSourceDetails&documentId=' + docId + '&chunkIndex=' + chunkIdx)
                    .then(function (res) {
                        return res.json();
                    })
                    .then(function (data) {
                        if (data.success) {
                            chunkTextDiv.textContent = data.content;
                            if (openDocBtn) {
                                // Direct secure authorized handler link
                                openDocBtn.href = '/Student/Chat?handler=DownloadDocument&documentId=' + docId;
                                openDocBtn.style.display = 'inline-flex';
                            }
                        } else {
                            chunkTextDiv.textContent = data.message || 'Failed to load source details.';
                        }
                    })
                    .catch(function () {
                        chunkTextDiv.textContent = 'An error occurred while fetching source details.';
                    });
            });
        }

        function appendMessage(role, content) {
            const row = document.createElement('div');
            
            if (role === 'user') {
                row.className = 'chat-message user d-flex align-self-end justify-content-end mb-3 w-100';
                const bubble = document.createElement('div');
                bubble.className = 'chat-bubble user text-white bg-primary';
                bubble.textContent = content;
                row.appendChild(bubble);
            } else {
                row.className = 'chat-message assistant d-flex align-self-start gap-2 mb-3 align-items-start w-100';
                
                const avatar = document.createElement('div');
                avatar.className = 'topbar-user-avatar bg-secondary text-white d-flex align-items-center justify-content-center fw-bold rounded-circle flex-shrink-0';
                avatar.style.width = '32px';
                avatar.style.height = '32px';
                avatar.style.fontSize = '0.75rem';
                avatar.textContent = 'AI';
                row.appendChild(avatar);

                const contentDiv = document.createElement('div');
                contentDiv.className = 'd-flex flex-column gap-1';

                const bubble = document.createElement('div');
                bubble.className = 'chat-bubble ai';
                bubble.innerHTML = renderMarkdown(content);
                contentDiv.appendChild(bubble);
                row.appendChild(contentDiv);
            }

            messagesContainer.appendChild(row);
            scrollToBottom();
        }

        function escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text || '';
            return div.innerHTML;
        }

        function renderMarkdown(text) {
            if (!text) return '';
            let html = escapeHtml(text);

            // Code blocks
            html = html.replace(/```([\s\S]*?)```/g, function (_, code) {
                return '<pre class="bg-light border rounded p-2 text-dark font-monospace" style="font-size: 0.85rem;"><code>' + code.trim() + '</code></pre>';
            });

            // Inline code, bold, italic
            html = html.replace(/`([^`]+)`/g, '<code class="bg-light px-1 text-danger">$1</code>');
            html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
            html = html.replace(/(?<!\*)\*([^*]+)\*(?!\*)/g, '<em>$1</em>');

            // Linebreaks and paragraphs
            const lines = html.split('\n');
            const result = [];
            let inList = false;
            let listType = '';

            for (let i = 0; i < lines.length; i++) {
                const line = lines[i];
                const trimmed = line.trim();

                if (/^[-*]\s+/.test(trimmed)) {
                    if (!inList || listType !== 'ul') {
                        if (inList) result.push('</' + listType + '>');
                        result.push('<ul class="ps-3 mb-2">');
                        inList = true;
                        listType = 'ul';
                    }
                    result.push('<li>' + trimmed.replace(/^[-*]\s+/, '') + '</li>');
                } else if (/^\d+\.\s+/.test(trimmed)) {
                    if (!inList || listType !== 'ol') {
                        if (inList) result.push('</' + listType + '>');
                        result.push('<ol class="ps-3 mb-2">');
                        inList = true;
                        listType = 'ol';
                    }
                    result.push('<li>' + trimmed.replace(/^\d+\.\s+/, '') + '</li>');
                } else {
                    if (inList) {
                        result.push('</' + listType + '>');
                        inList = false;
                        listType = '';
                    }

                    if (trimmed === '') {
                        result.push('<br />');
                    } else {
                        result.push('<p class="mb-2">' + line + '</p>');
                    }
                }
            }

            if (inList) result.push('</' + listType + '>');
            return result.join('');
        }

        scrollToBottom();
        updateSendBtn();
    }

    /* ---------- Quick Client Search Filter ---------- */
    function initClientSearch() {
        const topbarSearch = document.getElementById('topbarSearchInput');
        if (!topbarSearch) return;

        topbarSearch.addEventListener('input', function () {
            const query = this.value.toLowerCase().trim();

            // 1. Filter Courses Grid
            const courseCards = document.querySelectorAll('.course-grid-card');
            courseCards.forEach(function (card) {
                const code = card.dataset.code?.toLowerCase() || '';
                const name = card.dataset.name?.toLowerCase() || '';
                if (code.includes(query) || name.includes(query)) {
                    card.style.display = 'block';
                } else {
                    card.style.display = 'none';
                }
            });

            // 2. Filter Table Documents Rows
            const docRows = document.querySelectorAll('.doc-table-row');
            docRows.forEach(function (row) {
                const name = row.dataset.name?.toLowerCase() || '';
                const course = row.dataset.course?.toLowerCase() || '';
                if (name.includes(query) || course.includes(query)) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    }
})();

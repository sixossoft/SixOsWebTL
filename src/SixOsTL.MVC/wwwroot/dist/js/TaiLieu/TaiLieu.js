// ── State ─────────────────────────────────────────────────────
var _activeItem = null;
var _currentDocId = null;   // IDChucNang của doc đang mở
var _currentVideoId = null;   // ID của video đang mở (data-id)
var _relatedVideos = [];     // cache danh sách related
var _countdownTimer = null;   // setInterval ref
var _searchFilter = 'video'; // mặc định filter = video
var _historySaveTimer = null;
var _lastSavedVideoTime = -1;
// ── Autoplay countdown ────────────────────────
var _COUNTDOWN_SEC = 7;
var _ICON_PDF = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="9" y1="13" x2="15" y2="13"/><line x1="9" y1="17" x2="15" y2="17"/></svg>`;


// ── Group toggle ───────────────────────────────────────────────
function toggleGroup(id, isSearch = false) {
    const prefix = isSearch ? 'sr-group-container-' : 'group-';
    const grp = document.getElementById(prefix + id);
    if (grp) {
        const selector = isSearch ? '#searchResults .doc-group' : '#defaultList .doc-group';
        document.querySelectorAll(selector).forEach(g => {
            if (g !== grp) {
                g.classList.remove('open');
            }
        });
        grp.classList.toggle('open');
    }
}

function toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const viewer = document.querySelector('.viewer');
    const isCollapsed = sidebar.style.width === '40px';

    if (isCollapsed) {
        sidebar.style.width = '360px';
        sidebar.classList.remove('collapsed');
    } else {
        sidebar.style.width = '40px';
        sidebar.classList.add('collapsed');
    }
}

// ── Mở tài liệu ───────────────────────────────────────────────
function openDoc(el) {
    document.querySelectorAll('.doc-item').forEach(c => c.classList.remove('active'));
    el.classList.add('active');
    _activeItem = el;
    const activeClone = document.querySelector(`#searchResults .doc-item[data-id="${el.dataset.id}"][data-type="${el.dataset.type}"]`);
    if (activeClone) activeClone.classList.add('active');
    const type = el.dataset.type;
    const path = el.dataset.path;
    const name = el.dataset.name;
    const group = el.dataset.group;
    const cnId = el.dataset.cnId;
    const itemId = el.dataset.id;
    _currentDocId = cnId;

    // Auto-open parent group and close other groups
    const grp = document.getElementById('group-' + cnId);
    if (grp) {
        document.querySelectorAll('#defaultList .doc-group').forEach(g => {
            if (g !== grp) {
                g.classList.remove('open');
            }
        });
        grp.classList.add('open');
    }
    _currentVideoId = (type === 'video') ? itemId : null;
    document.getElementById('viewerBar').style.display = '';
    document.getElementById('viewerEmpty').style.display = 'none';
    document.getElementById('viewerBody').style.display = '';
    document.getElementById('viewerTitle').textContent = name;
    document.getElementById('viewerGroup').textContent = group;
    const badge = document.getElementById('viewerBadge');
    const icon = document.getElementById('viewerIcon');
    badge.className = 'viewer-badge';
    if (type === 'video') {
        badge.classList.add('badge-video'); badge.textContent = 'Video';
        icon.className = 'ti ti-player-play';
    } else if (type === 'pdf') {
        badge.classList.add('badge-pdf'); badge.textContent = 'PDF';
        icon.className = '';
        icon.innerHTML = `<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>`;
    }
    const streamUrl = '/TaiLieu/StreamFile?path=' + encodeURIComponent(path) + '&fileName=' + encodeURIComponent(name);
    document.getElementById('btnDownload').href = streamUrl;

    // reset related panel
    _clearCountdown();
    _relatedVideos = [];
    _hideRelatedPanel();

    renderViewer(type, streamUrl, name);

    // load related nếu là video
    if (type === 'video' && itemId) {
        _renderRelatedPanel(); // Render rỗng trước khi tải dữ liệu
        _loadRelatedVideos(itemId);
    }

    // load comments nếu panel đang mở
    const commentPanel = document.getElementById('commentPanel');
    if (commentPanel && commentPanel.classList.contains('open')) {
        loadComments(cnId);
    }

    if (window.innerWidth <= 768) {
        document.querySelector('.sidebar').classList.add('mobile-hidden');
        document.querySelector('.viewer').classList.add('mobile-active');
    }
}

function backToSidebar() {
    document.querySelector('.sidebar').classList.remove('mobile-hidden');
    document.querySelector('.viewer').classList.remove('mobile-active');
    _syncBackToViewerBtn(); // hiện nút quay lại nếu đang có video/doc đang phát
}

function _syncBackToViewerBtn() {
    const existing = document.getElementById('btnBackToViewer');
    if (existing) existing.remove();
    if (!_activeItem) return;

    const sidebar = document.querySelector('.sidebar');
    if (!sidebar) return;

    const btn = document.createElement('div');
    btn.id = 'btnBackToViewer';
    btn.className = 'back-to-viewer-bar';
    btn.innerHTML = `
        <button onclick="backToViewer()">
            <div class="eq-icon" aria-hidden="true">
                <span></span><span></span><span></span>
            </div>
            <span id="backToViewerName">${_activeItem?.dataset?.name ?? 'Video đang phát'}</span>
            <i class="ti ti-chevron-right" style="margin-left:auto;flex-shrink:0;"></i>
        </button>`;
    sidebar.appendChild(btn);
}

function backToViewer() {
    document.querySelector('.sidebar').classList.add('mobile-hidden');
    document.querySelector('.viewer').classList.add('mobile-active');
    document.getElementById('btnBackToViewer')?.remove();
}

window.addEventListener('resize', () => {
    if (window.innerWidth > 768) {
        document.querySelector('.sidebar')?.classList.remove('mobile-hidden');
        document.querySelector('.viewer')?.classList.remove('mobile-active');
    }
});

// ── Render viewer ──────────────────────────────────────────────
function _destroyCurrentVideo() {
    const vid = document.getElementById('mainVideo');
    if (!vid) return;
    vid.removeEventListener('ended', _onVideoEnded);
    vid.removeEventListener('click', _onVideoClick);
    vid.removeEventListener('pause', _queueSaveVideoHistory);
    vid.removeEventListener('seeked', _queueSaveVideoHistory);
    vid.removeEventListener('timeupdate', _trackVideoProgress);
    _flushVideoHistory();
    vid.pause();
    vid.removeAttribute('src');
    vid.querySelectorAll('source').forEach(s => s.remove());
    vid.load(); // reset network state, giải phóng buffer
}

function renderViewer(type, url, name) {
    _destroyCurrentVideo(); // cleanup trước khi replace DOM
    const body = document.getElementById('viewerBody');

    if (type === 'video') {
        body.innerHTML = `
        <div class="video-wrap" id="videoWrap" style="position:relative;display:flex;flex-direction:column;">
            <video id="mainVideo"
                controls
                controlsList="nodownload"
                preload="auto"
                data-video-id="${_currentVideoId ?? ''}"
                style="width:100%;aspect-ratio:16/9;max-height:65vh;display:block;background:#001830;margin:0 auto;"
                onloadedmetadata="onVideoReady(this)"
                onerror="onVideoError(this)">
                <source src="${url}" type="video/mp4">
                <source src="${url}" type="video/webm">
            </video>
            <div id="videoRipple" class="video-ripple">
                <div class="video-ripple-icon" id="videoRippleIcon"></div>
            </div>
            <div id="videoErrorMsg" style="display:none;pointer-events:none;position:absolute;
                 inset:0;background:#001830;align-items:center;justify-content:center;
                 flex-direction:column;gap:.75rem;color:rgba(255,255,255,.5);">
                <i class="ti ti-video-off" style="font-size:36px;opacity:.4;"></i>
                <span style="font-size:.85rem;">Không thể phát video. Thử tải về để xem.</span>
                <a href="${url}" download style="color:var(--c-sky);font-size:.8rem;pointer-events:all;">
                    <i class="ti ti-download"></i> Tải về
                </a>
            </div>
            <div id="fsRelatedOverlay" class="fs-related-overlay" style="display:none;"></div>
            <!-- Thanh controls custom — theater mode button -->
            <div class="video-custom-bar">
                <a class="btn-custom-fs" href="${url}" download target="_blank" style="text-decoration:none; display:flex; align-items:center; gap:6px; color:#fff; font-size: 0.85rem;">
                    <i class="ti ti-download"></i> Tải về
                </a>
                <button class="btn-custom-fs" onclick="toggleComment()" aria-label="Hỏi đáp" title="Hỏi đáp" style="display:flex; align-items:center; gap:6px; font-size: 0.85rem;">
                    <i class="ti ti-message-circle"></i> Chat
                </button>
                <button class="btn-custom-fs" onclick="_toggleFullscreen()">
                    <i class="ti ti-maximize" id="fsIcon"></i>
                    <span id="fsLabel">Phóng to</span>
                </button>
            </div>
        </div>
        <div id="relatedPanel" class="related-panel" style="display:none;"></div>`;

        // Gán listener trực tiếp — không cần rAF vì innerHTML là sync
        const vid = document.getElementById('mainVideo');
        if (vid) {
            vid.addEventListener('ended', _onVideoEnded);
            vid.addEventListener('click', _onVideoClick);
            vid.addEventListener('loadedmetadata', _restoreVideoHistory);
            vid.addEventListener('pause', _queueSaveVideoHistory);
            vid.addEventListener('seeked', _queueSaveVideoHistory);
            vid.addEventListener('timeupdate', _trackVideoProgress);
        }

    } else if (type === 'pdf') {
        body.innerHTML = `
            <div class="pdf-wrap" style="height:100%;display:flex;flex-direction:column;">
                <iframe src="${url}" title="${name}" allowfullscreen
                        style="width:100%;flex:1;min-height:0;border:none;display:block;">
                </iframe>
            </div>`;

    } else {
        body.innerHTML = `
            <div style="background:var(--c-white);border:1px solid var(--c-border);
                        border-radius:var(--radius);padding:2.5rem;text-align:center;">
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#1565C0" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" style="display:block;margin-bottom:.9rem;">
                    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                    <polyline points="14 2 14 8 20 8"/>
                    <line x1="16" y1="13" x2="8" y2="13"/>
                    <line x1="16" y1="17" x2="8" y2="17"/>
                    <line x1="10" y1="9" x2="8" y2="9"/>
                </svg>
                <div style="font-size:1rem;font-weight:600;color:var(--c-navy);margin-bottom:.4rem;">
                    ${name}
                </div>
                <p style="font-size:.83rem;color:var(--c-steel);margin-bottom:1.5rem;">
                    Tài liệu Word không thể xem trực tiếp trên trình duyệt.
                </p>
                <a href="${url}" download
                   style="display:inline-flex;align-items:center;gap:7px;padding:.7rem 1.75rem;
                          background:var(--c-navy);color:#fff;border-radius:var(--radius-sm);
                          font-size:.9rem;font-weight:500;text-decoration:none;
                          transition:background .15s;"
                   onmouseover="this.style.background='var(--c-primary)'"
                   onmouseout="this.style.background='var(--c-navy)'">
                    <i class="ti ti-download"></i> Tải về để xem
                </a>
            </div>`;
    }
}

function onVideoReady(video) {
    const err = document.getElementById('videoErrorMsg');
    if (err) err.style.display = 'none';
}

function _restoreVideoHistory() {
    const vid = document.getElementById('mainVideo');
    const currentVideoId = vid?.dataset?.videoId || _currentVideoId;
    if (!vid || !currentVideoId) return;

    fetch('/TaiLieu/GetLichSuXemVideo?idVideo=' + encodeURIComponent(currentVideoId))
        .then(r => {
            if (!r.ok) return null;
            return r.json();
        })
        .then(data => {
            if (!data || typeof data.tongGiay !== 'number' || data.tongGiay <= 0) return;
            const safeTime = Math.max(0, data.tongGiay - 1);
            if (Number.isFinite(vid.duration) && safeTime >= Math.floor(vid.duration)) return;
            vid.currentTime = safeTime;
        })
        .catch(() => { });
}

function onVideoError(video) {
    const err = document.getElementById('videoErrorMsg');
    if (err) err.style.display = 'flex';
    video.style.display = 'none';
}

function _getCurrentVideoTimeParts() {
    const vid = document.getElementById('mainVideo');
    if (!vid || !Number.isFinite(vid.currentTime)) return null;
    const total = Math.floor(vid.currentTime);
    return { phut: Math.floor(total / 60), giay: total % 60 };
}

function _queueSaveVideoHistory() {
    clearTimeout(_historySaveTimer);
    _historySaveTimer = setTimeout(_flushVideoHistory, 700);
}

function _trackVideoProgress() {
    const vid = document.getElementById('mainVideo');
    if (!vid || !Number.isFinite(vid.currentTime)) return;
    const current = Math.floor(vid.currentTime);
    if (current === _lastSavedVideoTime) return;
    if (current % 5 !== 0) return;
    _lastSavedVideoTime = current;
    _queueSaveVideoHistory();
}

function _flushVideoHistory() {
    clearTimeout(_historySaveTimer);
    _historySaveTimer = null;

    const vid = document.getElementById('mainVideo');
    const currentVideoId = vid?.dataset?.videoId || _currentVideoId;
    const timeParts = _getCurrentVideoTimeParts();
    if (!currentVideoId || !timeParts) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!token) return;

    const body = new URLSearchParams();
    body.append('idVideo', currentVideoId);
    body.append('phut', String(timeParts.phut));
    body.append('giay', String(timeParts.giay));
    body.append('__RequestVerificationToken', token);

    fetch('/TaiLieu/GhiLichSuXemVideo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: body.toString()
    }).catch(() => { });
}

function _onVideoClick(e) {
    const isPaused = e.currentTarget.paused;
    const ripple = document.getElementById('videoRipple');
    const icon = document.getElementById('videoRippleIcon');
    if (!ripple || !icon) return;
    icon.innerHTML = isPaused
        ? '<i class="ti ti-player-play-filled"></i>'
        : '<i class="ti ti-player-pause-filled"></i>';
    ripple.classList.remove('ripple-animate');
    void ripple.offsetWidth;
    ripple.classList.add('ripple-animate');
}

// ══════════════════════════════════════════════════════════════
// VIDEO LIÊN QUAN
// ══════════════════════════════════════════════════════════════
var _relatedFetchController = null;

function _loadRelatedVideos(videoId) {
    // Hủy fetch cũ nếu còn đang chạy
    if (_relatedFetchController) {
        _relatedFetchController.abort();
    }
    _relatedFetchController = new AbortController();
    const signal = _relatedFetchController.signal;

    fetch('/TaiLieu/GetVideoLienQuan?idVideo=' + videoId, { signal })
        .then(r => r.json())
        .then(data => {
            _relatedFetchController = null;
            _relatedVideos = data || [];
            _renderRelatedPanel();
            _syncRelatedOverlay();
        })
        .catch(err => {
            if (err.name === 'AbortError') return; // bỏ qua — đây là cancel có chủ đích
            _relatedVideos = [];
            _renderRelatedPanel();
        });
}

// Render panel dưới video (outside fullscreen)
function _renderRelatedPanel() {
    const panel = document.getElementById('relatedPanel');
    if (!panel) return;

    panel.style.display = '';
    const content = _relatedVideos.length > 0
        ? `<div class="related-list" id="relatedList">
               ${_relatedVideos.map((v, i) => _buildRelatedItem(v, i)).join('')}
           </div>`
        : `<div style="padding: 15px; text-align: center; color: var(--c-steel); font-size: 0.85rem; font-style: italic;">Chưa có video liên quan</div>`;

    panel.innerHTML = `
        <div class="related-header">
            <i class="ti ti-playlist"></i>
            <span>Video liên quan</span>
            <span class="related-count">${_relatedVideos.length}</span>
        </div>
        ${content}`;
}

function _buildRelatedItem(v, index) {
    return `
        <div class="related-item" data-index="${index}" onclick="_playRelated(${index})">
            <div class="related-info">
                <div class="related-name">${v.tenVideo}</div>
            </div>
        </div>`;
}

function _getTagLabel(idTag) {
    return idTag ? '#tag' : '';
}

function _hideRelatedPanel() {
    const panel = document.getElementById('relatedPanel');
    if (panel) panel.style.display = 'none';
    const overlay = document.getElementById('fsRelatedOverlay');
    if (overlay) overlay.style.display = 'none';
}

// ── Phát video liên quan ───────────────────────────────────────
function _playRelated(index) {
    const v = _relatedVideos[index];
    if (!v) return;
    _clearCountdown();
    const wasTheater = _theaterMode;
    const sidebarItem = document.querySelector(`.doc-item[data-id="${v.id}"][data-type="video"]`);
    if (sidebarItem) {
        openDoc(sidebarItem);
    } else {
        _openVideoDirectly(v);
    }
    if (wasTheater && !_theaterMode) _toggleFullscreen();
}

function _openVideoDirectly(v) {
    if (_activeItem) _activeItem.classList.remove('active');
    _activeItem = null;
    _currentVideoId = String(v.id);
    document.getElementById('navBreadcrumb').textContent = v.tenVideo;
    document.getElementById('viewerTitle').textContent = v.tenVideo;
    const streamUrl = '/TaiLieu/StreamFile?path=' + encodeURIComponent(v.duongDanFileVideo) + '&fileName=' + encodeURIComponent(v.tenVideo);
    document.getElementById('btnDownload').href = streamUrl;
    _relatedVideos = [];
    _hideRelatedPanel();
    renderViewer('video', streamUrl, v.tenVideo);
    _loadRelatedVideos(v.id);
}

function _onVideoEnded() {
    if (_relatedVideos.length === 0) return;
    _startCountdown(_relatedVideos[0], 0);
}

function _startCountdown(nextVideo, index) {
    let remaining = _COUNTDOWN_SEC;

    // Render overlay countdown
    const body = document.getElementById('viewerBody');
    const existing = document.getElementById('countdownOverlay');
    if (existing) existing.remove();

    const overlay = document.createElement('div');
    overlay.id = 'countdownOverlay';
    overlay.className = 'countdown-overlay';
    overlay.innerHTML = `
        <div class="countdown-box">
            <div class="countdown-next-label">Tiếp theo</div>
            <div class="countdown-title">${nextVideo.tenVideo}</div>
            <div class="countdown-timer">
                <svg class="countdown-ring" viewBox="0 0 48 48">
                    <circle class="ring-bg" cx="24" cy="24" r="20"/>
                    <circle class="ring-fg" id="ringFg" cx="24" cy="24" r="20"
                        stroke-dasharray="125.6"
                        stroke-dashoffset="0"/>
                </svg>
                <span id="countdownNum">${remaining}</span>
            </div>
            <div class="countdown-actions">
                <button class="countdown-play-now" onclick="_playRelated(${index})">
                    <i class="ti ti-player-play"></i> Phát ngay
                </button>
                <button class="countdown-cancel" onclick="_clearCountdown()">
                    <i class="ti ti-x"></i> Huỷ
                </button>
            </div>
        </div>`;

    // Đặt overlay lên video-wrap
    const videoWrap = body.querySelector('.video-wrap');
    if (videoWrap) videoWrap.appendChild(overlay);

    const totalDash = 125.6;
    _countdownTimer = setInterval(() => {
        remaining--;
        const numEl = document.getElementById('countdownNum');
        const ringFg = document.getElementById('ringFg');
        if (numEl) numEl.textContent = remaining;
        if (ringFg) ringFg.style.strokeDashoffset =
            String(totalDash * (1 - remaining / _COUNTDOWN_SEC));

        if (remaining <= 0) {
            _clearCountdown();
            _playRelated(index);
        }
    }, 1000);
}

function _clearCountdown() {
    if (_countdownTimer) {
        clearInterval(_countdownTimer);
        _countdownTimer = null;
    }
    const overlay = document.getElementById('countdownOverlay');
    if (overlay) overlay.remove();
}

// ── THEATER MODE ──────────────────────────────────────────────
var _theaterMode = false;
var _overlayCollapsed = false;

function _toggleFullscreen() {
    const viewer = document.querySelector('.viewer');
    if (!viewer) return;
    _theaterMode = !_theaterMode;
    viewer.classList.toggle('theater-mode', _theaterMode);
    const icon = document.getElementById('fsIcon');
    const label = document.getElementById('fsLabel');
    if (icon) icon.className = _theaterMode ? 'ti ti-minimize' : 'ti ti-maximize';
    if (label) label.textContent = _theaterMode ? 'Thu nhỏ' : 'Phóng to';
    document.body.style.overflow = _theaterMode ? 'hidden' : '';
    if (!_theaterMode && window.innerWidth <= 768) {
        document.querySelector('.sidebar')?.classList.add('mobile-hidden');
        document.querySelector('.viewer')?.classList.add('mobile-active');
    }

    _syncRelatedOverlay();
}

function _syncRelatedOverlay() {
    const overlay = document.getElementById('fsRelatedOverlay');
    if (!overlay) return;

    if (_theaterMode && _relatedVideos.length > 0) {
        overlay.style.display = 'flex';
        overlay.classList.toggle('overlay-collapsed', _overlayCollapsed);
        _renderFsOverlayContent(overlay);
    } else {
        overlay.style.display = 'none';
    }
}

function _toggleFsOverlay() {
    _overlayCollapsed = !_overlayCollapsed;
    const overlay = document.getElementById('fsRelatedOverlay');
    if (overlay) overlay.classList.toggle('overlay-collapsed', _overlayCollapsed);
    // Re-render để cập nhật icon chevron
    if (overlay) _renderFsOverlayContent(overlay);
}

function _renderFsOverlayContent(overlay) {
    overlay.innerHTML = `
        <div class="fs-related-inner">
            <div class="fs-related-title">
                <i class="ti ti-playlist"></i>
                <span>Video liên quan</span>
                <button class="fs-related-toggle" onclick="_toggleFsOverlay()" title="Ẩn/hiện">
                    <i class="ti ${_overlayCollapsed ? 'ti-chevron-left' : 'ti-chevron-right'}" id="fsToggleIcon"></i>
                </button>
            </div>
            <div class="fs-related-list">
                ${_relatedVideos.map((v, i) => `
                    <div class="fs-related-item" onclick="_playRelated(${i})">
                        <div class="fs-name">${v.tenVideo}</div>
                    </div>`).join('')}
            </div>
        </div>`;
}

// ESC thoát theater mode
document.addEventListener('keydown', e => {
    if (e.key === 'Escape' && _theaterMode) _toggleFullscreen();
});

function _onFullscreenChange() { /* unused — dùng theater mode */ }

// ══════════════════════════════════════════════════════════════
// SEARCH + FILTER DROPDOWN
// ══════════════════════════════════════════════════════════════
// Khởi tạo filter dropdown
var _filterOutsideClickHandler = null;

function initSearchFilter() {
    const wrap = document.getElementById('searchFilterWrap');
    if (!wrap) return;

    // Cleanup listener cũ nếu initSearchFilter bị gọi lại
    if (_filterOutsideClickHandler) {
        document.removeEventListener('click', _filterOutsideClickHandler);
        _filterOutsideClickHandler = null;
    }

    const options = [
        { value: 'video', label: 'Video', iconHtml: '<i class="ti ti-player-play"></i>' },
        { value: 'pdf', label: 'PDF', iconHtml: _ICON_PDF },

    ];
    wrap.innerHTML = `
        <div class="filter-dropdown" id="filterDropdown">
            <button class="filter-btn-toggle" id="filterBtnToggle"
                    onclick="toggleFilterDropdown()" type="button">
                <span id="filterIcon"><i class="ti ti-player-play"></i></span>
                <span id="filterLabel">Video</span>
                <i class="ti ti-chevron-down filter-chevron" id="filterChevron"></i>
            </button>
            <div class="filter-menu" id="filterMenu" style="display:none;">
                ${options.map(o => `
                    <div class="filter-option ${o.value === _searchFilter ? 'selected' : ''}"
                         data-value="${o.value}"
                         onclick="setSearchFilter('${o.value}')">
                        ${o.iconHtml} ${o.label}
                    </div>`).join('')}
            </div>
        </div>`;
    _filterOutsideClickHandler = e => {
        const dd = document.getElementById('filterDropdown');
        if (dd && !dd.contains(e.target)) closeFilterDropdown();
    };
    document.addEventListener('click', _filterOutsideClickHandler);
}

function toggleFilterDropdown() {
    const menu = document.getElementById('filterMenu');
    const chevron = document.getElementById('filterChevron');
    if (!menu) return;
    const isOpen = menu.style.display !== 'none';
    menu.style.display = isOpen ? 'none' : '';
    if (chevron) chevron.style.transform = isOpen ? '' : 'rotate(180deg)';
}

function closeFilterDropdown() {
    const menu = document.getElementById('filterMenu');
    const chevron = document.getElementById('filterChevron');
    if (menu) menu.style.display = 'none';
    if (chevron) chevron.style.transform = '';
}

var _FILTER_META = {
    video: { label: 'Video', iconHtml: '<i class="ti ti-player-play"></i>' },
    pdf: { label: 'PDF', iconHtml: _ICON_PDF },

};

function setSearchFilter(value) {
    _searchFilter = value;
    const meta = _FILTER_META[value] || _FILTER_META.video;
    const iconEl = document.getElementById('filterIcon');
    const labelEl = document.getElementById('filterLabel');
    if (iconEl) iconEl.innerHTML = meta.iconHtml; // innerHTML thay className
    if (labelEl) labelEl.textContent = meta.label;
    document.querySelectorAll('.filter-option').forEach(el => {
        el.classList.toggle('selected', el.dataset.value === value);
    });
    closeFilterDropdown();
    const input = document.getElementById('searchInput');
    if (input) handleSearch(input);
}

// ── Search ─────────────────────────────────────────────────────
function handleSearch(input) {
    const q = input.value.trim().toLowerCase();
    const clearBtn = document.getElementById('searchClear');
    if (clearBtn) clearBtn.classList.toggle('visible', q.length > 0);
    const defaultList = document.getElementById('defaultList');
    const searchResults = document.getElementById('searchResults');
    const searchEmpty = document.getElementById('searchEmpty');
    const hasQuery = q.length > 0;
    const hasFilter = _searchFilter !== 'all';

    function matchesItem(el) {
        if (hasFilter) {
            const t = el.dataset.type || '';
            if (_searchFilter === 'video' && t !== 'video') return false;
            if (_searchFilter === 'pdf' && t !== 'pdf') return false;
        }
        if (hasQuery) {
            const kw = (el.dataset.keyword || '') + ' ' + (el.dataset.name || '').toLowerCase() + ' ' + (el.dataset.group || '').toLowerCase();
            if (!kw.includes(q)) return false;
        }
        return true;
    }

    if (!hasQuery && !hasFilter) {
        defaultList.style.display = '';
        searchResults.style.display = 'none';
        searchEmpty.style.display = 'none';
        return;
    }

    defaultList.style.display = 'none';
    searchResults.style.display = '';

    const allItems = Array.from(document.querySelectorAll('#defaultList .doc-item'));
    const matched = allItems.filter(matchesItem);
    if (matched.length === 0) {
        searchResults.innerHTML = '';
        searchEmpty.style.display = 'block';
        return;
    }

    searchEmpty.style.display = 'none';
    searchResults.innerHTML = '';

    const products = new Map();
    const allGroups = document.querySelectorAll('#defaultList .doc-group');

    allGroups.forEach(docGroup => {
        const cnId = docGroup.id.replace('group-', '');
        const cnName = docGroup.querySelector('.group-name').textContent;
        const spId = docGroup.dataset.spId || '';
        const spName = docGroup.dataset.spName || '';
        const productKey = `${spId}__${spName}`;
        const groupItems = allItems.filter(el => el.dataset.cnId === cnId && matchesItem(el));

        if (!products.has(productKey)) {
            products.set(productKey, {
                spId,
                spName,
                groups: []
            });
        }

        products.get(productKey).groups.push({
            cnId,
            cnName,
            items: groupItems
        });
    });

    products.forEach(({ spName, groups }) => {
        if (spName) {
            const spLabel = document.createElement('div');
            spLabel.style.cssText = 'padding:6px 12px 3px;font-size:.7rem;font-weight:600;color:var(--c-steel);text-transform:uppercase;letter-spacing:.07em;';
            spLabel.textContent = spName;
            searchResults.appendChild(spLabel);
        }

        groups.forEach(({ cnName, cnId, items }) => {
            const group = document.createElement('div');
            group.className = 'doc-group';
            if (items.length > 0) {
                group.classList.add('open');
            }
            group.id = 'sr-group-container-' + cnId;
            group.innerHTML = `
             <div class="group-header" onclick="toggleGroup(${cnId}, true)">
                 <div class="group-left">
                     <i class="ti ti-chevron-right group-arrow" style="font-size: 12px;"></i>
                     <span class="group-name">${cnName}</span>
                 </div>
                 <div class="group-right">
                     <span class="group-count">${items.length}</span>
                 </div>
             </div>
             <div class="group-items" id="sr-group-${cnId}"></div>`;
            searchResults.appendChild(group);
            const itemsContainer = group.querySelector(`#sr-group-${cnId}`);

            if (items.length === 0) {
                itemsContainer.innerHTML = `<div class="doc-item-empty" style="padding: 10px 16px; color: var(--c-steel); font-size: 0.75rem; font-style: italic;">Chưa có tài liệu phù hợp</div>`;
            } else {
                items.forEach(el => {
                    const clone = el.cloneNode(true);
                    clone.classList.remove('active');
                    if (_activeItem === el) clone.classList.add('active');
                    clone.removeAttribute('onclick');
                    clone.addEventListener('click', () => {
                        document.querySelectorAll('#searchResults .doc-item').forEach(c => c.classList.remove('active'));
                        clone.classList.add('active');
                        _activeItem = el;
                        openDoc(el);
                    });
                    itemsContainer.appendChild(clone);
                });
            }
        });

        const divider = document.createElement('div');
        divider.className = 'group-divider';
        searchResults.appendChild(divider);
    });
}

function clearSearch() {
    const input = document.getElementById('searchInput');
    input.value = '';
    handleSearch(input);
    if (typeof collapseAllGroups === 'function') {
        collapseAllGroups();
    }
    input.focus();
}

// ── Comments ──────────────────────────────────────────────────
function loadComments(cnId) {
    fetch('/TaiLieu/GetHoiDap?idChucNang=' + cnId)
        .then(r => r.json())
        .then(data => {
            const body = document.getElementById('commentBody');
            const empty = document.getElementById('commentEmpty');

            if (!data || data.length === 0) {
                if (empty) empty.style.display = '';
                return;
            }
            if (empty) empty.style.display = 'none';
            body.querySelectorAll('.comment-item').forEach(e => e.remove());

            data.forEach(c => {
                const isAdmin = c.roles && c.roles.includes('ADMIN');
                const item = document.createElement('div');
                item.className = 'comment-item';
                item.innerHTML = `
                    <div class="comment-meta">
                        <div class="comment-avatar ${isAdmin ? 'admin-av' : ''}">
                            <i class="ti ${isAdmin ? 'ti-shield-check' : 'ti-user'}"></i>
                        </div>
                        <span class="comment-uname">${c.tenNguoiHoi || 'Người dùng'}</span>
                        <span class="comment-time">
                            ${new Date(c.ngayTao).toLocaleDateString('vi-VN')}
                        </span>
                    </div>
                    <div class="comment-bubble">${c.noiDung}</div>
                    ${c.hinhAnhs && c.hinhAnhs.length > 0 ? `<div class="comment-imgs">${c.hinhAnhs.map(a => `<img src="/TaiLieu/StreamFile?path=${encodeURIComponent(a.duongDanFileAnh)}&fileName=img.jpg" alt="" onclick="_openLightbox(this.src)" />`).join("")}</div>` : ""}`;

                if (c.traLois && c.traLois.length > 0) {
                    c.traLois.forEach(r => {
                        const isAdminReply = r.roles && r.roles.includes('ADMIN');
                        const reply = document.createElement('div');
                        reply.className = 'comment-item reply-item-wrap';
                        reply.innerHTML = `
                            <div class="comment-meta" style="margin-left:12px;">
                                <div class="comment-avatar ${isAdminReply ? 'admin-av' : ''}">
                                    <i class="ti ${isAdminReply ? 'ti-shield-check' : 'ti-user'}"></i>
                                </div>
                                <span class="comment-uname">${r.tenNguoiHoi || 'Admin'}</span>
                                <span class="comment-time">
                                    ${new Date(r.ngayTao).toLocaleDateString('vi-VN')}
                                </span>
                            </div>
                            <div class="comment-bubble admin-bubble">${r.noiDung}</div>
                            ${r.hinhAnhs && r.hinhAnhs.length > 0 ? `<div class="comment-imgs">${r.hinhAnhs.map(a => `<img src="/TaiLieu/StreamFile?path=${encodeURIComponent(a.duongDanFileAnh)}&fileName=img.jpg" alt="" onclick="_openLightbox(this.src)" />`).join("")}</div>` : ""}`;
                        item.appendChild(reply);
                    });
                }
                body.appendChild(item);
            });
        })
        .catch(() => { });
}

window.toggleComment = function () {
    const panel = document.getElementById('commentPanel');
    if (!panel) return;
    panel.classList.toggle('open');
    if (panel.classList.contains('open') && _currentDocId) {
        loadComments(_currentDocId);
    }
};

// ── Init ──────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    initSearchFilter();
    setSearchFilter(_searchFilter);
});

window.addEventListener('beforeunload', () => {
    _flushVideoHistory();
});
// ── Upload ảnh hỏi đáp ─────────────────────────────────────────
var _commentFiles = [];

function onCommentImgChange(input) {
    var newFiles = Array.from(input.files);
    newFiles.forEach(function (f) {
        if (!_commentFiles.find(function (x) { return x.name === f.name && x.size === f.size; })) {
            _commentFiles.push(f);
        }
    });
    input.value = '';
    _renderImgPreview();
}

function _renderImgPreview() {
    var wrap = document.getElementById('commentImgPreview');
    var badge = document.getElementById('uploadBadge');
    if (!wrap) return;

    wrap.innerHTML = '';
    _commentFiles.forEach(function (f, idx) {
        var thumb = document.createElement('div');
        thumb.className = 'preview-thumb';

        var img = document.createElement('img');
        img.src = URL.createObjectURL(f);
        img.alt = f.name;

        var btn = document.createElement('button');
        btn.className = 'btn-remove-img';
        btn.type = 'button';
        btn.innerHTML = '<i class="ti ti-x"></i>';
        btn.onclick = (function (i) {
            return function () {
                _commentFiles.splice(i, 1);
                _renderImgPreview();
            };
        })(idx);

        thumb.appendChild(img);
        thumb.appendChild(btn);
        wrap.appendChild(thumb);
    });

    if (badge) {
        if (_commentFiles.length > 0) {
            badge.textContent = _commentFiles.length;
            badge.style.display = 'flex';
        } else {
            badge.style.display = 'none';
        }
    }
}

window.submitComment = function () {
    var input = document.getElementById('commentInput');
    var text = input ? input.value.trim() : '';

    if (!text && _commentFiles.length === 0) return;
    if (!_currentDocId) return;

    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!token) return;

    var btn = document.querySelector('.btn-send');
    if (btn) btn.disabled = true;

    var fd = new FormData();
    fd.append('__RequestVerificationToken', token.value);
    fd.append('idChucNang', _currentDocId);
    fd.append('noiDung', text || '.');
    fd.append('congKhai', 'true');
    _commentFiles.forEach(function (f) {
        fd.append('hinhAnhs', f, f.name);
    });

    fetch('/TaiLieu/GuiCauHoi', {
        method: 'POST',
        body: fd
    })
        .then(function (r) {
            if (!r.ok) throw new Error('err');
            if (input) input.value = '';
            _commentFiles = [];
            _renderImgPreview();
            loadComments(_currentDocId);
        })
        .catch(function () { })
        .finally(function () {
            if (btn) btn.disabled = false;
        });
};

function _openLightbox(src) {
    var overlay = document.createElement('div');
    overlay.className = 'img-lightbox-overlay';
    overlay.onclick = function (e) {
        if (e.target === overlay) overlay.remove();
    };
    var img = document.createElement('img');
    img.src = src;
    var closeBtn = document.createElement('button');
    closeBtn.className = 'lightbox-close';
    closeBtn.innerHTML = '<i class="ti ti-x"></i>';
    closeBtn.onclick = function () { overlay.remove(); };
    overlay.appendChild(img);
    overlay.appendChild(closeBtn);
    document.body.appendChild(overlay);
}

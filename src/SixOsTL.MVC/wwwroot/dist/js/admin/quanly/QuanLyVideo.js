var _ftpFolderState = {
    currentPath: '',
    parentPath: '',
    folders: []
};

document.addEventListener('DOMContentLoaded', () => {
    initDropZone('videoDropZone', 'videoFile', 'videoFileName');
});

function openUploadModal() {
    openModal('uploadModal');
}

function openFolderPicker() {
    openModal('ftpFolderModal');
    ftpLoadFolders('');
}

function ftpGoRoot() {
    ftpLoadFolders('');
}

function ftpGoUp() {
    ftpLoadFolders(_ftpFolderState.parentPath || '');
}

function ftpSelectCurrentFolder() {
    const input = document.getElementById('remoteFolder');
    if (input) input.value = _ftpFolderState.currentPath || '';
    closeModal('ftpFolderModal');
}

function ftpClosePicker() {
    closeModal('ftpFolderModal');
}

function ftpJoinPath(base, child) {
    const left = (base || '').trim().replace(/\/+$/g, '');
    const right = (child || '').trim().replace(/^\/+|\/+$/g, '');
    return left ? `${left}/${right}` : right;
}

function ftpParentPath(path) {
    const clean = (path || '').trim().replace(/\/+$/g, '');
    if (!clean) return '';
    const idx = clean.lastIndexOf('/');
    return idx >= 0 ? clean.substring(0, idx) : '';
}

function ftpLoadFolders(path) {
    const results = document.getElementById('ftpFolderList');
    const breadcrumb = document.getElementById('ftpBreadcrumb');
    if (results) {
        results.innerHTML = '<div style="text-align:center;padding:1.25rem;color:var(--c-steel);font-size:.85rem;">Đang tải danh sách folder...</div>';
    }

    fetch('/Admin/GetFtpFolders?path=' + encodeURIComponent(path || ''))
        .then(r => r.json())
        .then(data => {
            _ftpFolderState.currentPath = data.path || '';
            _ftpFolderState.parentPath = ftpParentPath(_ftpFolderState.currentPath);
            _ftpFolderState.folders = data.folders || [];
            if (breadcrumb) {
                breadcrumb.textContent = _ftpFolderState.currentPath ? '/' + _ftpFolderState.currentPath : '/';
            }
            if (!_ftpFolderState.folders.length) {
                results.innerHTML = '<div style="text-align:center;padding:1.25rem;color:var(--c-steel);font-size:.85rem;">Không có folder con.</div>';
                return;
            }
            results.innerHTML = _ftpFolderState.folders.map(folder => `
                <button type="button" class="ftp-folder-item" style="display:flex;align-items:center;gap:10px;padding:10px 12px;border:1px solid var(--c-border);border-radius:var(--radius-sm);background:var(--c-white);text-align:left;width:100%;"
                        onclick="ftpEnterFolder('${folder.fullPath.replace(/'/g, "\\'")}')">
                    <i class="ti ti-folder" style="color:var(--c-cyan);font-size:18px;"></i>
                    <div style="flex:1;min-width:0;">
                        <div style="font-size:.85rem;font-weight:500;color:var(--c-navy);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${folder.name}</div>
                        <div style="font-size:.7rem;color:var(--c-steel);margin-top:2px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${folder.fullPath}</div>
                    </div>
                    <i class="ti ti-chevron-right" style="color:var(--c-steel);"></i>
                </button>
            `).join('');
            results.insertAdjacentHTML('beforeend', `
                <button type="button" class="ftp-folder-item" style="display:flex;align-items:center;gap:10px;padding:10px 12px;border:1px dashed var(--c-border);border-radius:var(--radius-sm);background:var(--c-ice);text-align:left;width:100%;margin-top:2px;"
                        onclick="ftpSelectCurrentFolder()">
                    <i class="ti ti-check" style="color:var(--c-success);font-size:18px;"></i>
                    <div style="flex:1;min-width:0;">
                        <div style="font-size:.85rem;font-weight:500;color:var(--c-navy);">Chọn folder hiện tại</div>
                        <div style="font-size:.7rem;color:var(--c-steel);margin-top:2px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${_ftpFolderState.currentPath ? '/' + _ftpFolderState.currentPath : '/'}</div>
                    </div>
                </button>
            `);
        })
        .catch(() => {
            if (results) results.innerHTML = '<div style="padding:1rem;color:var(--c-danger);font-size:.8rem;">Lỗi tải danh sách folder.</div>';
        });
}

function ftpEnterFolder(path) {
    ftpLoadFolders(path);
}

function ftpPickFolder(path) {
    const input = document.getElementById('remoteFolder');
    if (input) input.value = path || '';
    closeModal('ftpFolderModal');
}

function xoaVideo(id, idChucNang, remotePath, tenVideo, token) {
    showModalDanger('Xóa video <strong>' + tenVideo + '</strong>? Hành động này không thể hoàn tác.', () => {
        const body = new URLSearchParams();
        body.append('id', id);
        body.append('idChucNang', idChucNang);
        body.append('remotePath', remotePath);
        body.append('__RequestVerificationToken', token);

        fetch('/Admin/XoaVideo', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: body.toString()
        })
            .then(r => {
                if (r.ok || r.status === 302) {
                    showToast('Đã xóa video thành công!', 200);
                    setTimeout(() => location.reload(), 800);
                } else {
                    showToast('Xóa thất bại. Vui lòng thử lại.', 500);
                }
            })
            .catch(() => showToast('Lỗi kết nối.', 500));
    });
}

// ══════════════════════════════════════════════════════════════
// VIDEO LIÊN QUAN
// ══════════════════════════════════════════════════════════════
var _lqVideoId = null;
var _lqToken = null;
var _lqSearchTimer = null;
var _lqLinkedIds = new Set(); // cache ID các video đã liên kết

function openLienQuanModal(videoId, tenVideo, token) {
    _lqVideoId = videoId;
    _lqToken = token;
    _lqLinkedIds = new Set();
    document.getElementById('lqVideoName').textContent = tenVideo;
    document.getElementById('lqSearchInput').value = '';
    _lqRenderSearchPlaceholder();
    openModal('lienQuanModal');
    _lqLoadCurrent();
}

// ── Load danh sách liên quan hiện tại ─────────────────────────
function _lqLoadCurrent() {
    const list = document.getElementById('lqCurrentList');
    list.innerHTML = '<div style="text-align:center;padding:1.5rem;color:var(--c-steel);font-size:.8rem;"><span class="btn-spinner" style="margin:auto;display:block;width:18px;height:18px;"></span></div>';

    fetch('/Admin/GetVideoLienQuan?idVideo=' + _lqVideoId)
        .then(r => r.json())
        .then(data => _lqRenderCurrent(data))
        .catch(() => {
            list.innerHTML = '<div style="padding:1rem;color:var(--c-danger);font-size:.8rem;">Lỗi tải dữ liệu.</div>';
        });
}

function _lqRenderCurrent(items) {
    const list = document.getElementById('lqCurrentList');
    const countEl = document.getElementById('lqCurrentCount');
    countEl.textContent = items.length;

    // Cập nhật cache ID đã liên kết
    _lqLinkedIds = new Set(items.map(v => v.id));

    // Refresh kết quả search nếu đang hiển thị (để cập nhật trạng thái nút)
    const q = document.getElementById('lqSearchInput').value.trim();
    if (q) _lqDoSearch(q);

    if (!items.length) {
        list.innerHTML = `
            <div class="lq-empty" style="text-align:center;padding:2rem;color:var(--c-steel);font-size:.83rem;">
                <i class="ti ti-unlink" style="display:block;font-size:28px;opacity:.3;margin-bottom:.5rem;"></i>
                Chưa có video liên quan
            </div>`;
        return;
    }

    list.innerHTML = items.map(v => `
        <div class="lq-item" data-id="${v.id}" style="display:flex;align-items:center;gap:8px;padding:7px 10px;background:var(--c-ice);border:1px solid var(--c-border);border-radius:var(--radius-sm);">
            <div style="width:28px;height:28px;border-radius:6px;background:var(--c-powder);display:flex;align-items:center;justify-content:center;flex-shrink:0;">
                <i class="ti ti-player-play" style="color:var(--c-cyan);font-size:13px;"></i>
            </div>
            <div style="flex:1;min-width:0;">
                <div style="font-size:.8rem;font-weight:500;color:var(--c-navy);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${v.tenVideo}</div>
                <div style="font-size:.7rem;color:var(--c-steel);margin-top:1px;">${v.stt ?? ''}</div>
            </div>
            <button class="btn-danger btn-sm" style="flex-shrink:0;" onclick="_lqXoa(${v.lienQuanId ?? v.id}, this)" title="Gỡ liên kết">
                <i class="ti ti-x"></i>
            </button>
        </div>`).join('');
}

// ── Xóa liên kết ──────────────────────────────────────────────
function _lqXoa(lienQuanId, btn) {
    btn.disabled = true;
    const body = new URLSearchParams();
    body.append('id', lienQuanId);
    body.append('__RequestVerificationToken', _lqToken);

    fetch('/Admin/XoaVideoLienQuan', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: body.toString()
    })
        .then(r => {
            if (r.ok) {
                _lqLoadCurrent();
                showToast('Đã gỡ liên kết.', 200);
            } else {
                btn.disabled = false;
                showToast('Gỡ thất bại.', 500);
            }
        })
        .catch(() => { btn.disabled = false; showToast('Lỗi kết nối.', 500); });
}

// ── Tìm kiếm video để thêm ────────────────────────────────────
function lqSearch(q) {
    clearTimeout(_lqSearchTimer);
    if (!q.trim()) { _lqRenderSearchPlaceholder(); return; }
    _lqSearchTimer = setTimeout(() => _lqDoSearch(q.trim()), 280);
}

function _lqRenderSearchPlaceholder() {
    document.getElementById('lqSearchResults').innerHTML = `
        <div style="text-align:center;padding:1.5rem;color:var(--c-steel);font-size:.8rem;">
            <i class="ti ti-keyboard" style="display:block;font-size:22px;opacity:.3;margin-bottom:.4rem;"></i>
            Nhập tên để tìm video
        </div>`;
}

function _lqDoSearch(q) {
    const results = document.getElementById('lqSearchResults');
    results.innerHTML = '<div style="text-align:center;padding:1rem;"><span class="btn-spinner" style="display:inline-block;"></span></div>';

    fetch(`/Admin/SearchVideos?q=${encodeURIComponent(q)}&excludeId=${_lqVideoId}`)
        .then(r => r.json())
        .then(data => _lqRenderSearchResults(data))
        .catch(() => {
            results.innerHTML = '<div style="padding:.75rem;color:var(--c-danger);font-size:.8rem;">Lỗi tìm kiếm.</div>';
        });
}

function _lqRenderSearchResults(items) {
    const results = document.getElementById('lqSearchResults');
    if (!items.length) {
        results.innerHTML = '<div style="text-align:center;padding:1.5rem;color:var(--c-steel);font-size:.8rem;">Không tìm thấy video nào.</div>';
        return;
    }
    results.innerHTML = items.map(v => {
        const already = _lqLinkedIds.has(v.id);
        const btn = already
            ? `<button class="btn-cancel btn-sm" disabled style="flex-shrink:0;opacity:.7;cursor:default;">
                   <i class="ti ti-check"></i> Đã thêm
               </button>`
            : `<button class="btn-success btn-sm" style="flex-shrink:0;" onclick="_lqThem(${v.id}, '${v.tenVideo.replace(/'/g, "\\'")}', this)" title="Thêm liên kết">
                   <i class="ti ti-plus"></i> Thêm
               </button>`;
        return `
        <div style="display:flex;align-items:center;gap:8px;padding:7px 10px;border:1px solid var(--c-border);border-radius:var(--radius-sm);background:var(--c-white);">
            <div style="flex:1;min-width:0;">
                <div style="font-size:.8rem;font-weight:500;color:var(--c-navy);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${v.tenVideo}</div>
                <div style="font-size:.7rem;color:var(--c-steel);margin-top:1px;">${v.tenChucNang ?? ''}</div>
            </div>
            ${btn}
        </div>`;
    }).join('');
}

// ── Thêm liên kết ─────────────────────────────────────────────
function _lqThem(idVideoLienQuan, tenVideo, btn) {
    btn.disabled = true;
    btn.innerHTML = '<span class="btn-spinner"></span>';

    // Tính STT tiếp theo = max STT hiện tại + 1
    const currentItems = document.querySelectorAll('#lqCurrentList .lq-item');
    const nextStt = currentItems.length + 1;

    const body = new URLSearchParams();
    body.append('idVideo', _lqVideoId);
    body.append('idVideoLienQuan', idVideoLienQuan);
    body.append('stt', nextStt);
    body.append('__RequestVerificationToken', _lqToken);

    fetch('/Admin/ThemVideoLienQuan', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: body.toString()
    })
        .then(r => {
            if (r.ok) {
                btn.innerHTML = '<i class="ti ti-check"></i> Đã thêm';
                btn.className = 'btn-cancel btn-sm';
                btn.disabled = true;
                _lqLoadCurrent();
                showToast('Đã thêm "' + tenVideo + '" vào danh sách liên quan.', 200);
            } else {
                btn.disabled = false;
                btn.innerHTML = '<i class="ti ti-plus"></i> Thêm';
                showToast('Thêm thất bại. Có thể đã tồn tại.', 500);
            }
        })
        .catch(() => {
            btn.disabled = false;
            btn.innerHTML = '<i class="ti ti-plus"></i> Thêm';
            showToast('Lỗi kết nối.', 500);
        });
}
var _ftpFolderState = {
    currentPath: '',
    parentPath: '',
    folders: []
};

function toggleGroup(id) {
    const grp = document.getElementById('group-' + id);
    if (grp) grp.classList.toggle('open');
}

function expandAllGroups() {
    document.querySelectorAll('.doc-group').forEach(g => g.classList.add('open'));
}

function collapseAllGroups() {
    document.querySelectorAll('.doc-group').forEach(g => g.classList.remove('open'));
}
document.addEventListener('DOMContentLoaded', () => {
    initDropZone('fileDropZone', 'docFile', 'docFileName');
    collapseAllGroups();
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

function xoaFile(id, remotePath, tenFile, token) {
    showModalDanger(
        'Xóa tài liệu <strong>' + tenFile + '</strong>? Hành động này không thể hoàn tác.',
        () => {
            const body = new URLSearchParams();
            body.append('id', id);
            body.append('remotePath', remotePath);
            body.append('__RequestVerificationToken', token);

            fetch('/Admin/XoaFile', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: body.toString()
            })
                .then(r => {
                    if (r.ok || r.status === 302) {
                        showToast('Đã xóa tài liệu thành công!', 200);
                        setTimeout(() => location.reload(), 800);
                    } else {
                        showToast('Xóa thất bại. Vui lòng thử lại.', 500);
                    }
                })
                .catch(() => showToast('Lỗi kết nối.', 500));
        }
    );
}
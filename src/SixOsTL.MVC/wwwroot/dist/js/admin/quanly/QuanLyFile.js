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
});

function openUploadModal() {
    openModal('uploadModal');
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
var _currentDocId = null;
var _currentDocType = null;

function toggleComment() {
    const panel = document.getElementById('commentPanel');
    if (!panel) return;
    panel.classList.toggle('open');
    if (panel.classList.contains('open') && _currentDocId) loadComments(_currentDocId);
}

function submitComment() {
    const input = document.getElementById('commentInput');
    const text = input?.value.trim();
    if (!text || !_currentDocId) return;
    const body = document.getElementById('commentBody');
    const empty = document.getElementById('commentEmpty');
    if (empty) empty.style.display = 'none';
    const item = document.createElement('div');
    item.className = 'comment-item';
    item.innerHTML = `
                <div class="comment-meta">
                    <div class="comment-avatar"><i class="ti ti-user"></i></div>
                    <span class="comment-uname">@_hoTen</span>
                    <span class="comment-time">Vừa xong</span>
                </div>
                <div class="comment-bubble">${text.replace(/</g, '&lt;')}</div>`;
    body.appendChild(item);
    body.scrollTop = body.scrollHeight;
    input.value = '';
    const btn = document.querySelector('.btn-icon[onclick="toggleComment()"]');
    if (btn) btn.classList.add('has-comment');
    const token = document.querySelector('[name=__RequestVerificationToken]')?.value ?? '';
    fetch('/TaiLieu/GuiCauHoi', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `idChucNang=${_currentDocId}&noiDung=${encodeURIComponent(text)}&congKhai=true&__RequestVerificationToken=${token}`
    });
}

function loadComments(cnId) {
    fetch('/TaiLieu/GetHoiDap?idChucNang=' + cnId)
        .then(r => r.json())
        .then(data => {
            const body = document.getElementById('commentBody');
            const empty = document.getElementById('commentEmpty');
            if (!data || !data.length) { if (empty) empty.style.display = ''; return; }
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
                                <span class="comment-time">${new Date(c.ngayTao).toLocaleDateString('vi-VN')}</span>
                            </div>
                            <div class="comment-bubble ${isAdmin ? 'admin-bubble' : ''}">${c.noiDung}</div>`;
                body.appendChild(item);
            });
        }).catch(() => { });
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('select.ts-chucnang').forEach(el => {
        new TomSelect(el, {
            placeholder: el.querySelector('option[value=""]')?.textContent || '-- Chọn chức năng --',
            maxOptions: 500,
            render: {
                option: (data, escape) => `
                    <div class="ts-item-row">
                        <span class="ts-item-name">${escape(data.text.split('—')[1]?.trim() || data.text)}</span>
                        <span class="ts-item-ma">${escape(data.text.split('—')[0]?.trim())}</span>
                    </div>`,
                item: (data, escape) =>
                    `<div>${escape(data.text.split('—')[1]?.trim() || data.text)}</div>`,
            }
        });
    });
    document.querySelectorAll('select.ts-basic').forEach(el => {
        new TomSelect(el, {
            maxOptions: 50,
            controlInput: null, 
        });
    });
    document.addEventListener('click', e => {
        if (e.target.classList.contains('modal-overlay')) {
            e.target.classList.remove('open');
        }
    });
});
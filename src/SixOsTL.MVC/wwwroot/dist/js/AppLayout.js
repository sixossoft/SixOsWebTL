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
    if (!text || !_currentDocId) {
        console.warn('submitComment: thiếu text hoặc _currentDocId');
        return;
    }
    
    const body = document.getElementById('commentBody');
    const empty = document.getElementById('commentEmpty');
    if (empty) empty.style.display = 'none';
    
    // Lấy danh sách ảnh từ CommentImageHandler
    const images = typeof getSelectedImages === 'function' ? getSelectedImages() : [];
    console.log('submitComment: ảnh đã chọn:', images.length);
    
    // Tạo HTML cho ảnh preview
    let imagesHtml = '';
    if (images.length > 0) {
        imagesHtml = '<div class="comment-images">' +
            images.map(img => `<div class="comment-img-item"><img src="${img.dataUrl}" alt="Ảnh" /></div>`).join('') +
            '</div>';
    }
    
    const item = document.createElement('div');
    item.className = 'comment-item';
    item.innerHTML = `
                <div class="comment-meta">
                    <div class="comment-avatar"><i class="ti ti-user"></i></div>
                    <span class="comment-uname">@_hoTen</span>
                    <span class="comment-time">Vừa xong</span>
                </div>
                <div class="comment-bubble">
                    ${text.replace(/</g, '&lt;')}
                    ${imagesHtml}
                </div>`;
    body.appendChild(item);
    body.scrollTop = body.scrollHeight;
    input.value = '';
    
    // Gửi dữ liệu lên server
    const token = document.querySelector('[name=__RequestVerificationToken]')?.value ?? '';
    if (!token) {
        console.warn('submitComment: không tìm được token');
    }
    
    const formData = new FormData();
    formData.append('idChucNang', _currentDocId);
    formData.append('noiDung', text);
    formData.append('congKhai', 'true');
    formData.append('__RequestVerificationToken', token);
    
    // Thêm ảnh vào FormData
    images.forEach((img) => {
        if (img.file) {
            formData.append('images', img.file, img.name);
        }
    });
    
    console.log('submitComment: gửi request tới /TaiLieu/GuiCauHoi');
    
    fetch('/TaiLieu/GuiCauHoi', {
        method: 'POST',
        body: formData
    }).then(response => {
        console.log('submitComment: response status:', response.status);
        if (response.ok) {
            console.log('submitComment: thành công');
            // Xóa ảnh đã chọn
            if (typeof clearSelectedImages === 'function') {
                clearSelectedImages();
            }
        } else {
            console.error('submitComment: response không ok');
            return response.text().then(text => console.error('Response:', text));
        }
    }).catch(error => {
        console.error('submitComment: lỗi fetch:', error);
    });
    
    const btn = document.querySelector('.btn-icon[onclick="toggleComment()"]');
    if (btn) btn.classList.add('has-comment');
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
                
                // Xử lý hiển thị ảnh
                let imagesHtml = '';
                if (c.danhSachAnhs && c.danhSachAnhs.length > 0) {
                    imagesHtml = '<div class="comment-images">' +
                        c.danhSachAnhs.map(imgUrl => 
                            `<div class="comment-img-item" onclick="window.open('${imgUrl}', '_blank')">
                                <img src="${imgUrl}" alt="Ảnh đính kèm" />
                            </div>`
                        ).join('') +
                        '</div>';
                }
                
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
                            <div class="comment-bubble ${isAdmin ? 'admin-bubble' : ''}">
                                ${c.noiDung}
                                ${imagesHtml}
                            </div>`;
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
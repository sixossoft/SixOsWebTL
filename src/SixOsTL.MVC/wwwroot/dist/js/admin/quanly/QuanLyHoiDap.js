function toggleReply(id) {
    const box = document.getElementById('replyBox-' + id);
    box.classList.toggle('open');
    if (box.classList.contains('open')) {
        box.querySelector('textarea').focus();
    }
}

function initHoiDapImageUpload() {
    document.querySelectorAll('[data-upload-area]').forEach(area => {
        const id = area.getAttribute('data-upload-area');
        const picker = area.querySelector('.image-picker');
        const pickBtn = area.querySelector('[data-pick-image]');
        const previewList = area.querySelector('[data-preview-list]');
        const form = area.closest('form');
        const dt = new DataTransfer();

        pickBtn?.addEventListener('click', () => picker.click());

        const render = () => {
            previewList.innerHTML = '';
            [...dt.files].forEach((file, index) => {
                const url = URL.createObjectURL(file);
                const item = document.createElement('div');
                item.className = 'img-preview-item';
                item.innerHTML = `
                    <img src="${url}" alt="preview" />
                    <button type="button" class="img-remove" aria-label="Xóa ảnh">×</button>
                `;
                item.querySelector('.img-remove').addEventListener('click', () => {
                    const next = new DataTransfer();
                    [...dt.files].forEach((f, i) => { if (i !== index) next.items.add(f); });
                    picker.files = next.files;
                    dt.items.clear();
                    [...next.files].forEach(f => dt.items.add(f));
                    render();
                });
                previewList.appendChild(item);
                setTimeout(() => URL.revokeObjectURL(url), 1000);
            });
        };

        pickBtn?.addEventListener('click', () => picker.click());
        picker?.addEventListener('change', () => {
            [...picker.files].forEach(f => dt.items.add(f));
            picker.files = dt.files;
            render();
        });

        form?.addEventListener('submit', () => {
            picker.files = dt.files;
        });
    });
}

document.addEventListener('DOMContentLoaded', initHoiDapImageUpload);

function filterHoiDap(status, btn) {
    document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    document.querySelectorAll('.hoidap-card').forEach(card => {
        card.style.display = (status === 'all' || card.dataset.status === status) ? '' : 'none';
    });
}

function searchHoiDap(q) {
    q = q.toLowerCase();
    document.querySelectorAll('.hoidap-card').forEach(card => {
        card.style.display = (!q || card.dataset.content.includes(q)) ? '' : 'none';
    });
}

function anHoiDap(id, idChucNang, token) {
    showModalWarningYN(
        'Ẩn câu hỏi này khỏi danh sách hiển thị với người dùng?',
        () => {
            const body = new URLSearchParams();
            body.append('id', id);
            body.append('idChucNang', idChucNang);
            body.append('__RequestVerificationToken', token);

            fetch('/Admin/ToggleHoiDap', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: body.toString()
            })
                .then(r => {
                    if (r.ok || r.status === 302) {
                        showToast('Đã cập nhật trạng thái!', 200);
                        setTimeout(() => location.reload(), 800);
                    } else {
                        showToast('Thao tác thất bại.', 500);
                    }
                })
                .catch(() => showToast('Lỗi kết nối.', 500));
        }
    );
}
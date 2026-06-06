function toggleReply(id) {
    const box = document.getElementById('replyBox-' + id);
    box.classList.toggle('open');
    if (box.classList.contains('open')) {
        box.querySelector('textarea').focus();
    }
}

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
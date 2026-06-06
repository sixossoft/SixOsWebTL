function submitChucNang() {
    const form = document.getElementById('createModal').querySelector('form');
    if (!form.reportValidity()) return;
    const btn = document.getElementById('btnSaveCn');
    spinnerBtn(btn);
    const id = document.getElementById('cnId').value;
    const isEdit = id !== '0';
    const action = isEdit ? '/Admin/SuaChucNang' : '/Admin/TaoChucNang';
    fetch(action, {
        method: 'POST',
        body: new FormData(form)
    })
        .then(r => {
            if (r.ok) {
                showToast(isEdit ? 'Đã cập nhật chức năng!' : 'Tạo chức năng thành công!', 200);
                setTimeout(() => location.reload(), 800);
            } else {
                showToast('Lưu thất bại. Vui lòng thử lại.', 500);
                showBtn(btn, '<i class="ti ti-check"></i> Lưu');
            }
        })
        .catch(() => {
            showToast('Lỗi kết nối.', 500);
            showBtn(btn, '<i class="ti ti-check"></i> Lưu');
        });
}

function openEditCn(id) {
    fetch('/Admin/GetChucNang?id=' + id)
        .then(r => r.json())
        .then(data => {
            const form = document.getElementById('createModal').querySelector('form');
            form.querySelector('[name="id"]').value = data.id;
            form.querySelector('[name="chucNang"]').value = data.chucNang ?? '';
            form.querySelector('[name="duongDanFile"]').value = data.duongDanFile ?? '';
            const selSanPham = form.querySelector('[name="idSanPham"]');
            const selMucDo = form.querySelector('[name="idMucDoUuTien"]');
            if (selSanPham?.tomselect) {
                selSanPham.tomselect.setValue(data.idSanPham);
            } else if (selSanPham) {
                selSanPham.value = data.idSanPham;
            }
            if (selMucDo?.tomselect) {
                selMucDo.tomselect.setValue(data.idMucDoUuTien ?? '');
            } else if (selMucDo) {
                selMucDo.value = data.idMucDoUuTien ?? '';
            }
            document.getElementById('cnModalTitle').textContent = 'Sửa Chức năng';
            openModal('createModal');
        })
        .catch(err => {
            console.error('openEditCn error:', err);
            showToast('Không tải được dữ liệu.', 500);
        });
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('createModal')
        .addEventListener('click', e => {
            if (e.target.id === 'createModal') resetCnModal();
        });
});

function resetCnModal() {
    const form = document.getElementById('createModal').querySelector('form');
    form.reset();
    document.getElementById('cnId').value = '0';
    document.getElementById('cnModalTitle').textContent = 'Thêm Chức năng mới';
}

function xoaChucNang(id, tenChucNang, token) {
    showModalDanger(
        'Xóa chức năng <strong>' + tenChucNang + '</strong>?<br>' +
        '<span style="font-size:.8rem;color:var(--c-steel)">Tất cả video và tài liệu liên quan sẽ bị ẩn.</span>',
        () => {
            const body = new URLSearchParams();
            body.append('id', id);
            body.append('__RequestVerificationToken', token);

            fetch('/Admin/XoaChucNang', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: body.toString()
            })
                .then(r => {
                    if (r.ok || r.status === 302) {
                        showToast('Đã xóa chức năng!', 200);
                        setTimeout(() => location.reload(), 800);
                    } else {
                        showToast('Xóa thất bại. Vui lòng thử lại.', 500);
                    }
                })
                .catch(() => showToast('Lỗi kết nối.', 500));
        }
    );
}
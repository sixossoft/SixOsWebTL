function submitUser() {
    const form = document.getElementById('createModal').querySelector('form');
    const id = document.getElementById('userId').value;
    const isEdit = id !== '0';
    const matKhauInput = document.getElementById('inputMatKhau');
    if (isEdit) matKhauInput.removeAttribute('required');

    if (!form.reportValidity()) {
        if (isEdit) matKhauInput.setAttribute('required', '');
        return;
    }
    const btn = document.getElementById('btnSaveUser');
    spinnerBtn(btn);
    const action = isEdit ? '/Admin/SuaTaiKhoan' : '/Admin/TaoTaiKhoan';
    fetch(action, { method: 'POST', body: new FormData(form) })
        .then(r => {
            if (r.ok) {
                showToast(isEdit ? 'Đã cập nhật tài khoản!' : 'Tạo tài khoản thành công!', 200);
                setTimeout(() => location.reload(), 800);
            } else {
                showToast('Lưu thất bại.', 500);
                showBtn(btn, '<i class="ti ti-check"></i> ' + (isEdit ? 'Lưu' : 'Tạo tài khoản'));
            }
        })
        .catch(() => {
            showToast('Lỗi kết nối.', 500);
            showBtn(btn, '<i class="ti ti-check"></i> ' + (isEdit ? 'Lưu' : 'Tạo tài khoản'));
        });
}

function openEditUser(id) {
    fetch('/Admin/GetTaiKhoan?id=' + id)
        .then(r => r.json())
        .then(data => {
            const f = document.getElementById('createModal').querySelector('form');
            f.querySelector('[name="id"]').value = data.id;
            f.querySelector('[name="tenTK"]').value = data.tenTK;
            f.querySelector('[name="hoTen"]').value = data.hoTen ?? '';
            f.querySelector('[name="maCSKCB"]').value = data.maCSKCB ?? '';
            f.querySelector('[name="soDienThoai"]').value = data.soDienThoai ?? '';
            f.querySelector('[name="email"]').value = data.email ?? '';
            f.querySelector('[name="ngayBatDau"]').value = data.ngayBatDau ?? '';
            f.querySelector('[name="ngayKetThuc"]').value = data.ngayKetThuc ?? '';
            f.querySelector('[name="maVaiTros"]').value = data.maVaiTro ?? 'USER';
            const matKhauInput = document.getElementById('inputMatKhau');
            matKhauInput.value = '';
            matKhauInput.removeAttribute('required');
            document.getElementById('matKhauHint').style.display = '';
            document.getElementById('userModalTitle').textContent = 'Sửa tài khoản';
            document.getElementById('btnSaveUserText').textContent = 'Lưu';
            openModal('createModal');
        })
        .catch(err => {
            console.error('openEditUser error:', err);
            console.error('message:', err.message);
            console.error('stack:', err.stack);
            showToast('Không tải được dữ liệu.', 500);
        });
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('createModal')
        .addEventListener('click', e => {
            if (e.target.id === 'createModal') resetUserModal();
        });
});

function resetUserModal() {
    document.getElementById('createModal').querySelector('form').reset();
    document.getElementById('userId').value = '0';
    document.getElementById('userModalTitle').textContent = 'Tạo tài khoản mới';
    document.getElementById('btnSaveUserText').textContent = 'Tạo tài khoản';
    document.getElementById('inputMatKhau').setAttribute('required', '');
    document.getElementById('matKhauHint').style.display = 'none';
}

function xoaTaiKhoan(id, tenTK, token) {
    showModalDanger(
        'Xóa tài khoản <strong>' + tenTK + '</strong>? Hành động này không thể hoàn tác.',
        () => {
            const body = new URLSearchParams();
            body.append('id', id);
            body.append('__RequestVerificationToken', token);

            fetch('/Admin/XoaTaiKhoan', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: body.toString()
            })
                .then(r => {
                    if (r.ok || r.status === 302) {
                        showToast('Đã xóa tài khoản!', 200);
                        setTimeout(() => location.reload(), 800);
                    } else {
                        showToast('Xóa thất bại. Vui lòng thử lại.', 500);
                    }
                })
                .catch(() => showToast('Lỗi kết nối.', 500));
        }
    );
}
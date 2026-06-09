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
            
            // Load danh sách chức năng đã chọn
            selectedChucNangs = [];
            if (data.chucNangs && data.chucNangs.length > 0) {
                selectedChucNangs = data.chucNangs.map(cn => ({
                    id: cn.id,
                    tenSanPham: cn.tenSanPham,
                    tenChucNang: cn.tenChucNang
                }));
            }
            updateSelectedChucNangUI();
            
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

// Quản lý danh sách chức năng đã chọn
let selectedChucNangs = [];

function themChucNang() {
    const select = document.getElementById('chucNangSelect');
    const selectedOption = select.options[select.selectedIndex];
    
    if (!selectedOption.value) {
        showToast('Vui lòng chọn chức năng', 400);
        return;
    }
    
    const id = parseInt(selectedOption.value);
    const tenSanPham = selectedOption.getAttribute('data-sanpham');
    const tenChucNang = selectedOption.text;
    
    // Kiểm tra đã tồn tại chưa
    if (selectedChucNangs.some(cn => cn.id === id)) {
        showToast('Chức năng này đã được chọn', 400);
        return;
    }
    
    // Thêm vào danh sách
    selectedChucNangs.push({ id, tenSanPham, tenChucNang });
    updateSelectedChucNangUI();
    
    // Reset dropdown
    select.selectedIndex = 0;
    showToast('Đã thêm chức năng', 200);
}

function xoaChucNang(id) {
    selectedChucNangs = selectedChucNangs.filter(cn => cn.id !== id);
    updateSelectedChucNangUI();
    showToast('Đã xóa chức năng', 200);
}

function updateSelectedChucNangUI() {
    const container = document.getElementById('selectedChucNangList');
    const countSpan = document.getElementById('selectedCount');
    const hiddenInput = document.getElementById('selectedChucNangIds');
    
    countSpan.textContent = selectedChucNangs.length;
    hiddenInput.value = selectedChucNangs.map(cn => cn.id).join(',');
    
    if (selectedChucNangs.length === 0) {
        container.innerHTML = `
            <div class="empty-state" style="text-align:center;padding:2rem;color:var(--c-steel);font-size:.8rem;">
                <i class="ti ti-lock-open" style="display:block;font-size:24px;margin-bottom:8px;opacity:.3;"></i>
                Chưa chọn chức năng nào
            </div>
        `;
    } else {
        container.innerHTML = selectedChucNangs.map(cn => `
            <div style="display:flex;align-items:center;justify-content:space-between;padding:8px;background:var(--c-white);border:1px solid var(--c-border);border-radius:var(--radius-sm);margin-bottom:6px;">
                <div style="flex:1;min-width:0;">
                    <div style="font-size:.82rem;font-weight:500;color:var(--c-navy);">${cn.tenChucNang}</div>
                </div>
                <button type="button" class="btn-danger btn-sm" onclick="xoaChucNang(${cn.id})" style="padding:4px 8px;margin-left:8px;">
                    <i class="ti ti-x"></i>
                </button>
            </div>
        `).join('');
    }
}

function resetUserModal() {
    document.getElementById('createModal').querySelector('form').reset();
    document.getElementById('userId').value = '0';
    document.getElementById('userModalTitle').textContent = 'Tạo tài khoản mới';
    document.getElementById('btnSaveUserText').textContent = 'Tạo tài khoản';
    document.getElementById('inputMatKhau').setAttribute('required', '');
    document.getElementById('matKhauHint').style.display = 'none';
    
    // Reset danh sách chức năng
    selectedChucNangs = [];
    updateSelectedChucNangUI();
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
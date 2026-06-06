// ── PAGE LOADER ──────────────────────────────────────────
function showPageLoader() {
    document.getElementById('page-loader-body')?.classList.add('show');

}
function hidePageLoader() {
    document.getElementById('page-loader-body')?.classList.remove('show');
}

// ── TOAST ─────────────────────────────────────────────────
function showToast(message, statusCode) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const isSuccess = statusCode === 200;
    const isWarn = statusCode === 400;

    const typeClass = isSuccess ? 'toast-success' : isWarn ? 'toast-warn' : 'toast-danger';
    const icon = isSuccess
        ? `<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
               fill="none" stroke="currentColor" stroke-width="2.5"
               stroke-linecap="round" stroke-linejoin="round">
               <path d="M5 12l5 5l10-10"/>
           </svg>`
        : `<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
               fill="none" stroke="currentColor" stroke-width="2.5"
               stroke-linecap="round" stroke-linejoin="round">
               <circle cx="12" cy="12" r="9"/>
               <line x1="12" y1="8" x2="12" y2="12"/>
               <line x1="12" y1="16" x2="12.01" y2="16"/>
           </svg>`;

    const toast = document.createElement('div');
    toast.className = `app-toast ${typeClass}`;
    toast.innerHTML = `
        <span class="toast-icon">${icon}</span>
        <span class="toast-msg">${message}</span>
        <button class="toast-close" aria-label="Đóng">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
                 stroke="currentColor" stroke-width="2.5"
                 stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="6" x2="6" y2="18"/>
                <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
        </button>`;

    container.appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('show'));
    const dismiss = () => {
        toast.classList.remove('show');
        toast.addEventListener('transitionend', () => toast.remove(), { once: true });
    };

    toast.querySelector('.toast-close').addEventListener('click', dismiss);
    setTimeout(dismiss, isSuccess ? 1250 : 2500);
}

// ── PROGRESS ──────────────────────────────────────────────
function showProgress() {
    const el = document.getElementById('progress');
    if (el) el.style.display = '';
}
function hideProgress() {
    const el = document.getElementById('progress');
    if (el) el.style.display = 'none';
}
function showProgressInTable(tableEl) {
    const bar = document.createElement('div');
    bar.className = 'progress progress-inline';
    bar.innerHTML = '<div class="progress-bar-indeterminate"></div>';
    tableEl.prepend(bar);
}
function hideProgressInTable(tableEl) {
    tableEl.querySelector('.progress-inline')?.remove();
}

// ── DATEPICKER ────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    if (typeof flatpickr !== 'undefined') {
        flatpickr('.input-date', {
            locale: 'vn',
            dateFormat: 'Y-m-d',
            altInput: true,
            altFormat: 'd-m-Y',
            allowInput: true,
            disableMobile: true,
            onReady(_, __, fp) {
                fp.altInput.placeholder = 'dd-mm-yyyy';
                fp.altInput.className = fp.element.className;
            }
        });
        flatpickr('.input-date', {
            locale: 'vn',
            dateFormat: 'Y-m-d',
            altInput: true,
            altFormat: 'd-m-Y',
            allowInput: true,
            defaultDate: 'today',
            disableMobile: true,
        });
    }
});

// ── MODAL ─────────────────────────────────────────────────
function openModal(id) {
    const overlay = document.getElementById(id);
    if (!overlay) return;
    overlay.classList.add('open');
    overlay._outsideHandler = function (e) {
        if (e.target !== overlay) return;
        const openPicker = document.querySelector('.flatpickr-calendar.open');
        if (openPicker) return;
        closeModal(id);
    };
    overlay.addEventListener('click', overlay._outsideHandler);
    overlay._keyHandler = function (e) {
        if (e.key === 'Escape') closeModal(id);
    };
    document.addEventListener('keydown', overlay._keyHandler);
}
function closeModal(id) {
    const overlay = document.getElementById(id);
    if (!overlay) return;
    overlay.classList.remove('open');
    if (overlay._outsideHandler) {
        overlay.removeEventListener('click', overlay._outsideHandler);
        delete overlay._outsideHandler;
    }
    if (overlay._keyHandler) {
        document.removeEventListener('keydown', overlay._keyHandler);
        delete overlay._keyHandler;
    }
}
function showModalDanger(content, onConfirm) {
    document.getElementById('modal-danger-content').innerHTML = content;
    const btn = document.getElementById('btnDanger');
    // Reset listener cũ trước khi gán mới (tránh chồng event)
    const fresh = btn.cloneNode(true);
    btn.parentNode.replaceChild(fresh, btn);
    if (typeof onConfirm === 'function') {
        fresh.addEventListener('click', function () {
            closeModal('modal-danger');
            onConfirm();
        }, { once: true });
    }
    openModal('modal-danger');
}
function showModalWarning(content) {
    document.getElementById('modal-warning-content').innerHTML = content;
    openModal('modal-warning');
}
function showModalSuccess(content) {
    document.getElementById('modal-success-content').innerHTML = content;
    openModal('modal-success');
}
function showModalWarningYN(content, onConfirm) {
    document.getElementById('modal-warningYN-content').innerHTML = content;
    const btn = document.getElementById('btnWarningYN');
    const fresh = btn.cloneNode(true);
    btn.parentNode.replaceChild(fresh, btn);
    if (typeof onConfirm === 'function') {
        fresh.addEventListener('click', function () {
            closeModal('modal-warningYN');
            onConfirm();
        }, { once: true });
    }
    openModal('modal-warningYN');
}

// ── BUTTON LOADING STATE ──────────────────────────────────
function spinnerBtn(btn) {
    btn.dataset.originalHtml = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<span class="btn-spinner" role="status" aria-hidden="true"></span>';
}
function showBtn(btn, text) {
    btn.disabled = false;
    btn.innerHTML = text ?? btn.dataset.originalHtml ?? '';
    delete btn.dataset.originalHtml;
}

// ── TABLE FILTER ──────────────────────────────────────────
function filterTable(input, tableId) { // dùng chung cho mọi table có search
    const q = input.value.toLowerCase();
    document.querySelectorAll('#' + tableId + ' tbody tr').forEach(row => {
        row.style.display = row.textContent.toLowerCase().includes(q) ? '' : 'none';
    });
}

// ── FILE DROP / SELECT ────────────────────────────────────
function onFileSelected(input, labelId, zoneId) {
    const f = input.files[0];
    if (!f) return;
    document.getElementById(labelId).textContent = f.name + ' (' + (f.size / 1024 / 1024).toFixed(1) + ' MB)';
    document.getElementById(zoneId).style.borderColor = 'var(--c-cyan)';
}
function initDropZone(zoneId, inputId, labelId) {
    const zone = document.getElementById(zoneId);
    if (!zone) return;
    zone.addEventListener('dragover', e => { e.preventDefault(); zone.classList.add('dragover'); });
    zone.addEventListener('dragleave', () => zone.classList.remove('dragover'));
    zone.addEventListener('drop', e => {
        e.preventDefault();
        zone.classList.remove('dragover');
        const f = e.dataTransfer.files[0];
        if (!f) return;
        const input = document.getElementById(inputId);
        const dt = new DataTransfer();
        dt.items.add(f);
        input.files = dt.files;
        onFileSelected(input, labelId, zoneId);
    });
}
function submitUpload(formId, progressId, fillId, labelId, submitBtnId) {
    const form = document.getElementById(formId);
    if (!form.reportValidity()) {
        showToast("Vui lòng nhập đầy đủ thông tin!", 400);
        return;
    };
    const btn = submitBtnId ? document.getElementById(submitBtnId) : null;
    if (btn) spinnerBtn(btn);
    const prog = document.getElementById(progressId);
    const fill = document.getElementById(fillId);
    const lbl = document.getElementById(labelId);
    prog.style.display = 'block';
    fill.style.width = '0';
    fill.style.background = '';
    const xhr = new XMLHttpRequest();
    xhr.open('POST', form.action);
    xhr.upload.onprogress = e => {
        if (e.lengthComputable) {
            const pct = Math.round(e.loaded / e.total * 100);
            fill.style.width = pct + '%';
            lbl.textContent = 'Đang upload... ' + pct + '%';
        }
    };
    xhr.onload = () => {
        if (xhr.status === 200 || xhr.status === 302) {
            lbl.textContent = 'Upload thành công! Đang tải lại...';
            fill.style.background = 'var(--c-success)';
            fill.style.width = '100%';
            setTimeout(() => location.reload(), 900);
        } else {
            lbl.textContent = 'Upload thất bại: ' + (xhr.responseText || xhr.status);
            fill.style.background = 'var(--c-danger)';
            if (btn) showBtn(btn);
        }
    };
    xhr.onerror = () => {
        lbl.textContent = 'Lỗi kết nối. Vui lòng thử lại.';
        fill.style.background = 'var(--c-danger)';
        if (btn) showBtn(btn);
    };
    xhr.send(new FormData(form));
}
function fetchDelete(url, token, onSuccess) {
    const body = new URLSearchParams();
    body.append('__RequestVerificationToken', token);
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: body.toString()
    })
        .then(r => {
            if (r.ok || r.status === 302) {
                showToast('Xóa thành công!', 200);
                setTimeout(() => {
                    if (typeof onSuccess === 'function') onSuccess();
                    else location.reload();
                }, 800);
            } else {
                showToast('Xóa thất bại. Vui lòng thử lại.', 500);
            }
        })
        .catch(() => showToast('Lỗi kết nối.', 500));
}

// ── PAGE TRANSITION LOADER ────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    hidePageLoader();
    document.addEventListener('click', e => {
        const a = e.target.closest('a[href]');
        if (!a) return;
        const href = a.getAttribute('href');
        if (!href || href.startsWith('#') || href.startsWith('javascript')) return;
        if (a.target === '_blank') return;
        showPageLoader();
    });
});
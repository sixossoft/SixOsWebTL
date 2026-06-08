/**
 * CommentImageHandler.js
 * Xử lý upload và preview ảnh trong phần hỏi đáp
 */

let selectedImages = [];

/**
 * Xử lý khi người dùng chọn ảnh
 * @param {Event} event 
 */
function handleImageSelect(event) {
    try {
        const files = Array.from(event.target.files);
        
        if (files.length === 0) return;

        // Giới hạn số ảnh tối đa
        const maxImages = 5;
        const availableSlots = maxImages - selectedImages.length;

        if (files.length > availableSlots) {
            alert(`Bạn chỉ có thể đính kèm tối đa ${maxImages} ảnh`);
            return;
        }

        // Kiểm tra kích thước file (mỗi file tối đa 5MB)
        const maxSize = 5 * 1024 * 1024; // 5MB
        for (const file of files) {
            if (file.size > maxSize) {
                alert(`Ảnh "${file.name}" vượt quá 5MB`);
                return;
            }
            
            if (!file.type.startsWith('image/')) {
                alert(`File "${file.name}" không phải là ảnh`);
                return;
            }
        }

        // Thêm ảnh vào danh sách
        files.forEach(file => {
            const reader = new FileReader();
            reader.onload = (e) => {
                selectedImages.push({
                    file: file,
                    dataUrl: e.target.result,
                    name: file.name
                });
                renderImagePreview();
            };
            reader.readAsDataURL(file);
        });

        // Reset input để có thể chọn lại file giống
        event.target.value = '';
    } catch (error) {
        console.error('handleImageSelect error:', error);
        alert('Lỗi: ' + error.message);
    }
}

/**
 * Render preview ảnh
 */
function renderImagePreview() {
    const previewContainer = document.getElementById('commentPreview');
    
    if (!previewContainer) return;

    if (selectedImages.length === 0) {
        previewContainer.style.display = 'none';
        previewContainer.innerHTML = '';
        return;
    }

    previewContainer.style.display = 'flex';
    previewContainer.innerHTML = selectedImages.map((img, index) => `
        <div class="preview-item">
            <img src="${img.dataUrl}" alt="${img.name}" class="preview-img" />
            <button class="preview-remove" onclick="removeImage(${index})" aria-label="Xóa ảnh" type="button">
                <i class="ti ti-x"></i>
            </button>
        </div>
    `).join('');
}

/**
 * Xóa ảnh khỏi danh sách
 * @param {Number} index 
 */
function removeImage(index) {
    selectedImages.splice(index, 1);
    renderImagePreview();
}

/**
 * Lấy danh sách ảnh đã chọn
 * @returns {Array}
 */
function getSelectedImages() {
    return selectedImages;
}

/**
 * Reset danh sách ảnh
 */
function clearSelectedImages() {
    selectedImages = [];
    renderImagePreview();
}

/**
 * Tạo FormData với ảnh
 * @param {String} noiDung - Nội dung câu hỏi
 * @param {Object} options - Các tùy chọn khác
 * @returns {FormData}
 */
function createCommentFormData(noiDung, options = {}) {
    const formData = new FormData();
    
    formData.append('noiDung', noiDung);
    formData.append('idChucNang', options.idChucNang || '');
    formData.append('congKhai', options.congKhai !== undefined ? options.congKhai : true);
    
    if (options.parentId) {
        formData.append('parentId', options.parentId);
    }

    // Thêm ảnh vào FormData
    selectedImages.forEach((img, index) => {
        formData.append('images', img.file, img.name);
    });

    // Thêm anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    return formData;
}

/**
 * Render ảnh trong comment bubble
 * @param {Array} imageUrls - Danh sách URL ảnh
 * @returns {String} HTML
 */
function renderCommentImages(imageUrls) {
    if (!imageUrls || imageUrls.length === 0) return '';

    return `
        <div class="comment-images">
            ${imageUrls.map(url => `
                <div class="comment-img-item" onclick="openImageModal('${url}')">
                    <img src="${url}" alt="Ảnh đính kèm" />
                </div>
            `).join('')}
        </div>
    `;
}

/**
 * Mở modal xem ảnh (nếu có)
 * @param {String} imageUrl 
 */
function openImageModal(imageUrl) {
    // Có thể implement lightbox hoặc modal để xem ảnh full size
    window.open(imageUrl, '_blank');
}

/**
 * Upload ảnh lên server (nếu cần upload riêng)
 * @param {File} file 
 * @returns {Promise<String>} URL ảnh sau khi upload
 */
async function uploadImage(file) {
    const formData = new FormData();
    formData.append('image', file);

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    try {
        const response = await fetch('/api/upload-image', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (!response.ok) {
            throw new Error(`Upload failed: ${response.status}`);
        }

        const result = await response.json();
        return result.url;
    } catch (error) {
        console.error('Lỗi khi upload ảnh:', error);
        throw error;
    }
}

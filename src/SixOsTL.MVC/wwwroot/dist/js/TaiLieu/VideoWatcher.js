/**
 * VideoWatcher.js
 * Ghi lại thời gian xem video và gửi lên server
 */

class VideoWatcher {
    constructor(videoElement, idVideo, updateIntervalMs = 5000) {
        this.video = videoElement;
        this.idVideo = idVideo;
        this.updateIntervalMs = updateIntervalMs;
        this.lastReportedTime = { phut: 0, giay: 0 };
        this.timerInterval = null;
        this.antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        
        this.init();
    }

    init() {
        if (!this.video) return;

        // Lắng nghe sự kiện pause để ghi lịch sử
        this.video.addEventListener('pause', () => this.recordWatchTime());
        this.video.addEventListener('ended', () => this.recordWatchTime());
        
        // Ghi lịch sử mỗi khoảng thời gian
        if (this.timerInterval) clearInterval(this.timerInterval);
        this.timerInterval = setInterval(() => this.recordWatchTime(), this.updateIntervalMs);
    }

    /**
     * Tính toán thời gian xem hiện tại
     * @returns {Object} { phut, giay, tongGiay }
     */
    getCurrentWatchTime() {
        const totalSeconds = Math.floor(this.video.currentTime || 0);
        const phut = Math.floor(totalSeconds / 60);
        const giay = totalSeconds % 60;
        
        return {
            phut,
            giay,
            tongGiay: totalSeconds
        };
    }

    /**
     * Ghi lịch sử xem video lên server
     * @param {Boolean} force - Bắt buộc ghi ngay cả khi thời gian không thay đổi
     */
    recordWatchTime(force = false) {
        if (!this.idVideo) return;

        const currentTime = this.getCurrentWatchTime();
        
        // Chỉ gửi nếu có thay đổi hoặc bắt buộc
        if (!force && 
            currentTime.phut === this.lastReportedTime.phut && 
            currentTime.giay === this.lastReportedTime.giay) {
            return;
        }

        this.lastReportedTime = { ...currentTime };

        // Gửi request lên server
        this.sendToServer(currentTime.phut, currentTime.giay);
    }

    /**
     * Gửi thời gian xem đến server
     * @param {Number} phut 
     * @param {Number} giay 
     */
    sendToServer(phut, giay) {
        const formData = new FormData();
        formData.append('idVideo', this.idVideo);
        formData.append('phut', phut);
        formData.append('giay', giay);

        if (this.antiForgeryToken) {
            formData.append('__RequestVerificationToken', this.antiForgeryToken);
        }

        fetch('/TaiLieu/GhiLichSuXemVideo', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => {
            if (!response.ok) {
                console.warn(`Lỗi ghi lịch sử: ${response.status}`);
            }
        })
        .catch(error => {
            console.error('Lỗi khi ghi lịch sử xem video:', error);
        });
    }

    /**
     * Dừng theo dõi video
     */
    stop() {
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
        }
        // Ghi lịch sử lần cuối
        this.recordWatchTime(true);
    }

    /**
     * Xóa lịch sử theo dõi
     */
    destroy() {
        this.stop();
        if (this.video) {
            this.video.removeEventListener('pause', () => this.recordWatchTime());
            this.video.removeEventListener('ended', () => this.recordWatchTime());
        }
    }
}

/**
 * Hàm tiện lợi: khởi tạo VideoWatcher cho một video element
 * @param {String} selectorOrElement - CSS selector hoặc HTMLVideoElement
 * @param {Number} idVideo - Mã video
 * @param {Number} updateIntervalMs - Khoảng thời gian cập nhật (ms)
 * @returns {VideoWatcher}
 */
function initVideoWatcher(selectorOrElement, idVideo, updateIntervalMs = 5000) {
    let video = selectorOrElement;
    
    if (typeof selectorOrElement === 'string') {
        video = document.querySelector(selectorOrElement);
    }

    if (!video || !video.tagName || video.tagName !== 'VIDEO') {
        console.error('VideoWatcher: Element không phải là video element');
        return null;
    }

    return new VideoWatcher(video, idVideo, updateIntervalMs);
}

// Export cho module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { VideoWatcher, initVideoWatcher };
}

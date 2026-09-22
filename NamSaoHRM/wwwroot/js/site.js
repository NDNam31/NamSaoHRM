// Đồng hồ thời gian thực (trang Chấm công)
(function () {
    const clock = document.getElementById('liveClock');
    if (!clock) return;
    const dateEl = document.getElementById('liveDate');
    const thu = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
    const pad = n => String(n).padStart(2, '0');
    function tick() {
        const d = new Date();
        clock.textContent = `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
        if (dateEl) dateEl.textContent = `${thu[d.getDay()]}, ngày ${pad(d.getDate())} tháng ${pad(d.getMonth() + 1)} năm ${d.getFullYear()}`;
    }
    tick();
    setInterval(tick, 1000);
})();

// Hộp thoại xác nhận xóa dùng chung: <button data-confirm="..." data-action="/url">
document.addEventListener('click', function (e) {
    const btn = e.target.closest('[data-confirm]');
    if (!btn) return;
    const modalEl = document.getElementById('confirmModal');
    if (!modalEl) return;
    modalEl.querySelector('.confirm-text').textContent = btn.dataset.confirm;
    modalEl.querySelector('form').action = btn.dataset.action;
    bootstrap.Modal.getOrCreateInstance(modalEl).show();
});

using FluentAssertions;
using SixOsTL.Domain.Entities;

namespace SixOsTL.UnitTests.Domain
{
    public class TaiKhoanDaoTaoTests
    {
        // ── ConHieuLuc ────────────────────────────────────────────
        [Fact]
        public void ConHieuLuc_KhiIsDeletedTrue_TraVeFalse()
        {
            var tk = new TaiKhoanDaoTao { IsDeleted = true };
            tk.ConHieuLuc().Should().BeFalse();
        }

        [Fact]
        public void ConHieuLuc_KhiChuaDenNgayBatDau_TraVeFalse()
        {
            var tk = new TaiKhoanDaoTao
            {
                IsDeleted = false,
                NgayBatDau = DateTime.Now.AddDays(5) // thêm 5 ngày để test
            };
            tk.ConHieuLuc().Should().BeFalse();
        }

        [Fact]
        public void ConHieuLuc_KhiQuaNgayKetThuc_TraVeFalse()
        {
            var tk = new TaiKhoanDaoTao
            {
                IsDeleted = false,
                NgayKetThuc = DateTime.Now.AddDays(-1)  // hết hạn hôm qua
            };
            tk.ConHieuLuc().Should().BeFalse();
        }

        [Fact]
        public void ConHieuLuc_KhiTrongKhoangThoiGianHopLe_TraVeTrue()
        {
            var tk = new TaiKhoanDaoTao
            {
                IsDeleted = false,
                NgayBatDau = DateTime.Now.AddDays(-10),
                NgayKetThuc = DateTime.Now.AddDays(30)
            };
            tk.ConHieuLuc().Should().BeTrue();
        }

        [Fact]
        public void ConHieuLuc_KhiKhongCoNgayBatDauVaKetThuc_TraVeTrue()
        {
            var tk = new TaiKhoanDaoTao { IsDeleted = false };
            tk.ConHieuLuc().Should().BeTrue();
        }

    }
}

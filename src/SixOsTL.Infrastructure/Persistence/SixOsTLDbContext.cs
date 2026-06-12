using Microsoft.EntityFrameworkCore;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.Domain.Entities;

namespace SixOsTL.Infrastructure.Persistence
{
    public class SixOsTLDbContext : DbContext, IApplicationDbContext
    {
        public SixOsTLDbContext(DbContextOptions<SixOsTLDbContext> options) : base(options) { }

        public DbSet<ThongTinDoanhNghiepDaoTao> ThongTinDoanhNghieps => Set<ThongTinDoanhNghiepDaoTao>();
        public DbSet<TaiKhoanDaoTao> TaiKhoans => Set<TaiKhoanDaoTao>();
        public DbSet<DmVaiTro> VaiTros => Set<DmVaiTro>();
        public DbSet<TaiKhoanVaiTro> TaiKhoanVaiTros => Set<TaiKhoanVaiTro>();
        public DbSet<TaiKhoanChucNang> TaiKhoanChucNangs => Set<TaiKhoanChucNang>();
        public DbSet<DmVaiTroChucNang> VaiTroChucNangs => Set<DmVaiTroChucNang>();
        public DbSet<DmSanPham> SanPhams => Set<DmSanPham>();
        public DbSet<DmMucDoUuTien> MucDoUuTiens => Set<DmMucDoUuTien>();
        public DbSet<DmChucNang> ChucNangs => Set<DmChucNang>();
        public DbSet<TaiLieuVideo> Videos => Set<TaiLieuVideo>();
        public DbSet<TaiLieuFile> Files => Set<TaiLieuFile>();
        public DbSet<TaiLieuHoiDap> HoiDaps => Set<TaiLieuHoiDap>();
        public DbSet<TaiLieuVideoTag> VideoTags => Set<TaiLieuVideoTag>();
        public DbSet<TaiLieuVideoTagMap> VideoTagMaps => Set<TaiLieuVideoTagMap>();
        public DbSet<TaiLieuVideoLienQuan> VideoLienQuans => Set<TaiLieuVideoLienQuan>();
        public DbSet<LichSuXemVideo> LichSuXemVideos => Set<LichSuXemVideo>();
        public DbSet<TaiLieuHoiDapHinhAnh> TaiLieuHoiDapHinhAnhs => Set<TaiLieuHoiDapHinhAnh>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── ThongTinDoanhNghiepDaoTao ──────────────────────
            modelBuilder.Entity<ThongTinDoanhNghiepDaoTao>(e =>
            {
                e.ToTable("ThongTinDoanhNghiepDaoTao");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.MaCSKCB).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.MaCSKCB).IsUnique();
            });

            // ── DM_VaiTro ──────────────────────────────────────
            modelBuilder.Entity<DmVaiTro>(e =>
            {
                e.ToTable("DM_VaiTro");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.MaVaiTro).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.MaVaiTro).IsUnique();
            });

            // ── TaiKhoanDaoTao ─────────────────────────────────
            modelBuilder.Entity<TaiKhoanDaoTao>(e =>
            {
                e.ToTable("TaiKhoanDaoTao");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.TenTK).HasMaxLength(100).IsRequired();
                e.Property(x => x.MatKhau).HasMaxLength(250).IsRequired();
                e.HasOne(x => x.CoSoKCB)
                 .WithMany(c => c.TaiKhoans)
                 .HasForeignKey(x => x.MaCSKCB)
                 .HasPrincipalKey(c => c.MaCSKCB);
            });

            // ── TaiKhoanVaiTro (composite PK) ──────────────────
            modelBuilder.Entity<TaiKhoanVaiTro>(e =>
            {
                e.ToTable("TaiKhoan_VaiTro");
                e.HasKey(x => new { x.IDTaiKhoan, x.IDVaiTro });
                e.HasOne(x => x.TaiKhoan)
                 .WithMany(t => t.TaiKhoanVaiTros)
                 .HasForeignKey(x => x.IDTaiKhoan);
                e.HasOne(x => x.VaiTro)
                 .WithMany(v => v.TaiKhoanVaiTros)
                 .HasForeignKey(x => x.IDVaiTro);
            });

            // ── TaiKhoan_ChucNang ──────────────────────────────
            modelBuilder.Entity<TaiKhoanChucNang>(e =>
            {
                e.ToTable("TaiKhoan_ChucNang");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.IdCN).HasColumnName("IdCN");
                e.Property(x => x.IdTK).HasColumnName("IdTK");
                e.Property(x => x.Active).HasColumnName("Active");

                e.HasOne(x => x.ChucNang)
                 .WithMany(c => c.TaiKhoanChucNangs)
                 .HasForeignKey(x => x.IdCN)
                 .HasPrincipalKey(c => c.Id)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.TaiKhoan)
                 .WithMany(t => t.TaiKhoanChucNangs)
                 .HasForeignKey(x => x.IdTK)
                 .HasPrincipalKey(t => t.Id)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── DM_SanPham ─────────────────────────────────────
            modelBuilder.Entity<DmSanPham>(e =>
            {
                e.ToTable("DM_SanPham");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
            });

            // ── DM_MucDoUuTien ─────────────────────────────────
            modelBuilder.Entity<DmMucDoUuTien>(e =>
            {
                e.ToTable("DM_MucDoUuTien");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
            });

            // ── DM_ChucNang ────────────────────────────────────
            modelBuilder.Entity<DmChucNang>(e =>
            {
                e.ToTable("DM_ChucNang");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.HasOne(x => x.SanPham)
                 .WithMany(s => s.ChucNangs)
                 .HasForeignKey(x => x.IDSanPham);
                e.HasOne(x => x.MucDoUuTien)
                 .WithMany(m => m.ChucNangs)
                 .HasForeignKey(x => x.IDMucDoUuTien);
            });

            // ── TaiLieu_Video ──────────────────────────────────
            modelBuilder.Entity<TaiLieuVideo>(e =>
            {
                e.ToTable("TaiLieu_Video");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.HasOne(x => x.ChucNang)
                 .WithMany(c => c.Videos)
                 .HasForeignKey(x => x.IDChucNang);
            });

            // ── TaiLieu_File ───────────────────────────────────
            modelBuilder.Entity<TaiLieuFile>(e =>
            {
                e.ToTable("TaiLieu_File");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.HasOne(x => x.ChucNang)
                 .WithMany(c => c.Files)
                 .HasForeignKey(x => x.IDChucNang);
            });

            // ── TaiLieu_HoiDap (self-referencing) ─────────────
            modelBuilder.Entity<TaiLieuHoiDap>(e =>
            {
                e.ToTable("TaiLieu_HoiDap");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.NgayTao).HasDefaultValueSql("GETDATE()");
                e.HasOne(x => x.ChucNang)
                 .WithMany(c => c.HoiDaps)
                 .HasForeignKey(x => x.IDChucNang);
                e.HasOne(x => x.TaiKhoan)
                 .WithMany(t => t.HoiDaps)
                 .HasForeignKey(x => x.IDTaiKhoan);
                e.HasOne(x => x.ParentHoiDap)
                 .WithMany(h => h.TraLois)
                 .HasForeignKey(x => x.ParentHoiDapID);
            });

            // ── TaiLieu_VideoTag ───────────────────────────────────────────
            modelBuilder.Entity<TaiLieuVideoTag>(e =>
            {
                e.ToTable("TaiLieu_VideoTag");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.TenTag).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.TenTag).IsUnique();
            });

            // ── TaiLieu_Video_Tag (join table) ────────────────────────────
            modelBuilder.Entity<TaiLieuVideoTagMap>(e =>
            {
                e.ToTable("TaiLieu_Video_Tag");
                e.HasKey(x => new { x.IDVideo, x.IDTag });
                e.HasOne(x => x.Video)
                 .WithMany(v => v.VideoTagMaps)
                 .HasForeignKey(x => x.IDVideo);
                e.HasOne(x => x.Tag)
                 .WithMany(t => t.VideoTagMaps)
                 .HasForeignKey(x => x.IDTag);
            });

            // ── TaiLieu_VideoLienQuan ─────────────────────────────────────
            modelBuilder.Entity<TaiLieuVideoLienQuan>(e =>
            {
                e.ToTable("TaiLieu_VideoLienQuan");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");

                e.HasOne(x => x.Video)
                 .WithMany(v => v.LienQuans)        
                 .HasForeignKey(x => x.IDVideo)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.VideoLienQuan)
                 .WithMany()
                 .HasForeignKey(x => x.IDVideoLienQuan)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Tag)
                 .WithMany(t => t.LienQuans)
                 .HasForeignKey(x => x.IDTag)
                 .IsRequired(false);

                e.HasIndex(x => new { x.IDVideo, x.IDVideoLienQuan }).IsUnique();
            });

            // ── TaiLieu_HoiDap_HinhAnh ───────────────────────────────────
            modelBuilder.Entity<TaiLieuHoiDapHinhAnh>(e =>
            {
                e.ToTable("TaiLieu_HoiDap_HinhAnh");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.DuongDanFileAnh).HasMaxLength(500);

                e.HasOne(x => x.HoiDap)
                 .WithMany(h => h.HinhAnhs)
                 .HasForeignKey(x => x.IdTLHD)
                 .HasPrincipalKey(h => h.Id)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── LichSuXemVideo ────────────────────────────────────────────
            modelBuilder.Entity<LichSuXemVideo>(e =>
            {
                e.ToTable("LichSuXemVideo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");

                e.HasOne(x => x.Video)
                 .WithMany(v => v.LichSuXemVideos)
                 .HasForeignKey(x => x.IDVideo)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.TaiKhoanDaoTao)
                 .WithMany()
                 .HasForeignKey(x => x.IDTaiKhoanDT)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── DM_VaiTro_ChucNang (composite PK) ─────────────
            modelBuilder.Entity<DmVaiTroChucNang>(e =>
            {
                e.ToTable("DM_VaiTro_ChucNang");
                e.HasKey(x => new { x.IDVaiTro, x.IDChucNang });
                e.HasOne(x => x.VaiTro)
                 .WithMany()
                 .HasForeignKey(x => x.IDVaiTro);
                e.HasOne(x => x.ChucNang)
                 .WithMany()
                 .HasForeignKey(x => x.IDChucNang);
            });

        }
    }
}
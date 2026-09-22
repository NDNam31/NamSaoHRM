using Microsoft.EntityFrameworkCore;

namespace NamSaoHRM.Models;

/// <summary>Lớp trung gian giữa ứng dụng và CSDL SQL Server.</summary>
public class NamSaoDbContext : DbContext
{
    public NamSaoDbContext(DbContextOptions<NamSaoDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PhongBan> PhongBans { get; set; } = null!;
    public DbSet<NhanVien> NhanViens { get; set; } = null!;
    public DbSet<ChamCong> ChamCongs { get; set; } = null!;
    public DbSet<BangLuong> BangLuongs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users", t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Admin','NhanVien')"));
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.NgayTao).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<PhongBan>(e =>
        {
            e.ToTable("PhongBan");
            e.HasIndex(x => x.TenPhongBan).IsUnique();
        });

        modelBuilder.Entity<NhanVien>(e =>
        {
            e.ToTable("NhanVien", t => t.HasCheckConstraint("CK_NhanVien_LuongCoBan", "[LuongCoBan] > 0"));
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.UserID).IsUnique();
            e.Property(x => x.LuongCoBan).HasPrecision(18, 0);

            e.HasOne(x => x.PhongBan)
                .WithMany(p => p.NhanViens)
                .HasForeignKey(x => x.MaPhongBan)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.User)
                .WithOne(u => u.NhanVien)
                .HasForeignKey<NhanVien>(x => x.UserID)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ChamCong>(e =>
        {
            e.ToTable("ChamCong", t => t.HasCheckConstraint("CK_ChamCong_Gio",
                "[GioCheckOut] IS NULL OR [GioCheckOut] > [GioCheckIn]"));
            e.HasIndex(x => new { x.MaNV, x.NgayChamCong }).IsUnique();
            e.Property(x => x.SoGioLam).HasPrecision(4, 2);

            e.HasOne(x => x.NhanVien)
                .WithMany(n => n.ChamCongs)
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BangLuong>(e =>
        {
            e.ToTable("BangLuong", t => t.HasCheckConstraint("CK_BangLuong_Thang", "[Thang] BETWEEN 1 AND 12"));
            e.HasIndex(x => new { x.MaNV, x.Thang, x.Nam }).IsUnique();
            e.Property(x => x.SoNgayCong).HasPrecision(4, 1);
            e.Property(x => x.PhuCap).HasPrecision(18, 0);
            e.Property(x => x.KhauTru).HasPrecision(18, 0);
            e.Property(x => x.TongLuong).HasPrecision(18, 0);
            e.Property(x => x.NgayTao).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.NhanVien)
                .WithMany(n => n.BangLuongs)
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

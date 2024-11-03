using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QL_PhongTro_Web.Models;

public partial class QlphongTroContext : DbContext
{
    public QlphongTroContext()
    {
    }

    public QlphongTroContext(DbContextOptions<QlphongTroContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<Dien> Diens { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachThue> KhachThues { get; set; }

    public virtual DbSet<Nuoc> Nuocs { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=MANHBAO\\SQLEXPRESS;Initial Catalog=QLPhongTro;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietH__CDF0A114B53EE73B");

            entity.ToTable("ChiTietHoaDon");

            entity.Property(e => e.TienDien).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.TienNuoc).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.TienPhong).HasColumnType("decimal(10, 3)");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__ChiTietHo__MaHoa__5535A963");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__ChiTietHo__MaPho__5629CD9C");
        });

        modelBuilder.Entity<Dien>(entity =>
        {
            entity.HasKey(e => e.MaDien).HasName("PK__Dien__33326024774709B3");

            entity.ToTable("Dien");

            entity.Property(e => e.ChiSoCu).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.ChiSoMoi).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.GiaTien).HasColumnType("decimal(10, 3)");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Diens)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__Dien__MaPhong__571DF1D5");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDon__835ED13BBFC502E8");

            entity.ToTable("HoaDon");

            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(10, 3)");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK_HoaDon_KhachThue");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__HoaDon__MaPhong__5812160E");
        });

        modelBuilder.Entity<KhachThue>(entity =>
        {
            entity.HasKey(e => e.MaKhachThue).HasName("PK__KhachThu__1C7173F71550F41B");

            entity.ToTable("KhachThue");

            entity.Property(e => e.Cmnd)
                .HasMaxLength(100)
                .HasColumnName("CMND");
            entity.Property(e => e.DienThoai).HasMaxLength(20);
            entity.Property(e => e.HoTenDem).HasMaxLength(50);
            entity.Property(e => e.Ten).HasMaxLength(50);

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.KhachThues)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__KhachThue__MaPho__59FA5E80");
        });

        modelBuilder.Entity<Nuoc>(entity =>
        {
            entity.HasKey(e => e.MaNuoc).HasName("PK__Nuoc__21306FEA96E76EDB");

            entity.ToTable("Nuoc");

            entity.Property(e => e.ChiSoCu).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.ChiSoMoi).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.GiaTien).HasColumnType("decimal(10, 3)");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Nuocs)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__Nuoc__MaPhong__5AEE82B9");
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__Phong__20BD5E5B4BF014BF");

            entity.ToTable("Phong");

            entity.Property(e => e.GiaThueThang).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.LoaiPhong).HasMaxLength(50);
            entity.Property(e => e.SoPhong).HasMaxLength(50);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529F83F16BF");

            entity.ToTable("TaiKhoan");

            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.MatKhau).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

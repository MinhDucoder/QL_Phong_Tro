using System;
using System.Collections.Generic;

namespace QL_PhongTro_Web.Models;

public partial class KhachThue
{
    public int MaKhachThue { get; set; }

    public string HoTenDem { get; set; } = null!;

    public string Ten { get; set; } = null!;

    public string? Cmnd { get; set; }

    public string? DienThoai { get; set; }

    public DateOnly NgayVaoO { get; set; }

    public DateOnly? NgayTraPhong { get; set; }

    public int? MaPhong { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual Phong? MaPhongNavigation { get; set; }
}

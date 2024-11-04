using System;
using System.Collections.Generic;

namespace QL_PhongTro_Web.Models;

public partial class Phong
{
    public int MaPhong { get; set; }

    public string SoPhong { get; set; } = null!;

    public string LoaiPhong { get; set; } = null!;

    public decimal GiaThueThang { get; set; }

    public bool DaThue { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual ICollection<Dien> Diens { get; set; } = new List<Dien>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual ICollection<KhachThue> KhachThues { get; set; } = new List<KhachThue>();

    public virtual ICollection<Nuoc> Nuocs { get; set; } = new List<Nuoc>();
}

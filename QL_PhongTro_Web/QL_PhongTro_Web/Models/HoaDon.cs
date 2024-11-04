using System;
using System.Collections.Generic;

namespace QL_PhongTro_Web.Models;

public partial class HoaDon
{
    public int MaHoaDon { get; set; }
   
    public DateOnly NgayLap { get; set; }

    public decimal ThanhTien { get; set; }

    public int? MaPhong { get; set; }

    public int? MaKh { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual KhachThue? MaKhNavigation { get; set; }

    public virtual Phong? MaPhongNavigation { get; set; }
}

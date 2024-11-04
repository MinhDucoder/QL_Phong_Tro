using System;
using System.Collections.Generic;

namespace QL_PhongTro_Web.Models;

public partial class Nuoc
{
    public int MaNuoc { get; set; }

    public DateOnly NgayDocSo { get; set; }

    public decimal ChiSoCu { get; set; }

    public decimal? ChiSoMoi { get; set; }

    public decimal GiaTien { get; set; }

    public int? MaPhong { get; set; }

    public virtual Phong? MaPhongNavigation { get; set; }
}

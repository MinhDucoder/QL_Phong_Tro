using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro_Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
using iTextSharp.text;
using iTextSharp.text.pdf;



namespace QL_PhongTro_Web.Controllers

{
    public class AdminController : Controller
    {
        private readonly QlphongTroContext _ql;

        public AdminController(QlphongTroContext ql)
        {
            _ql = ql;
        }

        // GET: Login
        public IActionResult Login(string success)
        {
            if (!string.IsNullOrEmpty(success))
            {
                ViewData["success"] = success;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string sdt, string password)
        {
            var user = await _ql.TaiKhoans
                .FirstOrDefaultAsync(u => u.SoDienThoai == sdt && u.MatKhau == password);
            if (user != null)
            {
                if (user.ChucVu.Contains("Quản lý"))
                {
                    HttpContext.Session.SetString("TaiKhoan", user.SoDienThoai);
                    return RedirectToAction("Phong");
                }
                else
                {
                    HttpContext.Session.SetString("TaiKhoan", user.SoDienThoai);
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewData["err"] = "Tài khoản hoặc mật khẩu không đúng";
            return View();
        }


        public IActionResult Phong(int? page, string error, string success)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ViewData["err"] = error;
            }
            if (!string.IsNullOrEmpty(success))
            {
                ViewData["success"] = success;
            }

            int pageSize = 8; // Số lượng mục trên mỗi trang
            int pageNumber = page ?? 1;

            // Chuyển đổi danh sách thành IPagedList
            var phong = _ql.Phongs.OrderByDescending(p => p.MaPhong)
                                  .ToPagedList(pageNumber, pageSize); // Sử dụng ToPagedList

            return View(phong); // Truyền vào View kiểu IPagedList
        }

        public IActionResult Search(string searchString, int? page, bool? daThue)
        {
            var phongs = from p in _ql.Phongs select p;
            if (!string.IsNullOrEmpty(searchString))
            {
                phongs = phongs.Where(p => p.SoPhong.Contains(searchString));
            }
            if (daThue.HasValue)
            {
                phongs = phongs.Where(p => p.DaThue == daThue.Value);
            }

            int pageSize = 8;
            int pageNumber = page ?? 1;

            var paginatedList = phongs.OrderByDescending(p => p.MaPhong)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View("Phong", paginatedList);
        }

        public IActionResult AddRoom()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddRoom(Phong phong)
        {
            if (ModelState.IsValid)
            {
                bool roomExists = _ql.Phongs.Any(p => p.SoPhong == phong.SoPhong);
                if (roomExists)
                {
                    ViewData["err"] = "Số phòng đã tồn tại.";
                    return View(phong);
                }
                _ql.Phongs.Add(phong);
                _ql.SaveChanges();
                return RedirectToAction("Phong", new { success = "Bạn đã thêm phòng thành công" });
            }
            return View(phong);
        }

        public IActionResult EditRoom(int id)
        {
            var room = _ql.Phongs.Find(id);
            return View(room);
        }

        [HttpPost]
        public IActionResult EditRoom(Phong phong)
        {
            if (ModelState.IsValid)
            {
                Phong existingRoom = _ql.Phongs.Find(phong.MaPhong);
                if (existingRoom == null)
                {
                    return NotFound("Không tìm thấy phòng để cập nhật");
                }
                _ql.Entry(existingRoom).CurrentValues.SetValues(phong);
                _ql.SaveChanges();
                return RedirectToAction("Phong", new { success = "Bạn đã sửa phòng thành công" });
            }
            return View(phong);
        }
        public IActionResult DetailsRoom(int id)
        {
            // Tìm phòng theo id
            var phong = _ql.Phongs.Find(id);

            if (phong == null)
            {
                // Trả về 404 nếu không tìm thấy phòng
                return NotFound("Không tìm thấy phòng");
            }

            // Lấy thông tin nước và điện liên quan đến phòng
            var nuoc = _ql.Nuocs.SingleOrDefault(n => n.MaPhong == id);
            var dien = _ql.Diens.SingleOrDefault(d => d.MaPhong == id);
            var khachThue = _ql.KhachThues.Where(k => k.MaPhong == id).ToList();

            // Truyền dữ liệu vào ViewBag để sử dụng trong View
            ViewBag.Nuoc = nuoc;
            ViewBag.Dien = dien;
            ViewBag.KhachThue = khachThue;

            // Trả về View với model là phòng
            return View(phong);
        }


        public IActionResult DeleteRoom(int id)
        {
            Phong roomToDelete = _ql.Phongs.Find(id);
            if (roomToDelete == null)
            {
                return NotFound("Không tìm thấy phòng để xóa");
            }

            var khachThue = _ql.KhachThues.FirstOrDefault(k => k.MaPhong == id);
            if (khachThue != null)
            {
                return RedirectToAction("Phong", new { error = "Không thể xóa phòng đang được cho thuê." });
            }

            var dienRecords = _ql.Diens.Where(d => d.MaPhong == id);
            _ql.Diens.RemoveRange(dienRecords);

            var nuocRecords = _ql.Nuocs.Where(n => n.MaPhong == id);
            _ql.Nuocs.RemoveRange(nuocRecords);

            var chiTietHoaDonList = _ql.ChiTietHoaDons.Where(ct => ct.MaPhong == id);
            _ql.ChiTietHoaDons.RemoveRange(chiTietHoaDonList);

            var hoaDonList = _ql.HoaDons.Where(ct => ct.MaPhong == id);
            _ql.HoaDons.RemoveRange(hoaDonList);

            _ql.Phongs.Remove(roomToDelete);
            _ql.SaveChanges();

            return RedirectToAction("Phong", new { success = "Bạn đã xóa phòng thành công" });
        }

        public IActionResult TinhTienPhong(int maPhong)
        {
            var phong = _ql.Phongs.Find(maPhong);
            if (phong == null)
            {
                return NotFound("Không tìm thấy phòng.");
            }

            var dien = _ql.Diens.FirstOrDefault(d => d.MaPhong == maPhong);
            var nuoc = _ql.Nuocs.FirstOrDefault(n => n.MaPhong == maPhong);

            if (dien == null || nuoc == null)
            {
                return RedirectToAction("Phong", new { error = "Phòng này chưa tính chỉ số nước hoặc chỉ số điện." });
            }

            var kh = _ql.KhachThues.FirstOrDefault(k => k.MaPhong == maPhong);
            decimal tienPhong = phong.GiaThueThang;
            decimal? tienDien = (dien.ChiSoMoi - dien.ChiSoCu) * dien.GiaTien;
            decimal? tienNuoc = (nuoc.ChiSoMoi - nuoc.ChiSoCu) * nuoc.GiaTien;
            decimal? thanhTien = tienPhong + tienDien + tienNuoc;

            var hoadon = new HoaDon
            {
                NgayLap = DateOnly.FromDateTime(DateTime.Now),
                ThanhTien = thanhTien ?? 0,
                MaPhong = maPhong,
                MaKh = kh.MaKhachThue
            };

            _ql.HoaDons.Add(hoadon);
            _ql.SaveChanges();

            int maHoaDon = _ql.HoaDons.Max(u => u.MaHoaDon);

            var chiTietHoaDon = new ChiTietHoaDon
            {
                MaHoaDon = maHoaDon,
                MaPhong = maPhong,
                TienPhong = tienPhong,
                TienDien = tienDien ?? 0,
                TienNuoc = tienNuoc ?? 0
            };

            _ql.ChiTietHoaDons.Add(chiTietHoaDon);
            _ql.SaveChanges();

            return RedirectToAction("HoaDon", new { maHoaDon });
        }

        public IActionResult ChiTietHD(int id)
        {
            var cthd = _ql.ChiTietHoaDons.SingleOrDefault(u => u.MaHoaDon == id);
            if (cthd == null)
            {
                return NotFound("Không tìm thấy chi tiết hóa đơn.");
            }
            return View(cthd);
        }

        public IActionResult InHoaDonPDF(int maHoaDon)
        {
            var hoadon = _ql.HoaDons.Include(h => h.ChiTietHoaDons).SingleOrDefault(h => h.MaHoaDon == maHoaDon);
            if (hoadon == null)
            {
                return NotFound("Không tìm thấy hóa đơn.");
            }

            using (var memoryStream = new MemoryStream())
            {
                var doc = new iTextSharp.text.Document();
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, memoryStream);
                doc.Open();

                var content = new iTextSharp.text.Paragraph();
                content.Add(new iTextSharp.text.Phrase("Nội dung hóa đơn:\n", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12)));
                content.Add(new iTextSharp.text.Chunk($"Ngày lập: {hoadon.NgayLap}\n", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10)));
                content.Add(new iTextSharp.text.Chunk($"Tổng tiền: {hoadon.ThanhTien}\n", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10)));

                content.Add(new iTextSharp.text.Phrase("Chi tiết hóa đơn:\n", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12)));

                foreach (var chiTiet in hoadon.ChiTietHoaDons)
                {
                    content.Add(new iTextSharp.text.Chunk($"Mã phòng: {chiTiet.MaPhong}\n", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10)));
                    content.Add(new iTextSharp.text.Chunk($"Tiền phòng: {chiTiet.TienPhong}\n", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10)));
                    content.Add(new iTextSharp.text.Chunk($"Tiền điện: {chiTiet.TienDien}\n", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10)));
                    content.Add(new iTextSharp.text.Chunk($"Tiền nước: {chiTiet.TienNuoc}\n", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10)));
                }

                doc.Add(content);
                doc.Close();

                return File(memoryStream.ToArray(), "application/pdf", "HoaDon.pdf");
            }
        }

        public IActionResult Hoadon(int maHoaDon)
        {
            var hoadon = _ql.HoaDons.Include(h => h.ChiTietHoaDons).SingleOrDefault(h => h.MaHoaDon == maHoaDon);
            if (hoadon == null)
            {
                return NotFound("Không tìm thấy hóa đơn.");
            }
            return View(hoadon);
        }

        public IActionResult Dien(int? page, string success)
        {
            if (!string.IsNullOrEmpty(success))
            {
                ViewData["success"] = success;
            }

            int pageSize = 8;
            int pageNumber = page ?? 1;
            var dien = _ql.Diens.OrderBy(p => p.MaDien).ToPagedList(pageNumber, pageSize);
            return View(dien);
        }

        [HttpGet]
        public IActionResult AddDien()
        {
            ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
            return View();
        }

        [HttpPost]
        public IActionResult AddDien(Dien dien)
        {
            if (ModelState.IsValid)
            {
                if (_ql.Diens.Any(d => d.MaPhong == dien.MaPhong))
                {
                    ViewData["err"] = "Phòng này đã có chỉ số điện.";
                    ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
                    return View(dien);
                }

                _ql.Diens.Add(dien);
                _ql.SaveChanges();
                return RedirectToAction("Dien", new { success = "Bạn đã thêm chỉ số điện thành công" });
            }
            ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
            return View(dien);
        }

        public IActionResult EditDien(int id)
        {
            ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
            var room = _ql.Diens.SingleOrDefault(u => u.MaDien == id);
            return View(room);
        }

        [HttpPost]
        public IActionResult EditDien(Dien dien)
        {
            if (ModelState.IsValid)
            {
                var existingRoom = _ql.Diens.Find(dien.MaDien);
                if (existingRoom == null)
                {
                    return NotFound("Không tìm thấy điện để cập nhật");
                }

                if (_ql.Diens.Any(d => d.MaPhong == dien.MaPhong && d.MaDien != dien.MaDien))
                {
                    ViewData["err"] = "Phòng này đã có chỉ số điện vui lòng chọn lại.";
                    ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
                    return View(dien);
                }

                _ql.Entry(existingRoom).CurrentValues.SetValues(dien);
                _ql.SaveChanges();
                return RedirectToAction("Dien", new { success = "Bạn đã sửa chỉ số điện thành công" });
            }
            ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
            return View(dien);
        }

        public IActionResult DeleteDien(int id)
        {
            var roomToDelete = _ql.Diens.Find(id);
            if (roomToDelete == null)
            {
                return NotFound("Không tìm thấy điện để xóa");
            }

            _ql.Diens.Remove(roomToDelete);
            _ql.SaveChanges();

            return RedirectToAction("Dien", new { success = "Bạn đã xóa chỉ số điện thành công" });
        }

        // Action hiển thị danh sách chỉ số nước theo trang
        public IActionResult Nuoc(int? page, string success)
        {
            if (!string.IsNullOrEmpty(success))
            {
                ViewData["success"] = success;
            }

            int pageSize = 8;
            int pageNumber = page ?? 1;

            var nuoc = _ql.Nuocs.Include(n => n.MaPhongNavigation)
                               .OrderBy(n => n.MaNuoc)
                               .ToPagedList(pageNumber, pageSize);

            return View(nuoc);
        }

        [HttpGet]
        public IActionResult AddNuoc()
        {
            ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
            return View();
        }

        [HttpPost]
        public IActionResult AddNuoc(Nuoc nuoc)
        {
            if (ModelState.IsValid)
            {
                if (_ql.Nuocs.Any(d => d.MaPhong == nuoc.MaPhong))
                {
                    ViewData["err"] = "Phòng này đã có chỉ số nước.";
                    ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
                    return View(nuoc);
                }

                _ql.Nuocs.Add(nuoc);
                _ql.SaveChanges();
                return RedirectToAction("Nuoc", new { success = "Bạn đã thêm chỉ số nước thành công" });
            }

            ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
            return View(nuoc);
        }

        public IActionResult EditNuoc(int id)
        {
            ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
            var room = _ql.Nuocs.SingleOrDefault(n => n.MaNuoc == id);
            if (room == null)
            {
                return NotFound("Không tìm thấy chỉ số nước");
            }
            return View(room);
        }

        [HttpPost]
        public IActionResult EditNuoc(Nuoc nuoc)
        {
            if (ModelState.IsValid)
            {
                var existingRoom = _ql.Nuocs.Find(nuoc.MaNuoc);
                if (existingRoom == null)
                {
                    return NotFound("Không tìm thấy nước để cập nhật");
                }

                if (_ql.Nuocs.Any(d => d.MaPhong == nuoc.MaPhong && d.MaNuoc != nuoc.MaNuoc))
                {
                    ViewData["err"] = "Phòng này đã có chỉ số nước vui lòng chọn lại.";
                    ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
                    return View(nuoc);
                }

                _ql.Entry(existingRoom).CurrentValues.SetValues(nuoc);
                _ql.SaveChanges();

                return RedirectToAction("Nuoc", new { success = "Bạn đã sửa chỉ số nước thành công" });
            }

            ViewBag.MaPhongList = new SelectList(_ql.Phongs, "MaPhong", "SoPhong");
            return View(nuoc);
        }

        public IActionResult DeleteNuoc(int id)
        {
            var roomToDelete = _ql.Nuocs.Find(id);

            if (roomToDelete == null)
            {
                return NotFound("Không tìm thấy nước để xóa");
            }

            _ql.Nuocs.Remove(roomToDelete);
            _ql.SaveChanges();

            return RedirectToAction("Nuoc", new { success = "Bạn đã xóa chỉ số nước thành công" });
        }

        public async Task<IActionResult> KH(int? page, string success)
        {
            if (!string.IsNullOrEmpty(success))
            {
                ViewData["success"] = success;
            }

            int pageSize = 8;
            int pageNumber = page ?? 1;
            var khachThues = _ql.KhachThues.OrderByDescending(p => p.MaKhachThue)
                                                 .ToPagedList(pageNumber, pageSize);
            return View(khachThues);
        }

        [HttpGet]
        public IActionResult AddKH()
        {
            ViewBag.MaPhongList = new SelectList(_ql.Phongs.Where(u => u.DaThue == false), "MaPhong", "SoPhong");
            return View();
        }

        private bool ValidatePhoneNumber(string phoneNumber)
        {
            var phoneNumberRegex = new Regex(@"^\d{10}$");
            return phoneNumberRegex.IsMatch(phoneNumber);
        }


        private bool IsVietnameseIDValid(string vietnameseID)
        {
            var vietnameseIDRegex = new Regex(@"^\d{9}$|^\d{12}$");
            return vietnameseIDRegex.IsMatch(vietnameseID);
        }

        [HttpPost]
        public async Task<IActionResult> AddKH(KhachThue khach)
        {
            if (_ql.KhachThues.Any(tk => tk.DienThoai == khach.DienThoai))
            {
                ViewBag.MaPhongList = new SelectList(_ql.Phongs.Where(u => u.DaThue == false), "MaPhong", "SoPhong");
                ViewData["err"] = "Số điện thoại đã tồn tại.";
                return View(khach);
            }

            if (_ql.KhachThues.Any(tk => tk.Cmnd == khach.Cmnd))
            {
                ViewBag.MaPhongList = new SelectList(_ql.Phongs.Where(u => u.DaThue == false), "MaPhong", "SoPhong");
                ViewData["err"] = "Số CMND đã tồn tại.";
                return View(khach);
            }

            if (!IsPhoneNumberValid(khach.DienThoai))
            {
                ViewBag.MaPhongList = new SelectList(_ql.Phongs.Where(u => u.DaThue == false), "MaPhong", "SoPhong");
                ViewData["err"] = "Số điện thoại không hợp lệ. Số điện thoại phải có 10 số.";
                return View(khach);
            }

            if (!IsVietnameseIDValid(khach.Cmnd))
            {
                ViewBag.MaPhongList = new SelectList(_ql.Phongs.Where(u => u.DaThue == false), "MaPhong", "SoPhong");
                ViewData["err"] = "Số CMND không hợp lệ. Số CMND phải có 9 hoặc 12 chữ số.";
                return View(khach);
            }

            if (ModelState.IsValid)
            {
                _ql.KhachThues.Add(khach);
                await _ql.SaveChangesAsync();

                var phong = await _ql.Phongs.FindAsync(khach.MaPhong);
                if (phong != null)
                {
                    phong.DaThue = true;
                    await _ql.SaveChangesAsync();
                }

                return RedirectToAction("KH", new { success = "Bạn đã thêm khách thuê trọ thành công" });
            }

            ViewBag.MaPhongList = new SelectList(_ql.Phongs.Where(u => u.DaThue == false), "MaPhong", "SoPhong");
            return View(khach);
        }

        public async Task<IActionResult> EditKH(int id)
        {
            var khach = await _ql.KhachThues.FindAsync(id);
            if (khach == null)
            {
                return NotFound("Không tìm thấy khách hàng để chỉnh sửa");
            }

            ViewBag.MaPhongList = new SelectList(_ql.Phongs.Where(u => u.DaThue == false || u.MaPhong == khach.MaPhong), "MaPhong", "SoPhong");
            return View(khach);
        }

        [HttpPost]
        public async Task<IActionResult> EditKH(KhachThue khach, int MaPhongCu)
        {
            if (ModelState.IsValid)
            {
                var existingRoom = await _ql.KhachThues.FindAsync(khach.MaKhachThue);
                if (existingRoom == null)
                {
                    return NotFound("Không tìm thấy khách hàng để cập nhật");
                }

                var phongCu = await _ql.Phongs.FindAsync(MaPhongCu);
                var phongMoi = await _ql.Phongs.FindAsync(khach.MaPhong);

                _ql.Entry(existingRoom).CurrentValues.SetValues(khach);

                if (phongCu != null && phongMoi != null)
                {
                    phongCu.DaThue = false;
                    phongMoi.DaThue = true;
                }
                else
                {
                    existingRoom.MaPhong = MaPhongCu;
                }

                await _ql.SaveChangesAsync();

                return RedirectToAction("KH", new { success = "Bạn đã sửa thông tin khách hàng thành công" });
            }

            ViewBag.MaPhongList = new SelectList(_ql.Phongs.Where(u => u.DaThue == false || u.MaPhong == khach.MaPhong), "MaPhong", "SoPhong");
            return View(khach);
        }

        public async Task<IActionResult> DeleteKH(int id)
        {
            var roomToDelete = await _ql.KhachThues.FindAsync(id);
            if (roomToDelete == null)
            {
                return NotFound("Không tìm thấy khách hàng để xóa");
            }

            var phong = await _ql.Phongs.FindAsync(roomToDelete.MaPhong);
            if (phong != null)
            {
                phong.DaThue = false;
                await _ql.SaveChangesAsync();
            }

            var hoaDons = _ql.HoaDons.Where(u => u.MaKh == id).ToList();
            foreach (var hoaDon in hoaDons)
            {
                var chiTiet = await _ql.ChiTietHoaDons.FirstOrDefaultAsync(u => u.MaHoaDon == hoaDon.MaHoaDon);
                if (chiTiet != null)
                {
                    _ql.ChiTietHoaDons.Remove(chiTiet);
                }
                _ql.HoaDons.Remove(hoaDon);
            }

            _ql.KhachThues.Remove(roomToDelete);
            await _ql.SaveChangesAsync();

            return RedirectToAction("KH", new { success = "Bạn đã xóa khách hàng thành công" });
        }

        public async Task<IActionResult> DH(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var hoaDons = _ql.HoaDons.OrderBy(p => p.MaHoaDon).ToPagedList(pageNumber, pageSize);
            return View(hoaDons);
        }

        public ActionResult TaiKhoan(int? page, string success)
        {
            if (!string.IsNullOrEmpty(success))
            {
                ViewData["success"] = success;
            }
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var hd = _ql.TaiKhoans.OrderBy(p => p.MaTaiKhoan).ToPagedList(pageNumber, pageSize);
            return View(hd);
        }

        [HttpGet]
        public ActionResult CreateTK()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateTK(TaiKhoan taiKhoan, string NhapLaiMatKhau)
        {
            if (ModelState.IsValid)
            {
                bool sdtExists = _ql.TaiKhoans.Any(tk => tk.SoDienThoai == taiKhoan.SoDienThoai);

                if (sdtExists)
                {
                    ViewData["err"] = "Số điện thoại đã tồn tại.";
                    return View(taiKhoan);
                }

                sdtExists = _ql.KhachThues.Any(tk => tk.DienThoai == taiKhoan.SoDienThoai);
                if (!sdtExists)
                {
                    ViewData["err"] = "Số điện thoại bạn đang tạo không trùng với bất kì số điện thoại nào của khách hàng.";
                    return View(taiKhoan);
                }

                if (taiKhoan.MatKhau != NhapLaiMatKhau)
                {
                    ViewData["err"] = "Mật khẩu và nhập lại mật khẩu không khớp.";
                    return View(taiKhoan);
                }

                if (!IsPhoneNumberValid(taiKhoan.SoDienThoai))
                {
                    ViewData["err"] = "Số điện thoại không hợp lệ. Số điện thoại phải có 10 số.";
                    return View(taiKhoan);
                }

                _ql.TaiKhoans.Add(taiKhoan);
                _ql.SaveChanges();
                return RedirectToAction("TaiKhoan", new { success = "Bạn đã tạo tài khoản mới thành công" });
            }

            return View(taiKhoan);
        }

        public ActionResult EditTK(int id)
        {
            TaiKhoan taiKhoan = _ql.TaiKhoans.Find(id);

            if (taiKhoan == null)
            {
                return NotFound("Không tìm thấy tài khoản để chỉnh sửa.");
            }

            return View(taiKhoan);
        }

        [HttpPost]
        public ActionResult EditTK(TaiKhoan taiKhoan)
        {
            if (ModelState.IsValid)
            {
                bool sdtExists = _ql.TaiKhoans.Any(tk => tk.SoDienThoai == taiKhoan.SoDienThoai && tk.MaTaiKhoan != taiKhoan.MaTaiKhoan);

                if (sdtExists)
                {
                    ViewData["err"] = "Số điện thoại đã tồn tại.";
                    return View(taiKhoan);
                }

                TaiKhoan existingTaiKhoan = _ql.TaiKhoans.Find(taiKhoan.MaTaiKhoan);

                if (existingTaiKhoan == null)
                {
                    return NotFound("Không tìm thấy tài khoản để cập nhật.");
                }

                if (!IsPhoneNumberValid(taiKhoan.SoDienThoai))
                {
                    ViewData["err"] = "Số điện thoại không hợp lệ. Số điện thoại phải có 10 số.";
                    return View(taiKhoan);
                }

                _ql.Entry(existingTaiKhoan).CurrentValues.SetValues(taiKhoan);
                _ql.SaveChanges();
                return RedirectToAction("TaiKhoan", new { success = "Bạn đã sửa tài khoản thành công" });
            }

            return View(taiKhoan);
        }

        public ActionResult Pass(int id, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ViewData["err"] = error;
            }
            ViewBag.UserId = id;
            return View();
        }

        [HttpPost]
        public ActionResult Pass(int id, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                return RedirectToAction("Pass", new { id = id, error = "Mật khẩu phải trùng" });
            }

            var taiKhoan = _ql.TaiKhoans.SingleOrDefault(tk => tk.MaTaiKhoan == id);

            if (taiKhoan != null)
            {
                taiKhoan.MatKhau = newPassword;
                taiKhoan.TrangThai = true;
                _ql.SaveChanges();

                return RedirectToAction("TaiKhoan", new { success = "Bạn đã cấp mật khẩu cho tài khoản thành công" });
            }
            else
            {
                return RedirectToAction("TaiKhoan", new { error = "Bạn đã cấp mật khẩu cho tài khoản không thành công" });
            }
        }

        private bool IsPhoneNumberValid(string phoneNumber)
        {
            var phoneNumberRegex = new Regex(@"^\d{10}$");
            return phoneNumberRegex.IsMatch(phoneNumber);
        }

        // Action khi người dùng bấm vào quên mật khẩu
        [HttpGet]
        public ActionResult Forget(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ViewData["err"] = error;
            }
            return View();
        }

        // Hàm xử lý khi người dùng nhấn nút gửi
        [HttpPost]
        public ActionResult Forget(string phoneNumber, string s)
        {
            var tk = _ql.TaiKhoans.SingleOrDefault(u => u.SoDienThoai == phoneNumber);
            if (tk == null)
            {
                return RedirectToAction("Forget", new { error = "Số điện thoại không tồn tại trong hệ thống" });
            }

            // Đặt trạng thái tài khoản là false để chỉ ra rằng người dùng cần cấp lại mật khẩu
            tk.TrangThai = false;
            _ql.SaveChanges();

            return RedirectToAction("Login", new { success = "Yêu cầu thành công, vui lòng liên hệ admin để cấp lại mật khẩu" });
        }

        // Action xóa tài khoản
        public ActionResult DeleteTK(int id)
        {
            TaiKhoan taiKhoan = _ql.TaiKhoans.Find(id);

            if (taiKhoan == null)
            {
                return NotFound("Không tìm thấy tài khoản để xóa.");
            }

            // Xóa tài khoản khỏi cơ sở dữ liệu
            _ql.TaiKhoans.Remove(taiKhoan);
            _ql.SaveChanges();

            return RedirectToAction("TaiKhoan", new { success = "Bạn đã xóa tài khoản thành công" });
        }

    }

}

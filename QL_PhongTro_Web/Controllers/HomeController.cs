using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QL_PhongTro_Web.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QL_PhongTro_Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly QlphongTroContext _context;

		public HomeController(ILogger<HomeController> logger, QlphongTroContext context)
		{
			_logger = logger;
			_context = context;
		}

		public IActionResult Index()
		{
			// Lấy tài khoản từ Session
			string s = HttpContext.Session.GetString("TaiKhoan");

			// Kiểm tra tài khoản
			if (string.IsNullOrEmpty(s))
			{
				return RedirectToAction("Login", "Account"); // Điều hướng đến trang đăng nhập nếu tài khoản rỗng
			}

			var khachThue = _context.KhachThues.SingleOrDefault(k => k.DienThoai.Contains(s));
			if (khachThue == null)
			{
				return NotFound("Không tìm thấy khách thuê");
			}

			Phong phong = _context.Phongs.Find(khachThue.MaPhong);
			HttpContext.Session.SetString("MaKH", khachThue.MaKhachThue.ToString());



			if (phong == null)
			{
				return NotFound("Không tìm thấy phòng");
			}

			Nuoc nuoc = _context.Nuocs.SingleOrDefault(n => n.MaPhong == khachThue.MaPhong);
			Dien dien = _context.Diens.SingleOrDefault(d => d.MaPhong == khachThue.MaPhong);

			ViewBag.Nuoc = nuoc;
			ViewBag.Dien = dien;
			ViewBag.KhachThue = khachThue;

			return View(phong);
		}

		public IActionResult DonHang()
		{
			// Lấy tài khoản từ Session
			string taiKhoan = HttpContext.Session.GetString("TaiKhoan");
			if (string.IsNullOrEmpty(taiKhoan))
			{
				return RedirectToAction("Login", "Account"); // Điều hướng đến trang đăng nhập nếu tài khoản rỗng
			}

			var khach = _context.KhachThues.SingleOrDefault(u => u.DienThoai.Contains(taiKhoan));
			if (khach == null)
			{
				return NotFound("Không tìm thấy khách thuê");
			}

			var danhSachHoaDon = _context.HoaDons
				.Include(hd => hd.ChiTietHoaDons)
				.Where(hd => hd.MaKh == khach.MaKhachThue)
				.ToList();

			return View(danhSachHoaDon);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}

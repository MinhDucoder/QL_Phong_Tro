using Microsoft.EntityFrameworkCore;
using QL_PhongTro_Web.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace QL_PhongTro_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Cấu hình QlphongTroContext với chuỗi kết nối từ appsettings.json
            builder.Services.AddDbContext<QlphongTroContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("QlphongTroContext")));

            // Cấu hình ApplicationDbContext cho Identity
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("QlphongTroContext")));

            // Cấu hình ASP.NET Core Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Cấu hình xác thực mật khẩu
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;

                // Cấu hình xác thực email và username
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Cấu hình sử dụng Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thiết lập thời gian hết hạn Session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Cấu hình Authorization
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Sử dụng Session và Authentication/Authorization
            app.UseSession();
            app.UseAuthentication(); // Sử dụng xác thực
            app.UseAuthorization();

            // Định tuyến mặc định
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Admin}/{action=Login}/{id?}");

            app.Run();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using shop.Data;
using shop.Models;

namespace shop.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IPasswordHasher<Khachhang> _passwordHasher;

        public CustomerController(ApplicationDbContext context, IPasswordHasher<Khachhang> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        void GetData()
        {
            //đếm số mặt hàng trong giỏ hàng(sesion), ta có 2 cách 1. dùng ViewData["tên_biến"], 2.dùng ViewBag.tên_biến
            ViewData["soluong"] = GetCartItems().Count;

            //Lấy tất cả danh mục có trong database
            ViewBag.danhmuc= _context.Danhmucs.ToList();


            ViewBag.diachi = _context.Diachis.ToList();

            //lấy thông tin người dùng
            if (HttpContext.Session.GetString("khachhang") != "")
            {
                ViewBag.khachhang = _context.Khachhangs.FirstOrDefault(k => k.Email == HttpContext.Session.GetString("khachhang"));
            }
        }


        public async Task<IActionResult> List(int id)
        {
            var applicationDbContext = _context.Mathangs
                .Where(m => m.MaDm == id)
                .Include(m => m.MaDmNavigation);
            GetData();
            //Lấy tất cả danh mục được chọn
            ViewData["tendm"] = _context.Danhmucs.FirstOrDefault(d => d.MaDm == id).Ten ;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Mathangs.Include(m => m.MaDmNavigation);
            GetData();
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> Indexall()
        {
            var applicationDbContext = _context.Mathangs.Include(m => m.MaDmNavigation);
            GetData();
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var mathang = await _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMh == id);
            mathang.LuotXem += 1;
            if (mathang == null)
            {
                return NotFound();
            }
            GetData();
            _context.SaveChanges();
            return View(mathang);
        }


        private bool MathangExists(int id)
        {
            return _context.Mathangs.Any(e => e.MaMh == id);
        }

        //===================================
        // Các phương thức xử lý giỏ hàng
        // Đọc danh sách CartItem từ session
        List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session;
            string? jsoncart = session.GetString("shopcart");
            if (jsoncart != null)
            {
                //return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
                var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
                return cartItems ?? new List<CartItem>();
            }
            return new List<CartItem>();
        }

        // Lưu danh sách CartItem trong giỏ hàng vào session
        void SaveCartSession(List<CartItem> list)
        {
            var session = HttpContext.Session;
            string jsoncart = JsonConvert.SerializeObject(list);
            session.SetString("shopcart", jsoncart);
        }

        // Xóa session giỏ hàng
        void ClearCart()
        {
            var session = HttpContext.Session;
            session.Remove("shopcart");
        }

        // Cho hàng vào giỏ
        public async Task<IActionResult> AddToCart(int id)
        {
            var mathang = await _context.Mathangs
                .FirstOrDefaultAsync(m => m.MaMh == id);
            if (mathang == null)
            {
                return NotFound("Sản phẩm không tồn tại");
            }
            var cart = GetCartItems();
            var item = cart.Find(p => p.MatHang.MaMh == id);
            if (item != null)
            {
                item.SoLuong++;
            }
            else
            {
                cart.Add(new CartItem() { MatHang = mathang, SoLuong = 1 });
            }
            mathang.LuotMua += 1;
            mathang.SoLuong -= 1;

            _context.SaveChanges();
            SaveCartSession(cart);
            return RedirectToAction(nameof(ViewCart));
        }

        // Chuyển đến view xem giỏ hàng
        public IActionResult ViewCart()
        {
            GetData();
            return View(GetCartItems());
        }

        public IActionResult Gioithieu()
        {
            GetData();
            return View();
        }

        public IActionResult Cauhoi()
        {
            GetData();
            return View();
        }
        // Xóa một mặt hàng khỏi giỏ
        public IActionResult RemoveItem(int id)
        {
            var cart = GetCartItems();
            var item = cart.Find(p => p.MatHang.MaMh == id);
            if (item != null)
            {
                cart.Remove(item);
            }
            SaveCartSession(cart);
            return RedirectToAction(nameof(ViewCart));
        }

        // Cập nhật số lượng một mặt hàng trong giỏ
        public IActionResult UpdateItem(int id, int quantity)
        {
            var cart = GetCartItems();
            var item = cart.Find(p => p.MatHang.MaMh == id);
            if (item != null)
            {
                item.SoLuong = quantity;
            }
            SaveCartSession(cart);
            return RedirectToAction(nameof(ViewCart));
        }

        // Chuyển đến view thanh toán
        public IActionResult CheckOut()
        {
            GetData();
            return View(GetCartItems());
        }
        
        // Lập hóa đơn: lưu hóa đơn, lưu chi tiết hóa đơn
        [HttpPost, ActionName("CreateBill")]
        public async Task<IActionResult> CreateBill(string email, string hoten, string dienthoai, string diachi)
        {
            // Xử lý thông tin khách hàng (trường hợp khách mới)
            var kh = new Khachhang();
            kh.Email = email;
            kh.Ten = hoten;
            kh.DienThoai = dienthoai;
            _context.Add(kh);
            await _context.SaveChangesAsync();

            var hd = new Hoadon();
            hd.Ngay = DateTime.Now;
            hd.MaKh = kh.MaKh;
            _context.Add(hd);
            await _context.SaveChangesAsync();


            // thêm chi tiết hóa đơn
            var cart = GetCartItems();

            int thanhtien = 0;
            int tongtien = 0;
            foreach (var i in cart)
            {
                var ct = new Cthoadon();
                ct.MaHd = hd.MaHd;
                ct.MaMh = i.MatHang.MaMh;
                thanhtien = i.MatHang.GiaBan * i.SoLuong??1;
                tongtien += thanhtien;
                ct.DonGia = i.MatHang.GiaBan;
                ct.SoLuong = (short)i.SoLuong;
                ct.ThanhTien = thanhtien;
                _context.Add(ct);
            }
            await _context.SaveChangesAsync();

            // cập nhật tổng tiền hóa đơn
            hd.TongTien = tongtien;
            _context.Update(hd);
            await _context.SaveChangesAsync();

            // xóa giỏ hàng
            ClearCart();
            GetData();
            return View(hd);
        }

        //CÁC PHƯƠNG THỨC XÁC THỰC NGƯỜI DÙNG
        // GET: hiển thị form đăng ký
        public IActionResult Register()
        {
            GetData();
            return View();
        }
        // POST: xử lý dữ liệu đăng ký
        [HttpPost]
        public async Task<IActionResult> Register(string hoten, string dienthoai,
            string email, string matkhau)
        {
            Khachhang kh=new Khachhang();
            kh.Ten = hoten;
            kh.DienThoai = dienthoai;
            kh.Email = email;
            kh.MatKhau = _passwordHasher.HashPassword(kh, matkhau); //mã hóa mật khẩu
            if(ModelState.IsValid)
            {
                _context.Add(kh);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Loginrelate()
        {
            GetData();
            return View();
        }

        public IActionResult Huongdanmuahang()
        {
            GetData();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Loginrelate(string email, string matkhau)
        {
            var kh = await _context.Khachhangs
            .FirstOrDefaultAsync(m => m.Email == email);
            if (kh != null && _passwordHasher.VerifyHashedPassword(kh, kh.MatKhau, matkhau) == PasswordVerificationResult.Success)
            {
                //Đăng nhập thành công, thực hiện các hành động cần thiết
                //Ví dụ: Ghi thông tin người dùng vào Session
                HttpContext.Session.SetString("khachhang", kh.Email);
                return RedirectToAction(nameof(CustomerInfo));
            }
            return RedirectToAction(nameof(Loginrelate));
        }
        // GET: hiển thị form đăng nhập
        public IActionResult Login()
        {
            GetData();
            return View();
        }
        // POST: xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string email, string matkhau)
        {
            var kh = await _context.Khachhangs
            .FirstOrDefaultAsync(m => m.Email == email);
            if(kh != null && _passwordHasher.VerifyHashedPassword(kh, kh.MatKhau, matkhau) == PasswordVerificationResult.Success)
            {
                //Đăng nhập thành công, thực hiện các hành động cần thiết
                //Ví dụ: Ghi thông tin người dùng vào Session
                HttpContext.Session.SetString("khachhang", kh.Email);
                return RedirectToAction(nameof(CustomerInfo));
            }
            return RedirectToAction(nameof(Loginrelate));
        }


        //loginrelate
       
        //Trang hồ sơ khách hàng
        public IActionResult CustomerInfo()
        {
            GetData();
            return View();
        }

        public IActionResult Signout()
        {
            HttpContext.Session.SetString("khachhang", "");
            GetData();
            return RedirectToAction("Index");
        }
    }
}

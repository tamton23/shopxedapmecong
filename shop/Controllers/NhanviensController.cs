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
    public class NhanviensController : Controller
    {
    
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Nhanvien> _passwordHasher;
        public NhanviensController(ApplicationDbContext context, IPasswordHasher<Nhanvien> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        void GetData()
        {
            //đếm số mặt hàng trong giỏ hàng(sesion), ta có 2 cách 1. dùng ViewData["tên_biến"], 2.dùng ViewBag.tên_biến

            //Lấy tất cả danh mục có trong database
            ViewBag.danhmuc = _context.Danhmucs.ToList();
            ViewBag.Hoadons = _context.Hoadons.ToList();
            ViewBag.Khachhang = _context.Khachhangs.ToList();

            //lấy thông tin người dùng
            if (HttpContext.Session.GetString("nhanvien") != "")
            {
                ViewBag.nhanvien = _context.Nhanviens.FirstOrDefault(k => k.Email == HttpContext.Session.GetString("nhanvien"));
            }
        }
        // GET: Khachhangs
        public async Task<IActionResult> Indexkhachhang()
        {
            GetData();
            return View(await _context.Khachhangs.ToListAsync());
        }

        // GET: Khachhangs/Details/5
        public async Task<IActionResult> Detailskhachhang(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachhang = await _context.Khachhangs
                .FirstOrDefaultAsync(m => m.MaKh == id);
            if (khachhang == null)
            {
                return NotFound();
            }
            GetData();
            return View(khachhang);
        }

        // GET: Khachhangs/Create
        public IActionResult Createkhachhang()
        {
            GetData();
            return View();
        }

        // POST: Khachhangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Createkhachhang([Bind("MaKh,Ten,DienThoai,Email,MatKhau")] Khachhang khachhang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(khachhang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Indexkhachhang));
            }
            GetData();
            return View(khachhang);
        }

        // GET: Khachhangs/Edit/5
        public async Task<IActionResult> Editkhachhang(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachhang = await _context.Khachhangs.FindAsync(id);
            if (khachhang == null)
            {
                return NotFound();
            }
            GetData();
            return View(khachhang);
        }

        // POST: Khachhangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editkhachhang(int id, [Bind("MaKh,Ten,DienThoai,Email,MatKhau")] Khachhang khachhang)
        {
            if (id != khachhang.MaKh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khachhang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachhangExists(khachhang.MaKh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Indexkhachhang));
            }
            GetData();
            return View(khachhang);
        }

        // GET: Khachhangs/Delete/5
        public async Task<IActionResult> Deletekhachhang(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachhang = await _context.Khachhangs
                .FirstOrDefaultAsync(m => m.MaKh == id);
            if (khachhang == null)
            {
                return NotFound();
            }
            GetData();
            return View(khachhang);
        }

        // POST: Khachhangs/Delete/5
        [HttpPost, ActionName("Deletekhachhang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedkhachhang(int id)
        {
            var khachhang = await _context.Khachhangs.FindAsync(id);
            if (khachhang != null)
            {
                _context.Khachhangs.Remove(khachhang);
            }

            await _context.SaveChangesAsync();
            GetData();
            return RedirectToAction(nameof(Indexkhachhang));
        }

        private bool KhachhangExists(int id)
        {
            GetData();
            return _context.Khachhangs.Any(e => e.MaKh == id);
        }


        // hoa don
        public async Task<IActionResult> Indexhoadon()
        {
            GetData();
            return View(await _context.Hoadons.ToListAsync());
        }

        // GET: Hoadons/Details/5
        public async Task<IActionResult> Detailshoadon(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoadon = await _context.Hoadons
                .Include(h => h.MaKhNavigation)
                .FirstOrDefaultAsync(m => m.MaHd == id);
            if (hoadon == null)
            {
                return NotFound();
            }
            GetData();
            return View(hoadon);
        }

        // GET: Hoadons/Create
        public IActionResult Createhoadon()
        {
            ViewData["MaKh"] = new SelectList(_context.Khachhangs, "MaKh", "MaKh");
            GetData();
            return View();
        }

        // POST: Hoadons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Createhoadon")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Createhoadon([Bind("MaHd,Ngay,TongTien,MaKh,TrangThai")] Hoadon hoadon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hoadon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Indexhoadon));
            }
            ViewData["MaKh"] = new SelectList(_context.Khachhangs, "MaKh", "MaKh", hoadon.MaKh);
            GetData();
            return View(hoadon);
        }

        // GET: Hoadons/Edit/5
        public async Task<IActionResult> Edithoadon(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoadon = await _context.Hoadons.FindAsync(id);
            if (hoadon == null)
            {
                return NotFound();
            }
            ViewData["MaKh"] = new SelectList(_context.Khachhangs, "MaKh", "MaKh", hoadon.MaKh);
            GetData();
            return View(hoadon);
        }

        // POST: Hoadons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edithoadon")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edithoadon(int id, [Bind("MaHd,Ngay,TongTien,MaKh,TrangThai")] Hoadon hoadon)
        {
            if (id != hoadon.MaHd)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hoadon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoadonExists(hoadon.MaHd))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Indexhoadon));
            }
            ViewData["MaKh"] = new SelectList(_context.Khachhangs, "MaKh", "MaKh", hoadon.MaKh);
            GetData();
            return View(hoadon);
        }

        // GET: Hoadons/Delete/5
        public async Task<IActionResult> Deletehoadon(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoadon = await _context.Hoadons
                .Include(h => h.MaKhNavigation)
                .FirstOrDefaultAsync(m => m.MaHd == id);
            if (hoadon == null)
            {
                return NotFound();
            }
            GetData();
            return View(hoadon);
        }

        // POST: Hoadons/Delete/5
        [HttpPost, ActionName("Deletehoadon")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedhoadon(int id)
        {
            var hoadon = await _context.Hoadons.FindAsync(id);
            if (hoadon != null)
            {
                _context.Hoadons.Remove(hoadon);
            }

            await _context.SaveChangesAsync();
            GetData();
            return RedirectToAction(nameof(Indexhoadon));
        }

        private bool HoadonExists(int id)
        {
            GetData();
            return _context.Hoadons.Any(e => e.MaHd == id);
        }
        //mathang
        public async Task<IActionResult> Indexmathang()
        {
            GetData();
            return View(await _context.Mathangs.ToListAsync());
        }

        // GET: Mathangs/Details/5
        public async Task<IActionResult> Detailsmathang(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mathang = await _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMh == id);
            if (mathang == null)
            {
                return NotFound();
            }
            GetData();
            return View(mathang);
        }
        public string? Upload(IFormFile file)
        {
            string? uploadFileName = null;
            if (file != null)
            {
                uploadFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var path = $"wwwroot\\images\\{uploadFileName}";
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            GetData();
            return uploadFileName;
        }
        // GET: Mathangs/Create
        public IActionResult Createmathang()
        {
            ViewData["MaDm"] = new SelectList(_context.Danhmucs, "MaDm", "MaDm");
            GetData();
            return View();
        }

        // POST: Mathangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Createmathang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Createmathang(IFormFile HinhAnh, [Bind("MaMh,Ten,GiaGoc,GiaBan,SoLuong,MoTa,HinhAnh,MaDm,LuotXem,LuotMua")] Mathang mathang)
        {
            if (ModelState.IsValid)
            {
                mathang.HinhAnh = Upload(HinhAnh);
                mathang.LuotMua = 0;
                mathang.LuotXem = 0;
                _context.Add(mathang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Indexmathang));
            }
            ViewData["MaDm"] = new SelectList(_context.Danhmucs, "MaDm", "MaDm", mathang.MaDm);
            GetData();
            return View(mathang);
        }

        // GET: Mathangs/Edit/5
        public async Task<IActionResult> Editmathang(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mathang = await _context.Mathangs.FindAsync(id);
            if (mathang == null)
            {
                return NotFound();
            }
            ViewData["MaDm"] = new SelectList(_context.Danhmucs, "MaDm", "MaDm", mathang.MaDm);
            GetData();
            return View(mathang);
        }

        // POST: Mathangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Editmathang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editmathang(IFormFile HinhAnh, int id, [Bind("MaMh,Ten,GiaGoc,GiaBan,SoLuong,MoTa,HinhAnh,MaDm,LuotXem,LuotMua")] Mathang mathang)
        {
            if (id != mathang.MaMh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mathang.HinhAnh = Upload(HinhAnh);
                    _context.Update(mathang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MathangExists(mathang.MaMh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Indexmathang));
            }
            ViewData["MaDm"] = new SelectList(_context.Danhmucs, "MaDm", "MaDm", mathang.MaDm);
            GetData();
            return View(mathang);
        }

        // GET: Mathangs/Delete/5
        public async Task<IActionResult> Deletemathang(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mathang = await _context.Mathangs
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMh == id);
            if (mathang == null)
            {
                return NotFound();
            }
            GetData();
            return View(mathang);
        }

        // POST: Mathangs/Delete/5
        [HttpPost, ActionName("Deletemathang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedmathang(int id)
        {
            var mathang = await _context.Mathangs.FindAsync(id);
            if (mathang != null)
            {
                _context.Mathangs.Remove(mathang);
            }
            GetData();
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Indexmathang));
        }

        private bool MathangExists(int id)
        {
            GetData();
            return _context.Mathangs.Any(e => e.MaMh == id);
        }

        //Danh muc
        public async Task<IActionResult> Indexdanhmuc()
        {
            GetData();
            return View(await _context.Danhmucs.ToListAsync());
        }
        // GET: Danhmucs/Details/5
        public async Task<IActionResult> Detailsdanhmuc(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhmuc = await _context.Danhmucs
                .FirstOrDefaultAsync(m => m.MaDm == id);
            if (danhmuc == null)
            {
                return NotFound();
            }
            GetData();
            return View(danhmuc);
        }
        // GET: Danhmucs/Create
        public IActionResult Createdanhmuc()
        {
            GetData();
            return View();
        }

        // POST: Danhmucs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Createdanhmuc([Bind("MaDm,Ten")] Danhmuc danhmuc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhmuc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Indexdanhmuc));
            }
            GetData();
            return View(danhmuc);
        }

        // GET: Danhmucs/Delete/5
        public async Task<IActionResult> Deletedanhmuc(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhmuc = await _context.Danhmucs
                .FirstOrDefaultAsync(m => m.MaDm == id);
            if (danhmuc == null)
            {
                return NotFound();
            }
            GetData();
            return View(danhmuc);
        }

        // POST: Danhmucs/Delete/5
        [HttpPost, ActionName("Deletedanhmuc")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmeddanhmuc(int id)
        {
            var danhmuc = await _context.Danhmucs.FindAsync(id);
            if (danhmuc != null)
            {
                _context.Danhmucs.Remove(danhmuc);
            }

            await _context.SaveChangesAsync();
            GetData();
            return RedirectToAction(nameof(Indexdanhmuc));
        }

        private bool DanhmucExists(int id)
        {
            GetData();
            return _context.Danhmucs.Any(e => e.MaDm == id);
        }
        // GET: Danhmucs/Edit/5
        public async Task<IActionResult> Editdanhmuc(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhmuc = await _context.Danhmucs.FindAsync(id);
            if (danhmuc == null)
            {
                return NotFound();
            }
            GetData();
            return View(danhmuc);
        }

        // POST: Danhmucs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editdanhmuc(int id, [Bind("MaDm,Ten")] Danhmuc danhmuc)
        {
            if (id != danhmuc.MaDm)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhmuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhmucExists(danhmuc.MaDm))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Indexdanhmuc));
            }
            GetData();
            return View(danhmuc);
        }




        // GET: Nhanviens
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Nhanviens.Include(n => n.MaCvNavigation);
            GetData();
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Nhanviens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanvien = await _context.Nhanviens
                .Include(n => n.MaCvNavigation)
                .FirstOrDefaultAsync(m => m.MaNv == id);
            if (nhanvien == null)
            {
                return NotFound();
            }
            GetData();
            return View(nhanvien);
        }
        /// <summary>
        public IActionResult Loginrelate()
        {
            GetData();
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Loginrelate(string email, string matkhau)
        {
            var nv = await _context.Nhanviens
            .FirstOrDefaultAsync(m => m.Email == email);
            if (nv != null && _passwordHasher.VerifyHashedPassword(nv, nv.MatKhau, matkhau) == PasswordVerificationResult.Success)
            {
                //Đăng nhập thành công, thực hiện các hành động cần thiết
                //Ví dụ: Ghi thông tin người dùng vào Session
                HttpContext.Session.SetString("nhanvien", nv.Email);
                return RedirectToAction(nameof(CustomerInfo));
            }
            GetData();
            return RedirectToAction(nameof(Loginrelate));
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
        public async Task<IActionResult> Register(string hoten, string dienthoai, int macv, 
            string email, string matkhau)
        {
            Nhanvien kh = new Nhanvien();
            kh.Ten = hoten;
            kh.DienThoai = dienthoai;
            kh.MaCv = macv;
            kh.Email = email;
            kh.MatKhau = _passwordHasher.HashPassword(kh, matkhau); //mã hóa mật khẩu
            if (ModelState.IsValid)
            {
                _context.Add(kh);
                await _context.SaveChangesAsync();
            }
            GetData();
            return RedirectToAction(nameof(Login));
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
            var nv = await _context.Nhanviens
            .FirstOrDefaultAsync(m => m.Email == email);
            GetData();
            if (nv != null && _passwordHasher.VerifyHashedPassword(nv, nv.MatKhau, matkhau) == PasswordVerificationResult.Success)
            {
                //Đăng nhập thành công, thực hiện các hành động cần thiết
                //Ví dụ: Ghi thông tin người dùng vào Session
                HttpContext.Session.SetString("nhanvien",  nv.Email);
                return RedirectToAction(nameof(CustomerInfo));
            }

            return RedirectToAction(nameof(Loginrelate));
        }


        //loginrelate

        //Trang hồ sơ khách hàng
        public async Task<IActionResult> CustomerInfo()
        {
            GetData();
            return View(await _context.Khachhangs.ToListAsync());
        }

        /// </summary>
        /// <returns></returns>

        // GET: Nhanviens/Create
        public IActionResult Create()
        {
            ViewData["MaCv"] = new SelectList(_context.Chucvus, "MaCv", "MaCv");
            GetData();
            return View();
        }

        // POST: Nhanviens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNv,Ten,MaCv,DienThoai,Email,MatKhau")] Nhanvien nhanvien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhanvien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaCv"] = new SelectList(_context.Chucvus, "MaCv", "MaCv", nhanvien.MaCv);
            GetData();
            return View(nhanvien);
        }

        // GET: Nhanviens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanvien = await _context.Nhanviens.FindAsync(id);
            if (nhanvien == null)
            {
                return NotFound();
            }
            ViewData["MaCv"] = new SelectList(_context.Chucvus, "MaCv", "MaCv", nhanvien.MaCv);
            GetData();
            return View(nhanvien);
        }

        // POST: Nhanviens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNv,Ten,MaCv,DienThoai,Email,MatKhau")] Nhanvien nhanvien)
        {
            if (id != nhanvien.MaNv)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanvien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanvienExists(nhanvien.MaNv))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaCv"] = new SelectList(_context.Chucvus, "MaCv", "MaCv", nhanvien.MaCv);
            GetData();
            return View(nhanvien);
        }

        // GET: Nhanviens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanvien = await _context.Nhanviens
                .Include(n => n.MaCvNavigation)
                .FirstOrDefaultAsync(m => m.MaNv == id);
            if (nhanvien == null)
            {
                return NotFound();
            }
            GetData();
            return View(nhanvien);
        }

        // POST: Nhanviens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanvien = await _context.Nhanviens.FindAsync(id);
            if (nhanvien != null)
            {
                _context.Nhanviens.Remove(nhanvien);
            }

            await _context.SaveChangesAsync();
            GetData();
            return RedirectToAction(nameof(Index));
        }

        private bool NhanvienExists(int id)
        {
            GetData();
            return _context.Nhanviens.Any(e => e.MaNv == id);
        }
    }
}

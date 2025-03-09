using cellphones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace cellphones.Controllers
{
    public class AccountController : Controller
    {
        SGTechEntities db1 = new SGTechEntities();
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(User user)
        {
            var check = db1.Users.Where(s => s.Email == user.Email && s.Password == user.Password).FirstOrDefault();

            if(check == null)
            {
                ViewBag.ErrorInfo = "tai khoan khong ton tai";
                return View();
            }

            Session["name"] = check.Username;
            Session["id"] = check.UserID;

            if(check.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if(check.Role =="nhanvien")
            {
                return RedirectToAction("Index","Admin");
            }
            else
            {
                return RedirectToAction("IndexUser", "Product");
            }
           
        }
        //logout
        public ActionResult logout()
        {
            Session.Abandon();

            return RedirectToAction("Index");
        }
        //SginUp
        public ActionResult SginUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SginUp(User _user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db1.Users.FirstOrDefault(s =>
                s.Username == _user.Username &&
                s.Email == _user.Email);

                if (existingUser != null)
                {
                    ViewBag.Errorinfo = "Tài khoản hoặc email đã tồn tại";
                    return View();
                }

                try
                {
                    _user.UserID = Guid.NewGuid().ToString("N").Substring(0, 8);
                    _user.Role = "Customer";
                    _user.spent = 0;
                    db1.Users.Add(_user);
                    db1.SaveChanges();
                    return RedirectToAction("IndexUser", "Product");
                }
                catch (Exception ex)
                {
                    ViewBag.Errorinfo = "Có lỗi xảy ra: " + ex.Message;
                    return View();
                }
            }
            return View();
        }
        //ForgetPassword
        public ActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPassword(string Password)
        {
            if (string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Vui lòng nhập Email.";
                return View();
            }

            var matchingAccounts = db1.Users.Where(e => e.Email == Password).ToList();

            if (matchingAccounts.Count == 0)
            {
                ViewBag.Error = "Không tìm thấy tài khoản này.";
                return View();
            }

            var user = matchingAccounts.FirstOrDefault();
            var email = user.Email; // Lấy email của người dùng
            var resetLink = Url.Action("UpdatePassword", "Account", new { email = email }, Request.Url.Scheme);
            var mailService = new MailService();
            Task.Run(() => mailService.SendMailAsync(email, "Cập Nhật Mật Khẩu", $"Nhấn <a href='{resetLink}'>vào đây</a> để cập nhật mật khẩu của bạn."));

            ViewBag.Success = "Liên kết cập nhật mật khẩu đã được gửi đến email của bạn.";
            return View();
        }

        // GET: Cập Nhật Mật Khẩu
        public ActionResult UpdatePassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return HttpNotFound();
            }

            var user = db1.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản.";
                return View();
            }

            ViewBag.Email = email; // Lưu email vào ViewBag để truyền vào form
            return View(user);
        }

        [HttpPost]
        public ActionResult UpdatePassword(string email, string newPassword, string confirmPassword)
        {
            // Kiểm tra điều kiện hợp lệ
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                ViewBag.Email = email; // Trả lại email vào form
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu không khớp. Vui lòng nhập lại.";
                ViewBag.Email = email; // Trả lại email vào form
                return View();
            }

            // Tìm người dùng bằng email
            var user = db1.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản.";
                return View();
            }

            // Cập nhật mật khẩu
            user.Password = newPassword; // Mã hóa mật khẩu trước khi lưu nếu cần
            db1.SaveChanges();

            ViewBag.Success = "Cập nhật mật khẩu thành công.";
            return RedirectToAction("Index"); // Chuyển hướng về trang khác nếu cần
        }

    }
}
using cellphones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cellphones.Controllers
{
    public class ChatController : Controller
    {
        // GET: Chat
        SGTechEntities db = new SGTechEntities();

        // Hiển thị danh sách người dùng để chat
        public ActionResult Index()
        {
            var currentUserId = GetCurrentUserId();
            var users = db.Users
                .Where(u => u.UserID != currentUserId && u.Role == "Customer")
                .ToList();

            return View(users);
        }

        public ActionResult MessagesAdmin(string receiverId)
        {
            var currentUserId = GetCurrentUserId();

            var messages = db.Messages
             .Where(m =>
            (m.SenderID == receiverId && m.ReceiverID == currentUserId) ||
            (m.SenderID == currentUserId && m.ReceiverID == receiverId))
             .OrderBy(m => m.Timestamp)
             .ToList();

            ViewBag.ReceiverId = receiverId;
            return View(messages);
        }

        // Gửi tin nhắn
        [HttpPost]
        public ActionResult SendMessageAdmin(string receiverId, string content)
        {
            var message = new Message
            {
                MessageID = Guid.NewGuid().ToString("N").Substring(0, 8),
                SenderID = GetCurrentUserId(),
                ReceiverID = receiverId,
                Content = content,
                Timestamp = DateTime.Now
            };

            db.Messages.Add(message);
            db.SaveChanges();

            return RedirectToAction("MessagesAdmin", new { receiverId = receiverId });
        }
        // Xem tin nhắn với một người dùng cụ thể
        public ActionResult Messages()
        {
            string receiverId = "18189b2d";
            var currentUserId = GetCurrentUserId();

            var messages = db.Messages
                .Where(m =>
                    (m.SenderID == currentUserId && m.ReceiverID == receiverId) ||
                    (m.SenderID == receiverId && m.ReceiverID == currentUserId))
                .OrderBy(m => m.Timestamp)
                .ToList();

            ViewBag.ReceiverId = receiverId;
            return View(messages);
        }

        // Gửi tin nhắn
        [HttpPost]
        public ActionResult SendMessage(string receiverId, string content)
        {
            var message = new Message
            {
                MessageID = Guid.NewGuid().ToString("N").Substring(0, 8),
                SenderID = GetCurrentUserId(),
                ReceiverID = receiverId,
                Content = content,
                Timestamp = DateTime.Now
            };

            db.Messages.Add(message);
            db.SaveChanges();

            return RedirectToAction("Messages");
        }

        private string GetCurrentUserId()
        {
            return Session["id"] as string ?? "DefaultUserId";
        }
    }
}
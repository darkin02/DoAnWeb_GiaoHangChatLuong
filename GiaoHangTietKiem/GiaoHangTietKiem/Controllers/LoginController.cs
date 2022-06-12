﻿using Common1;
using GiaoHangTietKiem.Controllers.Model;
using GiaoHangTietKiem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GiaoHangTietKiem.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            LoginModel lg = new LoginModel();
            return View(lg);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            UserKH tk = Dataprovider.Instance.DB.UserKHs.FirstOrDefault(p => p.SDT.Equals(model.UserName) && p.MatKhau.Equals(model.Password));
            if (tk != null)
            {
                var userSession = new UserLogin();
                userSession.UserName = tk.UserName;
                userSession.UserID = tk.MaKH;
                Session.Add(Common.Common.USER_SESSION, userSession);
                Session["UserName"] = tk.UserName;
                return RedirectToAction("Index", "GiaoHang");
            }
            else
            {
                ModelState.AddModelError("", "mk sai");
            }
            return View(model);
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(DangKy model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var temp = Dataprovider.Instance.DB.KhachHangs.FirstOrDefault(p => p.SDT.Equals(model.SDT1));
            if (temp != null)
            {
                SetAlert("Số điện thoại đã có, Vui lòng nhập số khác!" + model.DiaChi1, "error");
                return View(model);
            }
            string content = System.IO.File.ReadAllText(Server.MapPath("~/template/newoder.html"));
            content = content.Replace("{{CustomerName}}", model.TenKH1);
            content = content.Replace("{{Phone}}", model.SDT1);
            content = content.Replace("{{Email}}", model.Email1);
            content = content.Replace("{{Address}}", model.DiaChi1);
            String SMS;
            Random ramdom = new Random();
            SMS = ramdom.Next(100000, 999999).ToString();
            content = content.Replace("{{SMS}}", SMS);
            Session["SMS"] = SMS;
            new MailHelper().SendMail(model.Email1, "Đăng ký mới từ web", content);
            Session["DangKy"] = model;
            return RedirectToAction("ConfimRegister");
        }
        public ActionResult ConfimRegister()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfimRegister(string sms)
        {
            if (Session["SMS"].Equals(sms))
            {
                DangKy model = (DangKy)Session["DangKy"];
                KhachHang kh = new KhachHang(model.TenKH1, model.SDT1, model.DiaChi1, model.GioiTinh1.Equals("Nam") ? true : false);
                string s = string.Format("INSERT dbo.KhachHang( MaKH,TenKH, SDT, DiaChi, GioiTinh)VALUES( DEFAULT, N'{0}', '{1}', N'{2}', {3})", kh.TenKH, kh.SDT, kh.DiaChi, kh.GioiTinh == true ? 1 : 0);
                Dataprovider.Instance.DB.Database.ExecuteSqlCommand(s);
                Dataprovider.Instance.DB.SaveChanges();
                string makh = Dataprovider.Instance.DB.KhachHangs.FirstOrDefault(p => p.SDT.Equals(model.SDT1)).MaKH;
                makh.Replace(" ", "");
                UserKH user = new UserKH(model.SDT1, model.MatKhau1, model.Email1, makh, model.UserName1);
                Dataprovider.Instance.DB.UserKHs.Add(user);
                Dataprovider.Instance.DB.SaveChanges();
                SetAlert("Tạo tài khoản thành công ", "success");
                return RedirectToAction("Register");
            }
            SetAlert("Mã xác nhận không đúng", "error");
            return View();
        }
        protected void SetAlert(string mess, string type)
        {
            TempData["AlretMessage"] = mess;
            if (type == "success")
            {
                TempData["AlretType"] = "alret-success";
            }
            else if (type == "warning")
            {
                TempData["AlretType"] = "alret-warning";
            }
            else if (type == "error")
            {
                TempData["AlretType"] = "alret-danger";
            }
        }
    }

}
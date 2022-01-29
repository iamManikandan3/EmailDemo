using EmailDemo.Data;
using EmailDemo.Helpers;
using EmailDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly emailContext _context;
        public IConfiguration configuration;

        public AccountController(emailContext context, IConfiguration config)
        {
            _context = context;
            configuration = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(register reg)
        {
            reg.Password = BCrypt.Net.BCrypt.HashPassword(reg.Password.Trim()); //skip this line.
            reg.Status = false;
            reg.SecurityCode = RandomHelper.RandomString(6); //Creating a random string of length 6.
            reg.CreationDate = DateTime.Now;

            //Saving the record to the database.
            _context.register.Add(reg);
            _context.SaveChanges();

            //Sending Email
            var mailHelper = new MainHelper(configuration);
            string content = "Security Code: " + reg.SecurityCode;
            mailHelper.Send(configuration["Gmail:Username"], reg.Email, "Clinic Management System", content);

            //Saving the username in session.
            HttpContext.Session.SetString("username", reg.Username);
            return RedirectToAction("Active");
        }

        public IActionResult Active()
        {
            ViewBag.username = HttpContext.Session.GetString("username");
            return View();
        }

        public IActionResult Resend()
        {
            var username = HttpContext.Session.GetString("username");
            var register = _context.register.SingleOrDefault(a => a.Username == username);
            register.SecurityCode = RandomHelper.RandomString(6);
            register.CreationDate = DateTime.Now;

            // Saving the updated values to the database.
            _context.SaveChanges();

            //Sending Email
            var mailHelper = new MainHelper(configuration);
            string content = "Security Code: " + register.SecurityCode;
            mailHelper.Send(configuration["Gmail:Username"], register.Email, "Clinic Management System", content);

            return RedirectToAction("Active");
        }

        [HttpPost]
        public IActionResult Active(string securitycode)
        {
            var username = HttpContext.Session.GetString("username");
            var register = _context.register.SingleOrDefault(a => a.Username == username);
            int seconds = DateTime.Now.Subtract(register.CreationDate).Seconds;
            
            if(register.SecurityCode == securitycode && seconds <= 120) //Make sure the security code correct and not expired.
            {
                register.Status = true; //Enable the user account active.
                _context.SaveChanges(); //Make changes in the database.
                return RedirectToAction("login");
            }
            else
            {
                ViewBag.username = HttpContext.Session.GetString("username");
                ViewBag.msg = "Invalid";
                return View();
            }
        }
    }
}

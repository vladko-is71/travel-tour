using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace Travel2021.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var connectionString = WebConfigurationManager.AppSettings["ConnectionString"];

            ViewBag.test = false;

            using (SqlConnection conn = new SqlConnection(connectionString)) {

                ViewBag.test = "The database is connected properly.";

            }

            return View();
        }

        public ActionResult SignInOrRegister()
        {
            return View();
        }

        public ActionResult ShowTour()
        {
            ViewBag.markettitle = "Best tour EVER!";
            ViewBag.destination = "Tirana, Albania";
            ViewBag.hotel = "Best Hotel Ever";
            ViewBag.dateFirst = "20 January 2022";
            ViewBag.dateSecond = "30 January 2022";
            ViewBag.price = 10000;
            return View();
        }

    }
}
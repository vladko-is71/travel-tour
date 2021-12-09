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
        public bool loggedin = false;
        private string username = null;


        private string GetConnectionString()
        {
            string connectionString = @"Data Source=.\SQLEXPRESS;
                          AttachDbFilename=D:\source\Travel2021\Travel2021\App_Data\Travel3.mdf;
                          Integrated Security=True;
                          Connect Timeout=30;
                          User Instance=True";

            return connectionString;
        }

        private Dictionary<int, string> GetPlaces()
        {
            var con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = GetConnectionString();
            con.Open();

            SqlCommand command = new SqlCommand("SELECT id, directionname from Directions", con);
            var reader = command.ExecuteReader();

            var places = new Dictionary<int, string>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    places[reader.GetInt32(0)] = reader.GetString(1).Trim();
                }
            }
            reader.Close();
            con.Close();

            return places;
        }


        public ActionResult Index()
        {
            ViewBag.places = GetPlaces();
            ViewBag.loggedin = loggedin;
            return View();
        }

        public ActionResult SignInOrRegister()
        {
            return View();
        }

        public ActionResult ShowTour(int hotelid, string hotel, string dateFirst, string dateSecond, int price, int flight1id, int flight2id)
        {
            ViewBag.hotelid = hotelid;
            ViewBag.hotel = hotel;
            ViewBag.dateFirst = dateFirst;
            ViewBag.dateSecond = dateSecond;
            ViewBag.price = price;
            ViewBag.loggedin = loggedin;
            ViewBag.flight1id = flight1id;
            ViewBag.flight2id = flight2id;
            return View();
        }

        public ActionResult ViewOptions(int place, DateTime start, DateTime end, int? low, int? high, int sorter)
        {
            var con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = GetConnectionString();
            con.Open();

            string flight1 = null;
            string flight2 = null;

            int? flight1id = null;
            int? flight2id = null;

            SqlCommand findFlights = new SqlCommand($"SELECT flightnr, id from Flights where directionid = {place} and way = 1", con);
            var reader1 = findFlights.ExecuteReader();

            while (reader1.Read())
            {
                flight1 = reader1.GetString(0).Trim();
                flight1id = reader1.GetInt32(1);
                break;
            }

            reader1.Close();
            ViewBag.flight = flight1;
            ViewBag.flightId = flight1id;

            SqlCommand findFlightsBack = new SqlCommand($"SELECT flightnr, id from Flights where directionid = {place} and way = 2", con);
            var reader2 = findFlightsBack.ExecuteReader();

            while (reader2.Read())
            {
                flight2 = reader2.GetString(0).Trim();
                flight2id = reader2.GetInt32(1);
                break;
            }

            reader2.Close();
            ViewBag.flightBack = flight2;
            ViewBag.flightBackId = flight2id;

            SqlCommand findHotels = new SqlCommand($"SELECT id, hotelname from Hotels where directionid = {place}", con);
            var reader = findHotels.ExecuteReader();

            var hotels = new Dictionary<int, Tuple<string, int>>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int price = 1000 + 1000;
                    var days = (end - start).Days;
                    price += (days * 1000);

                    bool includeHotelOnLow = (low == null || low <= price);
                    bool includeHotelOnHigh = (high == null || price <= high);

                    if (includeHotelOnLow && includeHotelOnHigh)
                    {
                        hotels[reader.GetInt32(0)] = new Tuple<string, int>(reader.GetString(1).Trim(), price);
                    }
                }
            }

            var hotelsIdInOrder = new List<int>(); // will contain IDs of hotels!
            var prices = new List<int>();
            foreach (var t in hotels)
            {
                hotelsIdInOrder.Add(t.Key);
                prices.Add(t.Value.Item2);
            }
            if (sorter == 1)
            {
                int index = 0;
                hotelsIdInOrder = hotelsIdInOrder.OrderBy(d => prices[index++]).ToList();
            }
            else if (sorter == 2)
            {
                int index = 0;
                hotelsIdInOrder = hotelsIdInOrder.OrderByDescending(d => prices[index++]).ToList();
            }

            reader.Close();

            con.Close();
            ViewBag.hotels = hotels;
            ViewBag.hotelsIdInOrder = hotelsIdInOrder;
            ViewBag.dateFirst = start.ToString().Substring(0, 10);
            ViewBag.dateSecond = end.ToString().Substring(0, 10);
            
            return View();
        }

        [HttpPost]
        public ActionResult RegistrationSuccessful(string fname, string lname, string email, string password, string confirmation)
        {
            if (fname.Length == 0 || lname.Length == 0 || email.Length == 0 || password.Length == 0)
                return View("~/Views/Shared/Error.cshtml");

            if (!email.Contains("@"))
                return View("~/Views/Shared/Error.cshtml");

            var con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = GetConnectionString();
            con.Open();

            SqlCommand emailSearch = new SqlCommand($"SELECT id, email from Customers where email = \'{email}\'", con);
            var reader1 = emailSearch.ExecuteReader();

            while (reader1.Read())
            {
                return View("~/Views/Shared/Error.cshtml");
            }

            reader1.Close();

            if (password != confirmation)
                return View("~/Views/Shared/Error.cshtml");

            SqlCommand maxSearch = new SqlCommand($"SELECT MAX(id) from Customers", con);
            var value = Convert.ToInt32(maxSearch.ExecuteScalar());


            SqlCommand nq = new SqlCommand("INSERT INTO Customers(id, firstname, lastname, email, pwd) "
                + $"VALUES ({value + 1}, \'{fname}\', \'{lname}\', \'{email}\', \'{password}\')", con);
            nq.ExecuteNonQuery();


            con.Close();

            ViewBag.places = GetPlaces();
            ViewBag.loggedin = loggedin;

            return View("~/Views/Home/Index.cshtml");
        }


        [HttpPost]
        public ActionResult LoginSuccessful(string email_signin, string password_signin)
        {
            bool success = false;

            var con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = GetConnectionString();
            con.Open();

            SqlCommand emailSearch = new SqlCommand("SELECT id, email from Customers "
                + $"where email = \'{email_signin}\' and pwd = \'{password_signin}\'", con);
            var reader1 = emailSearch.ExecuteReader();

            while (reader1.Read())
            {
                success = true;
                loggedin = true;
                username = email_signin;
            }

            reader1.Close();

            con.Close();

            ViewBag.places = GetPlaces();
            ViewBag.loggedin = loggedin;

            if (success)
                return View("~/Views/Home/Index.cshtml");
            else
                return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult LogOut()
        {
            loggedin = false;
            username = null;
            return View();
        }

        public ActionResult Book(int hotelid, string dateFirst, string dateSecond, int price, int flight1id, int flight2id, string phone)
        {
            var con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = GetConnectionString();
            con.Open();

            int customerId = -1;

            SqlCommand emailSearch = new SqlCommand($"SELECT id from Customers where email = \'{username}\'", con);
            var reader1 = emailSearch.ExecuteReader();

            while (reader1.Read())
            {
                customerId = reader1.GetInt32(0);
                break;
            }

            reader1.Close();


            SqlCommand maxSearch = new SqlCommand($"SELECT MAX(id) from BookedTours", con);
            var value = Convert.ToInt32(maxSearch.ExecuteScalar());

            SqlCommand nq = new SqlCommand("INSERT INTO BookedTours(id, departure, arrival, flightid, flightbackid, hotelid, customerid, price, orderstatus) "
                + $"VALUES ({value + 1}, \'{dateFirst}\', \'{dateSecond}\', \'{flight1id}\', \'{flight2id}\', \'{hotelid}\', \'{customerId}\', \'{price}\', 0)", con);
            nq.ExecuteNonQuery();

            SqlCommand count = new SqlCommand($"SELECT COUNT(id) from BookedTours", con);
            ViewBag.counted = Convert.ToInt32(count.ExecuteScalar());

            con.Close();

            return View();
        }
    }
}
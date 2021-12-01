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
            return View();
        }

        public ActionResult SignInOrRegister()
        {
            return View();
        }

        public ActionResult ShowTour(string hotel, string dateFirst, string dateSecond, int price)
        {
            ViewBag.hotel = hotel;
            ViewBag.dateFirst = dateFirst;
            ViewBag.dateSecond = dateSecond;
            ViewBag.price = price;
            return View();
        }

        public ActionResult ViewOptions(int place, DateTime start, DateTime end, int? low, int? high)
        {
            var con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = GetConnectionString();
            con.Open();

            string flight1 = null;
            string flight2 = null;

            SqlCommand findFlights = new SqlCommand($"SELECT flightnr from Flights where directionid = {place} and way = 1", con);
            var reader1 = findFlights.ExecuteReader();

            while (reader1.Read())
            {
                flight1 = reader1.GetString(0).Trim();
                break;
            }

            reader1.Close();
            ViewBag.flight = flight1;

            SqlCommand findFlightsBack = new SqlCommand($"SELECT flightnr from Flights where directionid = {place} and way = 2", con);
            var reader2 = findFlightsBack.ExecuteReader();

            while (reader2.Read())
            {
                flight2 = reader2.GetString(0).Trim();
                break;
            }

            reader2.Close();
            ViewBag.flightBack = flight2;

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

            reader.Close();

            con.Close();
            ViewBag.hotels = hotels;
            ViewBag.dateFirst = start.ToString().Substring(0, 10);
            ViewBag.dateSecond = end.ToString().Substring(0, 10);
            return View();
        }

        [HttpPost]
        public ActionResult RegistrationSuccessful(string fname, string lname, string email, string password, string confirmation)
        {
            bool result = true;

            var con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = GetConnectionString();
            con.Open();

            SqlCommand emailSearch = new SqlCommand($"SELECT id, email from Customers where email = \'{email}\'", con);
            var reader1 = emailSearch.ExecuteReader();

            while (reader1.Read())
            {
                result = false;
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

            return View("~/Views/Home/Index.cshtml");
        }

    }
}
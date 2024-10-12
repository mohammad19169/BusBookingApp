using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ExpressBookerProject.Models;
using ExpressBookerProject.Services;

namespace ExpressBookerProject.Controllers
{
    public class UserController : Controller
    {
        private readonly BookingFacade _facade;

        public UserController()
        {
            _facade = new BookingFacade();
        }

     
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(user model)
        {
            if (ModelState.IsValid)
            {
                var user = _facade.AuthenticateUser(model.username, model.password);
                if (user != null)
                {
                    Session["UserID"] = user.userid;  // Store user ID in session
                    return RedirectToAction("BusSchedule", "User");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid username or password.";
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult BusSchedule()
        {
            var schedules = _facade.GetBusSchedules();
            return View(schedules);
        }

        public ActionResult BookSeats(int id)
        {
            var schedule = _facade.GetBusSchedule(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }

            ViewBag.AvailableSeats = _facade.GetAvailableSeats(schedule);
            return View(schedule);
        }

        [HttpPost]
        public ActionResult BookSeats(int scheduleId, int seats)
        {
            var schedule = _facade.GetBusSchedule(scheduleId);
            if (schedule == null)
            {
                return HttpNotFound();
            }

            var availableSeats = _facade.GetAvailableSeats(schedule);
            if (seats > availableSeats)
            {
                ModelState.AddModelError("", "Not enough available seats.");
                ViewBag.AvailableSeats = availableSeats;
                return View(schedule);
            }

            var totalPrice = _facade.CalculatePrice(schedule) * seats;

            ViewBag.ScheduleId = schedule.scheduleid;
            ViewBag.BusNumber = schedule.bus.busnumber;
            ViewBag.DepartureTime = schedule.departuretime;
            ViewBag.ArrivalTime = schedule.arrivaltime;
            ViewBag.Source = schedule.route.source;
            ViewBag.Destination = schedule.route.destination;
            ViewBag.NumSeats = seats;
            ViewBag.TotalPrice = totalPrice;

            return View("ConfirmBooking");
        }

        [HttpPost]
        public ActionResult ConfirmBooking(int scheduleId, int numSeats)
        {
            int userId = (int)Session["UserID"]; // Retrieve the actual user ID from session

            var success = _facade.BookSeats(userId, scheduleId, numSeats, out var bookedSeatNumbers);
            if (!success)
            {
                ModelState.AddModelError("", "Not enough available seats.");
                return View("Error");
            }

            return RedirectToAction("Payment", new { scheduleId, numSeats, bookedSeatNumbers });
        }

        public ActionResult Payment(int scheduleId, int numSeats)
        {
            ViewBag.ScheduleId = scheduleId;
            ViewBag.NumSeats = numSeats;
            return View();
        }

        [HttpPost]
        public ActionResult Payment(int scheduleId, int numSeats, string paymentMethod)
        {
            int userId = (int)Session["UserID"]; // Retrieve the actual user ID from session

            var success = _facade.ProcessPayment(userId, scheduleId, numSeats, paymentMethod);
            if (!success)
            {
                return View("Error");
            }

            return RedirectToAction("BookingSuccess");
        }

        public ActionResult BookingSuccess()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session["UserID"] = null; // Clear the session
            return RedirectToAction("Login", "User");
        }
    }
}

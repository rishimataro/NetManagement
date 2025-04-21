using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetManagement.Data;
using NetManagement.Services;

namespace NetManagement.Controllers
{
    public class StatisticsController : BaseController
    {
        private readonly StatisticsService _statisticsService;
        private readonly NetManagementContext _context;

        public StatisticsController(NetManagementContext context, StatisticsService statisticsService)
        {
            _context = context;
            _statisticsService = statisticsService;
        }

        [HttpGet]
        public IActionResult RevenueByTime(DateTime? startDate, DateTime? endDate, string period = "daily", string sortBy = "StartTime", bool ascending = true)
        {
            // Set default dates if not provided
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            // Validate dates
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be later than end date.");
                return View();
            }

            // Validate date range (max 1 year)
            if ((endDate.Value - startDate.Value).TotalDays > 365)
            {
                ModelState.AddModelError("", "Date range cannot exceed 1 year.");
                return View();
            }

            try
            {
                var revenue = _statisticsService.GetTotalRevenue(startDate.Value, endDate.Value);
                var orders = _statisticsService.GetOrdersByTime(startDate.Value, endDate.Value, sortBy, ascending);
                var revenueByPeriod = _statisticsService.GetRevenueByTimePeriod(startDate.Value, endDate.Value, period);

                ViewBag.Revenue = revenue;
                ViewBag.Orders = orders;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.Period = period;
                ViewBag.SortBy = sortBy;
                ViewBag.Ascending = ascending;
                ViewBag.RevenueByPeriod = revenueByPeriod;

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while retrieving data.");
                // Log the exception
                return View();
            }
        }

        [HttpGet]
        public IActionResult RevenueByComputer(DateTime? startDate, DateTime? endDate, int? computerId, string period = "daily")
        {
            // Set default dates if not provided  
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            // Get list of computers for dropdown  
            ViewBag.Computers = _context.Computer.ToList();

            // Validate dates  
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be later than end date.");
                return View();
            }

            // Validate date range (max 1 year)  
            if ((endDate.Value - startDate.Value).TotalDays > 365)
            {
                ModelState.AddModelError("", "Date range cannot exceed 1 year.");
                return View();
            }

            if (!computerId.HasValue)
            {
                return View();
            }

            try
            {
                var revenue = _statisticsService.GetRevenueByComputer(startDate.Value, endDate.Value, computerId.Value);
                var orders = _statisticsService.GetOrdersByComputer(startDate.Value, endDate.Value, computerId.Value);
                var revenueByComputer = _statisticsService.GetRevenueByTimePeriod(startDate.Value, endDate.Value, period);

                ViewBag.Revenue = revenue;
                ViewBag.Orders = orders;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.ComputerId = computerId;
                ViewBag.Period = period;
                ViewBag.RevenueByComputer = revenueByComputer;

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while retrieving data.");
                // Log the exception  
                return View();
            }
        }

        [HttpGet]
        public IActionResult RevenueByUser(DateTime? startDate, DateTime? endDate, int? userId, string period = "daily")
        {
            // Set default dates if not provided
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            // Get list of users for dropdown
            ViewBag.Users = _context.User.ToList();

            // Validate dates
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be later than end date.");
                return View();
            }

            // Validate date range (max 1 year)
            if ((endDate.Value - startDate.Value).TotalDays > 365)
            {
                ModelState.AddModelError("", "Date range cannot exceed 1 year.");
                return View();
            }

            if (!userId.HasValue)
            {
                return View();
            }

            try
            {
                var revenue = _statisticsService.GetRevenueByUser(startDate.Value, endDate.Value, userId.Value);
                var orders = _statisticsService.GetOrdersByUser(startDate.Value, endDate.Value, userId.Value);
                var revenueByUser = _statisticsService.GetRevenueByTimePeriod(startDate.Value, endDate.Value, period);

                ViewBag.Revenue = revenue;
                ViewBag.Orders = orders;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.UserId = userId;
                ViewBag.Period = period;
                ViewBag.RevenueByUser = revenueByUser;

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while retrieving data.");
                // Log the exception
                return View();
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                ViewBag.TodayRevenue = _statisticsService.GetTodayRevenue();
                ViewBag.WeekRevenue = _statisticsService.GetWeekRevenue();
                ViewBag.MonthRevenue = _statisticsService.GetMonthRevenue();
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", "An error occurred while retrieving statistics.");
            }

            return View();
        }
    }
}

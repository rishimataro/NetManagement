using System;
using System.Linq;
using NetManagement.Data;
using NetManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace NetManagement.Services
{
    public class StatisticsService
    {
        private readonly NetManagementContext _context;

        public StatisticsService(NetManagementContext context)
        {
            _context = context;
        }

        public decimal GetTotalRevenue(DateTime startDate, DateTime endDate)
        {
            return _context.Order
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate)
                .Sum(o => o.TotalCost);
        }

        public List<Order> GetOrdersByTime(DateTime startDate, DateTime endDate)
        {
            return _context.Order
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate)
                .ToList();
        }

        public decimal GetRevenueByComputer(DateTime startDate, DateTime endDate, int computerId)
        {
            return _context.Order
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate && o.ComputerId == computerId)
                .Sum(o => o.TotalCost);
        }

        public List<Order> GetOrdersByComputer(DateTime startDate, DateTime endDate, int computerId)
        {
            return _context.Order
                .Include(o => o.User)
                .Include(o => o.Computer)
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate && o.ComputerId == computerId)
                .ToList();
        }

        public decimal GetRevenueByUser(DateTime startDate, DateTime endDate, int userId)
        {
            return _context.Order
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate && o.UserId == userId)
                .Sum(o => o.TotalCost);
        }

        public List<Order> GetOrdersByUser(DateTime startDate, DateTime endDate, int userId)
        {
            return _context.Order
                .Include(o => o.User)
                .Include(o => o.Computer)
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate && o.UserId == userId)
                .ToList();
        }

        public Dictionary<DateTime, decimal> GetDailyRevenue(DateTime startDate, DateTime endDate)
        {
            return _context.Order
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate)
                .GroupBy(o => o.StartTime.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalCost) })
                .ToDictionary(x => x.Date, x => x.Revenue);
        }

        public Dictionary<string, decimal> GetRevenueByTimePeriod(DateTime startDate, DateTime endDate, string period = "daily")
        {
            var orders = _context.Order
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate);

            Dictionary<string, decimal> result = new Dictionary<string, decimal>();

            switch (period.ToLower())
            {
                case "weekly":
                    var weeks = Enumerable.Range(0, (int)(endDate - startDate).TotalDays / 7 + 1)
                        .Select(i => startDate.AddDays(i * 7))
                        .Select(d => new { Year = d.Year, Week = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(d, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) })
                        .Distinct();

                    foreach (var week in weeks)
                    {
                        var weekKey = $"Week {week.Week}, {week.Year}";
                        var weekRevenue = orders
                            .Where(o => System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(o.StartTime, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) == week.Week && o.StartTime.Year == week.Year)
                            .Sum(o => o.TotalCost);
                        result[weekKey] = weekRevenue;
                    }
                    break;

                case "monthly":
                    var months = Enumerable.Range(0, (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month + 1)
                        .Select(i => startDate.AddMonths(i))
                        .Select(d => new { d.Year, d.Month })
                        .Distinct();

                    foreach (var month in months)
                    {
                        var monthKey = $"{month.Year}-{month.Month}";
                        var monthRevenue = orders
                            .Where(o => o.StartTime.Year == month.Year && o.StartTime.Month == month.Month)
                            .Sum(o => o.TotalCost);
                        result[monthKey] = monthRevenue;
                    }
                    break;

                default: // daily
                    var days = Enumerable.Range(0, (int)(endDate - startDate).TotalDays + 1)
                        .Select(i => startDate.AddDays(i));

                    foreach (var day in days)
                    {
                        var dayKey = day.ToString("yyyy-MM-dd");
                        var dayRevenue = orders
                            .Where(o => o.StartTime.Date == day.Date)
                            .Sum(o => o.TotalCost);
                        result[dayKey] = dayRevenue;
                    }
                    break;
            }

            return result;
        }

        public List<Order> GetOrdersByTime(DateTime startDate, DateTime endDate, string sortBy = "StartTime", bool ascending = true)
        {
            var query = _context.Order
                .Include(o => o.User)
                .Include(o => o.Computer)
                .Where(o => o.StartTime >= startDate && o.EndTime <= endDate);

            query = sortBy.ToLower() switch
            {
                "totalcost" => ascending ? query.OrderBy(o => o.TotalCost) : query.OrderByDescending(o => o.TotalCost),
                "username" => ascending ? query.OrderBy(o => o.User.UserName) : query.OrderByDescending(o => o.User.UserName),
                "computername" => ascending ? query.OrderBy(o => o.Computer.Name) : query.OrderByDescending(o => o.Computer.Name),
                _ => ascending ? query.OrderBy(o => o.StartTime) : query.OrderByDescending(o => o.StartTime)
            };

            return query.ToList();
        }

        public decimal GetTodayRevenue()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return GetTotalRevenue(today, tomorrow);
        }

        public decimal GetWeekRevenue()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            return GetTotalRevenue(startOfWeek, endOfWeek);
        }

        public decimal GetMonthRevenue()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            return GetTotalRevenue(startOfMonth, endOfMonth);
        }
    }
}

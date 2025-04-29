using FinScope.Data;
using FinScope.Models;
using Microsoft.EntityFrameworkCore;

namespace FinScope.Services
{
    public class ForecastService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new();

        public ForecastService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<DateTime, decimal>> GetForecastAsync(int days = 7)
        {
            var historical = await _context.Transactions
                .GroupBy(t => t.TransactionDateTime.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            if (!historical.Any())
                return new Dictionary<DateTime, decimal>();

            // Calculate trend (slope)
            int n = historical.Count;
            double sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;
            for (int i = 0; i < n; i++)
            {
                sumX += i;
                sumY += (double)historical[i].Total;
                sumXY += i * (double)historical[i].Total;
                sumXX += i * i;
            }

            double slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            var forecast = new Dictionary<DateTime, decimal>();
            var lastDate = historical.Max(x => x.Date);

            for (int i = 1; i <= days; i++)
            {
                int futureIndex = n + i;
                double predicted = slope * futureIndex + intercept;

                // Add random variation (+/- up to 5%)
                double noise = 1 + (_random.NextDouble() - 0.5) * 0.1;
                predicted *= noise;

                forecast[lastDate.AddDays(i)] = Math.Round((decimal)predicted, 2);
            }

            return forecast;
        }
    }
}

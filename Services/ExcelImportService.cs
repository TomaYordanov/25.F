using finalProject.Data;
using finalProject.Models;
using OfficeOpenXml;
using System.Globalization;

namespace finalProject.Services
{
    public class ExcelImportService
    {
        private readonly ApplicationDbContext _context;

        public ExcelImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ImportTransactionsAsync(Stream fileStream, string userId, int defaultCategoryId, int defaultAccountId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found.");

            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) 
            {
                
                var dateStr = worksheet.Cells[row, 1].Text; 
                var timeStr = worksheet.Cells[row, 2].Text; 
                var paymentsStr = worksheet.Cells[row, 3].Text;
                var proceedsStr = worksheet.Cells[row, 4].Text;
                var description = worksheet.Cells[row, 5].Text;
                var reason = worksheet.Cells[row, 6].Text;
                var moreDetails = worksheet.Cells[row, 7].Text;
                var reference = worksheet.Cells[row, 8].Text;

                // Parse date & time
                DateTime dateTime;
                var combinedDateTime = $"{dateStr} {timeStr}";
                if (!DateTime.TryParseExact(combinedDateTime, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    continue; 
                }

                decimal amount = 0;
                if (!string.IsNullOrWhiteSpace(paymentsStr))
                    decimal.TryParse(paymentsStr, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
                else if (!string.IsNullOrWhiteSpace(proceedsStr))
                    decimal.TryParse(proceedsStr, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);

                if (amount == 0)
                    continue; 

                var transaction = new Transaction
                {
                    TransactionDateTime = dateTime,
                    Amount = amount,
                    Description = $"{description} | {reason} | {moreDetails} | Ref: {reference}",
                    CategoryId = defaultCategoryId,
                    AccountId = defaultAccountId,
                    UserId = userId
                };

                _context.Transactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
        }
    }
}

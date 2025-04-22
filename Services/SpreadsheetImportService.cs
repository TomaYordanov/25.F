using finalProject.Data;
using finalProject.Models;
using OfficeOpenXml;
using System.Globalization;

namespace finalProject.Services
{
    public class SpreadsheetImportService
    {
        private readonly ApplicationDbContext _context;

        public SpreadsheetImportService(ApplicationDbContext context)
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
                var description = worksheet.Cells[row, 5].Text?.Trim();
                var reason = worksheet.Cells[row, 6].Text?.Trim(); 
                var moreDetails = worksheet.Cells[row, 7].Text?.Trim();
                var reference = worksheet.Cells[row, 8].Text;

               
                DateTime dateTime;
                var combinedDateTime = $"{dateStr} {timeStr}";
                if (!DateTime.TryParseExact(combinedDateTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
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

                
                var category = _context.Categories.FirstOrDefault(c => c.Name.ToLower() == reason.ToLower().Trim());
                if (category == null)
                {
                    category = new Category { Name = reason.Trim() };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }

                var transaction = new Transaction
                {
                    TransactionDateTime = dateTime,
                    Amount = amount,
                    Description = $"{description} | {moreDetails} | Ref: {reference}",
                    CategoryId = category.Id,
                    AccountId = defaultAccountId,
                    UserId = userId
                };

                _context.Transactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
        }
    }
}

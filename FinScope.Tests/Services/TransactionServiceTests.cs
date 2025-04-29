using FinScope.Data;
using FinScope.Models;
using FinScope.Services;
using FinScope.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinScope.Tests.Services
{
    [TestFixture]
    public class TransactionServiceTests
    {
        private ApplicationDbContext _context;
        private TransactionService _service;
        private Mock<SpreadsheetImportService> _spreadsheetImportServiceMock;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _spreadsheetImportServiceMock = new Mock<SpreadsheetImportService>(MockBehavior.Loose, (object)null);
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null
            );

            _service = new TransactionService(_context, _spreadsheetImportServiceMock.Object, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllTransactions_ReturnsCorrectData()
        {
            var userId = "user1";
            var account = new Account { Id = 1, UserId = userId, Balance = 1000, Name = "Checking" };
            var category = new Category { Id = 1, Name = "Food" };
            _context.Accounts.Add(account);
            _context.Categories.Add(category);

            var transaction = new Transaction
            {
                Id = 1,
                UserId = userId,
                Amount = 200,
                TransactionDateTime = DateTime.UtcNow,
                Description = "Test transaction",
                AccountId = account.Id,
                Account = account,
                CategoryId = category.Id,
                Category = category
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var result = await _service.GetAllTransactions(userId, pageNumber: 1, pageSize: 10);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Transactions.Count);
            Assert.AreEqual(200, result.Transactions.First().Amount);
            Assert.AreEqual("Food", result.Transactions.First().CategoryName);
            Assert.AreEqual("Checking", result.Transactions.First().AccountName);
        }

        [Test]
        public async Task ManualAddTransaction_AddsTransactionAndUpdatesBalance()
        {
            var userId = "user1";
            var account = new Account { Id = 1, UserId = userId, Balance = 0, Name = "Wallet" };
            var category = new Category { Id = 1, Name = "Groceries" };
            await _context.Accounts.AddAsync(account);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var model = new TransactionIndexViewModel
            {
                ManualAmount = 50,
                ManualDescription = "Grocery shopping",
                ManualDateTime = DateTime.UtcNow,
                SelectedAccountId = account.Id,
                SelectedCategoryId = category.Id
            };

            var result = await _service.ManualAddTransaction(model, userId);

            Assert.IsTrue(result);

            var transaction = await _context.Transactions.FirstOrDefaultAsync();
            Assert.IsNotNull(transaction);
            Assert.AreEqual(50, transaction.Amount);
            Assert.AreEqual(account.Id, transaction.AccountId);
            Assert.AreEqual(category.Id, transaction.CategoryId);

            var updatedAccount = await _context.Accounts.FindAsync(account.Id);
            Assert.AreEqual(50, updatedAccount.Balance);
        }

        [Test]
        public async Task DeleteTransaction_RemovesTransactionAndUpdatesBalance()
        {
            var userId = "user1";
            var account = new Account
            {
                Id = 1,
                UserId = userId,
                Name = "Test Account",
                Balance = 0
            };
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var transaction = new Transaction
            {
                Id = 1,
                UserId = userId,
                Amount = 50,
                TransactionDateTime = DateTime.UtcNow,
                AccountId = account.Id,
                Account = account
            };
            _context.Transactions.Add(transaction);

            account.Balance += transaction.Amount; 

            await _context.SaveChangesAsync();

            var result = await _service.DeleteTransaction(transaction.Id, userId);

            Assert.IsTrue(result);

            var deletedTransaction = await _context.Transactions.FindAsync(transaction.Id);
            Assert.IsNull(deletedTransaction);

            var updatedAccount = await _context.Accounts.FindAsync(account.Id);
            Assert.AreEqual(0, updatedAccount.Balance); 
        }



        [Test]
        public async Task TransferTransaction_CreatesNewTransactionAndUpdatesBalances()
        {
            var userId = "user1";
            var sourceAccount = new Account { Id = 1, UserId = userId, Balance = 500, Name = "Source" };
            var targetAccount = new Account { Id = 2, UserId = userId, Balance = 300, Name = "Target" };
            var transaction = new Transaction
            {
                Id = 1,
                UserId = userId,
                Amount = 200,
                TransactionDateTime = DateTime.UtcNow,
                AccountId = sourceAccount.Id,
                Account = sourceAccount
            };
            _context.Accounts.AddRange(sourceAccount, targetAccount);
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var result = await _service.TransferTransaction(transaction.Id, targetAccount.Id, 100, userId);

            Assert.IsTrue(result);

            var updatedSourceAccount = await _context.Accounts.FindAsync(sourceAccount.Id);
            var updatedTargetAccount = await _context.Accounts.FindAsync(targetAccount.Id);

            Assert.AreEqual(400, updatedSourceAccount.Balance);
            Assert.AreEqual(400, updatedTargetAccount.Balance); 

            var transactions = await _context.Transactions.ToListAsync();
            Assert.AreEqual(2, transactions.Count); 
        }
    }
}

using FinScope.Data;
using FinScope.Models;
using FinScope.Services;
using FinScope.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinScope.Tests.Services
{
    [TestFixture]
    public class DashboardServiceTests
    {
        private ApplicationDbContext _context;
        private DashboardService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new DashboardService(_context, null); 
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        [Test]
        public async Task GetDashboardDataAsync_ReturnsCorrectDashboardData()
        {
            var userId = "user1";
            var account = new Account { Id = 1, UserId = userId, Balance = 1000, Name = "Main" };
            _context.Accounts.Add(account);

            var asset = new Asset
            {
                Id = 1,
                UserId = userId,
                Value = 500,
                AssetType = "Investment", 
                Name = "Stock Portfolio" 
            };
            _context.Assets.Add(asset);

            await _context.SaveChangesAsync();

            var result = await _service.GetDashboardDataAsync(userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1000, result.TotalBalance);
            Assert.AreEqual(500, result.TotalAssetValue);
            Assert.AreEqual(1500, result.NetWorth);
        }


        [Test]
        public async Task GetDashboardDataAsync_CreatesDefaultAccountIfNoneExist()
        {
            var userId = "user2"; 

            var result = await _service.GetDashboardDataAsync(userId);

            var account = _context.Accounts.FirstOrDefault(a => a.UserId == userId);
            Assert.IsNotNull(account);
            Assert.AreEqual("Main", account.Name);
            Assert.AreEqual(0, account.Balance);
        }

        [Test]
        public async Task AddAccountAsync_ReturnsTrueWhenAccountIsAdded()
        {
            var userId = "user1";
            var model = new DashboardViewModel
            {
                NewAccountName = "New Account",
                NewAccountBalance = 500
            };

            var result = await _service.AddAccountAsync(userId, model);

            Assert.IsTrue(result);
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Name == "New Account");
            Assert.IsNotNull(account);
            Assert.AreEqual(500, account.Balance);
        }

        [Test]
        public async Task AddAccountAsync_ReturnsFalseWhenAccountNameIsEmpty()
        {
            var userId = "user1";
            var model = new DashboardViewModel
            {
                NewAccountName = "",
                NewAccountBalance = 500
            };

            var result = await _service.AddAccountAsync(userId, model);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteAccountAsync_RemovesAccountSuccessfully()
        {
            var userId = "user1";
            var account = new Account { Id = 1, UserId = userId, Balance = 500, Name = "Test Account" };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteAccountAsync(userId, account.Id);

            Assert.IsTrue(result);
            var deletedAccount = await _context.Accounts.FindAsync(account.Id);
            Assert.IsNull(deletedAccount);
        }

        [Test]
        public async Task DeleteAccountAsync_FailsToRemoveMainAccount()
        {
            var userId = "user1";
            var account = new Account { Id = 1, UserId = userId, Balance = 500, Name = "Main" };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteAccountAsync(userId, account.Id);

            Assert.IsFalse(result);
            var existingAccount = await _context.Accounts.FindAsync(account.Id);
            Assert.IsNotNull(existingAccount);
        }
    }
}

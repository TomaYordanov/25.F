using FinScope.Data;
using FinScope.Models;
using FinScope.Services;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinScope.Tests.Services
{
    [TestFixture]
    public class SavingGoalServiceTests
    {
        private ApplicationDbContext _context;
        private SavingGoalService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new SavingGoalService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddGoalAsync_AddsGoalSuccessfully()
        {
            var goal = new SavingGoal
            {
                UserId = "user1",
                Title = "Vacation Fund",
                TargetAmount = 2000,
                CurrentAmount = 500
            };

            await _service.AddGoalAsync(goal);

            var savedGoal = await _context.SavingGoals.FirstOrDefaultAsync(g => g.Title == "Vacation Fund");
            Assert.IsNotNull(savedGoal);
            Assert.AreEqual("Vacation Fund", savedGoal.Title);
        }

        [Test]
        public void AddGoalAsync_ThrowsArgumentException_WhenUserIdIsNull()
        {
            var goal = new SavingGoal
            {
                UserId = null, 
                Title = "Emergency Fund",
                TargetAmount = 1000,
                CurrentAmount = 200
            };

            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddGoalAsync(goal));
        }

        [Test]
        public async Task GetGoalsAsync_ReturnsGoalsForUser()
        {
            var userId = "user1";
            var goal = new SavingGoal
            {
                UserId = userId,
                Title = "House Down Payment",
                TargetAmount = 50000,
                CurrentAmount = 10000
            };
            _context.SavingGoals.Add(goal);
            await _context.SaveChangesAsync();

            var result = await _service.GetGoalsAsync(userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("House Down Payment", result.First().Title);
        }

        [Test]
        public async Task GetGoalByIdAsync_ReturnsGoalIfExists()
        {
            var userId = "user1";
            var goal = new SavingGoal
            {
                UserId = userId,
                Title = "Retirement Fund",
                TargetAmount = 100000,
                CurrentAmount = 25000
            };
            _context.SavingGoals.Add(goal);
            await _context.SaveChangesAsync();

            var result = await _service.GetGoalByIdAsync(goal.Id, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual("Retirement Fund", result.Title);
        }

        [Test]
        public async Task UpdateGoalAsync_UpdatesGoalCorrectly()
        {
            var userId = "user1";
            var goal = new SavingGoal
            {
                UserId = userId,
                Title = "Emergency Fund",
                TargetAmount = 5000,
                CurrentAmount = 1000
            };
            _context.SavingGoals.Add(goal);
            await _context.SaveChangesAsync();

            goal.CurrentAmount = 2000;
            await _service.UpdateGoalAsync(goal);

            var updatedGoal = await _context.SavingGoals.FirstOrDefaultAsync(g => g.Id == goal.Id);
            Assert.IsNotNull(updatedGoal);
            Assert.AreEqual(2000, updatedGoal.CurrentAmount);
            Assert.IsFalse(updatedGoal.IsCompleted); 
        }


        [Test]
        public async Task DeleteGoalAsync_RemovesGoalSuccessfully()
        {
            var userId = "user1";
            var goal = new SavingGoal
            {
                UserId = userId,
                Title = "Vacation Fund",
                TargetAmount = 3000,
                CurrentAmount = 1500
            };
            _context.SavingGoals.Add(goal);
            await _context.SaveChangesAsync();

            await _service.DeleteGoalAsync(goal.Id, userId);

            var deletedGoal = await _context.SavingGoals.FindAsync(goal.Id);
            Assert.IsNull(deletedGoal);
        }
    }
}

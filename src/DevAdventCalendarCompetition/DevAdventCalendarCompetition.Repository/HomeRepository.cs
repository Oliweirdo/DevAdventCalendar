﻿using System;
using System.Collections.Generic;
using System.Linq;
using DevAdventCalendarCompetition.Repository.Context;
using DevAdventCalendarCompetition.Repository.Interfaces;
using DevAdventCalendarCompetition.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace DevAdventCalendarCompetition.Repository
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public HomeRepository(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public Test GetCurrentTest()
        {
            return this._dbContext.Set<Test>().FirstOrDefault(el => el.Status == TestStatus.Started);
        }

        public Test GetTestByNumber(int testNumber)
        {
            return this._dbContext.Set<Test>().FirstOrDefault(t => t.Number == testNumber);
        }

        public TestAnswer GetTestAnswerByUserId(string userId, int testId)
        {
            return this._dbContext.Set<TestAnswer>().FirstOrDefault(el => el.UserId == userId && el.TestId == testId);
        }

        public List<Test> GetAllTests()
        {
            return this._dbContext.Set<Test>()
                .OrderBy(t => t.Number).ToList();
        }

        public List<Test> GetTestsWithUserAnswers()
        {
            return this._dbContext.Set<Test>()
                .Include(t => t.WrongAnswers)
                .Include(t => t.Answers)
                .ThenInclude(ta => ta.User)
                .OrderBy(el => el.Number).ToList();
        }

        public int GetCorrectAnswersCountForUser(string userId)
        {
            return this._dbContext.Set<TestAnswer>()
                .Where(a => a.UserId == userId)
                .GroupBy(t => t.TestId)
                .Count();
        }

        public IDictionary<string, int> GetCorrectAnswersPerUserForDateRange(DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            return this._dbContext
                .TestAnswer
                .Where(a => a.AnsweringTime.CompareTo(dateFrom.DateTime) >= 0 &&
                            a.AnsweringTime.CompareTo(dateTo.DateTime) < 0)
                .GroupBy(a => a.UserId)
                .Select(ug => new KeyValuePair<string, int>(ug.Key, ug.Count()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public IDictionary<string, int> GetWrongAnswersPerUserForDateRange(DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            return this._dbContext
                .TestWrongAnswer
                .Where(a => a.Time.CompareTo(dateFrom.DateTime) >= 0 &&
                            a.Time.CompareTo(dateTo.DateTime) < 0)
                .GroupBy(a => a.UserId)
                .Select(ug => new KeyValuePair<string, int>(ug.Key, ug.Count()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public UserPosition GetUserPosition(string userId)
        {
            var result = this._dbContext.Results
                .FirstOrDefault(x => x.UserId == userId);

            var userPosition = new UserPosition();

            if (result == null)
            {
                return userPosition;
            }

            if (result.FinalPlace > 0)
            {
                userPosition.FinalPlace = result.FinalPlace.Value;
            }

            if (result.Week3Place > 0)
            {
                userPosition.Week3Place = result.Week3Place.Value;
            }

            if (result.Week2Place > 0)
            {
                userPosition.Week2Place = result.Week2Place.Value;
            }

            if (result.Week1Place > 0)
            {
                userPosition.Week1Place = result.Week1Place.Value;
            }

            return userPosition;
        }

        public List<Result> GetTestResultsForWeek(int weekNumber)
        {
            switch (weekNumber)
            {
                case 1:
                    return this._dbContext.Results
                        .Include(u => u.User)
                        .Where(r => r.Week1Points != null)
                        .OrderBy(r => r.Week1Place)
                        .ToList();
                case 2:
                    return this._dbContext.Results
                        .Include(u => u.User)
                        .Where(r => r.Week2Points != null)
                        .OrderBy(r => r.Week2Place)
                        .ToList();
                case 3:
                    return this._dbContext.Results
                        .Include(u => u.User)
                        .Where(r => r.Week3Points != null)
                        .OrderBy(r => r.Week3Place)
                        .ToList();
                case 4:
                    return this._dbContext.Results
                        .Include(u => u.User)
                        .Where(r => r.FinalPoints != null)
                        .OrderBy(r => r.FinalPlace)
                        .ToList();
                default:
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                    throw new ArgumentException("Invalid week number.");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            }
        }
    }
}
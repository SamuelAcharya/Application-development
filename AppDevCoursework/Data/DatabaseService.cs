using SQLite;

namespace AppDevCoursework.Data
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
        }

        async Task InitAsync()
        {
            if (_database is not null)
                return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<JournalEntry>();
            await _database.CreateTableAsync<Mood>();
            await _database.CreateTableAsync<Tag>();
            
            await SeedTagsAsync();
        }

        private async Task SeedTagsAsync()
        {
            var count = await _database.Table<Tag>().CountAsync();
            if (count == 0)
            {
                var tags = new List<Tag>
                {
                    new Tag { Name = "Music" }, new Tag { Name = "Finance" }, new Tag { Name = "Hobbies" },
                    new Tag { Name = "Reflection" }, new Tag { Name = "Fitness" }, new Tag { Name = "Vacation" },
                    new Tag { Name = "Cooking" }, new Tag { Name = "Career" }, new Tag { Name = "Meditation" },
                    new Tag { Name = "Friends" }, new Tag { Name = "Planning" }, new Tag { Name = "Travel" },
                    new Tag { Name = "Self-care" }, new Tag { Name = "Projects" }, new Tag { Name = "Holiday" },
                    new Tag { Name = "Studies" }, new Tag { Name = "Nature" }, new Tag { Name = "Reading" },
                    new Tag { Name = "Exercise" }, new Tag { Name = "Relationships" }, new Tag { Name = "Birthday" },
                    new Tag { Name = "Personal Growth" }, new Tag { Name = "Shopping" }, new Tag { Name = "Work" },
                    new Tag { Name = "Spirituality" }, new Tag { Name = "Family" }, new Tag { Name = "Yoga" },
                    new Tag { Name = "Celebration" }, new Tag { Name = "Parenting" }, new Tag { Name = "Writing" },
                    new Tag { Name = "Health" }
                };

                await _database.InsertAllAsync(tags);
            }
        }

        public async Task<JournalEntry> GetEntryByDateAsync(DateTime date)
        {
            await InitAsync();
            
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
            
            return await _database.Table<JournalEntry>()
                            .Where(e => e.EntryDate >= startOfDay && e.EntryDate <= endOfDay)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveEntryAsync(JournalEntry entry)
        {
            await InitAsync();
            if (entry.Id != 0)
            {
                entry.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(entry);
            }
            else
            {
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;
                return await _database.InsertAsync(entry);
            }
        }

        public async Task<int> DeleteEntryAsync(JournalEntry entry)
        {
            await InitAsync();
            return await _database.DeleteAsync(entry);
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            await InitAsync();
            return await _database.Table<Tag>().ToListAsync();
        }
        
        public async Task<int> SaveTagAsync(Tag tag)
        {
            await InitAsync();
            return await _database.InsertAsync(tag);
        }

        // Simple method to get all entries for a specific month
        public async Task<List<JournalEntry>> GetEntriesForMonthAsync(DateTime monthDate)
        {
            await InitAsync();
            var start = new DateTime(monthDate.Year, monthDate.Month, 1);
            var end = start.AddMonths(1).AddTicks(-1);

            return await _database.Table<JournalEntry>()
                            .Where(e => e.EntryDate >= start && e.EntryDate <= end)
                            .ToListAsync();
        }

        public async Task<List<JournalEntry>> GetJournalEntriesAsync(string searchText, DateTime? date, string mood, string tag, int skip, int take)
        {
            await InitAsync();
            var query = _database.Table<JournalEntry>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(e => e.Title.Contains(searchText) || e.Content.Contains(searchText));
            }

            if (date.HasValue)
            {
                var start = date.Value.Date;
                var end = start.AddDays(1).AddTicks(-1);
                query = query.Where(e => e.EntryDate >= start && e.EntryDate <= end);
            }

            if (!string.IsNullOrWhiteSpace(mood))
            {
                query = query.Where(e => e.PrimaryMood.Equals(mood) || e.SecondaryMoods.Contains(mood));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(e => e.Tags.Contains(tag));
            }

            return await query.OrderByDescending(e => e.EntryDate)
                            .Skip(skip)
                            .Take(take)
                            .ToListAsync();
        }

        public async Task<int> GetTotalEntriesCountAsync(string searchText, DateTime? date, string mood, string tag)
        {
            await InitAsync();
            var query = _database.Table<JournalEntry>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(e => e.Title.Contains(searchText) || e.Content.Contains(searchText));
            }

            if (date.HasValue)
            {
                var start = date.Value.Date;
                var end = start.AddDays(1).AddTicks(-1);
                query = query.Where(e => e.EntryDate >= start && e.EntryDate <= end);
            }

            if (!string.IsNullOrWhiteSpace(mood))
            {
                query = query.Where(e => e.PrimaryMood.Equals(mood) || e.SecondaryMoods.Contains(mood));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(e => e.Tags.Contains(tag));
            }

            return await query.CountAsync();
        }

        public async Task<StreakStats> GetStreakStatsAsync()
        {
            await InitAsync();
            var entries = await _database.Table<JournalEntry>().ToListAsync();
            var dates = entries.Select(e => e.EntryDate.Date).Distinct().OrderByDescending(d => d).ToList();

            int currentStreak = 0;
            int longestStreak = 0;
            int totalEntries = entries.Count;

            if (dates.Any())
            {
                // Calculate Current Streak
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);

                if (dates.Contains(today))
                {
                    currentStreak = 1;
                    var checkDate = today.AddDays(-1);
                    while (dates.Contains(checkDate))
                    {
                        currentStreak++;
                        checkDate = checkDate.AddDays(-1);
                    }
                }
                else if (dates.Contains(yesterday))
                {
                    currentStreak = 1;
                    var checkDate = yesterday.AddDays(-1);
                    while (dates.Contains(checkDate))
                    {
                        currentStreak++;
                        checkDate = checkDate.AddDays(-1);
                    }
                }

                // Calculate Longest Streak
                int tempStreak = 0;
                // Since dates are descending
                for (int i = 0; i < dates.Count; i++)
                {
                    if (i == 0)
                    {
                        tempStreak = 1;
                    }
                    else
                    {
                        var prevDate = dates[i - 1];
                        var currDate = dates[i];

                        if (prevDate.AddDays(-1) == currDate)
                        {
                            tempStreak++;
                        }
                        else
                        {
                            if (tempStreak > longestStreak) longestStreak = tempStreak;
                            tempStreak = 1;
                        }
                    }
                }
                if (tempStreak > longestStreak) longestStreak = tempStreak;
            }

            return new StreakStats
            {
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                TotalEntries = totalEntries
            };
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(DateTime start, DateTime end)
        {
            await InitAsync();
            var allEntries = await _database.Table<JournalEntry>()
                                          .Where(e => e.EntryDate >= start && e.EntryDate <= end)
                                          .ToListAsync();

            var stats = new DashboardStats
            {
                TotalEntries = allEntries.Count
            };

            // Streak Info
            var streakStats = await GetStreakStatsAsync();
            stats.CurrentStreak = streakStats.CurrentStreak;
            stats.LongestStreak = streakStats.LongestStreak;

            if (allEntries.Any())
            {
                // Mood Distribution
                var positiveMoods = new HashSet<string> { "Happy", "Excited", "Relaxed", "Grateful", "Confident" };
                var neutralMoods = new HashSet<string> { "Calm", "Thoughtful", "Curious", "Nostalgic", "Bored" };
                var negativeMoods = new HashSet<string> { "Sad", "Angry", "Stressed", "Lonely", "Anxious" };

                int pos = 0, neu = 0, neg = 0;
                var moodCounts = new Dictionary<string, int>();

                foreach (var entry in allEntries)
                {
                    if (positiveMoods.Contains(entry.PrimaryMood)) pos++;
                    else if (neutralMoods.Contains(entry.PrimaryMood)) neu++;
                    else if (negativeMoods.Contains(entry.PrimaryMood)) neg++;

                    if (!string.IsNullOrEmpty(entry.PrimaryMood))
                    {
                        moodCounts[entry.PrimaryMood] = moodCounts.GetValueOrDefault(entry.PrimaryMood) + 1;
                    }
                }

                stats.MoodDistribution["Positive"] = pos;
                stats.MoodDistribution["Neutral"] = neu;
                stats.MoodDistribution["Negative"] = neg;

                stats.MostFrequentMood = moodCounts.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "None";

                // Tags
                var tagCounts = new Dictionary<string, int>();
                foreach (var entry in allEntries)
                {
                    if (!string.IsNullOrEmpty(entry.Tags))
                    {
                        foreach (var tag in entry.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            var t = tag.Trim();
                            tagCounts[t] = tagCounts.GetValueOrDefault(t) + 1;
                        }
                    }
                }
                stats.TopTags = tagCounts.OrderByDescending(x => x.Value).Take(5).ToList();

                // Category Breakdown
                var catCounts = new Dictionary<string, int>();
                foreach (var entry in allEntries)
                {
                    if (!string.IsNullOrEmpty(entry.Category))
                    {
                        catCounts[entry.Category] = catCounts.GetValueOrDefault(entry.Category) + 1;
                    }
                }
                stats.CategoryBreakdown = catCounts;

                // Word Count Trends
                stats.WordCountTrends = allEntries.OrderBy(e => e.EntryDate)
                                                .Select(e => new KeyValuePair<DateTime, int>(e.EntryDate.Date, CountWords(e.Content)))
                                                .ToList();
                
                // Missed Days
                var dateSet = new HashSet<DateTime>(allEntries.Select(e => e.EntryDate.Date));
                for (var dt = start.Date; dt <= end.Date; dt = dt.AddDays(1))
                {
                    if (!dateSet.Contains(dt) && dt < DateTime.Today)
                    {
                        stats.MissedDays.Add(dt);
                    }
                }
            }

            return stats;
        }

        private int CountWords(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return 0;
            var plainText = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty);
            return plainText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }

    public class StreakStats
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalEntries { get; set; }
    }

    public class DashboardStats
    {
        public int TotalEntries { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public List<DateTime> MissedDays { get; set; } = new();

        public Dictionary<string, int> MoodDistribution { get; set; } = new();
        public string MostFrequentMood { get; set; } = "None";
        
        public List<KeyValuePair<string, int>> TopTags { get; set; } = new();
        public Dictionary<string, int> CategoryBreakdown { get; set; } = new();
        
        public List<KeyValuePair<DateTime, int>> WordCountTrends { get; set; } = new();
    }
}

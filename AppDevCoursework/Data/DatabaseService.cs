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
    }
}

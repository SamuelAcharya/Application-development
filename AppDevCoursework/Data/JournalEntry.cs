using SQLite;

namespace AppDevCoursework.Data
{
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public DateTime EntryDate { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // Rich text HTML content

        public string PrimaryMood { get; set; } = string.Empty;
        public string SecondaryMoods { get; set; } = string.Empty; // Comma separated
        public string Tags { get; set; } = string.Empty; // Comma separated
        public string Category { get; set; } = string.Empty; 

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

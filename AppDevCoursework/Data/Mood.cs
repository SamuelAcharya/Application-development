using SQLite;

namespace AppDevCoursework.Data
{
    public class Mood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Positive, Neutral, Negative
    }
}

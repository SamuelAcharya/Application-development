using SQLite;

namespace AppDevCoursework.Data
{
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string Name { get; set; } = string.Empty;
    }
}

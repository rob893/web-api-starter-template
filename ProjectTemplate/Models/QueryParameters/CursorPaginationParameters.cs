namespace ProjectTemplate.Models.QueryParameters
{
    public class CursorPaginationParameters
    {
        public int? First { get; set; }
        public string After { get; set; }
        public int? Last { get; set; }
        public string Before { get; set; }
        public bool IncludeTotal { get; set; } = false;
    }
}
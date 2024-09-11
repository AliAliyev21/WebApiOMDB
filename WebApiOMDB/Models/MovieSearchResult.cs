namespace WebApiOMDB.Models
{
    public class MovieSearchResult
    {
        public string? Title { get; set; }
        public string? Director { get; set; }
        public string? Genre { get; set; }
        public decimal Rating { get; set; }
        public string? Description { get; set; }
    }
}

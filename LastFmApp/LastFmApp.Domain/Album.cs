namespace LastFmApp.Domain
{
    public class Album
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Mbid { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public int ArtistId { get; set; }
        public Artist Artist { get; set; } = default!;
    }
}

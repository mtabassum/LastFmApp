namespace LastFmApp.Domain
{
    public class ArtistTag
    {
        public int ArtistId { get; set; }
        public Artist Artist { get; set; } = default!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = default!;
    }
}

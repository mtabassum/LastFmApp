namespace LastFmApp.Domain
{
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Mbid { get; set; }    
        public string? Url { get; set; }
        public ICollection<Album> Albums { get; set; } = new List<Album>();
        public ICollection<ArtistTag> ArtistTags { get; set; } = new List<ArtistTag>();
    }
}

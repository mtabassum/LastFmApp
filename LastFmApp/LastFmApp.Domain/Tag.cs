namespace LastFmApp.Domain
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public ICollection<ArtistTag> ArtistTags { get; set; } = new List<ArtistTag>();
    }
}

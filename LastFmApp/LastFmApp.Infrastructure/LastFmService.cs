using LastFmApp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace LastFmApp.Infrastructure
{
    public class LastFmService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LastFmService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://ws.audioscrobbler.com/2.0/";

        public LastFmService(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<LastFmService> logger,
            HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;

            _apiKey = configuration["LastFm:ApiKey"]
                ?? throw new InvalidOperationException("LastFm:ApiKey not configured");
        
        }

        public async Task ImportArtistsByTagAsync(string tagName, int limit = 50)
        {
            try
            {
                _logger.LogInformation($"Starting import for tag: {tagName}");

                // ensure the tag exists in the database
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                // Get top artists for this tag from Last.fm API
                var topArtistsJson = await GetTopArtistsByTagAsync(tagName, limit);

                if (topArtistsJson == null)
                {
                    _logger.LogWarning($"No response for tag: {tagName}");
                    return;
                }

                var topArtists = topArtistsJson.RootElement
                    .GetProperty("topartists")
                    .GetProperty("artist");

                foreach (var artistElement in topArtists.EnumerateArray())
                {
                    try
                    {
                        var artistName = artistElement.GetProperty("name").GetString();
                        var mbid = artistElement.TryGetProperty("mbid", out var mbidElement)
                            ? mbidElement.GetString() : null;
                        var url = artistElement.TryGetProperty("url", out var urlElement)
                            ? urlElement.GetString() : null;

                        // Check if artist already exists
                        var existingArtist = await _context.Artists
                            .Include(a => a.ArtistTags)
                            .Include(a => a.Albums)
                            .FirstOrDefaultAsync(a => a.Name == artistName);

                        Artist artist;
                        if (existingArtist == null)
                        {
                            // Create new artist
                            artist = new Artist
                            {
                                Name = artistName!,
                                Mbid = mbid,
                                Url = url
                            };
                            _context.Artists.Add(artist);
                        }
                        else
                        {
                            artist = existingArtist;
                        }

                        // Add tag relationship if not exists
                        if (!artist.ArtistTags.Any(at => at.TagId == tag.Id))
                        {
                            artist.ArtistTags.Add(new ArtistTag
                            {
                                Artist = artist,
                                Tag = tag
                            });
                        }

                        await _context.SaveChangesAsync();

                        // Get artist's top albums
                        await ImportArtistAlbumsAsync(artist, artistName!);

                        _logger.LogInformation($"Imported artist: {artist.Name}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error importing artist");
                    }
                }

                _logger.LogInformation($"Import completed for tag: {tagName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error importing artists for tag: {tagName}");
                throw;
            }
        }

        private async Task<JsonDocument?> GetTopArtistsByTagAsync(string tagName, int limit)
        {
            var url = $"{_baseUrl}?method=tag.gettopartists&tag={Uri.EscapeDataString(tagName)}" +
                     $"&api_key={_apiKey}&format=json&limit={limit}";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                return JsonDocument.Parse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching artists for tag: {tagName}");
                return null;
            }
        }
        private async Task ImportArtistAlbumsAsync(Artist artist, string artistName)
        {
            try
            {
                // Get top albums for the artist from Last.fm API
                var topAlbumsJson = await GetTopAlbumsByArtistAsync(artistName);

                if (topAlbumsJson == null)
                {
                    _logger.LogWarning($"No albums found for artist: {artistName}");
                    return;
                }

                var albumsElement = topAlbumsJson.RootElement
                    .GetProperty("topalbums");

                if (!albumsElement.TryGetProperty("album", out var albums))
                {
                    return;
                }

                foreach (var albumElement in albums.EnumerateArray().Take(10))
                {
                    try
                    {
                        var albumName = albumElement.GetProperty("name").GetString();
                        var mbid = albumElement.TryGetProperty("mbid", out var mbidElement)
                            ? mbidElement.GetString() : null;
                        var url = albumElement.TryGetProperty("url", out var urlElement)
                            ? urlElement.GetString() : null;

                        // Get the largest image URL
                        string? imageUrl = null;
                        if (albumElement.TryGetProperty("image", out var images))
                        {
                            var largeImage = images.EnumerateArray()
                                .LastOrDefault(img => img.TryGetProperty("#text", out var _));

                            if (largeImage.ValueKind != JsonValueKind.Undefined)
                            {
                                imageUrl = largeImage.GetProperty("#text").GetString();
                            }
                        }

                        // Check if album already exists for this artist
                        var existingAlbum = artist.Albums
                            .FirstOrDefault(a => a.Title == albumName);

                        if (existingAlbum == null && !string.IsNullOrWhiteSpace(albumName))
                        {
                            var album = new Album
                            {
                                Title = albumName,
                                Mbid = mbid,
                                Url = url,
                                ImageUrl = imageUrl,
                                Artist = artist
                            };
                            artist.Albums.Add(album);
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error importing album");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error importing albums for artist: {artistName}");
            }
        }

        private async Task<JsonDocument?> GetTopAlbumsByArtistAsync(string artistName)
        {
            var url = $"{_baseUrl}?method=artist.gettopalbums&artist={Uri.EscapeDataString(artistName)}" +
                     $"&api_key={_apiKey}&format=json&limit=10";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                return JsonDocument.Parse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching albums for artist: {artistName}");
                return null;
            }
        }

       
    }
}

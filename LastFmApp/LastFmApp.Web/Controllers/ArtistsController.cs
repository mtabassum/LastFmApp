using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LastFmApp.Domain;
using LastFmApp.Infrastructure;

namespace LastFmWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly LastFmService _lastFmService;
        private readonly ILogger<ArtistsController> _logger;

        public ArtistsController(
            AppDbContext context,
            LastFmService lastFmService,
            ILogger<ArtistsController> logger)
        {
            _context = context;
            _lastFmService = lastFmService;
            _logger = logger;
        }

        // GET: api/artists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtistDto>>> GetArtists()
        {
            var artists = await _context.Artists
                .Include(a => a.ArtistTags)
                    .ThenInclude(at => at.Tag)
                .Select(a => new ArtistDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Url = a.Url,
                    Tags = a.ArtistTags.Select(at => at.Tag.Name).ToList()
                })
                .ToListAsync();

            return Ok(artists);
        }

        // GET: api/artists/complete
        [HttpGet("complete")]
        public async Task<ActionResult<IEnumerable<ArtistDetailDto>>> GetAllArtistsWithAlbums()
        {
            var artists = await _context.Artists
                .Include(a => a.Albums)
                .Include(a => a.ArtistTags)
                    .ThenInclude(at => at.Tag)
                .Select(a => new ArtistDetailDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Mbid = a.Mbid,
                    Url = a.Url,
                    Tags = a.ArtistTags.Select(at => at.Tag.Name).ToList(),
                    Albums = a.Albums.Select(album => new AlbumDto
                    {
                        Id = album.Id,
                        Title = album.Title,
                        Url = album.Url,
                        ImageUrl = album.ImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return Ok(artists);
        }

        // GET: api/artists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ArtistDetailDto>> GetArtist(int id)
        {
            var artist = await _context.Artists
                .Include(a => a.Albums)
                .Include(a => a.ArtistTags)
                    .ThenInclude(at => at.Tag)
                .Where(a => a.Id == id)
                .Select(a => new ArtistDetailDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Mbid = a.Mbid,
                    Url = a.Url,
                    Tags = a.ArtistTags.Select(at => at.Tag.Name).ToList(),
                    Albums = a.Albums.Select(album => new AlbumDto
                    {
                        Id = album.Id,
                        Title = album.Title,
                        Url = album.Url,
                        ImageUrl = album.ImageUrl
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (artist == null)
            {
                return NotFound();
            }

            return Ok(artist);
        }

    }


    public class ArtistDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Url { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class ArtistDetailDto : ArtistDto
    {
        public string? Mbid { get; set; }
        public List<AlbumDto> Albums { get; set; } = new();
    }

    public class AlbumDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
    }

}
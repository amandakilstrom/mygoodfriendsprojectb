using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DbContext;
using Microsoft.EntityFrameworkCore;

namespace AppRazor.Pages
{
    public class OverviewPageModel : PageModel
    {
        private readonly MainDbContext _context;
        private readonly ILogger<OverviewPageModel> _logger;

        [BindProperty]
        public List<CountryCityOverview> Overview { get; set; }

        public OverviewPageModel(MainDbContext context, ILogger<OverviewPageModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            var friends = await _context.InfoFriendsView.ToListAsync();
            var pets = await _context.InfoPetsView.ToListAsync();

            Overview = friends
                .GroupJoin(
                    pets,
                    f => new { f.Country, f.City },
                    p => new { p.Country, p.City },
                    (f, p) => new CountryCityOverview
                    {
                        Country = f.Country,
                        City = f.City,
                        FriendCount = f.NrFriends,
                        PetsCount = p.FirstOrDefault()?.NrPets ?? 0
                    })
                .OrderBy(x => x.Country)
                .ThenBy(x => x.City)
                .Where(o => !string.IsNullOrWhiteSpace(o.Country))
                .ToList();

            return Page();
        }

        public class CountryCityOverview
        {
            public string Country { get; set; }
            public string City { get; set; }

            public int FriendCount { get; set; }
            public int PetsCount { get; set; }
        }
    }
}

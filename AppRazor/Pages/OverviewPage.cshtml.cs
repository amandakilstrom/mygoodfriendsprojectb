using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services.Interfaces;
using System.Linq;
using Models;

namespace AppRazor.Pages
{
    public class OverviewPageModel : PageModel
    {
        private readonly IFriendsService _friendsService;
        private readonly IAddressesService _addressService;
        private readonly ILogger<OverviewPageModel> _logger;

        [BindProperty]
        public List<IAddress> Addresses { get; set; } = new();

        [BindProperty]
        public List<Country> Countries { get; set; }

        public OverviewPageModel(IFriendsService friendsService, IAddressesService addressesService, ILogger<OverviewPageModel> logger)
        {
            _friendsService = friendsService;
            _addressService = addressesService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            var friends = await _friendsService.ReadFriendsAsync(true, false, null, 0, 10);

            var addresses = await _addressService.ReadAddressesAsync(true, false, null, 0, 10);

            Countries = Country.GetCountries(addresses.PageItems);

            return Page();
        }

        public class Country
        {
            public string Name { get; set; }
            static int NrOfCountries { get; set; }

            public Country(string name, int nrOfCountries)
            {
                Name = name;
                NrOfCountries = nrOfCountries;
            }

            public static List<Country> GetCountries(List<IAddress> addresses)
            {
                List<Country> countries = new();
                int currentCount = 1;

                foreach (var address in addresses.DistinctBy(c => c.Country))
                {
                    Country country = new Country(address.Country, currentCount);
                    countries.Add(country);

                    currentCount++;
                }

                NrOfCountries = currentCount;

                return countries;
            }
        }
    }
}

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class ListOfFriendsModel : PageModel
    {
        private readonly IFriendsService _friendsService;
        private readonly ILogger<ListOfFriendsModel> _logger;

        [BindProperty]
        public bool UseSeeds { get; set; } = true;

        public List<IFriend> Friends { get; set; } 

        public int FriendsCount { get; set; }

        //Pagination properties
        public int NrOfPages { get; set; }
        public int PageSize { get; } = 10;

        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;

        public ListOfFriendsModel(IFriendsService friendsService, ILogger<ListOfFriendsModel> logger)
        {
            _friendsService = friendsService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            if (int.TryParse(Request.Query["pagenr"], out int pagenr))
            {
                ThisPageNr = pagenr;
            }

            var response = await _friendsService.ReadFriendsAsync(UseSeeds, false, "", ThisPageNr, PageSize);
            Friends = response.PageItems;
            FriendsCount = response.DbItemsCount;

            UpdatePagination(FriendsCount);

            return Page();
        }

        private void UpdatePagination(int nrOfItems)
        {
            //Pagination
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }


    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class ViewFriendModel : PageModel
    {
        private readonly IFriendsService _friendsService;
        readonly IPetsService _petService;
        readonly IQuotesService _quoteService;

        public IFriend Friend { get; set; }

        public ViewFriendModel(IFriendsService friendsService, IPetsService petsService, IQuotesService quotesService)
        {
            _friendsService = friendsService;
            _petService = petsService;
            _quoteService = quotesService;
        }

        public async Task<IActionResult> OnGet()
        {
            Guid friendId = Guid.Parse(Request.Query["id"]);
            Friend = (await _friendsService.ReadFriendAsync(friendId, false)).Item;

            return Page();
        }

        public async Task<IActionResult> OnPostDeletePet(Guid petId)
        {
            await _petService.DeletePetAsync(petId);
            return RedirectToPage(new { id = Friend.FriendId });
        }

        public async Task<IActionResult> OnPostDeleteQuote(Guid quoteId)
        {
            await _quoteService.DeleteQuoteAsync(quoteId);
            return RedirectToPage(new { id = Friend.FriendId });
        }
    }
}

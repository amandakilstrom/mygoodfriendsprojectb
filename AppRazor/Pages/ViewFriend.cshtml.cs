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

        public IFriend Friend { get; set; }

        public ViewFriendModel(IFriendsService friendsService)
        {
            _friendsService = friendsService;
        }

        public async Task<IActionResult> OnGet()
        {
            Guid friendId = Guid.Parse(Request.Query["id"]);
            Friend = (await _friendsService.ReadFriendAsync(friendId, false)).Item;

            return Page();
        }
    }
}

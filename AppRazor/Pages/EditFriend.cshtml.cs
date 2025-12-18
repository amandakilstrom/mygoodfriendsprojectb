using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages
{
    public class EditFriendModel : PageModel
    {
        public enum StatusIM { Unknow, Unchanged, Inserted, Modified, Deleted }

        [BindProperty]
        public FriendIM Friend
        public void OnGet()
        {
        }

        public IActionResult OnPostEditFriend(Guid friendId)
        {
            int index =
        }

        public class FriendIM
        {
            public StatusIM StatusIM { get; set; }
            public Guid FriendId { get; set; }

            [Required(ErrorMessage = "You must provide a first name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "You must provide a last name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "You must provide an email")]
            public string Email { get; set; }

            public DateTime? Birthday { get; set; } = null;


        }

        public class AddressIM
        {
            public StatusIM StatusIM { get; set; }
            public Guid AddressId { get; set; }

            [Required(ErrorMessage = "You must provide a street name")]
            public string StreetAddress { get; set; }

            [Required(ErrorMessage = "You must privade a zip code")]
            public int ZipCode { get; set; }

            [Required(ErrorMessage = "You must provide a city")]
            public string City { get; set; }

            [Required(ErrorMessage = "You must provide a country")]
            public string Country { get; set; }

            public AddressIM() { }
            public AddressIM(AddressIM original)
            {
                StatusIM = original.StatusIM;
                AddressId = original.AddressId;
                StreetAddress = original.StreetAddress;
                ZipCode = original.ZipCode;
                City = original.City;
                Country = original.Country;
            }
        }
    }
}

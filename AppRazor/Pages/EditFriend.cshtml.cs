using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;
using AppRazor.SeidoHelpers;
using Models;

namespace AppRazor.Pages
{
    public class EditFriendModel : PageModel
    {
        public enum StatusIM { Unknow, Unchanged, Inserted, Modified, Deleted }

        readonly IFriendsService _frService = null;
        readonly IAddressesService _adService = null;

        [BindProperty]
        public FriendIM FriendInput { get; set; }

        [BindProperty]
        public string PageHeader { get; set; }

        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);


        public EditFriendModel(IFriendsService frService, IAddressesService adService)
        {
            _frService = frService;
            _adService = adService;
        }

        public async Task<IActionResult> OnGet()
        {
            if (Guid.TryParse(Request.Query["id"], out Guid _friendId))
            {
                var friend = await _frService.ReadFriendAsync(_friendId, false);

                FriendInput = new FriendIM(friend.Item);
                PageHeader = "Edit details of a friend";

            }
            else
            {
                FriendInput = new FriendIM();
                FriendInput.StatusIM = StatusIM.Inserted;
                FriendInput.Address = null;

                PageHeader = "Create a new friend";
            }

            return Page();
        }

        public IActionResult OnPostEditAddress()
        {
            string[] keys =
            {
                "FriendInput.Address.editStreetAddress",
                "FriendInput.Address.editZipCode",
                "FriendInput.Address.editCity",
                "FriendInput.Address.editCountry"
            };

            if (!ModelState.IsValidPartially(out ModelValidationResult vr, keys))
            {
                ValidationResult = vr;
                return Page();
            }

            var a = FriendInput.Address;

            if (a.StatusIM != StatusIM.Inserted)
                a.StatusIM = StatusIM.Modified;

            // Commit changes
            a.StreetAddress = a.editStreetAddress;
            a.ZipCode = a.editZipCode;
            a.City = a.editCity;
            a.Country = a.editCountry;

            return Page();
        }

        public async Task<IActionResult> OnPostSave()
        {
            string[] keys =
            {
                "FriendInput.FirstName",
                "FriendInput.LastName",
                "FriendInput.Email",
                "FriendInput.Birthday"
            };

            if (!ModelState.IsValidPartially(out ModelValidationResult vr, keys))
            {
                ValidationResult = vr;
                return Page();
            }

            var fr = await _frService.ReadFriendAsync(FriendInput.FriendId, false);

            var friend = FriendInput.UpdateModel(fr.Item);

            var friendDto = new FriendCuDto(friend);

            if (FriendInput.Address != null)
            {
                if (FriendInput.Address.StatusIM == StatusIM.Inserted)
                {
                    var addrRes = await _adService.CreateAddressAsync(
                        FriendInput.Address.CreateCUdto()
                    );

                    FriendInput.Address.AddressId = addrRes.Item.AddressId;
                    friendDto.AddressId = addrRes.Item.AddressId;
                }
                else if (FriendInput.Address.StatusIM == StatusIM.Modified)
                {
                    await _adService.UpdateAddressAsync(
                        FriendInput.Address.CreateCUdto()
                    );

                    friendDto.AddressId = FriendInput.Address.AddressId;
                }
            }
            
            await _frService.UpdateFriendAsync(friendDto);

            return RedirectToPage("ViewFriend", new { id = friend.FriendId });
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

            public AddressIM Address { get; set; }

            public FriendIM() { }

            public FriendIM(IFriend model)
            {
                StatusIM = StatusIM.Unchanged;
                FriendId = model.FriendId;
                FirstName = model.FirstName;
                LastName = model.LastName;
                Email = model.Email;
                Birthday = model.Birthday;

                Address = model.Address != null
                    ? new AddressIM(model.Address)
                    : new AddressIM { StatusIM = StatusIM.Inserted };

            }

            public IFriend UpdateModel(IFriend model)
            {
                model.FirstName = this.FirstName;
                model.LastName = this.LastName;
                model.Email = this.Email;
                model.Birthday = this.Birthday;
                return model;
            }
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


            [Required(ErrorMessage = "You must provide a street name")]
            public string editStreetAddress { get; set; }

            [Required(ErrorMessage = "You must privade a zip code")]
            public int editZipCode { get; set; }

            [Required(ErrorMessage = "You must provide a city")]
            public string editCity { get; set; }

            [Required(ErrorMessage = "You must provide a country")]
            public string editCountry { get; set; }

            public AddressIM() { }
            public AddressIM(AddressIM original)
            {
                StatusIM = original.StatusIM;
                AddressId = original.AddressId;
                StreetAddress = original.StreetAddress;
                ZipCode = original.ZipCode;
                City = original.City;
                Country = original.Country;

                editStreetAddress = original.editStreetAddress;
                editZipCode = original.editZipCode;
                editCity = original.editCity;
                editCountry = original.editCountry;
            }

            public AddressIM(IAddress model)
            {
                StatusIM = StatusIM.Unchanged;
                AddressId = model.AddressId;
                StreetAddress = editStreetAddress = model.StreetAddress;
                ZipCode = editZipCode = model.ZipCode;
                City = editCity = model.City;
                Country = editCountry = model.Country;
            }

            public IAddress UpdateModel(IAddress model)
            {
                if (this.StatusIM != StatusIM.Inserted)
                {
                    model.AddressId = this.AddressId;
                }

                model.StreetAddress = this.StreetAddress;
                model.ZipCode = this.ZipCode;
                model.City = this.City;
                model.Country = this.Country;
                return model;
            }

            public AddressCuDto CreateCUdto() => new AddressCuDto()
            {
                AddressId = this.StatusIM == StatusIM.Inserted ? null : this.AddressId,
                StreetAddress = this.StreetAddress,
                ZipCode = this.ZipCode,
                City = this.City,
                Country = this.Country
            };
        }
    }
}

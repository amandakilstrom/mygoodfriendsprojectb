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
        readonly IPetsService _petService = null;
        readonly IQuotesService _quoteService;

        [BindProperty]
        public FriendIM FriendInput { get; set; }

        [BindProperty]
        public string PageHeader { get; set; }

        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);


        public EditFriendModel(IFriendsService frService, IAddressesService adService, IPetsService petService, IQuotesService quoteService)
        {
            _frService = frService;
            _adService = adService;
            _petService = petService;
            _quoteService = quoteService;
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
            {
                a.StatusIM = StatusIM.Modified;
            }
            
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

            // DELETE Pets
            var deletedPets = FriendInput.Pets.Where(p => p.StatusIM == StatusIM.Deleted).ToList();
            foreach (var pet in deletedPets)
            {
                await _petService.DeletePetAsync(pet.PetId);
            }

            // DELETE Quotes
            var deletedQuotes = FriendInput.Quotes.Where(q => q.StatusIM == StatusIM.Deleted).ToList();
            foreach (var quote in deletedQuotes)
            {
                await _quoteService.DeleteQuoteAsync(quote.QuoteId);
            }

            FriendInput.Pets.RemoveAll(p => p.StatusIM == StatusIM.Deleted);
            FriendInput.Quotes.RemoveAll(q => q.StatusIM == StatusIM.Deleted);

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

        public IActionResult OnPostDeletePet(Guid petId)
        {
            FriendInput.Pets.First(p => p.PetId == petId).StatusIM = StatusIM.Deleted;

            return Page();
        }

        public IActionResult OnPostDeleteQuote(Guid quoteId)
        {
            FriendInput.Quotes.First(q => q.QuoteId == quoteId).StatusIM = StatusIM.Deleted;

            return Page();
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

            public List<PetIM> Pets { get; set; } = new();
            public List<QuoteIM> Quotes { get; set; } = new();

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

                Pets = model.Pets?.Select(p => new PetIM(p)).ToList();
                Quotes = model.Quotes?.Select(q => new QuoteIM(q)).ToList();
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

        public class PetIM
        {
            public StatusIM StatusIM { get; set; }

            public Guid PetId { get; set; }

            public string Name { get; set; }
            public AnimalKind Kind { get; set; }

            public PetIM() { }

            public PetIM(IPet model)
            {
                StatusIM = StatusIM.Unchanged;
                PetId = model.PetId;
                Name = model.Name;
                Kind = model.Kind;
            }
        }

        public class QuoteIM
        {
            public StatusIM StatusIM { get; set; }

            public Guid QuoteId { get; set; }

            public string QuoteText { get; set; }
            public string Author { get; set; }

            public QuoteIM() { }

            public QuoteIM(IQuote model)
            {
                StatusIM = StatusIM.Unchanged;
                QuoteId = model.QuoteId;
                QuoteText = model.QuoteText;
                Author = model.Author;
            }
        }
    }
}

using System;
using System.IO;
using System.Linq;
using BlipPhone.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PhoneNumbers;

namespace BlipPhone.Web.Controllers
{
    public class PhoneController : Controller
    {
        private static PhoneNumberUtil _phoneUtil;
        private Countries _countries;

        public PhoneController(IHostingEnvironment env)
        {
            _phoneUtil = PhoneNumberUtil.GetInstance();
            string hostingPath = Path.GetDirectoryName(env.ContentRootPath);
            string dataPath = "BlipPhone.Web\\Data\\iso3166-slim-2.json";
            _countries = new Countries(Path.Combine(hostingPath, dataPath));
        }

        public IActionResult Check()
        {
            var model = new PhoneNumberCheckViewModel()
            {
                CountryCodeSelected = "US",
                Countries = _countries.CountrySelectList
            };
            return View(model);
        }

        // Antiforgery tokens are validated globally using AutoValidateAntiforgeryToken. See Startup.cs.
        [HttpPost]
        public IActionResult Check(PhoneNumberCheckViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Parse the number to check into a PhoneNumber object.
                    PhoneNumber phoneNumber = _phoneUtil.Parse(model.PhoneNumberRaw, model.CountryCodeSelected);

                    // Use the PhoneNumber object to get information from the utility and assign it to the raw state of the model.
                    // The values can't be assigned directly to the model because they have previously existing models.
                    // ASP.NET Core Tag Helpers work differently than Html Helpers in this respect (and others).
                    ModelState.FirstOrDefault(x => x.Key == nameof(model.Valid)).Value.RawValue =
                        _phoneUtil.IsValidNumberForRegion(phoneNumber, model.CountryCodeSelected);
                    ModelState.FirstOrDefault(x => x.Key == nameof(model.PhoneNumberType)).Value.RawValue =
                        _phoneUtil.GetNumberType(phoneNumber);
                    ModelState.FirstOrDefault(x => x.Key == nameof(model.CountryCode)).Value.RawValue =
                        _phoneUtil.GetRegionCodeForNumber(phoneNumber);
                    ModelState.FirstOrDefault(x => x.Key == nameof(model.PhoneNumberFormatted)).Value.RawValue =
                        _phoneUtil.FormatOutOfCountryCallingNumber(phoneNumber, model.CountryCodeSelected);
                    ModelState.FirstOrDefault(x => x.Key == nameof(model.PhoneNumberMobileDialing)).Value.RawValue =
                        _phoneUtil.FormatNumberForMobileDialing(phoneNumber, model.CountryCodeSelected, true);

                    ModelState.FirstOrDefault(x => x.Key == nameof(model.HasExtension)).Value.RawValue =
                        phoneNumber.HasExtension;

                    // The submitted value has to be returned as the raw value.
                    ModelState.FirstOrDefault(x => x.Key == nameof(model.CountryCodeSelected)).Value.RawValue =
                        model.CountryCodeSelected;
                    ModelState.FirstOrDefault(x => x.Key == nameof(model.PhoneNumberRaw)).Value.RawValue =
                        model.PhoneNumberRaw;

                    // Because the Countries property of the view model doesn't exist at this point (it's not passed back by
                    // the model binder when the form is submitted) it can be assigned directly before being returned to the view.
                    model.Countries = _countries.CountrySelectList;

                    return View(model);
                }
                catch (NumberParseException npex)
                {
                    // If PhoneNumberUtil throws an error, add it to the list of ModelState errors.
                    // This will change ModelState.IsValid to false.
                    ModelState.AddModelError(npex.ErrorType.ToString(), npex.Message);
                }
            }

            // If there is an unspecified ModelState error or a NumberParseException
            // repopulate the list of countries, selected country, and attempted phone number.
            // Clear the values of the results in both the ModelState values and the model.
            // Doing both is required by MVC handling of Tag Helpers. For Html Helpers, only
            // the model values need to be reset.
            // In production code, the results section would best be implemented with a partial
            // page with its own view model, which could then be reinitialized for each new
            // attempt. It's handled this way to keep the focus on the library functionality.

            model.Countries = _countries.CountrySelectList;

            ModelState.SetModelValue(nameof(model.CountryCodeSelected), model.CountryCodeSelected, model.CountryCodeSelected);
            ModelState.SetModelValue(nameof(model.PhoneNumberRaw), model.PhoneNumberRaw, model.PhoneNumberRaw);

            ModelState.SetModelValue(nameof(model.Valid), false, null);
            model.Valid = false;
            ModelState.SetModelValue(nameof(model.HasExtension), false, null);
            model.HasExtension = false;
            ModelState.SetModelValue(nameof(model.PhoneNumberType), null, null);
            model.PhoneNumberType = null;
            ModelState.SetModelValue(nameof(model.CountryCode), null, null);
            model.CountryCode = null;
            ModelState.SetModelValue(nameof(model.PhoneNumberFormatted), null, null);
            model.PhoneNumberFormatted = null;
            ModelState.SetModelValue(nameof(model.PhoneNumberMobileDialing), null, null);
            model.PhoneNumberMobileDialing = null;

            return View(model);
        }
    }
}
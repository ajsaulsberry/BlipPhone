using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlipPhone.Web.Models;
using PhoneNumbers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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

            if(ModelState.IsValid)
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

            // If there is an unspecified ModelState error or a NumberParseException repopulate the model and return to the view.
            ModelState.FirstOrDefault(x => x.Key == nameof(model.CountryCodeSelected)).Value.RawValue =
                model.CountryCodeSelected;
            ModelState.FirstOrDefault(x => x.Key == nameof(model.PhoneNumberRaw)).Value.RawValue =
                model.PhoneNumberRaw;
            model.Countries = _countries.CountrySelectList;

            return View(model);
        }
    }
}
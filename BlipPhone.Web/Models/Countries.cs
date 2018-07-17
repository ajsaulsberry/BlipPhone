using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BlipPhone.Web.Models
{
    public class Countries
    {
        public IEnumerable<Country> CountryList { get; private set; }
        public IEnumerable<SelectListItem> CountrySelectList { get; private set; }

        public Countries(string filePath)
        {
            CountryList = SeedCountries(filePath);
            CountrySelectList = SetCountriesSelectList();
        }

        private IEnumerable<Country> SeedCountries(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var countries = JsonConvert.DeserializeObject<IEnumerable<Country>>(json);
            return countries;
        }

        private IEnumerable<SelectListItem> SetCountriesSelectList()
        {
            if (CountryList.Any())
            {
                var countrySelectList = new List<SelectListItem>();
                foreach (var c in CountryList)
                {
                    var country = new SelectListItem
                    {
                        Value = c.IsoAlpha2,
                        Text = c.CountryNameEnglish
                    };
                    countrySelectList.Add(country);
                }
                return countrySelectList;
            }
            return null;
        }
    }

    public class Country
    {
        public string CountryNameEnglish { get; set; }

        public string IsoAlpha2 { get; set; }

        public string Iso3 { get; set; }
    }
}
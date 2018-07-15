using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BlipPhone.Web.Models
{
    public interface ICountries
    {
        IEnumerable<Country> CountryList { get; set; }
        IEnumerable<SelectListItem> CountrySelectList { get; set; }
    }

    public class Countries : ICountries
    {
        private readonly IHostingEnvironment _env;
        private static IEnumerable<Country> _countryList;
        private static IEnumerable<SelectListItem> _countrySelectList;

        public IEnumerable<SelectListItem> CountrySelectList
        {
            get => _countrySelectList;
            set => _countrySelectList = value;
        }

        public IEnumerable<Country> CountryList
        {
            get => _countryList;
            set => _countryList = value;
        }

        public Countries(IHostingEnvironment env)
        {
            _env = env;
            _countryList = SeedCountries(Path.GetDirectoryName(_env.ContentRootPath));
            _countrySelectList = SetCountriesSelectList();
        }

        private IEnumerable<SelectListItem> SetCountriesSelectList()
        {
            var countryList = new List<SelectListItem>();
            foreach (var c in _countryList)
            {
                var country = new SelectListItem
                {
                    Value = c.IsoAlpha2,
                    Text = c.CountryNameEnglish
                };
                countryList.Add(country);
            }
            return countryList;
        }

        private IEnumerable<Country> SeedCountries(string hostingPath)
        {
            string filePath = Path.Combine(hostingPath, "BlipPhone.Web\\Data\\iso3166-slim-2.json");
            var json = File.ReadAllText(filePath);
            var countries = JsonConvert.DeserializeObject<IEnumerable<Country>>(json);
            return countries;
        }
    }

    public class Country
    {
        public string CountryNameEnglish { get; set; }

        public string IsoAlpha2 { get; set; }

        public string Iso3 { get; set; }
    }
}

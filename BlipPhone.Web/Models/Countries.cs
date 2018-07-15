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
        IEnumerable<Country> GetCountries();
        IEnumerable<SelectListItem> GetCountriesSelectList();
    }

    public class Countries : ICountries
    {
        private readonly IHostingEnvironment _env;
        private static IEnumerable<Country> _countryList;

        public Countries(IHostingEnvironment env)
        {
            _env = env;
            string hostingPath = Path.GetDirectoryName(_env.ContentRootPath);
            SeedCountries(hostingPath);
        }

        public IEnumerable<Country> GetCountries()
        {
            return _countryList;
        }

        public IEnumerable<SelectListItem> GetCountriesSelectList()
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

        private void SeedCountries(string hostingPath)
        {
            string filePath = Path.Combine(hostingPath, "BlipPhone.Web\\Data\\iso3166-slim-2.json");
            var json = File.ReadAllText(filePath);
            var countries = JsonConvert.DeserializeObject<IEnumerable<Country>>(json);
            _countryList = countries;
        }

    }

    public class Country
    {
        public string CountryNameEnglish { get; set; }

        public string IsoAlpha2 { get; set; }

        public string Iso3 { get; set; }
    }
}

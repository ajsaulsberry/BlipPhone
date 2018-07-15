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

        public IEnumerable<SelectListItem> CountrySelectList { get; set; }

        public IEnumerable<Country> CountryList { get; set; }

        public Countries(IHostingEnvironment env)
        {
            _env = env;
            CountryList = SeedCountries(Path.GetDirectoryName(_env.ContentRootPath));
            CountrySelectList = SetCountriesSelectList();
        }

        private IEnumerable<Country> SeedCountries(string hostingPath)
        {
            string filePath = Path.Combine(hostingPath, "BlipPhone.Web\\Data\\iso3166-slim-2.json");
            var json = File.ReadAllText(filePath);
            var countries = JsonConvert.DeserializeObject<IEnumerable<Country>>(json);
            return countries;
        }

        private IEnumerable<SelectListItem> SetCountriesSelectList()
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
    }

    public class Country
    {
        public string CountryNameEnglish { get; set; }

        public string IsoAlpha2 { get; set; }

        public string Iso3 { get; set; }
    }
}

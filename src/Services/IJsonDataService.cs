using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boostlingo.Services
{
    public interface IJsonDataService
    {
        public Task<string> GetJsonDataAsync(string url);
    }
}

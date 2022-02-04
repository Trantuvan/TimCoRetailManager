using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TRMDesktopUI.Library.Models;

namespace TRMDesktopUI.Library.Api
{
    public class ProductEndpoint : IProductEndpoint
    {
        private readonly IAPIHelper _apiHelper;

        public ProductEndpoint(IAPIHelper aPIHelper)
        {
            _apiHelper = aPIHelper;
        }

        public async Task<List<ProductModel>> GetAll()
        {
            // _aPIHelper.ApiClient access ApiClient prop to get _apiHelper instance in APIHeler
            // _apiHelper contains info of authorization Token and login data
            using (HttpResponseMessage response = await _apiHelper.ApiClient.GetAsync("api/Product"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<List<ProductModel>>();
                    return result;
                }
                else
                {
                    throw new Exception(response?.ReasonPhrase);
                }
            }
        }
    }
}
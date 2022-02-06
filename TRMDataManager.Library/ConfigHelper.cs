using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRMDataManager.Library
{
    public class ConfigHelper
    {
        public static decimal GetTaxRate()
        {
            //TODO: Move this from config to the API
            string rateText = ConfigurationManager.AppSettings["taxRate"];

            // if it not a valid number (rateText tra lai string test khong convert double TryParse ==> false)
            // if convert dc return true; convert output
            bool IsValidTaxRate = Decimal.TryParse(rateText, out decimal output);

            if (IsValidTaxRate == false)
            {
                throw new ConfigurationErrorsException("The tax rate is not setup property");
            }

            return output;
        }
    }
}

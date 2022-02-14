using System;
using System.Configuration;

namespace TRMDataManager.Library
{
    public class ConfigHelper
    {
        public static decimal GetTaxRate(string rateText)
        {
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

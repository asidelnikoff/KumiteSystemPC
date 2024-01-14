using LanguageLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SharedComponentsLibrary
{
    public class IsStringIntegerValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var str = (string)value;
                int val = Convert.ToInt32(str);
            }
            catch
            {
                return new ValidationResult(false, Resources.PleaseEnterIntgeer);
            }

            return ValidationResult.ValidResult;
        }
    }
}

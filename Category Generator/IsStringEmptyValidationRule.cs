using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using LanguageLibrary;

namespace Category_Generator
{
    public class IsStringEmptyValidationRule : ValidationRule
    { 
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var str = (string)value;
                str = str.Trim();
                if (str.Length == 0)
                    throw new Exception();
            }
            catch
            {
                return new ValidationResult(false, Resources.PleaseEnterANonEmptyString);
            }

            return ValidationResult.ValidResult;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KumiteSystem
{
    public class SanctionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int sanction = (int)value;
            int level = int.Parse(parameter.ToString());

            return sanction >= level;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int sanction = int.Parse(parameter.ToString());
            return (bool)value ? sanction : sanction - 1; 
        }
    }
}

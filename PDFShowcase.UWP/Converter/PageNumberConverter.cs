using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PDFShowcase.UWP.Converter
{
    public class PageNumberConverter : IValueConverter
    {
        public PageNumberConverter()
        {

        }
        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var result= System.Convert.ToInt32(value);
            return result + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

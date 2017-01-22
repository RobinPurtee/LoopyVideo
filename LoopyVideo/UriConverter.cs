using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
namespace LoopyVideo
{
    public class UriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object ret = DependencyProperty.UnsetValue;
            if (typeof(string) == targetType)
            {
                Uri uri = value as Uri;
                if(null != uri)
                {
                    ret = uri.ToString();
                }
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            object ret = DependencyProperty.UnsetValue;
            if(typeof(Uri) ==  targetType)
            {
                ret = new Uri(value as string);
            }
            return ret;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
namespace LoopyVideo
{
    public class UriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string ret = string.Empty;
            if(typeof(string) == targetType)
            {
                Uri uri = value as Uri;
                if(null != uri)
                {
                    ret = uri.ToString();
                }
            }
            else
            {
                throw new ArgumentException("UriConverter.Convert: Only Uri to String convertion supported");
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Uri ret = null;
            if(typeof(Uri) ==  targetType)
            {
                ret = new Uri(value as string);
            }
            else
            {
                throw new ArgumentException("UriConverter.ConvertBack: Only String to Uri convertion supported");
            }
            return ret;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interactivity;


namespace Kbtter4.Views
{
    public sealed class Int32ToShortenNumberStringConverter : IValueConverter
    {
        //エクサまで対応

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var t = (long)value;
            if (t < 1000)
            {
                return t.ToString();
            }
            else if (t < 1000000)
            {
                var dn = ((double)t) / 1000.0;
                return dn >= 100 ? dn.ToString("#") + "K" : dn.ToString("#.#") + "K";
            }
            else if (t < 1000000000)
            {
                var dn = ((double)t) / 1000000.0;
                return dn >= 100 ? dn.ToString("#") + "M" : dn.ToString("#.#") + "M";
            }
            else if (t < 1000000000000)
            {
                var dn = ((double)t) / 1000000000.0;
                return dn >= 100 ? dn.ToString("#") + "G" : dn.ToString("#.#") + "G";
            }
            else if (t < 1000000000000000)
            {
                var dn = ((double)t) / 1000000000000.0;
                return dn >= 100 ? dn.ToString("#") + "T" : dn.ToString("#.#") + "T";
            }
            else if (t < 1000000000000000000)
            {
                var dn = ((double)t) / 1000000000000000.0;
                return dn >= 100 ? dn.ToString("#") + "P" : dn.ToString("#.#") + "P";
            }
            else
            {
                var dn = ((double)t) / 1000000000000000.0;
                return dn >= 100 ? dn.ToString("#") + "E" : dn.ToString("#.#") + "E";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

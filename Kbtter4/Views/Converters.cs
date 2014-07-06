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

using Kbtter4.ViewModels;

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

    public sealed class Kbtter4NotificationIconConverter : IValueConverter
    {
        static Dictionary<Kbtter4NotificationIconKind, BitmapImage> Icons = new Dictionary<Kbtter4NotificationIconKind, BitmapImage>
        {
            {Kbtter4NotificationIconKind.Favorited,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_fav.png"))},
            {Kbtter4NotificationIconKind.Unfavorited,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_favno.png"))},
            {Kbtter4NotificationIconKind.Retweeted,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_rt.png"))},
            {Kbtter4NotificationIconKind.ListAdded,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_list.png"))},
            {Kbtter4NotificationIconKind.ListRemoved,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_listno.png"))},
            {Kbtter4NotificationIconKind.Followed,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_user.png"))},
            {Kbtter4NotificationIconKind.Unfollowed,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_userno.png"))},
            {Kbtter4NotificationIconKind.Blocked,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_block.png"))},
            {Kbtter4NotificationIconKind.Unblocked,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_cancel.png"))},
            {Kbtter4NotificationIconKind.Undefined,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_cancel.png"))},
            {Kbtter4NotificationIconKind.None,new BitmapImage(new Uri("pack://application:,,,/Resources/icon_cancel.png"))},
        };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = (Kbtter4NotificationIconKind)value;

            return Icons[v];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

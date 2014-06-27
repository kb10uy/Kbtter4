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
    public class HyperlinkMouseOverColorChangeBehavior : Behavior<Hyperlink>
    {
        public static DependencyProperty MouseEnteredForegroundProperty =
            DependencyProperty.Register(
                "MouseEnteredForeground",
                typeof(Brush),
                typeof(HyperlinkMouseOverColorChangeBehavior),
                new UIPropertyMetadata(null));

        public static DependencyProperty MouseLeftForegroundProperty =
            DependencyProperty.Register(
                "MouseLeftForeground",
                typeof(Brush),
                typeof(HyperlinkMouseOverColorChangeBehavior),
                new UIPropertyMetadata(null));

        public Brush MouseEnteredForeground
        {
            get { return GetValue(MouseEnteredForegroundProperty) as Brush; }
            set { SetValue(MouseEnteredForegroundProperty, value); }
        }

        public Brush MouseLeftForeground
        {
            get { return GetValue(MouseLeftForegroundProperty) as Brush; }
            set { SetValue(MouseLeftForegroundProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.Foreground = MouseLeftForeground;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
        {
            AssociatedObject.Foreground = MouseEnteredForeground;
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            AssociatedObject.Foreground = MouseLeftForeground;
        }

    }

    public sealed class ImageWebLazyBindBehavior : Behavior<Image>
    {
        public static DependencyProperty UriSourceProperty =
            DependencyProperty.RegisterAttached(
                "UriSource",
                typeof(Uri),
                typeof(ImageWebLazyBindBehavior),
                new UIPropertyMetadata(null, RefreshImageSource));

        public Uri UriSource
        {
            get { return GetValue(UriSourceProperty) as Uri; }
            set { SetValue(UriSourceProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private static async void RefreshImageSource(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            using (var wc = new WebClient { CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable) })
            {
                try
                {
                    var ba = await wc.DownloadDataTaskAsync(e.NewValue as Uri);
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.CreateOptions = BitmapCreateOptions.None;
                    bi.StreamSource = new MemoryStream(ba);
                    bi.EndInit();
                    bi.Freeze();
                    (sender as ImageWebLazyBindBehavior).AssociatedObject.Source = bi;
                }
                catch
                {

                }
            }
        }
    }

    public sealed class TextBlockTextChangedStoryboardBehavior : Behavior<TextBlock>
    {
        public static DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(TextBlockTextChangedStoryboardBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, TextChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        public static DependencyProperty StoryboardProperty =
            DependencyProperty.Register(
                "Storyboard",
                typeof(Storyboard),
                typeof(TextBlockTextChangedStoryboardBehavior),
                new UIPropertyMetadata(null));

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        public Storyboard Storyboard
        {
            get { return GetValue(StoryboardProperty) as Storyboard; }
            set { SetValue(StoryboardProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private static void TextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var bh = (sender as TextBlockTextChangedStoryboardBehavior);
            bh.AssociatedObject.Text = bh.Text;
            bh.Storyboard.Begin();
        }
    }

    public sealed class ExtraPropertyMetadata : PropertyMetadata
    {
        protected override void OnApply(DependencyProperty dp, Type targetType)
        {
            base.OnApply(dp, targetType);

        }
    }

    public sealed class Kbtter4NotificationIconBehavior : Behavior<Image>
    {
        public static DependencyProperty IconKindProperty =
            DependencyProperty.RegisterAttached(
                "IconKind",
                typeof(Kbtter4NotificationIconKind),
                typeof(Kbtter4NotificationIconBehavior),
                new UIPropertyMetadata(
                    Kbtter4NotificationIconKind.Undefined,
                    IconChanged)
            );

        public Kbtter4NotificationIconKind IconKind
        {
            get { return (Kbtter4NotificationIconKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private static void IconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var bh = sender as Kbtter4NotificationIconBehavior;
            try
            {
                Uri uri;
                switch (bh.IconKind)
                {
                    case Kbtter4NotificationIconKind.Unfavorited:
                        uri = new Uri("/Resources/icon_favno.png", UriKind.Relative);
                        break;
                    case Kbtter4NotificationIconKind.Favorited:
                        uri = new Uri("/Resources/icon_fav.png", UriKind.Relative);
                        break;

                    case Kbtter4NotificationIconKind.Followed:
                        uri = new Uri("/Resources/icon_user.png", UriKind.Relative);
                        break;
                    case Kbtter4NotificationIconKind.Unfollowed:
                        uri = new Uri("/Resources/icon_userno.png", UriKind.Relative);
                        break;

                    case Kbtter4NotificationIconKind.ListAdded:
                        uri = new Uri("/Resources/icon_list.png", UriKind.Relative);
                        break;
                    case Kbtter4NotificationIconKind.ListRemoved:
                        uri = new Uri("/Resources/icon_listno.png", UriKind.Relative);
                        break;

                    case Kbtter4NotificationIconKind.Blocked:
                        uri = new Uri("/Resources/icon_block.png", UriKind.Relative);
                        break;

                    default:
                        uri = new Uri("/Resources/icon_cancel.png", UriKind.Relative);
                        break;
                }
                var bi = new BitmapImage(uri);
                bh.AssociatedObject.Source = bi;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

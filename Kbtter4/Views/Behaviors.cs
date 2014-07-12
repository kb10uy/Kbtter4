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
using Livet;
using Livet.Commands;

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

    public class HyperlinkMouseOverUnderlineBehavior : Behavior<Hyperlink>
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.TextDecorations = null;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
        {
            AssociatedObject.TextDecorations = TextDecorations.Underline;
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            AssociatedObject.TextDecorations = null;
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

    public sealed class ListViewSelectedItemBindingBehavior : Behavior<ListView>
    {
        public static DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(UserViewModel),
                typeof(ListViewSelectedItemBindingBehavior),
                new UIPropertyMetadata(null));

        public UserViewModel SelectedItem
        {
            get { return GetValue(SelectedItemProperty) as UserViewModel; }
            set
            {
                SetValue(SelectedItemProperty, value);
                UpdateSelected(value, false);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void UpdateSelected(object val, bool uic)
        {
            for (int i = 0; i < AssociatedObject.Items.Count; i++)
            {
                if (AssociatedObject.Items[i] == val)
                {
                    AssociatedObject.SelectedIndex = i;
                    break;
                }
            }
        }

        void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetValue(SelectedItemProperty, e.AddedItems[0]);
        }
    }

    public sealed class TextBlockStatusTextBehavior : Behavior<TextBlock>
    {
        public static DependencyProperty TextElementsProperty =
            DependencyProperty.RegisterAttached(
                "TextElements",
                typeof(ObservableSynchronizedCollection<StatusTextElement>),
                typeof(TextBlockStatusTextBehavior));

        public ObservableSynchronizedCollection<StatusTextElement> TextElements
        {
            get { return GetValue(TextElementsProperty) as ObservableSynchronizedCollection<StatusTextElement>; }
            set
            {
                SetValue(TextElementsProperty, value);
                RefreshInline(value);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            RefreshInline(GetValue(TextElementsProperty) as ObservableSynchronizedCollection<StatusTextElement>);
        }

        private void RefreshInline(IList<StatusTextElement> elms)
        {
            AssociatedObject.Inlines.Clear();
            foreach (var i in elms)
            {
                switch (i.Type)
                {
                    case StatusTextElementType.None:
                        AssociatedObject.Inlines.Add(i.Surface);
                        break;
                    case StatusTextElementType.Uri:
                    case StatusTextElementType.Media:
                        var hl = new Hyperlink();
                        hl.Inlines.Add(i.Surface);
                        hl.Command = new DelegateCommand<string>(i.Action);
                        hl.CommandParameter = i.Link.ToString();
                        AssociatedObject.Inlines.Add(hl);
                        break;
                    case StatusTextElementType.User:
                    case StatusTextElementType.Hashtag:
                        var hl2 = new Hyperlink();
                        hl2.Inlines.Add(i.Surface);
                        hl2.Command = new DelegateCommand<string>(i.Action);
                        hl2.CommandParameter = i.Link.ToString();
                        AssociatedObject.Inlines.Add(hl2);
                        break;
                }
            }
        }
    }

    public sealed class FileDragDropBehavior : Behavior<FrameworkElement>
    {
        public static DependencyProperty ListenerCommandProperty =
            DependencyProperty.Register(
                "ListenerCommand",
                typeof(ListenerCommand<FileDragDropResult>),
                typeof(FileDragDropBehavior),
                new UIPropertyMetadata(null));

        public ListenerCommand<FileDragDropResult> ListenerCommand
        {
            get { return GetValue(ListenerCommandProperty) as ListenerCommand<FileDragDropResult>; }
            set { SetValue(ListenerCommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewDragOver += AssociatedObject_PreviewDragOver;
            AssociatedObject.Drop += AssociatedObject_Drop;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewDragOver -= AssociatedObject_PreviewDragOver;
            AssociatedObject.Drop -= AssociatedObject_Drop;
        }

        void AssociatedObject_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (ListenerCommand != null && ListenerCommand.CanExecute) ListenerCommand.Execute(new FileDragDropResult(files));
        }
    }

    public sealed class DelegateCommand<T> : ICommand
        where T : class
    {
        Action<T> action;

        public DelegateCommand(Action<T> act)
        {
            action = act;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            action(parameter as T);
        }
    }

}

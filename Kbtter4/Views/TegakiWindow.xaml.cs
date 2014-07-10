using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using emanual.Wpf.Controls;
using System.IO;
using Kbtter4.ViewModels;
using ColorDialog = emanual.Wpf.Controls.WpfColorDialog;

namespace Kbtter4.Views
{
    /* 
     * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// TegakiWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TegakiWindow : Window
    {
        public static string TegakiImageFolder = "tegaki";

        public TegakiWindow()
        {
            InitializeComponent();
            InkCanvasMain.DefaultDrawingAttributes.Color = Colors.Black;
            InkCanvasMain.DefaultDrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Ellipse;
            RectanglePenBrush.Fill = new SolidColorBrush(Colors.Black);
            RectangleBackgroundBrush.Fill = Brushes.White;

            var dt = DateTime.Now;
            TextBoxFileName.Text = string.Format("{5:D4}-{0:D2}-{1:D2}-{2:D2}{3:D2}{4:D2}.png", dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Year);
        }

        private void RectanglePenBrush_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var bs = new ColorDialog();
            bs.OwnerWindow = this;
            if (bs.ShowDialog() ?? false)
            {
                InkCanvasMain.DefaultDrawingAttributes.Color = bs.SelectedColor;
                RectanglePenBrush.Fill = new SolidColorBrush(bs.SelectedColor);
            }
        }

        private void RectanglebackgroundBrush_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var bs = new WpfBrushEditor();
            bs.OwnerWindow = this;
            if (bs.ShowDialog() ?? false)
            {
                RectangleBackgroundBrush.Fill = bs.SelectedBrush;
            }
        }

        private void ButtonVanish_Click(object sender, RoutedEventArgs e)
        {
            InkCanvasMain.Strokes.Clear();
        }

        private void RadioButtonEraser_Click(object sender, RoutedEventArgs e)
        {
            InkCanvasMain.EditingMode = InkCanvasEditingMode.EraseByPoint;
        }

        private void RadioButtonPen_Click(object sender, RoutedEventArgs e)
        {
            InkCanvasMain.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void SliderPenWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InkCanvasMain.DefaultDrawingAttributes.Width = e.NewValue;
            InkCanvasMain.DefaultDrawingAttributes.Height = e.NewValue;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var rect = new Rect { Width = 512, Height = 384 };
            var dv = new DrawingVisual();

            var dc = dv.RenderOpen();
            dc.PushTransform(new TranslateTransform(-rect.X, -rect.Y));
            dc.DrawRectangle(InkCanvasMain.Background, null, rect);
            InkCanvasMain.Strokes.Draw(dc);
            dc.Close();

            var rtb = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
            rtb.Render(dv);
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));

            var fn = TextBoxFileName.Text;
            if (!fn.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) fn += ".png";
            using (Stream s = File.Create(TegakiImageFolder + "/" + fn))
            {
                enc.Save(s);
            }
            ((TegakiWindowViewModel)DataContext).AddToMediaList(System.IO.Path.GetFullPath(TegakiImageFolder + "/" + fn));
            Close();
        }
    }
}
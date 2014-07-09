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
        public TegakiWindow()
        {
            InitializeComponent();
        }

        private void RectanglePenBrush_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var bs = new WpfColorDialog();
            bs.OwnerWindow = this;
            if (bs.ShowDialog() ?? false)
            {
                InkCanvasMain.DefaultDrawingAttributes.Color = bs.SelectedColor;
            }
        }

        private void RectanglebackgroundBrush_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var bs = new WpfBrushEditor();
            bs.OwnerWindow = this;
            if (bs.ShowDialog() ?? false)
            {
                InkCanvasMain.Background = bs.SelectedBrush;
            }
        }
    }
}
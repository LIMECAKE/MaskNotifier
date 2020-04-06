using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MaskNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeTitleBar();
        }
        
        private void InitializeTitleBar()
        {
            titleBar.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs args)
            {
                this.Hide();
            };
        }
    }
}
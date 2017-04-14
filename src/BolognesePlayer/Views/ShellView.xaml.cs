using System.Windows;
using System.Windows.Input;

namespace Bolognese.Desktop
{
    /// <summary>
    /// Interaction logic for MainShell.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
            MouseDown += ShellView_MouseDown;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void ShellView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}

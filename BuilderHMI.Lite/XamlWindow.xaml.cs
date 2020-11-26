using System.Windows;

namespace BuilderHMI.Lite
{
    /// <summary>
    /// Interaction logic for XamlWindow.xaml
    /// </summary>
    public partial class XamlWindow : Window
    {
        public XamlWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}

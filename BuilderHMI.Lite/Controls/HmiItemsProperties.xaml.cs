using System.Windows.Controls;

namespace BuilderHMI.Lite
{
    /// <summary>
    /// Interaction logic for HmiItemsProperties.xaml
    /// </summary>
    public partial class HmiItemsProperties : UserControl, IHmiPropertyPage
    {
        public HmiItemsProperties(string title, string hint = "")
        {
            InitializeComponent();
            tbTitle.Text = title;
            tbHint.Text = hint;
        }

        public void Reset()
        {
            control = null;
        }

        private IHmiListControl control = null;

        public IHmiListControl TheControl
        {
            get
            {
                return control;
            }
            set
            {
                if (control != value)
                {
                    control = null;
                    tbName.SetText(value.Name);
                    tbItems.SetText(value.Elements);
                    control = value;
                }
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (control != null)
                control.OwnerPage.SetName(control, tbName.Text);
        }

        private void Items_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (control != null)
                control.Elements = tbItems.Text;
        }
    }
}

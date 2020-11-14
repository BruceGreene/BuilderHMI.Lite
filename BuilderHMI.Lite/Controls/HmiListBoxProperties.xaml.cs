using System.Windows.Controls;

namespace BuilderHMI.Lite
{
    /// <summary>
    /// Interaction logic for HmiListBoxProperties.xaml
    /// </summary>
    public partial class HmiListBoxProperties : UserControl, IHmiPropertyPage
    {
        public HmiListBoxProperties(string title)
        {
            InitializeComponent();
            tbTitle.Text = title;
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

using System.Windows;
using System.Windows.Controls;

namespace BuilderHMI.Lite
{
    /// <summary>
    /// Interaction logic for HmiControlProperties.xaml
    /// </summary>
    public partial class HmiControlProperties : UserControl, IHmiPropertyPage
    {
        public HmiControlProperties(string title)
        {
            InitializeComponent();
            tbTitle.Text = title;
        }

        public void Reset()
        {
            control = null;
        }

        private IHmiControl control = null;

        public IHmiControl TheControl
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
                    if (value != null)
                    {
                        tbName.SetText(value.Name);
                        control = value;
                    }
                }
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (control != null)
                control.OwnerPage.SetName(control, tbName.Text);
        }
    }
}

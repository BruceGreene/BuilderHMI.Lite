using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BuilderHMI.Lite
{
    /// <summary>
    /// Interaction logic for HmiRangeBaseProperties.xaml
    /// </summary>
    public partial class HmiRangeBaseProperties : UserControl, IHmiPropertyPage
    {
        public HmiRangeBaseProperties(string title)
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
                    if (value is HmiProgressBar pbar)
                    {
                        tbName.SetText(value.Name);
                        tbMin.SetText(pbar.Minimum);
                        tbMax.SetText(pbar.Maximum);
                        control = pbar;
                    }
                    else if (value is HmiSlider slider)
                    {
                        tbName.SetText(value.Name);
                        tbMin.SetText(slider.Minimum);
                        tbMax.SetText(slider.Maximum);
                        control = slider;
                    }
                }
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (control != null)
                control.OwnerPage.SetName(control, tbName.Text);
        }

        private void MinMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (control is RangeBase rb)
            {
                double value;
                rb.Minimum = double.TryParse(tbMin.Text, out value) ? value : 0.0;
                rb.Maximum = double.TryParse(tbMax.Text, out value) ? value : 0.0;
            }
        }
    }
}

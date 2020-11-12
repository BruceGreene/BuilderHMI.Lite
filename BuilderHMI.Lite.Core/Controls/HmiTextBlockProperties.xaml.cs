using System.Windows;
using System.Windows.Controls;

namespace BuilderHMI.Lite.Core
{
    /// <summary>
    /// Interaction logic for HmiTextBlockProperties.xaml
    /// </summary>
    public partial class HmiTextBlockProperties : UserControl, IHmiPropertyPage
    {
        public HmiTextBlockProperties()
        {
            InitializeComponent();
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
                    if (value is HmiTextBlock tb)
                    {
                        tbName.SetText(value.Name);
                        tbTitle.Text = "Text Block Properties";
                        tbText.SetText(tb.Text.Replace('\n', '|'));
                        control = tb;
                    }
                    else if (value is HmiGroupBox group)
                    {
                        tbName.SetText(value.Name);
                        tbTitle.Text = "Group Box Properties";
                        if (group.Header is string glabel)
                            tbText.SetText(glabel.Replace('\n', '|'));
                        control = group;
                    }
                }
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (control != null)
                control.OwnerPage.SetName(control, tbName.Text);
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (control is HmiTextBlock tb)
                tb.Text = tbText.Text.Replace('|', '\n');
            else if (control is HmiGroupBox group)
                group.Header = tbText.Text.Replace('|', '\n');
        }
    }
}

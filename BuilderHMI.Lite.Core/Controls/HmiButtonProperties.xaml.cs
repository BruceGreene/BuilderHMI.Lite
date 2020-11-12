using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BuilderHMI.Lite.Core
{
    /// <summary>
    /// Interaction logic for HmiButtonProperties.xaml
    /// </summary>
    public partial class HmiButtonProperties : UserControl, IHmiPropertyPage
    {
        public HmiButtonProperties()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            button = null;
        }

        private HmiButton button = null;

        public HmiButton TheButton
        {
            get
            {
                return button;
            }
            set
            {
                if (button != value)
                {
                    button = null;
                    if (value != null)
                    {
                        tbName.SetText(value.Name);
                        tbLabel.SetText(value.Text.Replace('\n', '|'));
                        tbImageFile.Text = string.IsNullOrEmpty(value.ImageFile) ? "(no file selected)" : value.ImageFile;
                        button = value;
                    }
                }
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (button != null)
                button.OwnerPage.SetName(button, tbName.Text);
        }

        private void Label_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (button != null)
                button.Text = tbLabel.Text.Replace('|', '\n');
        }

        private void ImageFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (button != null)
                button.ImageFile = (tbImageFile.Text == "(no file selected)") ? "" : tbImageFile.Text;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            var dbox = new OpenFileDialog();
            dbox.Title = "Select an Image File";
            dbox.Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*";
            dbox.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", button.ImageFile);
            if (File.Exists(path))
                dbox.FileName = Path.GetFileName(path);
            if (dbox.ShowDialog() == true && File.Exists(dbox.FileName))
                tbImageFile.Text = Path.GetFileName(dbox.FileName);
        }
    }
}

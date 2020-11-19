using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BuilderHMI.Lite
{
    /// <summary>
    /// Interaction logic for HmiImageProperties.xaml
    /// </summary>
    public partial class HmiImageProperties : UserControl, IHmiPropertyPage
    {
        public HmiImageProperties()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            image = null;
        }

        private HmiImage image = null;

        public HmiImage TheImage
        {
            get
            {
                return image;
            }
            set
            {
                if (image != value)
                {
                    image = null;
                    if (value != null)
                    {
                        tbName.SetText(value.Name);
                        tbImageFile.Text = string.IsNullOrEmpty(value.ImageFile) ? "(no file selected)" : value.ImageFile;
                        cbStretch.SelectedIndex = (int)value.Stretch;
                        image = value;
                    }
                }
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (image != null)
                image.OwnerPage.SetName(image, tbName.Text);
        }

        private void Stretch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (image != null && cbStretch.SelectedIndex >= 0)
                image.Stretch = (System.Windows.Media.Stretch)cbStretch.SelectedIndex;
        }

        private void ImageFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (image != null)
                image.ImageFile = (tbImageFile.Text == "(no file selected)") ? "" : tbImageFile.Text;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            var dbox = new OpenFileDialog();
            dbox.Title = "Select an Image File";
            dbox.Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*";
            string imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            dbox.InitialDirectory = imageDirectory;
            string path = Path.Combine(imageDirectory, image.ImageFile);
            if (File.Exists(path))
                dbox.FileName = Path.GetFileName(path);
            if (dbox.ShowDialog() == true && File.Exists(dbox.FileName))
            {
                string imageFileName = Path.GetFileName(dbox.FileName);
                if (!Path.GetDirectoryName(dbox.FileName).Equals(imageDirectory, StringComparison.InvariantCultureIgnoreCase))
                    File.Copy(dbox.FileName, Path.Combine(imageDirectory, imageFileName));
                tbImageFile.Text = imageFileName;
            }
        }
    }
}

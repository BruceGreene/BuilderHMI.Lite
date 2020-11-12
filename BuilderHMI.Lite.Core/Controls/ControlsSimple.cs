using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BuilderHMI.Lite.Core
{
    // Simple controls do not respond to user input: TextBlock, GroupBox, Border and Image

    public class HmiTextBlock : TextBlock, IHmiControl
    {
        public HmiTextBlock()
        {
            Text = "TEXT BLOCK";
            SetResourceReference(StyleProperty, "TextBlockStyle");
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "text"; } }
        public Size InitialSize { get { return new Size(double.NaN, double.NaN); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.None; } }

        private static HmiTextBlockProperties properties = new HmiTextBlockProperties();
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool vs = false)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<TextBlock Style=\"{DynamicResource TextBlockStyle}\"" : "<HmiTextBlock");
            sb.AppendFormat(" Name=\"{0}\"", Name);
            if (!string.IsNullOrEmpty(Text)) sb.AppendFormat(" Text=\"{0}\"", WebUtility.HtmlEncode(Text).Replace("\n", "&#10;"));
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }
    }

    public class HmiGroupBox : GroupBox, IHmiControl
    {
        public HmiGroupBox()
        {
            Header = "GROUP";
            Content = new Rectangle()
            {
                Fill = Brushes.White,
                Opacity = 0.4
            };
            SetResourceReference(StyleProperty, "GroupBoxStyle");
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "group"; } }
        public Size InitialSize { get { return new Size(100, 100); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize | ECtrlFlags.IsGroup; } }

        private static HmiTextBlockProperties properties = new HmiTextBlockProperties();
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool vs = false)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<GroupBox Style=\"{DynamicResource GroupBoxStyle}\"" : "<HmiGroupBox");
            sb.AppendFormat(" Name=\"{0}\"", Name);
            if (Header is string header)
                sb.AppendFormat(" Header=\"{0}\"", WebUtility.HtmlEncode(header).Replace("\n", "&#10;"));
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }
    }

    public class HmiBorder : Border, IHmiControl
    {
        public HmiBorder()
        {
            SetResourceReference(StyleProperty, "BorderStyle");
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "border"; } }
        public Size InitialSize { get { return new Size(100, 100); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize | ECtrlFlags.IsGroup; } }

        private static HmiControlProperties properties = new HmiControlProperties("Border Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool vs = false)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<Border Style=\"{DynamicResource BorderStyle}\"" : "<HmiBorder");
            sb.AppendFormat(" Name=\"{0}\"", Name);
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }
    }

    public class HmiImage : Grid, IHmiControl
    {
        public HmiImage()
        {
            Children.Add(image);
            ImageFile = "";
            Stretch = Stretch.UniformToFill;
            Background = Brushes.Transparent;
            SetResourceReference(StyleProperty, "ImageStyle");
        }

        private Image image = new Image();

        public static readonly DependencyProperty ImageFileProperty =
            DependencyProperty.Register("ImageFile", typeof(string), typeof(HmiImage), new FrameworkPropertyMetadata("(none)", OnImageFileChanged));

        public string ImageFile
        {
            get { return (string)GetValue(ImageFileProperty); }
            set { SetValue(ImageFileProperty, value); }
        }

        private static void OnImageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HmiImage).SetSource((string)e.NewValue);
        }

        private void SetSource(string imagefile)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", imagefile);
            if (File.Exists(path))
            {
                try
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;  // prevent file lock
                    bi.UriSource = new Uri(path);
                    bi.EndInit();
                    image.Source = bi;
                    return;
                }
                catch { }
            }

            image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/image.png", UriKind.Absolute));
        }

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(HmiImage), new FrameworkPropertyMetadata(Stretch.None, OnStretchChanged));

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HmiImage).image.Stretch = (Stretch)e.NewValue;
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "image"; } }
        public Size InitialSize { get { return new Size(100, 100); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize; } }

        private static HmiImageProperties properties = new HmiImageProperties();
        public UserControl PropertyPage { get { properties.TheImage = this; return properties; } }

        public string ToXaml(int indentLevel, bool vs = false)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<Image" : "<HmiImage");
            sb.AppendFormat(" Name=\"{0}\" Stretch=\"{1}\"", Name, Stretch);
            if (ImageFile.Length > 0) sb.AppendFormat(vs ? " Source=\"Images/{0}\"" : " ImageFile=\"{0}\"", ImageFile);
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BuilderHMI.Lite
{
    // Command controls respond to user inputs: Button, TextBox, Slider, Hyperlink

    public class HmiButton : Button, IHmiControl
    {
        public HmiButton()
        {
            Text = "BUTTON";
            SetResourceReference(StyleProperty, "ButtonStyle");
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HmiButton), new FrameworkPropertyMetadata("", OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HmiButton button = (HmiButton)d;
            button.SetContent((string)e.NewValue, button.ImageFile);
        }

        public static readonly DependencyProperty ImageFileProperty =
            DependencyProperty.Register("ImageFile", typeof(string), typeof(HmiButton), new FrameworkPropertyMetadata("", OnImageFileChanged));

        public string ImageFile
        {
            get { return (string)GetValue(ImageFileProperty); }
            set { SetValue(ImageFileProperty, value); }
        }

        private static void OnImageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HmiButton button = (HmiButton)d;
            button.SetContent(button.Text, (string)e.NewValue);
        }

        private void SetContent(string text, string imagefile)
        {
            BitmapImage bi = null;
            if (imagefile.Length > 0)
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", imagefile);
                if (File.Exists(path))
                {
                    try
                    {
                        bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;  // prevent file lock
                        bi.UriSource = new Uri(path);
                        bi.EndInit();
                    }
                    catch
                    {
                        bi = null;
                    }
                }
            }

            if (text.Length > 0 && bi != null)
            {
                var sp = new StackPanel();
                var image = new Image();
                image.Source = bi;
                image.Stretch = System.Windows.Media.Stretch.None;
                sp.Children.Add(image);
                var tb = new TextBlock();
                tb.Text = text;
                sp.Children.Add(tb);
                Content = sp;
            }
            else if (bi != null)
            {
                var image = new Image();
                image.Source = bi;
                image.Stretch = System.Windows.Media.Stretch.None;
                Content = image;
            }
            else if (text.Length > 0)
            {
                Content = text;
            }
            else
            {
                Content = null;
            }
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "button"; } }
        public Size InitialSize { get { return new Size(100, 100); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize; } }
        public bool IsEmpty { get { return false; } }

        private static HmiButtonProperties properties = new HmiButtonProperties();
        public UserControl PropertyPage { get { properties.TheButton = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            string text = WebUtility.HtmlEncode(Text).Replace("\n", "&#10;");

            if (vs)
            {
                sb.AppendFormat("<Button Style=\"{{DynamicResource ButtonStyle}}\" Name=\"{0}\"", Name);
                if (eventHandlers)
                    sb.AppendFormat(" Click=\"{0}{1}_Click\"", char.ToUpper(Name[0]), Name.Substring(1));
                if (text.Length > 0 && ImageFile.Length > 0)
                {
                    OwnerPage.AppendLocationXaml(this, sb);
                    sb.AppendLine(">");
                    for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                    sb.AppendLine("<StackPanel>");
                    for (int i = 0; i < indentLevel + 2; i++) sb.Append("    ");
                    sb.AppendFormat("<Image Source=\"Images/{0}\" Stretch=\"None\" />\r\n", ImageFile);
                    for (int i = 0; i < indentLevel + 2; i++) sb.Append("    ");
                    sb.AppendFormat("<TextBlock Text=\"{0}\" />\r\n", text);
                    for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                    sb.AppendLine("</StackPanel>");
                    for (int i = 0; i < indentLevel; i++) sb.Append("    ");
                    sb.Append("</Button>");
                }
                else if (ImageFile.Length > 0)
                {
                    OwnerPage.AppendLocationXaml(this, sb);
                    sb.AppendLine(">");
                    for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                    sb.AppendFormat("<Image Source=\"Images/{0}\" Stretch=\"None\" />\r\n", ImageFile);
                    for (int i = 0; i < indentLevel; i++) sb.Append("    ");
                    sb.Append("</Button>");
                }
                else
                {
                    if (text.Length > 0) sb.AppendFormat(" Content=\"{0}\"", text);
                    OwnerPage.AppendLocationXaml(this, sb);
                    sb.Append(" />");
                }
            }
            else
            {
                sb.AppendFormat("<HmiButton Name=\"{0}\"", Name);
                if (text.Length > 0) sb.AppendFormat(" Text=\"{0}\"", text);
                if (ImageFile.Length > 0) sb.AppendFormat(" ImageFile=\"{0}\"", ImageFile);
                OwnerPage.AppendLocationXaml(this, sb);
                sb.Append(" />");
            }

            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_Click(object sender, RoutedEventArgs e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendLine("        }");
        }
    }

    public class HmiTextBox : TextBox, IHmiControl
    {
        public HmiTextBox()
        {
            SetResourceReference(StyleProperty, "TextBoxStyle");
            Focusable = false;
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "box"; } }
        public Size InitialSize { get { return new Size(100, double.NaN); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.ResizeWidth; } }
        public bool IsEmpty { get { return false; } }

        private static HmiControlProperties properties = new HmiControlProperties("Text Box Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<TextBox Style=\"{DynamicResource TextBoxStyle}\"" : "<HmiTextBox");
            sb.AppendFormat(" Name=\"{0}\"", Name);
            if (eventHandlers)
                sb.AppendFormat(" TextChanged=\"{0}{1}_TextChanged\"", char.ToUpper(Name[0]), Name.Substring(1));
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_TextChanged(object sender, TextChangedEventArgs e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendFormat("            // {0}.Text\r\n", Name);
            sb.AppendLine("        }");
        }
    }

    public class HmiSlider : Slider, IHmiControl
    {
        public HmiSlider()
        {
            Focusable = false;
            SetResourceReference(StyleProperty, "SliderStyle");
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Orientation = IsVertical ? Orientation.Vertical : Orientation.Horizontal;
        }

        private bool IsVertical
        {
            get
            {
                if (double.IsNaN(Width)) return false;
                if (double.IsNaN(Height)) return true;
                return (Height > Width);
            }
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "slider"; } }
        public Size InitialSize { get { return new Size(100, 30); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize; } }
        public bool IsEmpty { get { return false; } }

        private static HmiRangeBaseProperties properties = new HmiRangeBaseProperties("Slider Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<Slider Style=\"{DynamicResource SliderStyle}\"" : "<HmiSlider");
            sb.AppendFormat(" Name=\"{0}\" Minimum=\"{1}\" Maximum=\"{2}\"", Name, Minimum, Maximum);
            if (vs && IsVertical) sb.Append(" Orientation=\"Vertical\"");
            if (eventHandlers)
                sb.AppendFormat(" ValueChanged=\"{0}{1}_ValueChanged\"", char.ToUpper(Name[0]), Name.Substring(1));
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendFormat("            // {0}.Value\r\n", Name);
            sb.AppendLine("        }");
        }
    }

    public class HmiHyperlink : TextBlock, IHmiControl
    {
        public HmiHyperlink()
        {
            Text = "HYPERLINK";
            SetResourceReference(StyleProperty, "HyperlinkStyle");
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "link"; } }
        public Size InitialSize { get { return new Size(double.NaN, double.NaN); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.None; } }
        public bool IsEmpty { get { return string.IsNullOrEmpty(Text); } }

        private static HmiTextBlockProperties properties = new HmiTextBlockProperties();
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");

            if (vs)
            {
                sb.AppendFormat("<TextBlock Style=\"{{DynamicResource HyperlinkStyle}}\" Name=\"{0}\"", Name);
                OwnerPage.AppendLocationXaml(this, sb);
                sb.AppendLine(">");
                for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                if (eventHandlers)
                    sb.AppendFormat("<Hyperlink Click=\"{0}{1}_Click\">\r\n", char.ToUpper(Name[0]), Name.Substring(1));
                else
                    sb.AppendLine("<Hyperlink>");
                for (int i = 0; i < indentLevel + 2; i++) sb.Append("    ");
                sb.AppendLine(WebUtility.HtmlEncode(Text).Replace("\n", "&#10;"));
                for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                sb.AppendLine("</Hyperlink>");
                for (int i = 0; i < indentLevel; i++) sb.Append("    ");
                sb.Append("</TextBlock>");
            }
            else
            {
                sb.AppendFormat("<HmiHyperlink Name=\"{0}\" Text=\"{1}\"", Name, WebUtility.HtmlEncode(Text).Replace("\n", "&#10;"));
                OwnerPage.AppendLocationXaml(this, sb);
                sb.Append(" />");
            }

            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_Click(object sender, RoutedEventArgs e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendLine("        }");
        }
    }
}

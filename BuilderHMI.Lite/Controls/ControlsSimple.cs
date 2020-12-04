using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BuilderHMI.Lite
{
    // Simple controls do not respond to user input: TextBlock, GroupBox, Border, Image, ProgressBar

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
        public bool IsEmpty { get { return string.IsNullOrEmpty(Text); } }

        private static HmiTextBlockProperties properties = new HmiTextBlockProperties();
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<TextBlock Style=\"{DynamicResource TextBlockStyle}\"" : "<HmiTextBlock");
            sb.AppendFormat(" Name=\"{0}\" Text=\"{1}\"", Name, WebUtility.HtmlEncode(Text).Replace("\n", "&#10;"));
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
        }
    }

    public class HmiGroupBox : GroupBox, IGroupHmiControl
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
        public bool IsEmpty { get { return false; } }

        private static HmiTextBlockProperties properties = new HmiTextBlockProperties();
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
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

        public string ToXaml(int indentLevel, bool eventHandlers, Dictionary<IHmiControl, List<IHmiControl>> groups, Thickness frame)
        {
            var groupChildren = groups[this];
            if (groupChildren.Count == 0)
                return ToXaml(indentLevel, eventHandlers, true);

            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append("<GroupBox Style=\"{DynamicResource GroupBoxStyle}\"");
            sb.AppendFormat(" Name=\"{0}\"", Name);
            if (Header is string header)
                sb.AppendFormat(" Header=\"{0}\"", WebUtility.HtmlEncode(header).Replace("\n", "&#10;"));
            OwnerPage.AppendLocationXaml(this, sb);
            sb.AppendLine(">");
            for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
            sb.AppendLine("<Grid>");

            frame.Left += Margin.Left;
            frame.Top += Margin.Top;
            frame.Right += Margin.Right;
            frame.Bottom += Margin.Bottom;
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Center:
                    frame.Left += 6;
                    break;
                case HorizontalAlignment.Right:
                    frame.Right += 6;
                    break;
                case HorizontalAlignment.Stretch:
                    frame.Left += 6;
                    frame.Right += 6;
                    break;
            }
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                case VerticalAlignment.Center:
                    frame.Top += 20;
                    break;
                case VerticalAlignment.Bottom:
                    frame.Bottom += 6;
                    break;
                case VerticalAlignment.Stretch:
                    frame.Top += 20;
                    frame.Bottom += 6;
                    break;
            }

            foreach (IHmiControl control in groupChildren)
            {
                var margin0 = control.fe.Margin;
                MainWindow.Shift(control.fe, frame);
                if (groups.ContainsKey(control))
                    sb.AppendLine((control as IGroupHmiControl).ToXaml(indentLevel + 2, eventHandlers, groups, frame));
                else
                    sb.AppendLine(control.ToXaml(indentLevel + 2, eventHandlers, true));
                control.fe.Margin = margin0;
            }
            for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
            sb.AppendLine("</Grid>");
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append("</GroupBox>");
            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
        }
    }

    public class HmiBorder : Border, IGroupHmiControl
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
        public bool IsEmpty { get { return false; } }

        private static HmiControlProperties properties = new HmiControlProperties("Border Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<Border Style=\"{DynamicResource BorderStyle}\"" : "<HmiBorder");
            sb.AppendFormat(" Name=\"{0}\"", Name);
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }

        public string ToXaml(int indentLevel, bool eventHandlers, Dictionary<IHmiControl, List<IHmiControl>> groups, Thickness frame)
        {
            var groupChildren = groups[this];
            if (groupChildren.Count == 0)
                return ToXaml(indentLevel, eventHandlers, true);

            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append("<Border Style=\"{DynamicResource BorderStyle}\"");
            sb.AppendFormat(" Name=\"{0}\"", Name);
            OwnerPage.AppendLocationXaml(this, sb);
            sb.AppendLine(">");
            for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
            sb.AppendLine("<Grid>");
            frame.Left += Margin.Left;
            frame.Top += Margin.Top;
            frame.Right += Margin.Right;
            frame.Bottom += Margin.Bottom;
            foreach (IHmiControl control in groupChildren)
            {
                var margin0 = control.fe.Margin;
                MainWindow.Shift(control.fe, frame);
                if (groups.ContainsKey(control))
                    sb.AppendLine((control as IGroupHmiControl).ToXaml(indentLevel + 2, eventHandlers, groups, frame));
                else
                    sb.AppendLine(control.ToXaml(indentLevel + 2, eventHandlers, true));
                control.fe.Margin = margin0;
            }
            for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
            sb.AppendLine("</Grid>");
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append("</Border>");
            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
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
            var image = (HmiImage)d;
            image.image.Stretch = (Stretch)e.NewValue;

            if (image.image.Stretch == Stretch.Fill || image.image.Stretch == Stretch.UniformToFill)
            {
                image.image.HorizontalAlignment = HorizontalAlignment.Stretch;
                image.image.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                image.image.HorizontalAlignment = HorizontalAlignment.Left;
                image.image.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "image"; } }
        public Size InitialSize { get { return new Size(100, 100); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize; } }
        public bool IsEmpty { get { return false; } }

        private static HmiImageProperties properties = new HmiImageProperties();
        public UserControl PropertyPage { get { properties.TheImage = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<Image" : "<HmiImage");
            sb.AppendFormat(" Name=\"{0}\" Stretch=\"{1}\"", Name, Stretch);
            if (!string.IsNullOrEmpty(ImageFile))
                sb.AppendFormat(vs ? " Source=\"Images/{0}\"" : " ImageFile=\"{0}\"", ImageFile);
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
        }
    }

    public class HmiProgressBar : ProgressBar, IHmiControl
    {
        public HmiProgressBar()
        {
            Value = Maximum / 4;
            SetResourceReference(StyleProperty, "ProgressBarStyle");
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
        public string NamePrefix { get { return "progress"; } }
        public Size InitialSize { get { return new Size(100, 30); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize; } }
        public bool IsEmpty { get { return false; } }

        private static HmiRangeBaseProperties properties = new HmiRangeBaseProperties("Progress Bar Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");
            sb.Append(vs ? "<ProgressBar Style=\"{DynamicResource ProgressBarStyle}\"" : "<HmiProgressBar");
            sb.AppendFormat(" Name=\"{0}\" Minimum=\"{1}\" Maximum=\"{2}\"", Name, Minimum, Maximum);
            if (vs && IsVertical) sb.Append(" Orientation=\"Vertical\"");
            OwnerPage.AppendLocationXaml(this, sb);
            sb.Append(" />");
            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
        }
    }
}
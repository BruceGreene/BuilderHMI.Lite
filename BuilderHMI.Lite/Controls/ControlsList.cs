using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BuilderHMI.Lite
{
    // List controls manage a fixed-list of options: Listbox, Dropdown List, Checkboxes and Radio Buttons.

    public class HmiListBox : ListBox, IHmiListControl
    {
        public HmiListBox()
        {
            Focusable = false;
            Elements = "OPTION 1|OPTION 2";
            SetResourceReference(StyleProperty, "ListBoxStyle");
        }

        public static readonly DependencyProperty ElementsProperty =
            DependencyProperty.Register("Elements", typeof(string), typeof(HmiListBox), new FrameworkPropertyMetadata("", OnElementsChanged));

        public string Elements
        {
            get { return (string)GetValue(ElementsProperty); }
            set { SetValue(ElementsProperty, value); }
        }

        private static void OnElementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HmiListBox).CreateItems((string)e.NewValue);
        }

        private void CreateItems(string elements)
        {
            Items.Clear();
            string[] labels = elements.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string label in labels)
            {
                if (label.Length > 0)
                {
                    var lbi = new ListBoxItem();
                    lbi.SetResourceReference(StyleProperty, "ListBoxItemStyle");
                    lbi.Content = label;
                    Items.Add(lbi);
                }
            }
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "listbox"; } }
        public Size InitialSize { get { return new Size(100, 100); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize; } }
        public bool IsEmpty { get { return false; } }

        private static HmiItemsProperties properties = new HmiItemsProperties("List Box Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");

            if (vs)
            {
                sb.AppendFormat("<ListBox Style=\"{{DynamicResource ListBoxStyle}}\" Name=\"{0}\"", Name);
                if (eventHandlers)
                    sb.AppendFormat(" SelectionChanged=\"{0}{1}_SelectionChanged\"", char.ToUpper(Name[0]), Name.Substring(1));
                OwnerPage.AppendLocationXaml(this, sb);
                sb.AppendLine(">");
                string[] labels = Elements.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string label in labels)
                {
                    for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                    sb.AppendFormat("<ListBoxItem Style=\"{{DynamicResource ListBoxItemStyle}}\" Content=\"{0}\" />\r\n", label);
                }
                for (int i = 0; i < indentLevel; i++) sb.Append("    ");
                sb.Append("</ListBox>");
            }
            else
            {
                sb.AppendFormat("<HmiListBox Name=\"{0}\"", Name);
                if (Elements.Length > 0) sb.AppendFormat(" Elements=\"{0}\"", Elements);
                OwnerPage.AppendLocationXaml(this, sb);
                sb.Append(" />");
            }

            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_SelectionChanged(object sender, SelectionChangedEventArgs e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendFormat("            // {0}.SelectedIndex\r\n", Name);
            sb.AppendLine("        }");
        }
    }

    public class HmiDropdownList : ComboBox, IHmiListControl
    {
        public HmiDropdownList()
        {
            Focusable = false;
            Elements = "OPTION 1|OPTION 2";
            SetResourceReference(StyleProperty, "ComboBoxStyle");
        }

        public static readonly DependencyProperty ElementsProperty =
            DependencyProperty.Register("Elements", typeof(string), typeof(HmiDropdownList), new FrameworkPropertyMetadata("", OnElementsChanged));

        public string Elements
        {
            get { return (string)GetValue(ElementsProperty); }
            set { SetValue(ElementsProperty, value); }
        }

        private static void OnElementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HmiDropdownList).CreateItems((string)e.NewValue);
        }

        private void CreateItems(string elements)
        {
            Items.Clear();
            string[] labels = elements.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string label in labels)
            {
                if (label.Length > 0)
                {
                    var cbi = new ComboBoxItem();
                    cbi.SetResourceReference(StyleProperty, "ComboBoxItemStyle");
                    cbi.Content = label;
                    Items.Add(cbi);
                }
            }
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "dropdown"; } }
        public Size InitialSize { get { return new Size(100, double.NaN); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.ResizeWidth; } }
        public bool IsEmpty { get { return false; } }

        private static HmiItemsProperties properties = new HmiItemsProperties("Dropdown List Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");

            if (vs)
            {
                sb.AppendFormat("<ComboBox Style=\"{{DynamicResource ComboBoxStyle}}\" Name=\"{0}\"", Name);
                if (eventHandlers)
                    sb.AppendFormat(" SelectionChanged=\"{0}{1}_SelectionChanged\"", char.ToUpper(Name[0]), Name.Substring(1));
                OwnerPage.AppendLocationXaml(this, sb);
                sb.AppendLine(">");
                string[] labels = Elements.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string label in labels)
                {
                    for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                    sb.AppendFormat("<ComboBoxItem Style=\"{{DynamicResource ComboBoxItemStyle}}\" Content=\"{0}\" />\r\n", label);
                }
                for (int i = 0; i < indentLevel; i++) sb.Append("    ");
                sb.Append("</ComboBox>");
            }
            else
            {
                sb.AppendFormat("<HmiDropdownList Name=\"{0}\"", Name);
                if (Elements.Length > 0) sb.AppendFormat(" Elements=\"{0}\"", Elements);
                OwnerPage.AppendLocationXaml(this, sb);
                sb.Append(" />");
            }

            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_SelectionChanged(object sender, SelectionChangedEventArgs e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendFormat("            // {0}.SelectedIndex\r\n", Name);
            sb.AppendLine("        }");
        }
    }

    public class HmiCheckBoxes : StackPanel, IHmiListControl
    {
        public HmiCheckBoxes()
        {
            Background = Brushes.Transparent;
            MinWidth = MinHeight = 10;
            Elements = "OPTION 1|OPTION 2";
        }

        public static readonly DependencyProperty ElementsProperty =
            DependencyProperty.Register("Elements", typeof(string), typeof(HmiCheckBoxes), new FrameworkPropertyMetadata("", OnElementsChanged));

        public string Elements
        {
            get { return (string)GetValue(ElementsProperty); }
            set { SetValue(ElementsProperty, value); }
        }

        private static void OnElementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HmiCheckBoxes).CreateCheckBoxes((string)e.NewValue);
        }

        private void CreateCheckBoxes(string items)
        {
            Children.Clear();
            string[] labels = items.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string label in labels)
            {
                var cb = new CheckBox();
                cb.SetResourceReference(StyleProperty, "CheckBoxStyle");
                cb.Content = label;
                if (Children.Add(cb) >= 30)
                    break;
            }
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "check"; } }
        public Size InitialSize { get { return new Size(double.NaN, double.NaN); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.None; } }
        public bool IsEmpty { get { return string.IsNullOrEmpty(Elements); } }

        private static HmiItemsProperties properties = new HmiItemsProperties("Checkbox Group Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");

            if (vs)
            {
                sb.Append("<StackPanel");
                OwnerPage.AppendLocationXaml(this, sb);
                sb.AppendLine(">");
                int index = 0;
                string[] labels = Elements.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string label in labels)
                {
                    for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                    sb.AppendFormat("<CheckBox Style=\"{{DynamicResource CheckBoxStyle}}\" Name=\"{0}_box{1}\" Content=\"{2}\"", Name, ++index, label);
                    if (eventHandlers)
                        sb.AppendFormat(" Checked=\"{0}{1}_Checked\" Unchecked=\"{0}{1}_Checked\"", char.ToUpper(Name[0]), Name.Substring(1));
                    sb.AppendLine(" />");
                }
                for (int i = 0; i < indentLevel; i++) sb.Append("    ");
                sb.Append("</StackPanel>");
            }
            else
            {
                sb.AppendFormat("<HmiCheckBoxes Name=\"{0}\" Elements=\"{1}\"", Name, Elements);
                OwnerPage.AppendLocationXaml(this, sb);
                sb.Append(" />");
            }

            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_Checked(object sender, RoutedEventArgs e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendFormat("            // if ({0}_box1.IsChecked == true) {{ }}\r\n", Name);
            sb.AppendLine("        }");
        }
    }

    public class HmiRadioButtons : StackPanel, IHmiListControl
    {
        public HmiRadioButtons()
        {
            Background = Brushes.Transparent;
            MinWidth = MinHeight = 10;
            Elements = "OPTION 1|OPTION 2";
        }

        public static readonly DependencyProperty ElementsProperty =
            DependencyProperty.Register("Elements", typeof(string), typeof(HmiRadioButtons), new FrameworkPropertyMetadata("", OnElementsChanged));

        public string Elements
        {
            get { return (string)GetValue(ElementsProperty); }
            set { SetValue(ElementsProperty, value); }
        }

        private static void OnElementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HmiRadioButtons).CreateRadioButtons((string)e.NewValue);
        }

        private void CreateRadioButtons(string items)
        {
            Children.Clear();
            string[] labels = items.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string label in labels)
            {
                var rb = new RadioButton();
                rb.SetResourceReference(StyleProperty, "RadioButtonStyle");
                rb.Content = label;
                Children.Add(rb);
            }
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "radio"; } }
        public Size InitialSize { get { return new Size(double.NaN, double.NaN); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.None; } }
        public bool IsEmpty { get { return string.IsNullOrEmpty(Elements); } }

        private static HmiItemsProperties properties = new HmiItemsProperties("Radio Button Group Properties");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");

            if (vs)
            {
                sb.Append("<StackPanel");
                OwnerPage.AppendLocationXaml(this, sb);
                sb.AppendLine(">");
                int index = 0;
                string[] labels = Elements.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string label in labels)
                {
                    for (int i = 0; i < indentLevel + 1; i++) sb.Append("    ");
                    sb.AppendFormat("<RadioButton Style=\"{{DynamicResource RadioButtonStyle}}\" Name=\"{0}_button{1}\" Content=\"{2}\"", Name, ++index, label);
                    if (eventHandlers)
                        sb.AppendFormat(" Checked=\"{0}{1}_Checked\"", char.ToUpper(Name[0]), Name.Substring(1));
                    sb.AppendLine(" />");
                }
                for (int i = 0; i < indentLevel; i++) sb.Append("    ");
                sb.Append("</StackPanel>");
            }
            else
            {
                sb.AppendFormat("<HmiRadioButtons Name=\"{0}\" Elements=\"{1}\"", Name, Elements);
                OwnerPage.AppendLocationXaml(this, sb);
                sb.Append(" />");
            }

            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_Checked(object sender, RoutedEventArgs e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendFormat("            // if ({0}_button1.IsChecked == true) {{ }}\r\n", Name);
            sb.AppendLine("        }");
        }
    }

    public class HmiTreeView : TreeView, IHmiListControl
    {
        public HmiTreeView()
        {
            Focusable = false;
            Elements = "Item 1|Item 2||Item 2a||Item 2b|||Item 2b1||Item 2c|Item 3";
            SetResourceReference(StyleProperty, "TreeViewStyle");
        }

        public static readonly DependencyProperty ElementsProperty =
            DependencyProperty.Register("Elements", typeof(string), typeof(HmiTreeView), new FrameworkPropertyMetadata("", OnElementsChanged));

        public string Elements
        {
            get { return (string)GetValue(ElementsProperty); }
            set { SetValue(ElementsProperty, value); }
        }

        private static void OnElementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HmiTreeView).CreateItems((string)e.NewValue);
        }

        private void CreateItems(string elements)
        {
            Items.Clear();
            ItemsControl node = this;  // node is either the tree view or the last item added
            int nodeLevel = 0, node2Level = 1;  // level 0 is the tree view, level 1 is an item on the trunk
            string[] labels = elements.Split('|');  // "Item 1|Item 2||Item 2a||Item 2b|||Item 2b1"
            foreach (string label in labels)
            {
                if (label.Length > 0)
                {
                    var tvi = new TreeViewItem();
                    tvi.SetResourceReference(StyleProperty, "TreeViewItemStyle");
                    tvi.Header = label;
                    tvi.IsExpanded = true;

                    while (nodeLevel >= node2Level)
                    {
                        node = ItemsControlFromItemContainer(node);
                        --nodeLevel;
                    }

                    node.Items.Add(tvi);
                    node = tvi;
                    ++nodeLevel;
                    node2Level = 1;
                }
                else  // double-pipe
                {
                    ++node2Level;
                }
            }
        }

        public FrameworkElement fe { get { return this; } }
        public MainWindow OwnerPage { get; set; }
        public string NamePrefix { get { return "tree"; } }
        public Size InitialSize { get { return new Size(100, 100); } }
        public ECtrlFlags Flags { get { return ECtrlFlags.Resize; } }
        public bool IsEmpty { get { return false; } }

        private static HmiItemsProperties properties = new HmiItemsProperties("Tree View Properties", "Double pipe \"||\" for level 2 items, etc.");
        public UserControl PropertyPage { get { properties.TheControl = this; return properties; } }

        public string ToXaml(int indentLevel, bool eventHandlers, bool vs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("    ");

            if (vs)
            {
                sb.AppendFormat("<TreeView Style=\"{{DynamicResource TreeViewStyle}}\" Name=\"{0}\"", Name);
                if (eventHandlers)
                    sb.AppendFormat(" SelectedItemChanged=\"{0}{1}_SelectedItemChanged\"", char.ToUpper(Name[0]), Name.Substring(1));
                OwnerPage.AppendLocationXaml(this, sb);
                sb.AppendLine(">");

                bool close = false;
                int nodeLevel = 1, node2Level = 1;
                string[] labels = Elements.Split('|');
                foreach (string label in labels)
                {
                    if (label.Length > 0)
                    {
                        node2Level = Math.Min(node2Level, nodeLevel + 1);
                        while (close && nodeLevel >= node2Level)
                        {
                            for (int i = 0; i < indentLevel + nodeLevel; i++) sb.Append("    ");
                            sb.AppendLine("</TreeViewItem>");
                            --nodeLevel;
                        }
                        nodeLevel = node2Level;
                        for (int i = 0; i < indentLevel + nodeLevel; i++) sb.Append("    ");
                        sb.AppendFormat("<TreeViewItem Style=\"{{DynamicResource TreeViewItemStyle}}\" Header=\"{0}\">\r\n", label);
                        close = true;
                        node2Level = 1;
                    }
                    else  // double-pipe
                    {
                        ++node2Level;
                    }
                }

                while (close && nodeLevel >= 1)
                {
                    for (int i = 0; i < indentLevel + nodeLevel; i++) sb.Append("    ");
                    sb.AppendLine("</TreeViewItem>");
                    --nodeLevel;
                }

                for (int i = 0; i < indentLevel; i++) sb.Append("    ");
                sb.Append("</TreeView>");
            }
            else
            {
                sb.AppendFormat("<HmiTreeView Name=\"{0}\"", Name);
                if (Elements.Length > 0) sb.AppendFormat(" Elements=\"{0}\"", Elements);
                OwnerPage.AppendLocationXaml(this, sb);
                sb.Append(" />");
            }

            return sb.ToString();
        }

        public void AppendCodeBehind(StringBuilder sb)
        {
            sb.AppendFormat("\r\n        private void {0}{1}_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)\r\n", char.ToUpper(Name[0]), Name.Substring(1));
            sb.AppendLine("        {");
            sb.AppendFormat("            // if ({0}.SelectedItem is TreeViewItem item) {{ }}\r\n", Name);
            sb.AppendLine("        }");
        }
    }
}

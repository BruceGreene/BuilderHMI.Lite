using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace BuilderHMI.Lite
{
    // Interfaces and extension classes.

    [Flags]
    public enum ECtrlFlags
    {
        None = 0x00,
        ResizeWidth = 0x01,  // false for labels, text displays, etc
        ResizeHeight = 0x02, // false for labels, text entries, etc
        Resize = 0x03,       // composite of width and height
        IsGroup = 0x04,      // Border and GroupBox
    }

    public interface IHmiControl
    {
        FrameworkElement fe { get; }      // this as FrameworkElement
        string Name { get; set; }         // FrameworkElement.Name
        MainWindow OwnerPage { get; set; }
        string NamePrefix { get; }        // "button", etc for default names
        Size InitialSize { get; }         // design size for new controls
        ECtrlFlags Flags { get; }
        UserControl PropertyPage { get; }
        bool IsEmpty { get; }             // empty TextBlock, etc
        string ToXaml(int indentLevel, bool eventHandlers, bool vs);
        void AppendCodeBehind(StringBuilder sb);
    }

    public interface IGroupHmiControl : IHmiControl  // Border and GroupBox
    {
        string ToXaml(int indentLevel, bool eventHandlers, Dictionary<IHmiControl, List<IHmiControl>> groups, Thickness frame);
    }

    public interface IHmiListControl : IHmiControl  // Listbox, DropdownList, Checkboxes and RadioButtons
    {
        string Elements { get; set; }
    }

    public interface IHmiPropertyPage
    {
        void Reset();
    }

    public static class TextExtensions
    {
        public static void SetText(this TextBox tb, string text)
        {
            tb.Text = string.IsNullOrEmpty(text) ? "" : text;
            tb.SelectAll();
        }

        public static void SetText(this TextBox tb, double value)
        {
            tb.Text = value.ToString();
            tb.SelectAll();
        }

        public static string ToString2(this Thickness thickness)
        {
            if (thickness.Left == thickness.Right && thickness.Top == thickness.Bottom)
            {
                if (thickness.Left == thickness.Top)  // uniform thickness
                    return ((int)thickness.Left).ToString();
                else
                    return string.Format("{0},{1}", (int)thickness.Left, (int)thickness.Top);
            }

            return string.Format("{0},{1},{2},{3}", (int)thickness.Left, (int)thickness.Top, (int)thickness.Right, (int)thickness.Bottom);
        }
    }
}
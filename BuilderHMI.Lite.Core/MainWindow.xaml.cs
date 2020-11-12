using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace BuilderHMI.Lite.Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // This class provides the page design surface. Control selection, mouse drag to move/size, hotkey functionality, 
        // xaml generation and VS project generation.

        public const int GridSize = 4;
        private const int pixelOffset = 12;

        private Designer designer;
        private Marker marker;  // red box surrounding SelectedControl
        private bool leftButtonPressed = false;
        private IHmiControl draggingControl = null, selectedControl = null;
        private Location0 location0 = new Location0();
        private List<Location0> moveList = new List<Location0>();  // children that move with parent

        public MainWindow()
        {
            InitializeComponent();

            designer = new Designer(this);
            marker = new Marker(gridCanvas);

            var label = new HmiTextBlock();
            label.Text = "• Add controls and specify their Alignment.\n• Left-drag to Move controls, Right-drag to Size.\n• Generate Visual Studio project and Build!";
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.VerticalAlignment = VerticalAlignment.Top;
            AddToCanvas(label, 100, 100);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            designer.Owner = this;
            designer.Show();
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.B: SelectedControlToBack(); break;
                    case Key.C: CopySelectedControl(); break;
                    case Key.F: SelectedControlToFront(); break;
                    case Key.V: Paste(); break;
                    case Key.X: CutSelectedControl(); break;
                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down: ArrowKeyNudge(e.Key, true); break;
                    default: return;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Delete: DeleteSelectedControl(); break;
                    case Key.Escape: StopDragging(); SelectedControl = null; break;
                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down: ArrowKeyNudge(e.Key, false); break;
                    default: return;
                }
            }

            e.Handled = true;
        }

        public IHmiControl SelectedControl
        {
            get
            {
                return selectedControl;
            }
            set
            {
                if (selectedControl != value)
                {
                    selectedControl = marker.Control = value;
                    designer.OnSelectedControlChanged();
                }
            }
        }

        #region Move and Size
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            StartDragging(e);
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            StartDragging(e);
            base.OnPreviewMouseRightButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            StopDragging();
            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            StopDragging();
            base.OnPreviewMouseRightButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (draggingControl != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
                    DoDragging(e);
                else
                    StopDragging();
            }
            base.OnMouseMove(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            StopDragging();
            base.OnLostMouseCapture(e);
        }

        private void StartDragging(MouseEventArgs e)
        {
            e.Handled = true;  // prevent button clicks, etc
            // Travel down the visual tree. Select the top IHmiControl but drag the control whose parent is the canvas-style Grid.
            draggingControl = null;
            IHmiControl selectedCtrl = null;
            FrameworkElement fe = null;
            if (e.OriginalSource is FrameworkElement fe2)
                fe = fe2;

            while (fe != null && fe != this)
            {
                FrameworkElement parent = VisualTreeHelper.GetParent(fe) as FrameworkElement;
                if (fe is IHmiControl control)
                {
                    if (selectedCtrl == null)
                        selectedCtrl = control;
                    if (parent == gridCanvas)
                    {
                        draggingControl = control;
                        break;
                    }
                }
                fe = parent;
            }

            SelectedControl = selectedCtrl;

            if (draggingControl != null)
            {
                if (e.RightButton == MouseButtonState.Pressed && (draggingControl.Flags & ECtrlFlags.Resize) == 0)
                {
                    draggingControl = null;
                    SystemSounds.Beep.Play();
                    return;
                }

                leftButtonPressed = (e.LeftButton == MouseButtonState.Pressed);
                location0.Initialize(draggingControl, e.GetPosition(this));
                CaptureMouse();

                if (leftButtonPressed && (draggingControl.Flags & ECtrlFlags.IsGroup) > 0 &&
                    (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift)))
                {
                    moveList.Clear();
                    foreach (object child in gridCanvas.Children)
                    {
                        if ((child is IHmiControl control) && IsInside(control, draggingControl))
                            moveList.Add(new Location0(control));
                    }
                }
            }
        }

        private void DoDragging(MouseEventArgs e)
        {
            e.Handled = true;
            Vector mouseDelta = e.GetPosition(this) - location0.mouse;
            double dx = mouseDelta.X, dy = mouseDelta.Y;
            if (leftButtonPressed)  // move
            {
                location0.Move(gridCanvas.RenderSize, ref dx, ref dy);
                if (dx != 0 || dy != 0)
                {
                    foreach (var item in moveList)
                        item.MoveSimple(dx, dy);
                }
            }
            else  // size
            {
                location0.Size(gridCanvas.RenderSize, ref dx, ref dy);
            }

            if (SelectedControl == draggingControl && (dx != 0 || dy != 0))
            {
                marker.Dragging = leftButtonPressed ? Marker.EDragging.Move : Marker.EDragging.Size;
                marker.Margin = draggingControl.fe.Margin;
                marker.SetAlignmentMarks();
                designer.UpdateLocation();
            }
        }

        private void StopDragging()
        {
            if (draggingControl != null)
            {
                marker.Dragging = Marker.EDragging.None;
                draggingControl = null;
                location0.Clear();
                moveList.Clear();
                ReleaseMouseCapture();
            }
        }

        public static bool IsInside(IHmiControl controlInner, IHmiControl controlOuter)
        {
            if (controlInner == controlOuter || Panel.GetZIndex(controlInner.fe) < Panel.GetZIndex(controlOuter.fe))
                return false;

            Size size = controlInner.fe.RenderSize;
            Point leftTopInner = GetLeftTop(controlInner);
            Point leftTopOuter = GetLeftTop(controlOuter);
            if (leftTopInner.X < leftTopOuter.X || leftTopInner.Y < leftTopOuter.Y) return false;
            if ((leftTopInner.X + size.Width) > (leftTopOuter.X + controlOuter.fe.ActualWidth)) return false;
            if ((leftTopInner.Y + size.Height) > (leftTopOuter.Y + controlOuter.fe.ActualHeight)) return false;

            return true;
        }

        public static bool InMoveList(IHmiControl control)
        {
            foreach (var item in control.OwnerPage.moveList)
            {
                if (item.control == control) return true;
            }
            return false;
        }

        public static double GetLeft(IHmiControl control)
        {
            if (control.fe.HorizontalAlignment == HorizontalAlignment.Right)
                return control.OwnerPage.gridCanvas.ActualWidth - control.fe.ActualWidth - control.fe.Margin.Right;
            else if (control.fe.HorizontalAlignment == HorizontalAlignment.Center)
                return control.fe.Margin.Left + (control.OwnerPage.gridCanvas.ActualWidth - control.fe.ActualWidth - control.fe.Margin.Left) / 2;
            return control.fe.Margin.Left;
        }

        public static double GetRight(IHmiControl control)
        {
            if (control.fe.HorizontalAlignment == HorizontalAlignment.Left)
                return control.OwnerPage.gridCanvas.ActualWidth - control.fe.ActualWidth - control.fe.Margin.Left;
            else if (control.fe.HorizontalAlignment == HorizontalAlignment.Center)
                return (control.OwnerPage.gridCanvas.ActualWidth - control.fe.ActualWidth - control.fe.Margin.Left) / 2;
            return control.fe.Margin.Right;
        }

        public static double GetTop(IHmiControl control)
        {
            if (control.fe.VerticalAlignment == VerticalAlignment.Bottom)
                return control.OwnerPage.gridCanvas.ActualHeight - control.fe.ActualHeight - control.fe.Margin.Bottom;
            else if (control.fe.VerticalAlignment == VerticalAlignment.Center)
                return control.fe.Margin.Top + (control.OwnerPage.gridCanvas.ActualHeight - control.fe.ActualHeight - control.fe.Margin.Top) / 2;
            return control.fe.Margin.Top;
        }

        public static double GetBottom(IHmiControl control)
        {
            if (control.fe.VerticalAlignment == VerticalAlignment.Top)
                return control.OwnerPage.gridCanvas.ActualHeight - control.fe.ActualHeight - control.fe.Margin.Top;
            else if (control.fe.VerticalAlignment == VerticalAlignment.Center)
                return (control.OwnerPage.gridCanvas.ActualHeight - control.fe.ActualHeight - control.fe.Margin.Top) / 2;
            return control.fe.Margin.Bottom;
        }

        private static Point GetLeftTop(IHmiControl control)
        {
            return new Point(GetLeft(control), GetTop(control));
        }

        public void SetAlignment(IHmiControl control, HorizontalAlignment alignH, VerticalAlignment alignV)
        {
            Thickness margin = control.fe.Margin;
            Size size = control.fe.RenderSize;
            Size sizeGrid = gridCanvas.RenderSize;

            if (alignH == HorizontalAlignment.Left && control.fe.HorizontalAlignment != HorizontalAlignment.Left)
            {
                if (control.fe.HorizontalAlignment == HorizontalAlignment.Center)
                {
                    double x2 = (sizeGrid.Width - size.Width - margin.Left) / 2;
                    double left = margin.Left + x2 + 0.5;
                    control.fe.Margin = new Thickness((int)left, margin.Top, 0, margin.Bottom);
                }
                else if (control.fe.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    double left = sizeGrid.Width - size.Width - margin.Right + 0.5;
                    control.fe.Margin = new Thickness((int)left, margin.Top, 0, margin.Bottom);
                }
                else if (control.fe.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    control.fe.Margin = new Thickness(margin.Left, margin.Top, 0, margin.Bottom);
                    control.fe.Width = (int)(sizeGrid.Width - margin.Left - margin.Right + 0.5);
                }
                control.fe.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if (alignH == HorizontalAlignment.Center && control.fe.HorizontalAlignment != HorizontalAlignment.Center)
            {
                if (control.fe.HorizontalAlignment == HorizontalAlignment.Left)
                {
                    double x1 = margin.Left;
                    double x2 = sizeGrid.Width - size.Width - margin.Left;
                    double left = x1 - x2 + 0.5;
                    control.fe.Margin = new Thickness((int)left, margin.Top, 0, margin.Bottom);
                }
                else if (control.fe.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    double x1 = sizeGrid.Width - size.Width - margin.Right;
                    double x2 = margin.Right;
                    double left = x1 - x2 + 0.5;
                    control.fe.Margin = new Thickness((int)left, margin.Top, 0, margin.Bottom);
                }
                else if (control.fe.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    double x1 = margin.Left;
                    double x2 = margin.Right;
                    double left = x1 - x2 + 0.5;
                    control.fe.Margin = new Thickness((int)left, margin.Top, 0, margin.Bottom);
                    control.fe.Width = (int)(sizeGrid.Width - margin.Left - margin.Right + 0.5);
                }
                control.fe.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else if (alignH == HorizontalAlignment.Right && control.fe.HorizontalAlignment != HorizontalAlignment.Right)
            {
                if (control.fe.HorizontalAlignment == HorizontalAlignment.Left)
                {
                    double right = sizeGrid.Width - size.Width - margin.Left + 0.5;
                    control.fe.Margin = new Thickness(0, margin.Top, (int)right, margin.Bottom);
                }
                else if (control.fe.HorizontalAlignment == HorizontalAlignment.Center)
                {
                    double x2 = (sizeGrid.Width - size.Width - margin.Left) / 2;
                    double right = x2 + 0.5;
                    control.fe.Margin = new Thickness(0, margin.Top, (int)right, margin.Bottom);
                }
                else if (control.fe.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    control.fe.Margin = new Thickness(0, margin.Top, margin.Right, margin.Bottom);
                    control.fe.Width = (int)(sizeGrid.Width - margin.Left - margin.Right + 0.5);
                }
                control.fe.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else if (alignH == HorizontalAlignment.Stretch && control.fe.HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                if (control.fe.HorizontalAlignment == HorizontalAlignment.Left)
                {
                    double right = sizeGrid.Width - size.Width - margin.Left + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, margin.Top, (int)right, margin.Bottom);
                }
                else if (control.fe.HorizontalAlignment == HorizontalAlignment.Center)
                {
                    double x2 = (sizeGrid.Width - size.Width - margin.Left) / 2;
                    double left = margin.Left + x2 + 0.5;
                    double right = x2 + 0.5;
                    control.fe.Margin = new Thickness((int)left, margin.Top, (int)right, margin.Bottom);
                }
                else if (control.fe.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    double left = sizeGrid.Width - size.Width - margin.Right + 0.5;
                    control.fe.Margin = new Thickness((int)left, margin.Top, margin.Right, margin.Bottom);
                }
                control.fe.Width = double.NaN;
                control.fe.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            if (alignV == VerticalAlignment.Top && control.fe.VerticalAlignment != VerticalAlignment.Top)
            {
                if (control.fe.VerticalAlignment == VerticalAlignment.Center)
                {
                    double y2 = (sizeGrid.Height - size.Height - margin.Top) / 2;
                    double top = margin.Top + y2 + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, (int)top, margin.Right, 0);
                }
                else if (control.fe.VerticalAlignment == VerticalAlignment.Bottom)
                {
                    double top = sizeGrid.Height - size.Height - margin.Bottom + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, (int)top, margin.Right, 0);
                }
                else if (control.fe.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    control.fe.Margin = new Thickness(margin.Left, margin.Top, margin.Right, 0);
                    control.fe.Height = (int)(sizeGrid.Height - margin.Top - margin.Bottom + 0.5);
                }
                control.fe.VerticalAlignment = VerticalAlignment.Top;
            }
            else if (alignV == VerticalAlignment.Center && control.fe.VerticalAlignment != VerticalAlignment.Center)
            {
                if (control.fe.VerticalAlignment == VerticalAlignment.Top)
                {
                    double y1 = margin.Top;
                    double y2 = sizeGrid.Height - size.Height - margin.Top;
                    double top = y1 - y2 + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, (int)top, margin.Right, 0);
                }
                else if (control.fe.VerticalAlignment == VerticalAlignment.Bottom)
                {
                    double y1 = sizeGrid.Height - size.Height - margin.Bottom;
                    double y2 = margin.Bottom;
                    double top = y1 - y2 + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, (int)top, margin.Right, 0);
                }
                else if (control.fe.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    double y1 = margin.Top;
                    double y2 = margin.Bottom;
                    double top = y1 - y2 + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, (int)top, margin.Right, 0);
                    control.fe.Height = (int)(sizeGrid.Height - margin.Top - margin.Bottom + 0.5);
                }
                control.fe.VerticalAlignment = VerticalAlignment.Center;
            }
            else if (alignV == VerticalAlignment.Bottom && control.fe.VerticalAlignment != VerticalAlignment.Bottom)
            {
                if (control.fe.VerticalAlignment == VerticalAlignment.Top)
                {
                    double bottom = sizeGrid.Height - size.Height - margin.Top + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, 0, margin.Right, (int)bottom);
                }
                else if (control.fe.VerticalAlignment == VerticalAlignment.Center)
                {
                    double y2 = (sizeGrid.Height - size.Height - margin.Top) / 2;
                    double bottom = y2 + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, 0, margin.Right, (int)bottom);
                }
                else if (control.fe.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    control.fe.Margin = new Thickness(margin.Left, 0, margin.Right, margin.Bottom);
                    control.fe.Height = (int)(sizeGrid.Height - margin.Top - margin.Bottom + 0.5);
                }
                control.fe.VerticalAlignment = VerticalAlignment.Bottom;
            }
            else if (alignV == VerticalAlignment.Stretch && control.fe.VerticalAlignment != VerticalAlignment.Stretch)
            {
                if (control.fe.VerticalAlignment == VerticalAlignment.Top)
                {
                    double bottom = sizeGrid.Height - size.Height - margin.Top + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, margin.Top, margin.Right, (int)bottom);
                }
                else if (control.fe.VerticalAlignment == VerticalAlignment.Center)
                {
                    double y2 = (sizeGrid.Height - size.Height - margin.Top) / 2;
                    double top = margin.Top + y2 + 0.5;
                    double bottom = y2 + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, (int)top, margin.Right, (int)bottom);
                }
                else if (control.fe.VerticalAlignment == VerticalAlignment.Bottom)
                {
                    double top = sizeGrid.Height - size.Height - margin.Bottom + 0.5;
                    control.fe.Margin = new Thickness(margin.Left, (int)top, margin.Right, margin.Bottom);
                }
                control.fe.Height = double.NaN;
                control.fe.VerticalAlignment = VerticalAlignment.Stretch;
            }

            if (SelectedControl == control)
            {
                marker.SetAlignment();
                designer.UpdateLocation();
            }
        }

        private void ArrowKeyNudge(Key key, bool ctrl)
        {
            IHmiControl control = SelectedControl;
            double delta = ctrl ? 10 * GridSize : GridSize;  // Ctrl+arrows for 10x nudge
            double left, left0 = GetLeft(control);
            double top, top0 = GetTop(control);
            switch (key)
            {
                case Key.Left: left = Math.Max(left0 - delta, 0); top = top0; break;
                case Key.Right: left = left0 + delta; top = top0; break;
                case Key.Up: left = left0; top = Math.Max(top0 - delta, 0); break;
                case Key.Down: left = left0; top = top0 + delta; break;
                default: return;
            }
            double dx = left - left0, dy = top - top0;

            if (dx == 0 && dy == 0)  // at the edge
            {
                SystemSounds.Beep.Play();
                return;
            }

            if ((control.Flags & ECtrlFlags.IsGroup) > 0 && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                foreach (object child in gridCanvas.Children)
                {
                    if ((child is IHmiControl control2) && IsInside(control2, control))
                        Location0.Shift(control2.fe, dx, dy);
                }
            }

            Location0.Shift(control.fe, dx, dy);

            if (SelectedControl == control)
            {
                marker.Margin = control.fe.Margin;
                designer.UpdateLocation();
            }
        }
        #endregion Move and Size

        private const int BASE_ZINDEX = 0x10000;
        private int zindexTop = BASE_ZINDEX;     // Top is incremented when a control is added or moved to front. 
        private int zindexBottom = BASE_ZINDEX;  // Bottom is decremented when a control is moved to back.

        private void AddToCanvas(IHmiControl control, double leftShift = 0, double topShift = 0)
        {
            control.OwnerPage = this;
            SetName(control, control.Name);
            Panel.SetZIndex(control.fe, zindexTop++);
            Location0.Shift(control.fe, leftShift, topShift);
            gridCanvas.Children.Add(control.fe);
        }

        public void SelectedControlToFront()
        {
            if (Panel.GetZIndex(SelectedControl.fe) == (zindexTop - 1) && (SelectedControl.Flags & ECtrlFlags.IsGroup) == 0)
            {
                SystemSounds.Beep.Play();
                return;
            }

            var sortedChildren = new SortedList<int, IHmiControl>(gridCanvas.Children.Count);
            if ((SelectedControl.Flags & ECtrlFlags.IsGroup) > 0)
            {
                foreach (object child in gridCanvas.Children)
                {
                    if (child is IHmiControl control && IsInside(control, SelectedControl))
                        sortedChildren[Panel.GetZIndex(control.fe)] = control;
                }
            }

            Panel.SetZIndex(SelectedControl.fe, zindexTop++);
            marker.Control = SelectedControl;

            if (sortedChildren.Count > 0)
            {
                foreach (IHmiControl control in sortedChildren.Values)
                    Panel.SetZIndex(control.fe, zindexTop++);
            }
        }

        public void SelectedControlToBack()
        {
            if (Panel.GetZIndex(SelectedControl.fe) == zindexBottom && (SelectedControl.Flags & ECtrlFlags.IsGroup) == 0)
            {
                SystemSounds.Beep.Play();
                return;
            }

            if ((SelectedControl.Flags & ECtrlFlags.IsGroup) > 0)
            {
                var sortedChildren = new SortedList<int, IHmiControl>(gridCanvas.Children.Count);
                foreach (object child in gridCanvas.Children)
                {
                    if (child is IHmiControl control && IsInside(control, SelectedControl))
                        sortedChildren[-Panel.GetZIndex(control.fe)] = control;
                }

                foreach (IHmiControl control in sortedChildren.Values)
                    Panel.SetZIndex(control.fe, --zindexBottom);
            }

            Panel.SetZIndex(SelectedControl.fe, --zindexBottom);
            marker.Control = SelectedControl;
        }

        private Dictionary<string, IHmiControl> childrenByName = new Dictionary<string, IHmiControl>();

        public void SetName(IHmiControl control, string name)
        {
            while (childrenByName.ContainsValue(control))
            {
                var item = childrenByName.First(pair => pair.Value == control);
                childrenByName.Remove(item.Key);
            }

            // Is the candidate name empty or invalid?
            name = name.Replace(' ', '_');
            bool invalid = (name.Length == 0 || !(char.IsLetter(name[0]) || name[0] == '_'));
            foreach (char ch in name)
            {
                if (!char.IsLetterOrDigit(ch) && ch != '_')
                {
                    invalid = true;
                    break;
                }
            }

            if (invalid)
            {
                // Generate a default name if invalid.
                int index = 1;
                do
                {
                    name = string.Format("{0}{1}", control.NamePrefix, index++);
                }
                while (childrenByName.ContainsKey(name));
            }
            else if (childrenByName.ContainsKey(name))
            {
                // Strip off appended "name_123" if present.
                string nameCore = name;
                int index = nameCore.LastIndexOf('_');
                if (index > 0)
                {
                    bool digitsOnly = true;
                    for (int i = index + 1; i < nameCore.Length; i++)
                    {
                        if (!char.IsDigit(nameCore[i]))
                        {
                            digitsOnly = false;
                            break;
                        }
                    }
                    if (digitsOnly)
                        nameCore = nameCore.Substring(0, index);
                }

                // Increment and append "name_123" until unique.
                index = 1;
                do
                {
                    name = string.Format("{0}_{1}", nameCore, index++);
                }
                while (childrenByName.ContainsKey(name));
            }

            control.Name = name;
            childrenByName[control.Name] = control;
        }

        public void AddNew<T>() where T : IHmiControl, new()
        {
            IHmiControl control = new T();
            SelectedControl = null;
            control.fe.HorizontalAlignment = HorizontalAlignment.Left;
            control.fe.VerticalAlignment = VerticalAlignment.Top;
            control.fe.Width = control.InitialSize.Width;
            control.fe.Height = control.InitialSize.Height;
            AddToCanvas(control, pixelOffset, pixelOffset);
            control.fe.SizeChanged += OnAddNewSizeChanged;
        }

        private void OnAddNewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            IHmiControl control = (IHmiControl)sender;
            if (control.fe.ActualWidth > 0 && control.fe.ActualHeight > 0)
            {
                control.fe.SizeChanged -= OnAddNewSizeChanged;  // one-shot
                SelectedControl = control;
            }
        }

        public void DeleteSelectedControl()
        {
            if (SelectedControl == null)
            {
                SystemSounds.Beep.Play();
                return;
            }

            IHmiControl control = SelectedControl;
            SelectedControl = null;

            if ((control.Flags & ECtrlFlags.IsGroup) > 0)
            {
                var sortedChildren = new SortedList<int, IHmiControl>(gridCanvas.Children.Count + 1);
                sortedChildren[-Panel.GetZIndex(control.fe)] = control;
                foreach (object child in gridCanvas.Children)
                {
                    if (child is IHmiControl control2 && IsInside(control2, control))
                        sortedChildren[-Panel.GetZIndex(control2.fe)] = control2;
                }

                foreach (IHmiControl control2 in sortedChildren.Values)
                {
                    childrenByName.Remove(control2.Name);
                    gridCanvas.Children.Remove(control2.fe);
                }
            }
            else
            {
                childrenByName.Remove(control.Name);
                gridCanvas.Children.Remove(control.fe);
            }
        }

        public void CopySelectedControl()
        {
            if (SelectedControl == null)
            {
                SystemSounds.Beep.Play();
                return;
            }

            // Paste does an immediate re-copy which occasionally fails to open the clipboard.
            string xaml = ControlToXaml(SelectedControl);
            try { Clipboard.SetText(xaml); }
            catch { }
        }

        public void CutSelectedControl()
        {
            if (SelectedControl == null)
            {
                SystemSounds.Beep.Play();
                return;
            }

            CopySelectedControl();
            DeleteSelectedControl();
        }

        public void Paste()
        {
            string xaml = Clipboard.GetText().Trim();
            PasteFromXaml(xaml);
        }

        #region XAML
        public static string InsertNamespaces(string xaml)
        {
            if (xaml.Contains("http://schemas.microsoft.com/winfx/2006/xaml/presentation")) return xaml;

            int index = xaml.IndexOf('>');
            int indexSpace = xaml.IndexOf(' ');
            if (indexSpace > 0)
                index = Math.Min(index, indexSpace);
            var sb = new StringBuilder(xaml);
            sb.Insert(index, " xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:local=\"clr-namespace:BuilderHMI.Lite;assembly=BuilderHMI.Lite\"");
            sb.Replace("<Hmi", "<local:Hmi");
            sb.Replace("</Hmi", "</local:Hmi");
            return sb.ToString();
        }

        public void AppendLocationXaml(IHmiControl control, StringBuilder sb)
        {
            if (control.fe.HorizontalAlignment != HorizontalAlignment.Stretch)
                sb.AppendFormat(" HorizontalAlignment=\"{0}\"", control.fe.HorizontalAlignment);
            if (control.fe.VerticalAlignment != VerticalAlignment.Stretch)
                sb.AppendFormat(" VerticalAlignment=\"{0}\"", control.fe.VerticalAlignment);
            string margin = control.fe.Margin.ToString2();
            if (margin != "0")
                sb.AppendFormat(" Margin=\"{0}\"", margin);
            if ((control.Flags & ECtrlFlags.ResizeWidth) > 0 && control.fe.HorizontalAlignment != HorizontalAlignment.Stretch)
                sb.AppendFormat(" Width=\"{0}\"", (int)control.fe.Width);
            if ((control.Flags & ECtrlFlags.ResizeHeight) > 0 && control.fe.VerticalAlignment != VerticalAlignment.Stretch)
                sb.AppendFormat(" Height=\"{0}\"", (int)control.fe.Height);
        }

        public string ControlToXaml(IHmiControl control)
        {
            if ((control.Flags & ECtrlFlags.IsGroup) > 0)
            {
                bool nested = false;
                var sb = new StringBuilder();
                sb.AppendLine("<Grid>");
                sb.AppendLine(control.ToXaml(1));
                foreach (object child in gridCanvas.Children)
                {
                    if ((child is IHmiControl control2) && IsInside(control2, control))
                    {
                        sb.AppendLine(control2.ToXaml(1));
                        nested = true;
                    }
                }
                sb.Append("</Grid>");
                return nested ? sb.ToString() : control.ToXaml(0);
            }
            else
            {
                return control.ToXaml(0);
            }
        }

        private void PasteFromXaml(string xaml)
        {
            // Create objects from xaml and add them to gridCanvas for Paste().
            if (!xaml.StartsWith("<"))
            {
                SystemSounds.Beep.Play();
                return;
            }

            xaml = InsertNamespaces(xaml);
            FrameworkElement fe;
            try
            {
                fe = XamlReader.Parse(xaml) as FrameworkElement;
            }
            catch
            {
                SystemSounds.Beep.Play();
                return;
            }

            if (fe is IHmiControl control)  // single control
            {
                SelectedControl = null;
                control.fe.SizeChanged += OnPasteSizeChanged;
                AddToCanvas(control, pixelOffset, pixelOffset);
            }
            else if (fe is Grid grid)  // collection of controls in a grid
            {
                int count = grid.Children.Count, added = 0;
                for (int i = 0; i < count; i++)
                {
                    if (grid.Children[0] is IHmiControl control2)
                    {
                        grid.Children.RemoveAt(0);  // remove before add!
                        ++added;
                        if (added == 1)
                        {
                            SelectedControl = null;
                            control2.fe.SizeChanged += OnPasteSizeChanged;
                        }
                        AddToCanvas(control2, pixelOffset, pixelOffset);
                    }
                    else
                    {
                        grid.Children.RemoveAt(0);
                    }
                }

                if (added == 0)
                    SystemSounds.Beep.Play();
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void OnPasteSizeChanged(object sender, SizeChangedEventArgs e)
        {
            IHmiControl control = (IHmiControl)sender;
            if (control.fe.ActualWidth > 0 && control.fe.ActualHeight > 0)
            {
                control.fe.SizeChanged -= OnPasteSizeChanged;  // one-shot
                SelectedControl = control;
                CopySelectedControl();  // copy the pixel offset
            }
        }
        #endregion XAML

        public void GenerateDotNetFrameworkProject(string projectName, string title, bool open)
        {
            if (title.Length == 0)
                title = projectName;
            title = title.Trim();
            projectName = projectName.Trim().Replace(" ", "");

            string visualStudioFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Visual Studio");
            string templateFolder = Path.Combine(visualStudioFolder, "Template");
            string projectFolder = Path.Combine(visualStudioFolder, projectName);
            string imagesFolder1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            string imagesFolder2 = Path.Combine(projectFolder, "Images");

            try
            {
                if (Directory.Exists(projectFolder))
                    Directory.Delete(projectFolder, true);
                Directory.CreateDirectory(projectFolder);
                Directory.CreateDirectory(imagesFolder2);

                // Generate AssemblyInfo.cs from the template:
                string path = Path.Combine(templateFolder, "AssemblyInfo.cs");
                string text = File.ReadAllText(path).Replace("__PROJECT_NAME__", projectName);
                path = Path.Combine(projectFolder, "Properties");
                Directory.CreateDirectory(path);
                path = Path.Combine(path, "AssemblyInfo.cs");
                File.WriteAllText(path, text);

                // Copy fixed source files from the template:
                Copy(@"Visual Studio\Template\App.config");
                Copy(@"Visual Studio\Template\Styles.xaml");

                // Copy project files from the template, specifying the project name:
                CopyProjectFile("App.xaml");
                CopyProjectFile("App.xaml.cs");
                CopyProjectFile("MainWindow.xaml.cs");

                // Generate xaml for all controls in Z-order:
                var sortedChildren = new SortedList<int, IHmiControl>(gridCanvas.Children.Count);
                foreach (object child in gridCanvas.Children)
                {
                    if (child is IHmiControl control)
                        sortedChildren[Panel.GetZIndex(control.fe)] = control;
                }
                var sb = new StringBuilder();
                foreach (IHmiControl child in sortedChildren.Values)
                    sb.AppendLine(child.ToXaml(2, true));

                // Generate MainWindow.xaml, specifying the project name and window title:
                path = Path.Combine(templateFolder, "MainWindow.xaml");
                text = File.ReadAllText(path).Replace("__PROJECT_NAME__", projectName).Replace("__WINDOW_TITLE__", title).Replace("__CONTROLS__", sb.ToString().Trim());
                path = Path.Combine(projectFolder, "MainWindow.xaml");
                File.WriteAllText(path, text);

                // Copy Images and add as project resources:
                var sbImageResources = new StringBuilder();
                foreach (var file in Directory.GetFiles(imagesFolder1))
                {
                    string name = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(imagesFolder2, name));
                    sbImageResources.AppendFormat("    <Resource Include=\"Images\\{0}\" />\r\n", name);
                }

                // Generate VS project file from the template:
                string csprojPath = string.Format(@"{0}\{1}.csproj", projectFolder, projectName);
                path = Path.Combine(templateFolder, "Template.csproj");
                text = File.ReadAllText(path).Replace("__PROJECT_NAME__", projectName);
                text = text.Replace("__PROJECT_GUID__", Guid.NewGuid().ToString().ToUpper());
                text = text.Replace("__ITEMGROUP_RESOURCES__", sbImageResources.ToString().Trim());
                File.WriteAllText(csprojPath, text);

                // Open the new project:
                if (open)
                    System.Diagnostics.Process.Start(csprojPath);  // open project in Visual Studio
                else
                    System.Diagnostics.Process.Start(projectFolder); // open project directory in File Explorer
            }
            catch (Exception exc)
            {
                string message = (exc.InnerException != null) ? exc.InnerException.Message : exc.Message;
                MessageBox.Show("Project generation failed: " + message);
            }

            void Copy(string file, string folder = "")
            {
                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
                if (File.Exists(file))
                {
                    folder = Path.Combine(projectFolder, folder);
                    if (!folder.Equals(projectFolder, StringComparison.InvariantCultureIgnoreCase))
                        Directory.CreateDirectory(folder);
                    File.Copy(file, Path.Combine(folder, Path.GetFileName(file)));
                }
            }

            void CopyProjectFile(string filename)
            {
                // Copy filename from the template, specifying the project name:
                string path = Path.Combine(templateFolder, filename);
                string text = File.ReadAllText(path).Replace("__PROJECT_NAME__", projectName);
                path = Path.Combine(projectFolder, filename);
                File.WriteAllText(path, text);
            }
        }

        public void GenerateDotNetCoreProject(string projectName, string title, bool open)
        {
            if (title.Length == 0)
                title = projectName;
            title = title.Trim();
            projectName = projectName.Trim().Replace(" ", "");

            string visualStudioFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Visual Studio");
            string templateFolder = Path.Combine(visualStudioFolder, "TemplateCore");
            string projectFolder = Path.Combine(visualStudioFolder, projectName);
            string imagesFolder1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            string imagesFolder2 = Path.Combine(projectFolder, "Images");

            try
            {
                if (Directory.Exists(projectFolder))
                    Directory.Delete(projectFolder, true);
                Directory.CreateDirectory(projectFolder);
                Directory.CreateDirectory(imagesFolder2);

                // Copy fixed source files from the template:
                Copy(@"Visual Studio\TemplateCore\AssemblyInfo.cs");
                Copy(@"Visual Studio\Template\Styles.xaml");

                // Copy project files from the template, specifying the project name:
                CopyProjectFile("App.xaml");
                CopyProjectFile("App.xaml.cs");
                CopyProjectFile("MainWindow.xaml.cs");

                // Generate xaml for all controls in Z-order:
                var sortedChildren = new SortedList<int, IHmiControl>(gridCanvas.Children.Count);
                foreach (object child in gridCanvas.Children)
                {
                    if (child is IHmiControl control)
                        sortedChildren[Panel.GetZIndex(control.fe)] = control;
                }
                var sb = new StringBuilder();
                foreach (IHmiControl child in sortedChildren.Values)
                    sb.AppendLine(child.ToXaml(2, true));

                // Generate MainWindow.xaml, specifying the project name and window title:
                string path = Path.Combine(templateFolder, "MainWindow.xaml");
                string text = File.ReadAllText(path).Replace("__PROJECT_NAME__", projectName).Replace("__WINDOW_TITLE__", title).Replace("__CONTROLS__", sb.ToString().Trim());
                path = Path.Combine(projectFolder, "MainWindow.xaml");
                File.WriteAllText(path, text);

                // Copy Images and add as project resources:
                var sbItemGroupResources = new StringBuilder();
                var sbItemGroupNone = new StringBuilder();
                foreach (var file in Directory.GetFiles(imagesFolder1))
                {
                    string name = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(imagesFolder2, name));
                    sbItemGroupResources.AppendFormat("    <Resource Include=\"Images\\{0}\" />\r\n", name);
                    sbItemGroupNone.AppendFormat("    <None Remove=\"Images\\{0}\" />\r\n", name);
                }

                // Generate VS project file from the template:
                string csprojPath = string.Format(@"{0}\{1}.csproj", projectFolder, projectName);
                path = Path.Combine(templateFolder, "Template.csproj");
                text = File.ReadAllText(path);
                text = text.Replace("__ITEMGROUP_RESOURCES__", sbItemGroupResources.ToString().Trim());
                text = text.Replace("__ITEMGROUP_NONE__", sbItemGroupNone.ToString().Trim());
                File.WriteAllText(csprojPath, text);

                path = Path.Combine(templateFolder, "Template.csproj.user");
                File.Copy(path, csprojPath + ".user");

                // Open the new project:
                if (open)
                    System.Diagnostics.Process.Start(csprojPath);  // open project in Visual Studio
                else
                    System.Diagnostics.Process.Start(projectFolder); // open project directory in File Explorer
            }
            catch (Exception exc)
            {
                string message = (exc.InnerException != null) ? exc.InnerException.Message : exc.Message;
                MessageBox.Show("Project generation failed: " + message);
            }

            void Copy(string file, string folder = "")
            {
                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
                if (File.Exists(file))
                {
                    folder = Path.Combine(projectFolder, folder);
                    if (!folder.Equals(projectFolder, StringComparison.InvariantCultureIgnoreCase))
                        Directory.CreateDirectory(folder);
                    File.Copy(file, Path.Combine(folder, Path.GetFileName(file)));
                }
            }

            void CopyProjectFile(string filename)
            {
                // Copy filename from the template, specifying the project name:
                string path = Path.Combine(templateFolder, filename);
                string text = File.ReadAllText(path).Replace("__PROJECT_NAME__", projectName);
                path = Path.Combine(projectFolder, filename);
                File.WriteAllText(path, text);
            }
        }
    }
}

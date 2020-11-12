using System;
using System.Windows;

namespace BuilderHMI.Lite.Core
{
    public class Location0
    {
        // This class is responsible for moving or sizing a control relative to initial control and mouse cursor locations.

        public IHmiControl control { get; private set; }
        public Point mouse { get; private set; }  // optional initial mouse location

        private double width1, width2, height1, height2;  // initial alignment-dependent control location

        public Location0(IHmiControl control = null)
        {
            if (control != null)
                Initialize(control);
        }

        public void Initialize(IHmiControl control)
        {
            this.control = control;

            switch (control.fe.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Center:
                    width1 = control.fe.Margin.Left;
                    width2 = control.fe.Width;
                    break;
                case HorizontalAlignment.Right:
                    width1 = control.fe.Width;
                    width2 = control.fe.Margin.Right;
                    break;
                case HorizontalAlignment.Stretch:
                    width1 = control.fe.Margin.Left;
                    width2 = control.fe.Margin.Right;
                    break;
            }

            switch (control.fe.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                case VerticalAlignment.Center:
                    height1 = control.fe.Margin.Top;
                    height2 = control.fe.Height;
                    break;
                case VerticalAlignment.Bottom:
                    height1 = control.fe.Height;
                    height2 = control.fe.Margin.Bottom;
                    break;
                case VerticalAlignment.Stretch:
                    height1 = control.fe.Margin.Top;
                    height2 = control.fe.Margin.Bottom;
                    break;
            }
        }

        public void Initialize(IHmiControl control, Point mouse)
        {
            Initialize(control);
            this.mouse = mouse;
        }

        public void Clear()
        {
            control = null;
        }

        public void MoveSimple(double dx, double dy)
        {
            // For controls within a border or group box that move with parent.
            double left = 0, top = 0, right = 0, bottom = 0;
            switch (control.fe.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    left = width1 + dx;
                    break;
                case HorizontalAlignment.Center:
                    left = width1 + 2 * dx;
                    break;
                case HorizontalAlignment.Right:
                    right = width2 - dx;
                    break;
                case HorizontalAlignment.Stretch:
                    left = width1 + dx;
                    right = width2 - dx;
                    break;
            }

            switch (control.fe.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    top = height1 + dy;
                    break;
                case VerticalAlignment.Center:
                    top = height1 + 2 * dy;
                    break;
                case VerticalAlignment.Bottom:
                    bottom = height2 - dy;
                    break;
                case VerticalAlignment.Stretch:
                    top = height1 + dy;
                    bottom = height2 - dy;
                    break;
            }
            control.fe.Margin = new Thickness(left, top, right, bottom);
        }

        public void Move(Size sizeCanvas, ref double dx, ref double dy)
        {
            if (dx == 0 && dy == 0) return;

            double left = 0, top = 0, right = 0, bottom = 0;
            switch (control.fe.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    left = Math.Max(width1 + dx, 0);
                    left = Math.Round(left / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    left = Math.Min(left, (int)(sizeCanvas.Width - control.fe.ActualWidth + 0.5));
                    dx = left - width1;
                    break;

                case HorizontalAlignment.Center:
                    left = Math.Max(width1 + 2 * dx, control.fe.ActualWidth - sizeCanvas.Width);
                    left = Math.Round(left / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    left = Math.Min(left, (int)(sizeCanvas.Width - control.fe.ActualWidth + 0.5));
                    dx = (left - width1) / 2;
                    break;

                case HorizontalAlignment.Right:
                    right = Math.Max(width2 - dx, 0);
                    right = Math.Round(right / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    right = Math.Min(right, (int)(sizeCanvas.Width - control.fe.ActualWidth + 0.5));
                    dx = width2 - right;
                    break;

                case HorizontalAlignment.Stretch:
                    left = Math.Max(width1 + dx, 0);
                    left = Math.Round(left / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    right = control.fe.Margin.Right + control.fe.Margin.Left - left;
                    if (right < 0)
                    {
                        left += right;
                        right = 0;
                    }
                    dx = left - width1;
                    break;
            }

            switch (control.fe.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    top = Math.Max(height1 + dy, 0);
                    top = Math.Round(top / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    top = Math.Min(top, (int)(sizeCanvas.Height - control.fe.ActualHeight + 0.5));
                    dy = top - height1;
                    break;

                case VerticalAlignment.Center:
                    top = Math.Max(height1 + 2 * dy, control.fe.ActualHeight - sizeCanvas.Height);
                    top = Math.Round(top / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    top = Math.Min(top, (int)(sizeCanvas.Height - control.fe.ActualHeight + 0.5));
                    dy = (top - height1) / 2;
                    break;

                case VerticalAlignment.Bottom:
                    bottom = Math.Max(height2 - dy, 0);
                    bottom = Math.Round(bottom / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    bottom = Math.Min(bottom, (int)(sizeCanvas.Height - control.fe.ActualHeight + 0.5));
                    dy = height2 - bottom;
                    break;

                case VerticalAlignment.Stretch:
                    top = Math.Max(height1 + dy, 0);
                    top = Math.Round(top / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    bottom = control.fe.Margin.Bottom + control.fe.Margin.Top - top;
                    if (bottom < 0)
                    {
                        top += bottom;
                        bottom = 0;
                    }
                    dy = top - height1;
                    break;
            }
            control.fe.Margin = new Thickness(left, top, right, bottom);
        }

        public void Size(Size sizeCanvas, ref double dx, ref double dy)
        {
            if ((control.Flags & ECtrlFlags.ResizeWidth) == 0) dx = 0;
            if ((control.Flags & ECtrlFlags.ResizeHeight) == 0) dy = 0;
            if (dx == 0 && dy == 0) return;

            double left = 0, top = 0, right = 0, bottom = 0, width, height;
            switch (control.fe.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    left = width1;
                    width = width2 + dx;
                    width = Math.Round(width / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    width = Math.Max(width, control.fe.MinWidth);
                    width = Math.Min(width, (int)(sizeCanvas.Width - left + 0.5));
                    control.fe.Width = width;
                    dx = width - width2;
                    break;

                case HorizontalAlignment.Center:
                    left = width1;
                    width = width2 + dx;
                    width = Math.Round(width / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    width = Math.Max(width, control.fe.MinWidth);
                    control.fe.Width = width;
                    dx = width - width2;
                    left = Math.Min(left + dx, (int)(sizeCanvas.Width - control.fe.ActualWidth + 0.5));
                    break;

                case HorizontalAlignment.Right:
                    right = Math.Max(width2 - dx, 0);
                    right = Math.Round(right / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    control.fe.Width = Math.Max(width1 + width2 - right, control.fe.MinWidth);
                    dx = width2 - right;
                    break;

                case HorizontalAlignment.Stretch:
                    left = width1;
                    right = Math.Max(width2 - dx, 0);
                    right = Math.Round(right / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    right = Math.Min(right, sizeCanvas.Width - left - control.fe.MinWidth);
                    dx = width2 - right;
                    break;
            }

            switch (control.fe.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    top = height1;
                    height = height2 + dy;
                    height = Math.Round(height / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    height = Math.Max(height, control.fe.MinHeight);
                    height = Math.Min(height, (int)(sizeCanvas.Height - top + 0.5));
                    control.fe.Height = height;
                    dy = height - height2;
                    break;

                case VerticalAlignment.Center:
                    top = height1;
                    height = height2 + dy;
                    height = Math.Round(height / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    height = Math.Max(height, control.fe.MinHeight);
                    control.fe.Height = height;
                    dy = height - height2;
                    top = Math.Min(top + dy, (int)(sizeCanvas.Height - control.fe.ActualHeight + 0.5));
                    break;

                case VerticalAlignment.Bottom:
                    bottom = Math.Max(height2 - dy, 0);
                    bottom = Math.Round(bottom / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    control.fe.Height = Math.Max(height1 + height2 - bottom, control.fe.MinHeight);
                    dy = height2 - bottom;
                    break;

                case VerticalAlignment.Stretch:
                    top = height1;
                    bottom = Math.Max(height2 - dy, 0);
                    bottom = Math.Round(bottom / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    bottom = Math.Min(bottom, sizeCanvas.Height - top - control.fe.MinHeight);
                    dy = height2 - bottom;
                    break;
            }
            control.fe.Margin = new Thickness(left, top, right, bottom);
        }

        public static void Shift(FrameworkElement fe, double dx, double dy)
        {
            // For arrow key nudge and the offset when copying then pasting.
            if (dx == 0 && dy == 0) return;

            double left = 0, top = 0, right = 0, bottom = 0;
            switch (fe.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    left = fe.Margin.Left + dx;
                    break;
                case HorizontalAlignment.Center:
                    left = fe.Margin.Left + 2 * dx;
                    break;
                case HorizontalAlignment.Right:
                    right = fe.Margin.Right - dx;
                    break;
                case HorizontalAlignment.Stretch:
                    left = fe.Margin.Left + dx;
                    right = fe.Margin.Right - dx;
                    break;
            }

            switch (fe.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    top = fe.Margin.Top + dy;
                    break;
                case VerticalAlignment.Center:
                    top = fe.Margin.Top + 2 * dy;
                    break;
                case VerticalAlignment.Bottom:
                    bottom = fe.Margin.Bottom - dy;
                    break;
                case VerticalAlignment.Stretch:
                    top = fe.Margin.Top + dy;
                    bottom = fe.Margin.Bottom - dy;
                    break;
            }
            fe.Margin = new Thickness(left, top, right, bottom);
        }
    }
}
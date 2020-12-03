using System;
using System.Windows;

namespace BuilderHMI.Lite
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

        public bool Move(Size sizeCanvas, ref double dx, ref double dy)
        {
            if (dx == 0 && dy == 0) return false;

            var alignH = control.fe.HorizontalAlignment;
            var alignV = control.fe.VerticalAlignment;
            double left = 0, top = 0, right = 0, bottom = 0, limit;
            switch (control.fe.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    left = Math.Max(width1 + dx, 0);
                    left = Math.Round(left / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    limit = Math.Round(sizeCanvas.Width - control.fe.ActualWidth);
                    if (left > limit) alignH = HorizontalAlignment.Right;  // Left -> Right gesture
                    left = Math.Min(left, limit);
                    dx = left - width1;
                    break;

                case HorizontalAlignment.Center:
                    left = width1 + 2 * dx;
                    limit = control.fe.ActualWidth - sizeCanvas.Width;
                    if (left < limit) alignH = HorizontalAlignment.Left;  // Center -> Left gesture
                    left = Math.Max(left, limit);
                    left = Math.Round(left / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    limit = Math.Round(sizeCanvas.Width - control.fe.ActualWidth);
                    if (left > limit) alignH = HorizontalAlignment.Right;  // Center -> Right gesture
                    left = Math.Min(left, limit);
                    dx = (left - width1) / 2;
                    break;

                case HorizontalAlignment.Right:
                    right = Math.Max(width2 - dx, 0);
                    right = Math.Round(right / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    limit = Math.Round(sizeCanvas.Width - control.fe.ActualWidth);
                    if (right > limit) alignH = HorizontalAlignment.Left;  // Right -> Left gesture
                    right = Math.Min(right, limit);
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
                    limit = Math.Round(sizeCanvas.Height - control.fe.ActualHeight);
                    if (top > limit) alignV = VerticalAlignment.Bottom;  // Top -> Bottom gesture
                    top = Math.Min(top, limit);
                    dy = top - height1;
                    break;

                case VerticalAlignment.Center:
                    top = height1 + 2 * dy;
                    limit = control.fe.ActualHeight - sizeCanvas.Height;
                    if (top < limit) alignV = VerticalAlignment.Top;  // Center -> Top gesture
                    top = Math.Max(top, limit);
                    top = Math.Round(top / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    limit = Math.Round(sizeCanvas.Height - control.fe.ActualHeight);
                    if (top > limit) alignV = VerticalAlignment.Bottom;  // Center -> Bottom gesture
                    top = Math.Min(top, limit);
                    dy = (top - height1) / 2;
                    break;

                case VerticalAlignment.Bottom:
                    bottom = Math.Max(height2 - dy, 0);
                    bottom = Math.Round(bottom / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    limit = Math.Round(sizeCanvas.Height - control.fe.ActualHeight);
                    if (bottom > limit) alignV = VerticalAlignment.Top;  // Bottom -> Top gesture
                    bottom = Math.Min(bottom, limit);
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
            if (alignH != control.fe.HorizontalAlignment || alignV != control.fe.VerticalAlignment)
            {
                MainWindow.SetControlAlignment(control, sizeCanvas, alignH, alignV);
                return true;
            }
            return false;
        }

        public bool Size(Size sizeCanvas, ref double dx, ref double dy)
        {
            if ((control.Flags & ECtrlFlags.ResizeWidth) == 0) dx = 0;
            if ((control.Flags & ECtrlFlags.ResizeHeight) == 0) dy = 0;
            if (dx == 0 && dy == 0) return false;

            var alignH = control.fe.HorizontalAlignment;
            var alignV = control.fe.VerticalAlignment;
            double left = 0, top = 0, right = 0, bottom = 0, width, height, limit;
            switch (control.fe.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    left = width1;
                    width = width2 + dx;
                    width = Math.Round(width / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    width = Math.Max(width, control.fe.MinWidth);
                    limit = Math.Round(sizeCanvas.Width - left);
                    if (width > limit) alignH = HorizontalAlignment.Stretch;  // Left -> Stretch gesture
                    width = Math.Min(width, limit);
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
                    left = Math.Min(left + dx, Math.Round(sizeCanvas.Width - control.fe.ActualWidth));
                    break;

                case HorizontalAlignment.Right:
                    right = Math.Max(width2 - dx, 0);
                    right = Math.Round(right / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    right = Math.Min(right, width1 + width2 - control.fe.MinWidth);
                    control.fe.Width = width1 + width2 - right;
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
                    limit = Math.Round(sizeCanvas.Height - top);
                    if (height > limit) alignV = VerticalAlignment.Stretch;  // Top -> Stretch gesture
                    height = Math.Min(height, limit);
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
                    top = Math.Min(top + dy, Math.Round(sizeCanvas.Height - control.fe.ActualHeight));
                    break;

                case VerticalAlignment.Bottom:
                    bottom = Math.Max(height2 - dy, 0);
                    bottom = Math.Round(bottom / MainWindow.GridSize) * MainWindow.GridSize;  // snap to grid
                    bottom = Math.Min(bottom, height1 + height2 - control.fe.MinHeight);
                    control.fe.Height = height1 + height2 - bottom;
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
            if (alignH != control.fe.HorizontalAlignment || alignV != control.fe.VerticalAlignment)
            {
                MainWindow.SetControlAlignment(control, sizeCanvas, alignH, alignV);
                return true;
            }
            return false;
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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BuilderHMI.Lite
{
    public class Marker : Grid
    {
        // This class provides the box surrounding the selected control as well as the alignment marks while moving.

        public Marker(Grid gridCanvas)
        {
            this.gridCanvas = gridCanvas;
            IsHitTestVisible = false;
            Visibility = Visibility.Hidden;

            border = new Rectangle();
            border.StrokeThickness = 1;
            border.SetResourceReference(Shape.StrokeProperty, "SelectedBrush");
            border.HorizontalAlignment = HorizontalAlignment.Stretch;
            border.VerticalAlignment = VerticalAlignment.Stretch;
            border.SnapsToDevicePixels = true;
            Children.Add(border);

            rect1 = new Rectangle();
            rect1.Width = rect1.Height = 5;
            rect1.SetResourceReference(Shape.FillProperty, "SelectedBrush");
            rect1.HorizontalAlignment = HorizontalAlignment.Left;
            rect1.VerticalAlignment = VerticalAlignment.Top;
            rect1.SnapsToDevicePixels = true;
            Children.Add(rect1);

            rect2 = new Rectangle();
            rect2.Width = rect2.Height = 5;
            rect2.SetResourceReference(Shape.FillProperty, "SelectedBrush");
            rect2.HorizontalAlignment = HorizontalAlignment.Right;
            rect2.VerticalAlignment = VerticalAlignment.Bottom;
            rect2.SnapsToDevicePixels = true;
            Children.Add(rect2);

            polyLeft = new Polygon();
            polyLeft.Points = new PointCollection(new Point[] { new Point(0, 0), new Point(6, 3), new Point(6, -3) });
            polyLeft.StrokeThickness = 0;
            polyLeft.SetResourceReference(Shape.FillProperty, "SelectedBrush");
            polyLeft.HorizontalAlignment = HorizontalAlignment.Left;
            polyLeft.VerticalAlignment = VerticalAlignment.Center;
            polyLeft.SnapsToDevicePixels = true;
            Children.Add(polyLeft);

            polyTop = new Polygon();
            polyTop.Points = new PointCollection(new Point[] { new Point(0, 0), new Point(3, 6), new Point(-3, 6) });
            polyTop.StrokeThickness = 0;
            polyTop.SetResourceReference(Shape.FillProperty, "SelectedBrush");
            polyTop.HorizontalAlignment = HorizontalAlignment.Center;
            polyTop.VerticalAlignment = VerticalAlignment.Top;
            polyTop.SnapsToDevicePixels = true;
            Children.Add(polyTop);

            polyRight = new Polygon();
            polyRight.Points = new PointCollection(new Point[] { new Point(6, 0), new Point(0, 3), new Point(0, -3) });
            polyRight.StrokeThickness = 0;
            polyRight.SetResourceReference(Shape.FillProperty, "SelectedBrush");
            polyRight.HorizontalAlignment = HorizontalAlignment.Right;
            polyRight.VerticalAlignment = VerticalAlignment.Center;
            polyRight.SnapsToDevicePixels = true;
            Children.Add(polyRight);

            polyBottom = new Polygon();
            polyBottom.Points = new PointCollection(new Point[] { new Point(0, 6), new Point(3, 0), new Point(-3, 0) });
            polyBottom.StrokeThickness = 0;
            polyBottom.SetResourceReference(Shape.FillProperty, "SelectedBrush");
            polyBottom.HorizontalAlignment = HorizontalAlignment.Center;
            polyBottom.VerticalAlignment = VerticalAlignment.Bottom;
            polyBottom.SnapsToDevicePixels = true;
            Children.Add(polyBottom);

            vline = new Line();
            vline.Opacity = 0.4;
            vline.X1 = vline.Y1 = vline.X2 = vline.Y2 = 0;
            vline.SetResourceReference(Shape.StrokeProperty, "SelectedBrush");
            Panel.SetZIndex(vline, MARKER_ZINDEX);

            vline2 = new Line();
            vline2.Opacity = 0.2;
            vline2.X1 = vline2.Y1 = vline2.X2 = vline2.Y2 = 0;
            vline2.SetResourceReference(Shape.StrokeProperty, "SelectedBrush");
            Panel.SetZIndex(vline2, MARKER_ZINDEX);

            hline = new Line();
            hline.Opacity = 0.4;
            hline.X1 = hline.Y1 = hline.X2 = hline.Y2 = 0;
            hline.SetResourceReference(Shape.StrokeProperty, "SelectedBrush");
            Panel.SetZIndex(hline, MARKER_ZINDEX);

            hline2 = new Line();
            hline2.Opacity = 0.2;
            hline2.X1 = hline2.Y1 = hline2.X2 = hline2.Y2 = 0;
            hline2.SetResourceReference(Shape.StrokeProperty, "SelectedBrush");
            Panel.SetZIndex(hline2, MARKER_ZINDEX);
        }

        private const int MARKER_ZINDEX = 0x20000;
        private Grid gridCanvas;
        private IHmiControl control = null;
        private Rectangle border, rect1, rect2;
        private Polygon polyLeft, polyTop, polyRight, polyBottom;
        private Line vline, vline2, hline, hline2;
        private SortedSet<double> vedges = new SortedSet<double>();
        private SortedSet<double> hedges = new SortedSet<double>();

        public enum EDragging { None, Move, Size }
        private EDragging dragging = EDragging.None;

        public EDragging Dragging
        {
            get { return dragging; }
            set
            {
                if (dragging != value)
                {
                    if (value == EDragging.None)
                    {
                        gridCanvas.Children.Remove(vline);
                        gridCanvas.Children.Remove(vline2);
                        gridCanvas.Children.Remove(hline);
                        gridCanvas.Children.Remove(hline2);
                    }
                    else
                    {
                        vedges.Clear();
                        hedges.Clear();
                        if (value == EDragging.Move)
                        {
                            if (control.fe.HorizontalAlignment == HorizontalAlignment.Left ||
                                control.fe.HorizontalAlignment == HorizontalAlignment.Stretch)
                            {
                                foreach (object child in gridCanvas.Children)
                                {
                                    if ((child is IHmiControl control2) && control2 != control && !MainWindow.InMoveList(control2))
                                    {
                                        if (control2.fe.HorizontalAlignment == HorizontalAlignment.Left ||
                                            control2.fe.HorizontalAlignment == HorizontalAlignment.Stretch)
                                            vedges.Add(control2.fe.Margin.Left);
                                    }
                                }
                            }

                            if (control.fe.VerticalAlignment == VerticalAlignment.Top ||
                                control.fe.VerticalAlignment == VerticalAlignment.Stretch)
                            {
                                foreach (object child in gridCanvas.Children)
                                {
                                    if ((child is IHmiControl control2) && control2 != control && !MainWindow.InMoveList(control2))
                                    {
                                        if (control2.fe.VerticalAlignment == VerticalAlignment.Top ||
                                            control2.fe.VerticalAlignment == VerticalAlignment.Stretch)
                                            hedges.Add(control2.fe.Margin.Top);
                                    }
                                }
                            }

                        }

                        vline.Y2 = vline2.Y2 = gridCanvas.ActualHeight;
                        hline.X2 = hline2.X2 = gridCanvas.ActualWidth;
                        vline2.Visibility = Visibility.Hidden;
                        hline2.Visibility = Visibility.Hidden;
                        gridCanvas.Children.Add(vline);
                        gridCanvas.Children.Add(vline2);
                        gridCanvas.Children.Add(hline);
                        gridCanvas.Children.Add(hline2);
                    }

                    dragging = value;
                }
            }
        }

        public IHmiControl Control
        {
            get { return control; }
            set
            {
                if (control != null)
                {
                    control.fe.SizeChanged -= OnControlSizeChanged;
                    gridCanvas.Children.Remove(this);
                }

                control = value;
                if (control != null)
                {
                    gridCanvas.Children.Add(this);
                    Panel.SetZIndex(this, GetZIndex(control.fe));
                    HorizontalAlignment = control.fe.HorizontalAlignment;
                    VerticalAlignment = control.fe.VerticalAlignment;
                    Margin = control.fe.Margin;
                    Width = control.fe.ActualWidth;
                    Height = control.fe.ActualHeight;
                    rect1.Visibility = Visibility.Visible;
                    rect2.Visibility = ((control.Flags & ECtrlFlags.Resize) > 0) ? Visibility.Visible : Visibility.Hidden;
                    polyLeft.Visibility = (HorizontalAlignment == HorizontalAlignment.Left || HorizontalAlignment == HorizontalAlignment.Stretch) ? Visibility.Visible : Visibility.Hidden;
                    polyTop.Visibility = (VerticalAlignment == VerticalAlignment.Top || VerticalAlignment == VerticalAlignment.Stretch) ? Visibility.Visible : Visibility.Hidden;
                    polyRight.Visibility = (HorizontalAlignment == HorizontalAlignment.Right || HorizontalAlignment == HorizontalAlignment.Stretch) ? Visibility.Visible : Visibility.Hidden;
                    polyBottom.Visibility = (VerticalAlignment == VerticalAlignment.Bottom || VerticalAlignment == VerticalAlignment.Stretch) ? Visibility.Visible : Visibility.Hidden;
                    control.fe.SizeChanged += OnControlSizeChanged;
                    Visibility = Visibility.Visible;
                }
                else
                {
                    Visibility = Visibility.Hidden;
                }
            }
        }

        public void SetAlignment()
        {
            HorizontalAlignment = control.fe.HorizontalAlignment;
            VerticalAlignment = control.fe.VerticalAlignment;
            Margin = control.fe.Margin;
            polyLeft.Visibility = (HorizontalAlignment == HorizontalAlignment.Left || HorizontalAlignment == HorizontalAlignment.Stretch) ? Visibility.Visible : Visibility.Hidden;
            polyTop.Visibility = (VerticalAlignment == VerticalAlignment.Top || VerticalAlignment == VerticalAlignment.Stretch) ? Visibility.Visible : Visibility.Hidden;
            polyRight.Visibility = (HorizontalAlignment == HorizontalAlignment.Right || HorizontalAlignment == HorizontalAlignment.Stretch) ? Visibility.Visible : Visibility.Hidden;
            polyBottom.Visibility = (VerticalAlignment == VerticalAlignment.Bottom || VerticalAlignment == VerticalAlignment.Stretch) ? Visibility.Visible : Visibility.Hidden;
        }

        public void SetAlignmentMarks()
        {
            if (Dragging == EDragging.Move)
            {
                if (HorizontalAlignment == HorizontalAlignment.Right)
                {
                    vline.X1 = vline.X2 = gridCanvas.ActualWidth - Width - Margin.Right;
                }
                else if (HorizontalAlignment == HorizontalAlignment.Center)
                {
                    double x2 = (gridCanvas.ActualWidth - Width - Margin.Left) / 2;
                    vline.X1 = vline.X2 = Margin.Left + x2;
                }
                else  // left or stretch
                {
                    vline.X1 = vline.X2 = Margin.Left;
                }

                vline2.Visibility = Visibility.Hidden;
                double dx, dxAbs, dxAbsMin = 40;
                foreach (double vedge in vedges)
                {
                    dx = vedge - vline.X1;
                    dxAbs = Math.Abs(dx);
                    if (dxAbsMin > dxAbs)
                    {
                        dxAbsMin = dxAbs;
                        vline2.X1 = vline2.X2 = vedge;
                        vline2.Visibility = Visibility.Visible;
                    }
                    if (dx >= 40) break;
                }

                if (VerticalAlignment == VerticalAlignment.Bottom)
                {
                    hline.Y1 = hline.Y2 = gridCanvas.ActualHeight - Height - Margin.Bottom;
                }
                else if (VerticalAlignment == VerticalAlignment.Center)
                {
                    double y2 = (gridCanvas.ActualHeight - Height - Margin.Top) / 2;
                    hline.Y1 = hline.Y2 = Margin.Top + y2;
                }
                else  // top or stretch
                {
                    hline.Y1 = hline.Y2 = Margin.Top;
                }

                hline2.Visibility = Visibility.Hidden;
                double dy, dyAbs, dyAbsMin = 40;
                foreach (double hedge in hedges)
                {
                    dy = hedge - hline.Y1;
                    dyAbs = Math.Abs(dy);
                    if (dyAbsMin > dyAbs)
                    {
                        dyAbsMin = dyAbs;
                        hline2.Y1 = hline2.Y2 = hedge;
                        hline2.Visibility = Visibility.Visible;
                    }
                    if (dy >= 40) break;
                }
            }
        }

        private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = control.fe.ActualWidth;
            Height = control.fe.ActualHeight;

            if (Dragging == EDragging.Size)
            {
                if (HorizontalAlignment == HorizontalAlignment.Right)
                {
                    vline.X1 = vline.X2 = gridCanvas.ActualWidth - Margin.Right;
                }
                else if (HorizontalAlignment == HorizontalAlignment.Center)
                {
                    double x2 = (gridCanvas.ActualWidth - Width - Margin.Left) / 2;
                    vline.X1 = vline.X2 = Margin.Left + Width + x2;
                }
                else  // left or stretch
                {
                    vline.X1 = vline.X2 = Margin.Left + Width;
                }

                if (VerticalAlignment == VerticalAlignment.Bottom)
                {
                    hline.Y1 = hline.Y2 = gridCanvas.ActualHeight - Margin.Bottom;
                }
                else if (VerticalAlignment == VerticalAlignment.Center)
                {
                    double y2 = (gridCanvas.ActualHeight - Height - Margin.Top) / 2;
                    hline.Y1 = hline.Y2 = Margin.Top + Height + y2;
                }
                else  // top or stretch
                {
                    hline.Y1 = hline.Y2 = Margin.Top + Height;
                }
            }
        }
    }
}

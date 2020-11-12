using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace BuilderHMI.Lite.Core
{
    /// <summary>
    /// Interaction logic for Designer.xaml
    /// </summary>
    public partial class Designer : Window
    {
        private MainWindow mainWindow;

        public Designer(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            mainWindow.OnKeyDown(sender, e);
        }

        private enum ETabs { Add, Edit, Export, Help }

        private ETabs tab = ETabs.Add;

        private ETabs Tab
        {
            set
            {
                if (tab != value)
                {
                    gridAdd.Visibility = Visibility.Hidden;
                    gridEdit.Visibility = Visibility.Hidden;
                    spExport.Visibility = Visibility.Hidden;
                    spHelp.Visibility = Visibility.Hidden;

                    tab = value;
                    MainTabs.SelectedIndex = (int)tab;
                    switch (tab)
                    {
                        case ETabs.Add: gridAdd.Visibility = Visibility.Visible; break;
                        case ETabs.Edit: gridEdit.Visibility = Visibility.Visible; break;
                        case ETabs.Export: spExport.Visibility = Visibility.Visible; break;
                        case ETabs.Help: spHelp.Visibility = Visibility.Visible; break;
                    }
                }
            }
        }

        private void MainTabs_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            while (obj != null)
            {
                obj = VisualTreeHelper.GetParent(obj);
                if (obj is TabItem ti)
                {
                    Tab = (ETabs)MainTabs.Items.IndexOf(ti);
                    break;
                }
            }
        }

        private void ButtonBase_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase button = e.OriginalSource as ButtonBase;
            if (button == null || e.Handled) return;

            switch (button.Name)
            {
                case "btnGroupBox": mainWindow.AddNew<HmiGroupBox>(); break;
                case "btnBorder": mainWindow.AddNew<HmiBorder>(); break;
                case "btnImage": mainWindow.AddNew<HmiImage>(); break;
                case "btnTextBlock": mainWindow.AddNew<HmiTextBlock>(); break;
                case "btnButton": mainWindow.AddNew<HmiButton>(); break;
                case "btnTextBox": mainWindow.AddNew<HmiTextBox>(); break;

                case "btnCut": mainWindow.CutSelectedControl(); break;
                case "btnCopy": mainWindow.CopySelectedControl(); break;
                case "btnPaste": mainWindow.Paste(); break;
                case "btnDelete": mainWindow.DeleteSelectedControl(); break;
                case "btnToFront": mainWindow.SelectedControlToFront(); break;
                case "btnToBack": mainWindow.SelectedControlToBack(); break;

                case "btnGenerateDotNetFrameworkProject":
                    mainWindow.GenerateDotNetFrameworkProject(tbProjectName.Text, tbTitle.Text, cbOpenInVS.IsChecked == true);
                    break;

                case "btnGenerateDotNetCoreProject":
                    mainWindow.GenerateDotNetCoreProject(tbProjectName.Text, tbTitle.Text, cbOpenInVS.IsChecked == true);
                    break;
            }
        }

        private void UpdatePropertyPage()
        {
            if (mainWindow.SelectedControl != null)
            {
                updatePageProperties = false;
                cbHorizontalAlignment.IsEnabled = cbVerticalAlignment.IsEnabled = true;
                // Disable Stretch alignment option for fixed size controls:
                (cbHorizontalAlignment.Items[3] as ComboBoxItem).IsEnabled = ((mainWindow.SelectedControl.Flags & ECtrlFlags.ResizeWidth) > 0);
                (cbVerticalAlignment.Items[3] as ComboBoxItem).IsEnabled = ((mainWindow.SelectedControl.Flags & ECtrlFlags.ResizeHeight) > 0);
                updatePageProperties = true;
                UpdateLocation();
                btnCut.IsEnabled = btnCopy.IsEnabled = btnDelete.IsEnabled = btnToFront.IsEnabled = btnToBack.IsEnabled = true;
            }
            else
            {
                cbHorizontalAlignment.IsEnabled = cbVerticalAlignment.IsEnabled =  false;
                cbHorizontalAlignment.SelectedIndex = cbVerticalAlignment.SelectedIndex = -1;
                tbLocation.Text = " (no control selected)";
                btnCut.IsEnabled = btnCopy.IsEnabled = btnDelete.IsEnabled = btnToFront.IsEnabled = btnToBack.IsEnabled = false;
            }
        }

        public void UpdateLocation()
        {
            FrameworkElement fe = mainWindow.SelectedControl.fe;
            updatePageProperties = false;
            cbHorizontalAlignment.SelectedIndex = (int)fe.HorizontalAlignment;
            cbVerticalAlignment.SelectedIndex = (int)fe.VerticalAlignment;
            updatePageProperties = true;

            string locH, locV;
            switch (fe.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Center:
                    if (double.IsNaN(fe.Width))
                        locH = string.Format("Left={0}", fe.Margin.Left);
                    else
                        locH = string.Format("Left={0}, Width={1}", fe.Margin.Left, fe.Width);
                    break;

                case HorizontalAlignment.Right:
                    if (double.IsNaN(fe.Width))
                        locH = string.Format("Right={0}", fe.Margin.Right);
                    else
                        locH = string.Format("Width={0}, Right={1}", fe.Width, fe.Margin.Right);
                    break;

                default:  // HorizontalAlignment.Stretch
                    locH = string.Format("Left={0}, Right={1}", fe.Margin.Left, fe.Margin.Right);
                    break;
            }

            switch (fe.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                case VerticalAlignment.Center:
                    if (double.IsNaN(fe.Height))
                        locV = string.Format("Top={0}", fe.Margin.Top);
                    else
                        locV = string.Format("Top={0}, Height={1}", fe.Margin.Top, fe.Height);
                    break;

                case VerticalAlignment.Bottom:
                    if (double.IsNaN(fe.Height))
                        locV = string.Format("Bottom={0}", fe.Margin.Bottom);
                    else
                        locV = string.Format("Height={0}, Bottom={1}", fe.Height, fe.Margin.Bottom);
                    break;

                default:  // VerticalAlignment.Stretch
                    locV = string.Format("Top={0}, Bottom={1}", fe.Margin.Top, fe.Margin.Bottom);
                    break;
            }

            tbLocation.Text = string.Format(" {0}, {1}", locH, locV);
        }

        private void Align_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (updatePageProperties && mainWindow.SelectedControl != null)
            {
                mainWindow.SetAlignment(mainWindow.SelectedControl, 
                    (HorizontalAlignment)cbHorizontalAlignment.SelectedIndex, (VerticalAlignment)cbVerticalAlignment.SelectedIndex);
            }
        }

        private bool updatePageProperties = false;

        public void OnSelectedControlChanged()
        {
            if (ccProperties.Content is IHmiPropertyPage ppage)
                ppage.Reset();
            if (mainWindow.SelectedControl != null)
            {
                ccProperties.Content = mainWindow.SelectedControl.PropertyPage;
                Tab = ETabs.Edit;
                gridEdit.Visibility = Visibility.Visible;
            }
            else
            {
                ccProperties.Content = null;
            }

            UpdatePropertyPage();
        }

        private void ProjectName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                if (tbProjectName.Text.Length > 0)
                    tbProjectPath.Text = string.Format(@"Visual Studio\{0}\{0}.csproj", tbProjectName.Text.Replace(" ", ""));
                else
                    tbProjectPath.Text = "(no file selected)";
            }
        }
    }
}

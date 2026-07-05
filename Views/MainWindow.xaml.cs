using ManagerForOmronFHVisionSystemSimulators.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ManagerForOmronFHVisionSystemSimulators.Views
{
    public partial class MainWindow : Window
    {
        // Zentrale Konstante für die Platzhalter-Fehlermeldung
        private const string DefaultNotFoundMessage = "File not found. Enter name and press Rename to create.";

        public MainWindow()
        {
            InitializeComponent();

            // ViewModel initialisieren und als DataContext zuweisen
            var vm = new MainViewModel();
            this.DataContext = vm;

            // Delegation: ViewModel fordert UI-Fokus auf das aktive UserControl an
            vm.FocusActiveListAction = () =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    switch (vm.SelectedTabIndex)
                    {
                        case 0: SimulatorsUc.SetFocusToActiveItem(); break;
                        case 1: USBDisksUc.SetFocusToActiveItem(); break;
                        case 2: SimulatorBackupsUc.SetFocusToActiveItem(); break;
                        case 3: USBDiskBackupsUc.SetFocusToActiveItem(); break;
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            };
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Wir entfernen das ContentRendered-Event, damit es sich nicht bei jedem kleinen Refresh wiederholt
            this.ContentRendered -= Window_ContentRendered;

            if (this.DataContext is ViewModels.MainViewModel vm)
            {
                // Der Dispatcher mit Background-Priorität sorgt dafür, dass WPF 
                // erst das Fenster fertig rendert und DANN unsere Initialisierung startet.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    vm.InitializeApp();
                    vm.RestoreListFocus();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // Globale Tastaturkürzel für die direkte Textbox-Navigation
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    FocusAndSelectText(ActiveSimulatorText);
                    e.Handled = true;
                    break;
                case Key.F3:
                    FocusAndSelectText(ActiveUSBDiskText);
                    e.Handled = true;
                    break;
                case Key.F4:
                    FocusAndSelectText(Selected_Item_TextBox);
                    e.Handled = true;
                    break;
            }
        }

        // Hilfsmethode zur Vermeidung von Code-Duplizierung bei den F-Tasten
        private void FocusAndSelectText(TextBox textBox)
        {
            textBox.Focus();
            textBox.SelectAll();
        }

        #region TextBox Focus & Placeholder Logic

        private void ActiveSimulatorText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && vm.ActiveSimulatorName == DefaultNotFoundMessage)
            {
                vm.ActiveSimulatorName = string.Empty;
            }
        }

        private void ActiveSimulatorText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && string.IsNullOrWhiteSpace(vm.ActiveSimulatorName))
            {
                vm.ActiveSimulatorName = DefaultNotFoundMessage;
                vm.ActiveSimulatorColor = Brushes.Red;
            }
        }

        private void ActiveUSBDiskText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && vm.ActiveUSBDiskName == DefaultNotFoundMessage)
            {
                vm.ActiveUSBDiskName = string.Empty;
            }
        }

        private void ActiveUSBDiskText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && string.IsNullOrWhiteSpace(vm.ActiveUSBDiskName))
            {
                vm.ActiveUSBDiskName = DefaultNotFoundMessage;
                vm.ActiveUSBDiskColor = Brushes.Red;
            }
        }

        private void Selected_Item_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && vm.SelectedItemName == DefaultNotFoundMessage)
            {
                vm.SelectedItemName = string.Empty;
            }
        }

        private void Selected_Item_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && string.IsNullOrWhiteSpace(vm.SelectedItemName))
            {
                vm.SelectedItemName = "";
                vm.SelectedItemColor = Brushes.Red;
            }
        }

        #endregion
    }
}
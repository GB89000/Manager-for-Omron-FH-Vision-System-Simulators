using ManagerForOmronFHVisionSystemSimulators.Properties;
using ManagerForOmronFHVisionSystemSimulators.ViewModels;
using ManagerForOmronFHVisionSystemSimulators.Views;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SettingsNamespace
{
    public partial class ProgrammSettings : Window
    {
        private MainWindow _mainWindow;
        //MainWindow mainWindow;
        // Globale ComboBoxItem-Variablen
        public ComboBoxItem ItemType0;
        public ComboBoxItem ItemType1;
        public ComboBoxItem ItemType2;
        public double Scale;

        public ProgrammSettings(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = this;
            _mainWindow = mainWindow;

            PathSimulatorTextBox.Text = Convert.ToString(Settings.Default.SimulatorPath);
            PathSimulatorsTextBox.Text = Convert.ToString(Settings.Default.SimulatorsPath);
            PathSimulatorBackupsTextBox.Text = Convert.ToString(Settings.Default.SimulatorBackupsPath);
            PathUSBDiskTextBox.Text = Convert.ToString(Settings.Default.USBDisksPath);
            PathUSBDiskBackupTextBox.Text = Convert.ToString(Settings.Default.USBDiskBackupsPath);
            TypesOfAdministrationNumber.SelectedIndex = ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.TypesOfAdministrationNumber;
            // === NEU: Die gespeicherte Skalierung vorladen und die Box richtig auswählen ===
            Scale = Settings.Default.UIScale;
            if (Scale == 0.5) UiScaleComboBox.SelectedIndex = 0;
            else if (Scale == 0.7) UiScaleComboBox.SelectedIndex = 1;
            else if (Scale == 1.0) UiScaleComboBox.SelectedIndex = 2;
            else if (Scale == 1.5) UiScaleComboBox.SelectedIndex = 3;

        }

        // === NEU: Wenn der User in der Box etwas anklickt ===
        private void UiScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Wenn das Fenster gerade erst aufbaut, abbrechen
            if (!this.IsLoaded) return;

            // 1. Auslesen, welcher Index gewählt wurde
            switch (UiScaleComboBox.SelectedIndex)
            {
                case 0: Scale = 0.5; break;
                case 1: Scale = 0.7; break;
                case 2: Scale = 1.0; break;
                case 3: Scale = 1.5; break;
            }

            // 3. Live an das Hauptfenster schicken!
            // Da du '_mainWindow' schon im Konstruktor gespeichert hast, nutzen wir das direkt:
            if (_mainWindow != null && _mainWindow.DataContext is MainViewModel mainVM)
            {
                mainVM.UIScale = Scale;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BrowseSimulatorButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Dialog-Instanz erstellen
            OpenFolderDialog dialog = new OpenFolderDialog();

            // 2. Optional: Startverzeichnis setzen (falls in TextBox schon was steht)
            if (System.IO.Directory.Exists(PathSimulatorTextBox.Text))
            {
                dialog.InitialDirectory = PathSimulatorTextBox.Text;
            }

            dialog.Title = "Select Backup Folder";

            // 3. Dialog anzeigen
            if (dialog.ShowDialog() == true)
            {
                // 4. Den gewählten Pfad in die TextBox schreiben
                PathSimulatorTextBox.Text = dialog.FolderName;
            }
        }

        private void BrowseSimulatorsButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Dialog-Instanz erstellen
            OpenFolderDialog dialog = new OpenFolderDialog();

            // 2. Optional: Startverzeichnis setzen (falls in TextBox schon was steht)
            if (System.IO.Directory.Exists(PathSimulatorsTextBox.Text))
            {
                dialog.InitialDirectory = PathSimulatorsTextBox.Text;
            }

            dialog.Title = "Select Backup Folder";

            // 3. Dialog anzeigen
            if (dialog.ShowDialog() == true)
            {
                // 4. Den gewählten Pfad in die TextBox schreiben
                PathSimulatorsTextBox.Text = dialog.FolderName;
            }
        }

        private void BrowseSimulatorsBackupsButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Dialog-Instanz erstellen
            OpenFolderDialog dialog = new OpenFolderDialog();

            // 2. Optional: Startverzeichnis setzen (falls in TextBox schon was steht)
            if (System.IO.Directory.Exists(PathSimulatorBackupsTextBox.Text))
            {
                dialog.InitialDirectory = PathSimulatorBackupsTextBox.Text;
            }

            dialog.Title = "Select Backup Folder";

            // 3. Dialog anzeigen
            if (dialog.ShowDialog() == true)
            {
                // 4. Den gewählten Pfad in die TextBox schreiben
                PathSimulatorBackupsTextBox.Text = dialog.FolderName;
            }
        }

        private void BrowseUSBDiskButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Dialog-Instanz erstellen
            OpenFolderDialog dialog = new OpenFolderDialog();

            // 2. Optional: Startverzeichnis setzen (falls in TextBox schon was steht)
            if (System.IO.Directory.Exists(PathUSBDiskTextBox.Text))
            {
                dialog.InitialDirectory = PathUSBDiskTextBox.Text;
            }

            dialog.Title = "Select USB Disk Folder";

            // 3. Dialog anzeigen
            if (dialog.ShowDialog() == true)
            {
                // 4. Den gewählten Pfad in die TextBox schreiben
                PathUSBDiskTextBox.Text = dialog.FolderName;
            }
        }

        private void BrowseUSBDiskBackupButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Dialog-Instanz erstellen
            OpenFolderDialog dialog = new OpenFolderDialog();

            // 2. Optional: Startverzeichnis setzen (falls in TextBox schon was steht)
            if (System.IO.Directory.Exists(PathUSBDiskBackupTextBox.Text))
            {
                dialog.InitialDirectory = PathUSBDiskBackupTextBox.Text;
            }

            dialog.Title = "Select USB Disk Backup Folder";

            // 3. Dialog anzeigen
            if (dialog.ShowDialog() == true)
            {
                // 4. Den gewählten Pfad in die TextBox schreiben
                PathUSBDiskBackupTextBox.Text = dialog.FolderName;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.SimulatorPath = PathSimulatorTextBox.Text;
            Settings.Default.SimulatorsPath = PathSimulatorsTextBox.Text;
            Settings.Default.SimulatorBackupsPath = PathSimulatorBackupsTextBox.Text;
            Settings.Default.USBDisksPath = PathUSBDiskTextBox.Text;
            Settings.Default.USBDiskBackupsPath = PathUSBDiskBackupTextBox.Text;
            Settings.Default.TypesOfAdministrationNumber = TypesOfAdministrationNumber.SelectedIndex;
            Settings.Default.UIScale = Scale;
            Settings.Default.Save();

            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null && _mainWindow.DataContext is MainViewModel mainVM)
            {
                mainVM.UIScale = Settings.Default.UIScale;
            }
            Close();
        }
    }
}

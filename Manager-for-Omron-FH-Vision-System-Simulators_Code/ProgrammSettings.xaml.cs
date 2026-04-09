using Microsoft.Win32;
using NamespaceManagerForOmronFHVisionSystemSimulatorsXAML;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System;
using ManagerForOmronFHVisionSystemSimulators.Properties;

namespace NamespaceSettingsXAML
{
    public partial class ProgrammSettings : Window
    {
        private MainWindow _mainWindow;
        //MainWindow mainWindow;
        // Globale ComboBoxItem-Variablen
        public ComboBoxItem Item0;
        public ComboBoxItem Item1;
        public ComboBoxItem Item2;
        public ComboBoxItem Item3;

        public ProgrammSettings(MainWindow mainWindow)
        {
            InitializeComponent();
            // Items instanziieren
            Item0 = new ComboBoxItem { Content = "HD720" };
            Item1 = new ComboBoxItem { Content = "HD1080" };
            Item2 = new ComboBoxItem { Content = "WQHD" };
            Item3 = new ComboBoxItem { Content = "UHD 4K" };

            // Globale Items zur ComboBox hinzufügen
            ColorComboBox.Items.Add(Item0);
            ColorComboBox.Items.Add(Item1);
            ColorComboBox.Items.Add(Item2);
            ColorComboBox.Items.Add(Item3);
            _mainWindow = mainWindow;

            ColorComboBox.SelectedIndex = Settings.Default.ResolutionNumber; // Setzt den Standardwert auf das erste Element

            PathSimulatorTextBox.Text = Convert.ToString(Settings.Default.SimulatorPath);
            PathSimulatorsTextBox.Text = Convert.ToString(Settings.Default.SimulatorsPath);
            PathSimulatorBackupsTextBox.Text = Convert.ToString(Settings.Default.SimulatorBackupsPath);
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.SimulatorPath = PathSimulatorTextBox.Text;
            Settings.Default.SimulatorsPath = PathSimulatorsTextBox.Text;
            Settings.Default.SimulatorBackupsPath = PathSimulatorBackupsTextBox.Text;

            int resolutionNumber = ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionNumber;
            if (ColorComboBox.SelectedItem == Item0)
            {
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionHeight = (int)(1105 * 0.63);
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionWidth = (int)(1045 * 0.63);
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionNumber = 0;
            }
            else if (ColorComboBox.SelectedItem == Item1)
            {
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionHeight = (int)(1105 * 0.95);
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionWidth = (int)(1045 * 0.95);
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionNumber = 1;
            }
            else if (ColorComboBox.SelectedItem == Item2)
            {
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionHeight = (int)(1105 * 1.27);
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionWidth = (int)(1045 * 1.27);
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionNumber = 2;
            }
            else if (ColorComboBox.SelectedItem == Item3)
            {
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionHeight = (int)(1105 * 1.9);
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionWidth = (int)(1045 * 1.9);
                ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionNumber = 3;
            }
            ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.Save();
            _mainWindow.ResizeWindow();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

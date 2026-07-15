using InfoWindowNamespace;
using ManagerForOmronFHVisionSystemSimulators.FolderHelperNamespace;
using ManagerForOmronFHVisionSystemSimulators.Properties;
using SettingsNamespace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static ManagerForOmronFHVisionSystemSimulators.FolderHelperNamespace.FolderHelper;
using static SharedUI.StylesAndGraphics.ButtonColors;

namespace ManagerForOmronFHVisionSystemSimulators.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // Zentrale Konstante für die Platzhalter-Fehlermeldung
        private const string DefaultNotFoundMessage = "File not found. Enter name and press Rename to create.";

        public MainViewModel()
        {
            SimulatorsVM = new SimulatorsViewModel();
            USBDisksVM = new USBDisksViewModel();
            SimulatorBackupsVM = new SimulatorBackupsViewModel();
            USBDiskBackupsVM = new USBDiskBackupsViewModel();

            SimulatorsVM.PropertyChanged += ChildViewModel_PropertyChanged;
            USBDisksVM.PropertyChanged += ChildViewModel_PropertyChanged;
            SimulatorBackupsVM.PropertyChanged += ChildViewModel_PropertyChanged;
            USBDiskBackupsVM.PropertyChanged += ChildViewModel_PropertyChanged;

            // Commands initialisieren
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            OpenInfoCommand = new RelayCommand(ExecuteOpenInfo);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            CloseCommand = new RelayCommand(ExecuteClose);
            RenameCommand = new RelayCommand(ExecuteRename);
            RenameUSBDiskCommand = new RelayCommand(ExecuteRenameUSBDisk, CanExecuteUSBFunction);
            RenameItemCommand = new RelayCommand(ExecuteRenameItem);
            LoadItemCommand = new RelayCommand(ExecuteLoadItem, CanExecuteUSBFunction);
            StartLauncherCommand = new RelayCommand(ExecuteStartLauncher);
            LoadCommand = new RelayCommand(ExecuteLoad);
            DeleteSelectedCommand = new RelayCommand(ExecuteDeleteSelected);
            OpenItemFolderCommand = new RelayCommand(ExecuteOpenItemFolder);
            OpenActiveUSBDiskFolderCommand = new RelayCommand(ExecuteOpenActiveUSBDiskFolder);
            SaveSimulatorChangesCommand = new RelayCommand(ExecuteSaveSimulatorChanges);
            SaveUSBDiskChangesCommand = new RelayCommand(ExecuteSaveUSBDiskChanges, CanExecuteUSBFunction);
            CopySelectedToFromBackupsCommand = new RelayCommand(ExecuteCopySelectedToFromBackups);
            ToggleDeleteAfterCopyCommand = new RelayCommand(ExecuteToggleDeleteAfterCopy);
            NextTabCommand = new RelayCommand(ExecuteNextTab);
            PreviousTabCommand = new RelayCommand(ExecutePreviousTab);

            _isDeleteAfterCopyEnabled = Settings.Default.DeleteOnOff;

            // Skalierungswert aus den Settings abrufen und anwenden
            double savedScale = Properties.Settings.Default.UIScale;
            if (savedScale <= 0)
            {
                savedScale = 1.0;
            }
            this.UIScale = savedScale;
        }

        #region Commands & Properties

        public ICommand StartLauncherCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenInfoCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand RenameUSBDiskCommand { get; }
        public ICommand RenameItemCommand { get; }
        public ICommand LoadItemCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveSimulatorChangesCommand { get; }
        public ICommand SaveUSBDiskChangesCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand OpenItemFolderCommand { get; }
        public ICommand OpenActiveUSBDiskFolderCommand { get; }
        public ICommand CopySelectedToFromBackupsCommand { get; }
        public ICommand ToggleDeleteAfterCopyCommand { get; }
        public ICommand NextTabCommand { get; }
        public ICommand PreviousTabCommand { get; }

        public SimulatorsViewModel SimulatorsVM { get; set; }
        public USBDisksViewModel USBDisksVM { get; set; }
        public SimulatorBackupsViewModel SimulatorBackupsVM { get; set; }
        public USBDiskBackupsViewModel USBDiskBackupsVM { get; set; }
        public Action FocusActiveListAction { get; set; }

        private double _uiScale = 1.0;
        public double UIScale
        {
            get => _uiScale;
            set { _uiScale = value; OnPropertyChanged(nameof(UIScale)); }
        }

        private string _activeSimulatorName;
        public string ActiveSimulatorName
        {
            get => _activeSimulatorName;
            set
            {
                if (_activeSimulatorName != value)
                {
                    _activeSimulatorName = value;
                    OnPropertyChanged(nameof(ActiveSimulatorName));

                    if (ActiveSimulatorColor == Brushes.Red && value != DefaultNotFoundMessage)
                    {
                        ActiveSimulatorColor = Brushes.White;
                    }
                }
            }
        }

        private string _activeUSBDiskName;
        public string ActiveUSBDiskName
        {
            get => _activeUSBDiskName;
            set
            {
                if (_activeUSBDiskName != value)
                {
                    _activeUSBDiskName = value;
                    OnPropertyChanged(nameof(ActiveUSBDiskName));

                    if (ActiveUSBDiskColor == Brushes.Red && value != DefaultNotFoundMessage)
                    {
                        ActiveUSBDiskColor = Brushes.White;
                    }
                }
            }
        }

        private string _selectedItemName;
        public string SelectedItemName
        {
            get => _selectedItemName;
            set
            {
                if (_selectedItemName != value)
                {
                    _selectedItemName = value;
                    OnPropertyChanged(nameof(SelectedItemName));
                }
            }
        }

        private string _linkedUSBDiskName;
        public string LinkedUSBDiskName
        {
            get => _linkedUSBDiskName;
            set
            {
                if (_linkedUSBDiskName != value)
                {
                    _linkedUSBDiskName = value;
                    OnPropertyChanged(nameof(LinkedUSBDiskName));
                }
            }
        }

        public bool IsUSBFeatureEnabled => Settings.Default.TypesOfAdministrationNumber != 0;

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    SelectedItemName = string.Empty;
                    ResetAllSelections();
                    TriggerTabLoading();
                    OnPropertyChanged(nameof(SelectedTabIndex));
                    RestoreListFocus();
                }
            }
        }

        private Brush _activeSimulatorColor = Brushes.White;
        public Brush ActiveSimulatorColor
        {
            get => _activeSimulatorColor;
            set { _activeSimulatorColor = value; OnPropertyChanged(nameof(ActiveSimulatorColor)); }
        }

        private Brush _activeUSBDiskColor = Brushes.White;
        public Brush ActiveUSBDiskColor
        {
            get => _activeUSBDiskColor;
            set { _activeUSBDiskColor = value; OnPropertyChanged(nameof(ActiveUSBDiskColor)); }
        }

        private Brush _selectedItemColor = Brushes.White;
        public Brush SelectedItemColor
        {
            get => _selectedItemColor;
            set { _selectedItemColor = value; OnPropertyChanged(nameof(SelectedItemColor)); }
        }

        private bool _isDeleteAfterCopyEnabled;
        public bool IsDeleteAfterCopyEnabled
        {
            get => _isDeleteAfterCopyEnabled;
            set
            {
                if (_isDeleteAfterCopyEnabled != value)
                {
                    _isDeleteAfterCopyEnabled = value;
                    Settings.Default.DeleteOnOff = value;
                    Settings.Default.Save();

                    OnPropertyChanged(nameof(IsDeleteAfterCopyEnabled));
                    OnPropertyChanged(nameof(DeleteAfterCopyText));
                    OnPropertyChanged(nameof(DeleteAfterCopyColor));
                }
            }
        }

        public string DeleteAfterCopyText => IsDeleteAfterCopyEnabled ? "Del. after\n Copy: ON" : "Del. after\n Copy: OFF";
        public Brush DeleteAfterCopyColor => IsDeleteAfterCopyEnabled ? ButtonGreen() : ButtonRed();

        #endregion

        #region File Checking & Navigation

        public void InitializeApp()
        {
            // 1. Erst jetzt prüfen wir, welche Simulatoren und USB-Disks aktiv sind
            CheckActiveFiles();

            // 2. Jetzt stoßen wir das Einlesen des aktuell sichtbaren Tabs an
            TriggerTabLoading();

            // 3. Optional: Den Fokus auf die erste Zeile setzen
            RestoreListFocus();
        }

        private bool CanExecuteUSBFunction(object parameter)
        {
            if (IsUSBFeatureEnabled)
                return true;
            else
                return false;

        }
        private void CheckActiveFiles()
        {
            string simulatorPath = Settings.Default.SimulatorPath;
            string usbDiskPath = Path.Combine(simulatorPath, "USBDisk");

            // 1. Check Active Simulator (.gb Datei)
            if (Directory.Exists(simulatorPath))
            {
                var gbFiles = Directory.GetFiles(simulatorPath, "*.gb");
                if (gbFiles.Length > 0)
                {
                    ActiveSimulatorName = Path.GetFileNameWithoutExtension(gbFiles[0]);
                    ActiveSimulatorColor = Brushes.White;
                }
                else
                {
                    ActiveSimulatorName = DefaultNotFoundMessage;
                    ActiveSimulatorColor = Brushes.Red;
                }
            }
            else
            {
                ActiveSimulatorName = DefaultNotFoundMessage;
                ActiveSimulatorColor = Brushes.Red;
            }

            // 2. Check Active USBDisk (.gb Datei im Unterordner)
            if (Directory.Exists(usbDiskPath))
            {
                var usbGbFiles = Directory.GetFiles(usbDiskPath, "*.gb");
                if (usbGbFiles.Length > 0)
                {
                    ActiveUSBDiskName = Path.GetFileNameWithoutExtension(usbGbFiles[0]);
                    ActiveUSBDiskColor = Brushes.White;
                }
                else
                {
                    ActiveUSBDiskName = DefaultNotFoundMessage;
                    ActiveUSBDiskColor = Brushes.Red;
                }
            }
            else
            {
                ActiveUSBDiskName = DefaultNotFoundMessage;
                ActiveUSBDiskColor = Brushes.Red;
            }
        }

        private void TriggerTabLoading()
        {
            switch (SelectedTabIndex)
            {
                case 0: SimulatorsVM?.LoadListFromDirectory(Settings.Default.SimulatorsPath); break;
                case 1: USBDisksVM?.LoadListFromDirectory(Settings.Default.USBDisksPath); break;
                case 2: SimulatorBackupsVM?.LoadListFromDirectory(Settings.Default.SimulatorBackupsPath); break;
                case 3: USBDiskBackupsVM?.LoadListFromDirectory(Settings.Default.USBDiskBackupsPath); break;
            }
        }

        public void RestoreListFocus()
        {
            if (SelectedTabIndex == 0 && SimulatorsVM?.SimulatorsList?.Count > 0)
            {
                SimulatorsVM.SelectedSimulator = SimulatorsVM.SimulatorsList[0];
            }
            else if (SelectedTabIndex == 1 && USBDisksVM?.USBDisksList?.Count > 0)
            {
                USBDisksVM.SelectedUSBDisk = USBDisksVM.USBDisksList[0];
            }
            else if (SelectedTabIndex == 2 && SimulatorBackupsVM?.SimulatorBackupsList?.Count > 0)
            {
                SimulatorBackupsVM.SelectedSimulatorBackups = SimulatorBackupsVM.SimulatorBackupsList[0];
            }
            else if (SelectedTabIndex == 3 && USBDiskBackupsVM?.USBDiskBackupsList?.Count > 0)
            {
                USBDiskBackupsVM.SelectedUSBDiskBackups = USBDiskBackupsVM.USBDiskBackupsList[0];
            }

            FocusActiveListAction?.Invoke();
        }

        private void ChildViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SimulatorsVM.SelectedSimulator) && SelectedTabIndex == 0)
            {
                SelectedItemName = SimulatorsVM.SelectedSimulator?.Name ?? string.Empty;
            }
            else if (e.PropertyName == nameof(USBDisksVM.SelectedUSBDisk) && SelectedTabIndex == 1)
            {
                SelectedItemName = USBDisksVM.SelectedUSBDisk?.Name ?? string.Empty;
            }
            else if (e.PropertyName == nameof(SimulatorBackupsVM.SelectedSimulatorBackups) && SelectedTabIndex == 2)
            {
                SelectedItemName = SimulatorBackupsVM.SelectedSimulatorBackups?.Name ?? string.Empty;
            }
            else if (e.PropertyName == nameof(USBDiskBackupsVM.SelectedUSBDiskBackups) && SelectedTabIndex == 3)
            {
                SelectedItemName = USBDiskBackupsVM.SelectedUSBDiskBackups?.Name ?? string.Empty;
            }

            // === NEU: Aktualisiert SOFORT die Anzeige für den verknüpften USB-Stick! ===
            UpdateLinkedUSBDiskName();
        }

        private void ResetAllSelections()
        {
            if (SimulatorsVM != null) SimulatorsVM.SelectedSimulator = null;
            if (USBDisksVM != null) USBDisksVM.SelectedUSBDisk = null;
            if (SimulatorBackupsVM != null) SimulatorBackupsVM.SelectedSimulatorBackups = null;
            if (USBDiskBackupsVM != null) USBDiskBackupsVM.SelectedUSBDiskBackups = null;
        }

        private void ExecuteNextTab(object parameter)
        {
            if (IsUSBFeatureEnabled)
            {
                SelectedTabIndex = (SelectedTabIndex >= 3) ? 0 : SelectedTabIndex + 1;
            }
            else
            {
                SelectedTabIndex = (SelectedTabIndex == 0) ? 2 : 0;
            }
        }

        private void ExecutePreviousTab(object parameter)
        {
            if (IsUSBFeatureEnabled)
            {
                SelectedTabIndex = (SelectedTabIndex <= 0) ? 3 : SelectedTabIndex - 1;
            }
            else
            {
                SelectedTabIndex = (SelectedTabIndex == 0) ? 2 : 0;
            }
        }

        #endregion

        #region Command Executions

        private void ExecuteOpenSettings(object parameter)
        {
            if (Application.Current.MainWindow is Views.MainWindow mainWin)
            {
                var settingsWindow = new ProgrammSettings(mainWin)
                {
                    Owner = mainWin
                };
                settingsWindow.ShowDialog();

                OnPropertyChanged(nameof(IsUSBFeatureEnabled));

                // Absicherung gegen "Geister-Tabs" nach dem Schließen der Settings
                if (!IsUSBFeatureEnabled)
                {
                    if (SelectedTabIndex == 1 || SelectedTabIndex == 3)
                    {
                        SelectedTabIndex = 0;
                    }

                    USBDisksVM?.USBDisksList?.Clear();
                    USBDiskBackupsVM?.USBDiskBackupsList?.Clear();
                    ResetAllSelections();
                }

                TriggerTabLoading();
                RestoreListFocus();
                CheckActiveFiles();
            }
        }

        private void ExecuteOpenInfo(object parameter)
        {
            var infoWindow = new InfoWindow
            {
                Owner = Application.Current.MainWindow
            };
            infoWindow.ShowDialog();
        }

        private void ExecuteMinimize(object parameter)
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
        }

        private void ExecuteClose(object parameter)
        {
            Application.Current.Shutdown();
        }

        private void ExecuteStartLauncher(object parameter)
        {
            string pfad = Settings.Default.LauncherPath;

            // 1. Prüfen, ob der Pfad leer ist oder die Datei gar nicht existiert
            if (string.IsNullOrWhiteSpace(pfad) || !File.Exists(pfad))
            {
                CustomMessageBox.Show(
                    "The specified path is invalid or the launcher file could not be found.",
                    "Notice",
                    Application.Current.MainWindow,
                    Brushes.Yellow
                );
                return;
            }

            try
            {
                // 2. Start-Informationen konfigurieren
                var startInfo = new ProcessStartInfo
                {
                    FileName = pfad,
                    // Setzt den Ordner der .exe als Arbeitsverzeichnis (wichtig, damit der Launcher seine Ressourcen findet)
                    WorkingDirectory = Path.GetDirectoryName(pfad),
                    // UseShellExecute stellt sicher, dass Windows die App wie bei einem Doppelklick startet
                    UseShellExecute = true
                };

                // 3. Programm starten
                Process.Start(startInfo);
            }
            catch (Exception ex) // Fängt Fehler ab (z. B. fehlende Admin-Rechte)
            {
                CustomMessageBox.Show(
                    $"The file could not be started:\n{ex.Message}",
                    "Error",
                    Application.Current.MainWindow,
                    Brushes.Red
                );
            }
        }

        private void ExecuteRename(object parameter)
        {
            string newName = ActiveSimulatorName?.Trim();
            string rootPath = Settings.Default.SimulatorPath;

            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                CustomMessageBox.Show("The specified folder path does not exist.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            if (string.IsNullOrWhiteSpace(newName) || newName.Contains("not found"))
            {
                CustomMessageBox.Show("Please enter a valid name for the simulator.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            try
            {
                var existingFiles = Directory.GetFiles(rootPath, "*.gb", SearchOption.TopDirectoryOnly);
                string newFilePath = Path.Combine(rootPath, newName + ".gb");

                string usbDiskContent = string.Empty;
                if (!string.IsNullOrWhiteSpace(ActiveUSBDiskName) && !ActiveUSBDiskName.Contains("not found"))
                {
                    usbDiskContent = ActiveUSBDiskName.Trim();
                }

                if (existingFiles.Length > 0)
                {
                    string oldFilePath = existingFiles[0];
                    if (!string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(oldFilePath, newFilePath);
                    }
                }
                else
                {
                    using (File.Create(newFilePath)) { }
                }

                if (Settings.Default.TypesOfAdministrationNumber == 0)
                {
                    ActiveUSBDiskName = ActiveSimulatorName;
                    usbDiskContent  = ActiveSimulatorName;
                    ExecuteRenameUSBDisk(RenameUSBDiskCommand);
                }

                File.WriteAllText(newFilePath, usbDiskContent);
                ActiveSimulatorColor = Brushes.White;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error processing the file: {ex.Message}", "Error", Application.Current.MainWindow, Brushes.Red);
            }
        }

        private void ExecuteRenameUSBDisk(object parameter)
        {
            string newName = ActiveUSBDiskName?.Trim();
            string rootPath = Path.Combine(Settings.Default.SimulatorPath, "USBDisk");

            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                CustomMessageBox.Show("The specified folder path does not exist.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            if (string.IsNullOrWhiteSpace(newName) || newName.Contains("not found"))
            {
                CustomMessageBox.Show("Please enter a valid name for the USB Disk.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            try
            {
                var existingFiles = Directory.GetFiles(rootPath, "*.gb", SearchOption.TopDirectoryOnly);
                string newFilePath = Path.Combine(rootPath, newName + ".gb");

                if (existingFiles.Length > 0)
                {
                    string oldFilePath = existingFiles[0];
                    if (!string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(oldFilePath, newFilePath);
                    }
                }
                else
                {
                    using (File.Create(newFilePath)) { }
                }

                if (Settings.Default.TypesOfAdministrationNumber != 0)
                {
                    ExecuteRename(RenameCommand);
                }
                ActiveUSBDiskColor = Brushes.White;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error processing the file: {ex.Message}", "Error", Application.Current.MainWindow, Brushes.Red);
            }
        }

        private void ExecuteRenameItem(object parameter)
        {
            string markierterName = null;
            string usbSourceFolder = null;
            string activeListPath = string.Empty;

            switch (SelectedTabIndex)
            {
                case 0:
                    markierterName = SimulatorsVM.SelectedSimulator?.Name;
                    activeListPath = Settings.Default.SimulatorsPath;
                    usbSourceFolder = Settings.Default.USBDisksPath;
                    break;
                case 1:
                    markierterName = USBDisksVM.SelectedUSBDisk?.Name;
                    activeListPath = Settings.Default.USBDisksPath;
                    usbSourceFolder = Settings.Default.USBDisksPath;
                    break;
                case 2:
                    markierterName = SimulatorBackupsVM.SelectedSimulatorBackups?.Name;
                    activeListPath = Settings.Default.SimulatorBackupsPath;
                    usbSourceFolder = Settings.Default.USBDiskBackupsPath;
                    break;
                case 3:
                    markierterName = USBDiskBackupsVM.SelectedUSBDiskBackups?.Name;
                    activeListPath = Settings.Default.USBDiskBackupsPath;
                    usbSourceFolder = Settings.Default.USBDiskBackupsPath;
                    break;
            }

            string newName = SelectedItemName?.Trim();

            if (Settings.Default.TypesOfAdministrationNumber == 0)
            {
                try
                {
                    // 1. Simulator umbenennen (Ordner + Datei)
                    string alterSimulatorOrdner = Path.Combine(activeListPath, markierterName);
                    string neuerSimulatorOrdner = Path.Combine(activeListPath, newName);

                    if (Directory.Exists(alterSimulatorOrdner) && !string.Equals(alterSimulatorOrdner, neuerSimulatorOrdner, StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Move(alterSimulatorOrdner, neuerSimulatorOrdner);
                    }

                    if (Directory.Exists(neuerSimulatorOrdner))
                    {
                        var existingFiles = Directory.GetFiles(neuerSimulatorOrdner, "*.gb", SearchOption.TopDirectoryOnly);
                        string newFilePath = Path.Combine(neuerSimulatorOrdner, newName + ".gb");

                        if (existingFiles.Length > 0)
                        {
                            string oldFilePath = existingFiles[0];
                            if (!string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Move(oldFilePath, newFilePath);
                            }
                        }
                        else
                        {
                            using (File.Create(newFilePath)) { }
                        }

                        File.WriteAllText(newFilePath, newName);
                    }

                    // 2. USB Disk umbenennen (Ordner + Datei)
                    string alterUsbOrdner = Path.Combine(usbSourceFolder, markierterName);
                    string neuerUsbOrdner = Path.Combine(usbSourceFolder, newName);

                    if (Directory.Exists(alterUsbOrdner) && !string.Equals(alterUsbOrdner, neuerUsbOrdner, StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Move(alterUsbOrdner, neuerUsbOrdner);
                    }

                    if (Directory.Exists(neuerUsbOrdner))
                    {
                        var existingUsbFiles = Directory.GetFiles(neuerUsbOrdner, "*.gb", SearchOption.TopDirectoryOnly);
                        string newUsbFilePath = Path.Combine(neuerUsbOrdner, newName + ".gb");

                        if (existingUsbFiles.Length > 0)
                        {
                            string oldUsbFilePath = existingUsbFiles[0];
                            if (!string.Equals(oldUsbFilePath, newUsbFilePath, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Move(oldUsbFilePath, newUsbFilePath);
                            }
                        }
                        else
                        {
                            using (File.Create(newUsbFilePath)) { }
                        }
                    }

                    TriggerTabLoading();
                    RestoreListFocus();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Error processing the file/folder: {ex.Message}", "Error", Application.Current.MainWindow, Brushes.Red);
                }
            }
            else if (Settings.Default.TypesOfAdministrationNumber != 0)
            {
                if (SelectedTabIndex == 1 || SelectedTabIndex == 3)
                {
                    var mainWin = Application.Current.MainWindow as Views.MainWindow;
                    bool renameUSBDisk = CustomMessageBoxYesNo.Show(
                        "Do you also want to rename this USB Disk?",
                        "Warning: The connection to the simulator could be lost!",
                        mainWin,
                        Brushes.White);

                    if (!renameUSBDisk) return;
                }

                try
                {
                    string alterSimulatorOrdner = Path.Combine(activeListPath, markierterName);
                    string neuerSimulatorOrdner = Path.Combine(activeListPath, newName);

                    if (Directory.Exists(alterSimulatorOrdner) && !string.Equals(alterSimulatorOrdner, neuerSimulatorOrdner, StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Move(alterSimulatorOrdner, neuerSimulatorOrdner);
                    }

                    if (Directory.Exists(neuerSimulatorOrdner))
                    {
                        var existingFiles = Directory.GetFiles(neuerSimulatorOrdner, "*.gb", SearchOption.TopDirectoryOnly);
                        string newFilePath = Path.Combine(neuerSimulatorOrdner, newName + ".gb");

                        if (existingFiles.Length > 0)
                        {
                            string oldFilePath = existingFiles[0];
                            if (!string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Move(oldFilePath, newFilePath);
                            }
                        }
                        else
                        {
                            using (File.Create(newFilePath)) { }
                        }
                    }

                    TriggerTabLoading();
                    RestoreListFocus();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Error processing the file/folder: {ex.Message}", "Error", Application.Current.MainWindow, Brushes.Red);
                }
            }
        }

        private async void ExecuteLoadItem(object parameter)
        {
            string markierterName = null;
            string usbSourceFolder = null;
            string activeListPath = string.Empty;

            switch (SelectedTabIndex)
            {
                case 0:
                    markierterName = SimulatorsVM.SelectedSimulator?.Name;
                    activeListPath = Settings.Default.SimulatorsPath;
                    usbSourceFolder = Settings.Default.USBDisksPath;
                    break;
                case 1:
                    markierterName = USBDisksVM.SelectedUSBDisk?.Name;
                    activeListPath = Settings.Default.USBDisksPath;
                    usbSourceFolder = Settings.Default.USBDisksPath;
                    break;
                case 2:
                    markierterName = SimulatorBackupsVM.SelectedSimulatorBackups?.Name;
                    activeListPath = Settings.Default.SimulatorBackupsPath;
                    usbSourceFolder = Settings.Default.USBDiskBackupsPath;
                    break;
                case 3:
                    markierterName = USBDiskBackupsVM.SelectedUSBDiskBackups?.Name;
                    activeListPath = Settings.Default.USBDiskBackupsPath;
                    usbSourceFolder = Settings.Default.USBDiskBackupsPath;
                    break;
            }

            if (markierterName == null)
            {
                CustomMessageBox.Show("Please select an entry from the list first.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            var mainWin = Application.Current.MainWindow as Views.MainWindow;
            if (mainWin != null) mainWin.IsHitTestVisible = false;

            var statusWin = new StatusWindow { Owner = mainWin };
            statusWin.Show();

            await Task.Delay(100);

            try
            {
                string simulatorPath = Settings.Default.SimulatorPath;
                string destinationFolder = Path.Combine(activeListPath, markierterName);

                await Task.Run(() =>
                {
                    if (SelectedTabIndex == 0 || SelectedTabIndex == 2)
                    {
                        if (Directory.Exists(simulatorPath))
                        {
                            EmptyDirectory(simulatorPath, mainWin, "USBDisk");
                            CopyDirectoryContents(destinationFolder, simulatorPath, true, mainWin);
                        }
                    }
                    else if (SelectedTabIndex == 1 || SelectedTabIndex == 3)
                    {
                        if (Directory.Exists(simulatorPath))
                        {
                            string usbDiskFolderTargetPath = Path.Combine(simulatorPath, "USBDisk");
                            EmptyDirectory(usbDiskFolderTargetPath, mainWin);
                            usbSourceFolder = Path.Combine(usbSourceFolder, markierterName);
                            CopyDirectoryContents(usbSourceFolder, usbDiskFolderTargetPath, true, mainWin);
                        }
                    }
                });

                CheckActiveFiles();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error: {ex.Message}", "Error", mainWin, Brushes.Red);
            }
            finally
            {
                statusWin.Close();
                if (mainWin != null) mainWin.IsHitTestVisible = true;
            }
        }

        private async void ExecuteLoad(object parameter)
        {
            string markierterName = null;
            string usbSourceFolder = null;
            string activeListPath = string.Empty;

            switch (SelectedTabIndex)
            {
                case 0:
                    markierterName = SimulatorsVM.SelectedSimulator?.Name;
                    activeListPath = Settings.Default.SimulatorsPath;
                    usbSourceFolder = Settings.Default.USBDisksPath;
                    break;
                case 1:
                    markierterName = USBDisksVM.SelectedUSBDisk?.Name;
                    activeListPath = Settings.Default.USBDisksPath;
                    usbSourceFolder = Settings.Default.USBDisksPath;
                    break;
                case 2:
                    markierterName = SimulatorBackupsVM.SelectedSimulatorBackups?.Name;
                    activeListPath = Settings.Default.SimulatorBackupsPath;
                    usbSourceFolder = Settings.Default.USBDiskBackupsPath;
                    break;
                case 3:
                    markierterName = USBDiskBackupsVM.SelectedUSBDiskBackups?.Name;
                    activeListPath = Settings.Default.USBDiskBackupsPath;
                    usbSourceFolder = Settings.Default.USBDiskBackupsPath;
                    break;
            }

            if (markierterName == null)
            {
                CustomMessageBox.Show("Please select an entry from the list first.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            var mainWin = Application.Current.MainWindow as Views.MainWindow;

            // Vorab-Validierung auf dem UI-Thread
            if (SelectedTabIndex == 1 || SelectedTabIndex == 3)
            {
                CustomMessageBox.Show("This item cannot be loaded directly via button.", "Note", mainWin, Brushes.White);
                return;
            }

            if (mainWin != null) mainWin.IsHitTestVisible = false;

            var statusWin = new StatusWindow { Owner = mainWin };
            statusWin.Show();

            await Task.Delay(100);

            try
            {
                string simulatorPath = Settings.Default.SimulatorPath;
                string destinationFolder = Path.Combine(activeListPath, markierterName);

                await Task.Run(() =>
                {
                    if (Settings.Default.TypesOfAdministrationNumber == 0)
                    {
                        if (Directory.Exists(simulatorPath))
                        {
                            EmptyDirectory(simulatorPath, mainWin);
                        }
                        CopyDirectoryContents(destinationFolder, simulatorPath, true, mainWin);

                        string usbDiskFolderTargetPath = Path.Combine(simulatorPath, "USBDisk");
                        Directory.CreateDirectory(usbDiskFolderTargetPath);

                        usbSourceFolder = Path.Combine(usbSourceFolder, markierterName);
                        CopyDirectoryContents(usbSourceFolder, usbDiskFolderTargetPath, true, mainWin);
                    }
                    else if (Settings.Default.TypesOfAdministrationNumber == 1)
                    {
                        var gbFiles = Directory.GetFiles(destinationFolder, "*.gb");
                        if (gbFiles.Length == 0)
                        {
                            throw new FileNotFoundException("Simulator .gb file was not found.");
                        }

                        string ausgelesenerUsbName = File.ReadAllText(gbFiles[0]).Trim();

                        // Hier greift deine Absicherung für leere Dateien:
                        if (string.IsNullOrWhiteSpace(ausgelesenerUsbName))
                        {
                            throw new InvalidDataException("No USB Disk name found in the simulator's .gb file (file is empty).");
                        }

                        if (Directory.Exists(simulatorPath))
                        {
                            EmptyDirectory(simulatorPath, mainWin);
                        }

                        CopyDirectoryContents(destinationFolder, simulatorPath, true, mainWin);

                        string usbDiskFolderTargetPath = Path.Combine(simulatorPath, "USBDisk");
                        Directory.CreateDirectory(usbDiskFolderTargetPath);

                        usbSourceFolder = Path.Combine(usbSourceFolder, ausgelesenerUsbName);
                        CopyDirectoryContents(usbSourceFolder, usbDiskFolderTargetPath, true, mainWin);
                    }
                });

                CheckActiveFiles();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error: {ex.Message}", "Error", mainWin, Brushes.Red);
            }
            finally
            {
                statusWin.Close();
                if (mainWin != null) mainWin.IsHitTestVisible = true;
            }
        }

        private async void ExecuteSaveSimulatorChanges(object parameter)
        {
            ExecuteRename(RenameCommand);

            string simName = ActiveSimulatorName?.Trim();
            string simulatorPath = Settings.Default.SimulatorPath;
            string simulatorsPath = Settings.Default.SimulatorsPath;
            int adminType = Settings.Default.TypesOfAdministrationNumber;

            if (string.IsNullOrWhiteSpace(simName) || simName.Contains("not found"))
            {
                CustomMessageBox.Show("Please enter a valid name for the simulator.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            if (string.IsNullOrWhiteSpace(simulatorPath) || !Directory.Exists(simulatorPath))
            {
                CustomMessageBox.Show("Active simulator path does not exist.", "Error", Application.Current.MainWindow, Brushes.Red);
                return;
            }

            var mainWin = Application.Current.MainWindow as Views.MainWindow;
            if (mainWin != null) mainWin.IsHitTestVisible = false;

            var statusWin = new StatusWindow { Owner = mainWin };
            statusWin.Show();

            await Task.Delay(100);

            try
            {
                await Task.Run(() =>
                {
                    string destSimFolder = Path.Combine(simulatorsPath, simName);

                    string[] gbFiles = Directory.GetFiles(simulatorPath, "*.gb", SearchOption.TopDirectoryOnly);
                    if (gbFiles.Length > 0)
                    {
                        File.SetLastWriteTime(gbFiles[0], DateTime.Now);
                    }

                    if (adminType == 0)
                    {
                        if (Directory.Exists(destSimFolder)) Directory.Delete(destSimFolder, true);
                        CopyDirectoryContents(simulatorPath, destSimFolder, true, mainWin, "USBDisk");

                        string activeUsbFolder = Path.Combine(simulatorPath, "USBDisk");
                        string destUsbFolder = Path.Combine(Settings.Default.USBDisksPath, simName);

                        if (Directory.Exists(activeUsbFolder))
                        {
                            if (Directory.Exists(destUsbFolder)) Directory.Delete(destUsbFolder, true);
                            CopyDirectoryContents(activeUsbFolder, destUsbFolder, true, mainWin);
                        }
                    }
                    else if (adminType == 1)
                    {
                        if (Directory.Exists(destSimFolder)) Directory.Delete(destSimFolder, true);
                        CopyDirectoryContents(simulatorPath, destSimFolder, true, mainWin, "USBDisk");
                    }
                });

                TriggerTabLoading();
                RestoreListFocus();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error saving changes: {ex.Message}", "Error", mainWin, Brushes.Red);
            }
            finally
            {
                statusWin.Close();
                if (mainWin != null) mainWin.IsHitTestVisible = true;
            }
        }

        private async void ExecuteSaveUSBDiskChanges(object parameter)
        {
            ExecuteRenameUSBDisk(null);

            int adminType = Settings.Default.TypesOfAdministrationNumber;
            if (adminType == 0) return;

            string simName = ActiveSimulatorName?.Trim();
            string usbName = ActiveUSBDiskName?.Trim();
            string simulatorPath = Settings.Default.SimulatorPath;

            if (string.IsNullOrWhiteSpace(usbName) || usbName.Contains("not found"))
            {
                CustomMessageBox.Show("Please enter a valid name for the USB Disk.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            if (string.IsNullOrWhiteSpace(simulatorPath) || !Directory.Exists(simulatorPath))
            {
                CustomMessageBox.Show("Active simulator path does not exist.", "Error", Application.Current.MainWindow, Brushes.Red);
                return;
            }

            var mainWin = Application.Current.MainWindow as Views.MainWindow;
            bool saveSimulatorAsWell = CustomMessageBoxYesNo.Show(
                "Do you also want to save the Simulator changes?",
                "Save options",
                mainWin,
                Brushes.White);

            if (saveSimulatorAsWell && (string.IsNullOrWhiteSpace(simName) || simName.Contains("not found")))
            {
                CustomMessageBox.Show("Cannot save simulator: Please enter a valid name for the simulator first.", "Notice", mainWin, Brushes.Yellow);
                return;
            }

            if (mainWin != null) mainWin.IsHitTestVisible = false;

            var statusWin = new StatusWindow { Owner = mainWin };
            statusWin.Show();

            await Task.Delay(100);

            try
            {
                await Task.Run(() =>
                {
                    string activeUsbFolder = Path.Combine(simulatorPath, "USBDisk");
                    string destUsbFolder = Path.Combine(Settings.Default.USBDisksPath, usbName);

                    if (Directory.Exists(activeUsbFolder))
                    {
                        if (Directory.Exists(destUsbFolder)) Directory.Delete(destUsbFolder, true);
                        CopyDirectoryContents(activeUsbFolder, destUsbFolder, true, mainWin);
                    }

                    if (saveSimulatorAsWell)
                    {
                        string destSimFolder = Path.Combine(Settings.Default.SimulatorsPath, simName);

                        string[] gbFiles = Directory.GetFiles(simulatorPath, "*.gb", SearchOption.TopDirectoryOnly);
                        if (gbFiles.Length > 0)
                        {
                            File.SetLastWriteTime(gbFiles[0], DateTime.Now);
                        }

                        if (Directory.Exists(destSimFolder)) Directory.Delete(destSimFolder, true);
                        CopyDirectoryContents(simulatorPath, destSimFolder, true, mainWin, "USBDisk");
                    }
                });

                TriggerTabLoading();
                RestoreListFocus();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error saving changes: {ex.Message}", "Error", mainWin, Brushes.Red);
            }
            finally
            {
                statusWin.Close();
                if (mainWin != null) mainWin.IsHitTestVisible = true;
            }
        }

        private async void ExecuteCopySelectedToFromBackups(object parameter)
        {
            var ordnerNamenZumKopieren = new List<string>();
            string quellBasisPfad = string.Empty;
            string zielBasisPfad = string.Empty;

            // --- NEU: Admin-Typ und zusätzliche USB-Pfade für Typ 0 vorbereiten ---
            int adminType = Settings.Default.TypesOfAdministrationNumber;
            string usbQuellPfad = string.Empty;
            string usbZielPfad = string.Empty;

            switch (SelectedTabIndex)
            {
                case 0: // Simulator -> Backup
                    quellBasisPfad = Settings.Default.SimulatorsPath;
                    zielBasisPfad = Settings.Default.SimulatorBackupsPath;

                    // NEU: Wenn Typ 0, sichern wir die USB-Disk parallel ins USB-Backup!
                    if (adminType == 0)
                    {
                        usbQuellPfad = Settings.Default.USBDisksPath;
                        usbZielPfad = Settings.Default.USBDiskBackupsPath;
                    }

                    if (SimulatorsVM?.SimulatorsList != null)
                    {
                        ordnerNamenZumKopieren.AddRange(SimulatorsVM.SimulatorsList.Where(i => i.IsChecked).Select(i => i.Name));
                        if (ordnerNamenZumKopieren.Count == 0 && SimulatorsVM.SelectedSimulator != null)
                        {
                            ordnerNamenZumKopieren.Add(SimulatorsVM.SelectedSimulator.Name);
                        }
                    }
                    break;

                case 1: // USB -> Backup (Nur sichtbar bei Typ 1 und 2)
                    quellBasisPfad = Settings.Default.USBDisksPath;
                    zielBasisPfad = Settings.Default.USBDiskBackupsPath;
                    if (USBDisksVM?.USBDisksList != null)
                    {
                        ordnerNamenZumKopieren.AddRange(USBDisksVM.USBDisksList.Where(i => i.IsChecked).Select(i => i.Name));
                        if (ordnerNamenZumKopieren.Count == 0 && USBDisksVM.SelectedUSBDisk != null)
                        {
                            ordnerNamenZumKopieren.Add(USBDisksVM.SelectedUSBDisk.Name);
                        }
                    }
                    break;

                case 2: // Backup -> Simulator (Wiederherstellen)
                    quellBasisPfad = Settings.Default.SimulatorBackupsPath;
                    zielBasisPfad = Settings.Default.SimulatorsPath;

                    // NEU: Wenn Typ 0, stellen wir die USB-Disk parallel aus dem USB-Backup wieder her!
                    if (adminType == 0)
                    {
                        usbQuellPfad = Settings.Default.USBDiskBackupsPath;
                        usbZielPfad = Settings.Default.USBDisksPath;
                    }

                    if (SimulatorBackupsVM?.SimulatorBackupsList != null)
                    {
                        ordnerNamenZumKopieren.AddRange(SimulatorBackupsVM.SimulatorBackupsList.Where(i => i.IsChecked).Select(i => i.Name));
                        if (ordnerNamenZumKopieren.Count == 0 && SimulatorBackupsVM.SelectedSimulatorBackups != null)
                        {
                            ordnerNamenZumKopieren.Add(SimulatorBackupsVM.SelectedSimulatorBackups.Name);
                        }
                    }
                    break;

                case 3: // USB Backup -> USB (Nur sichtbar bei Typ 1 und 2)
                    quellBasisPfad = Settings.Default.USBDiskBackupsPath;
                    zielBasisPfad = Settings.Default.USBDisksPath;
                    if (USBDiskBackupsVM?.USBDiskBackupsList != null)
                    {
                        ordnerNamenZumKopieren.AddRange(USBDiskBackupsVM.USBDiskBackupsList.Where(i => i.IsChecked).Select(i => i.Name));
                        if (ordnerNamenZumKopieren.Count == 0 && USBDiskBackupsVM.SelectedUSBDiskBackups != null)
                        {
                            ordnerNamenZumKopieren.Add(USBDiskBackupsVM.SelectedUSBDiskBackups.Name);
                        }
                    }
                    break;
            }

            if (ordnerNamenZumKopieren.Count == 0)
            {
                CustomMessageBox.Show("Please check at least one item or select a row first.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            var mainWin = Application.Current.MainWindow as Views.MainWindow;
            if (mainWin != null) mainWin.IsHitTestVisible = false;

            var statusWin = new StatusWindow { Owner = mainWin };
            statusWin.Show();

            await Task.Delay(100);

            try
            {
                await Task.Run(() =>
                {
                    foreach (string name in ordnerNamenZumKopieren)
                    {
                        // ==========================================
                        // 1. HAUPT-KOPIERVORGANG (Simulator / USB)
                        // ==========================================
                        string quellOrdner = Path.Combine(quellBasisPfad, name);
                        string zielOrdner = Path.Combine(zielBasisPfad, name);

                        if (Directory.Exists(quellOrdner))
                        {
                            if (Directory.Exists(zielOrdner))
                            {
                                Directory.Delete(zielOrdner, true);
                            }

                            CopyDirectoryContents(quellOrdner, zielOrdner, true, mainWin);

                            // ==========================================
                            // 2. NEU: USB-DISK PARALLEL MITZIEHEN (Typ 0)
                            // ==========================================
                            // Wenn wir uns im Modus 0 befinden und die USB-Pfade gefüllt sind:
                            if (adminType == 0 && !string.IsNullOrEmpty(usbQuellPfad) && !string.IsNullOrEmpty(usbZielPfad))
                            {
                                string usbQuellOrdner = Path.Combine(usbQuellPfad, name);
                                string usbZielOrdner = Path.Combine(usbZielPfad, name);

                                // Nur kopieren, wenn für diesen Simulator überhaupt ein USB-Ordner existiert
                                if (Directory.Exists(usbQuellOrdner))
                                {
                                    if (Directory.Exists(usbZielOrdner))
                                    {
                                        Directory.Delete(usbZielOrdner, true);
                                    }

                                    CopyDirectoryContents(usbQuellOrdner, usbZielOrdner, true, mainWin);

                                    // Wenn von Aktiv nach Backup kopiert wird (Tab 0) und "Delete After Copy" an ist:
                                    if (SelectedTabIndex == 0 && IsDeleteAfterCopyEnabled)
                                    {
                                        Directory.Delete(usbQuellOrdner, true);
                                    }
                                }
                            }

                            // ==========================================
                            // 3. ORIGINAL LÖSCHEN ("Delete After Copy")
                            // ==========================================
                            if ((SelectedTabIndex == 0 || SelectedTabIndex == 1) && IsDeleteAfterCopyEnabled)
                            {
                                Directory.Delete(quellOrdner, true);
                            }
                        }
                    }
                });
                if ((SelectedTabIndex == 0 || SelectedTabIndex == 1) && IsDeleteAfterCopyEnabled)
                {
                    TriggerTabLoading();
                    RestoreListFocus();
                }
                
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error during copying: {ex.Message}", "Error", mainWin, Brushes.Red);
            }
            finally
            {
                statusWin.Close();
                if (mainWin != null) mainWin.IsHitTestVisible = true;
            }
        }

        private void ExecuteToggleDeleteAfterCopy(object parameter)
        {
            IsDeleteAfterCopyEnabled = !IsDeleteAfterCopyEnabled;
        }

        private async void ExecuteDeleteSelected(object parameter)
        {
            var ordnerZumLoeschen = new List<string>();
            var usbOrdnerZumLoeschenModus1 = new List<string>(); // Für bestätigte Modus-1 USB-Disks/Backups
            string aktuelleListePfad = string.Empty;

            int adminType = Settings.Default.TypesOfAdministrationNumber;
            string usbBasisPfadZumLoeschen = string.Empty;

            switch (SelectedTabIndex)
            {
                case 0: // Simulators
                    aktuelleListePfad = Settings.Default.SimulatorsPath;
                    if (adminType == 0)
                    {
                        usbBasisPfadZumLoeschen = Settings.Default.USBDisksPath;
                    }

                    if (SimulatorsVM?.SimulatorsList != null)
                    {
                        ordnerZumLoeschen.AddRange(SimulatorsVM.SimulatorsList.Where(i => i.IsChecked).Select(i => i.FolderPath));
                        if (ordnerZumLoeschen.Count == 0 && SimulatorsVM.SelectedSimulator != null)
                        {
                            ordnerZumLoeschen.Add(Path.Combine(aktuelleListePfad, SimulatorsVM.SelectedSimulator.Name));
                        }
                    }
                    break;

                case 1: // USB Disks
                    aktuelleListePfad = Settings.Default.USBDisksPath;
                    if (USBDisksVM?.USBDisksList != null)
                    {
                        ordnerZumLoeschen.AddRange(USBDisksVM.USBDisksList.Where(i => i.IsChecked).Select(i => i.FolderPath));
                        if (ordnerZumLoeschen.Count == 0 && USBDisksVM.SelectedUSBDisk != null)
                        {
                            ordnerZumLoeschen.Add(Path.Combine(aktuelleListePfad, USBDisksVM.SelectedUSBDisk.Name));
                        }
                    }
                    break;

                case 2: // Simulator Backups
                    aktuelleListePfad = Settings.Default.SimulatorBackupsPath;
                    if (adminType == 0)
                    {
                        usbBasisPfadZumLoeschen = Settings.Default.USBDiskBackupsPath;
                    }

                    if (SimulatorBackupsVM?.SimulatorBackupsList != null)
                    {
                        ordnerZumLoeschen.AddRange(SimulatorBackupsVM.SimulatorBackupsList.Where(i => i.IsChecked).Select(i => i.FolderPath));
                        if (ordnerZumLoeschen.Count == 0 && SimulatorBackupsVM.SelectedSimulatorBackups != null)
                        {
                            ordnerZumLoeschen.Add(Path.Combine(aktuelleListePfad, SimulatorBackupsVM.SelectedSimulatorBackups.Name));
                        }
                    }
                    break;

                case 3: // USB Disk Backups
                    aktuelleListePfad = Settings.Default.USBDiskBackupsPath;
                    if (USBDiskBackupsVM?.USBDiskBackupsList != null)
                    {
                        ordnerZumLoeschen.AddRange(USBDiskBackupsVM.USBDiskBackupsList.Where(i => i.IsChecked).Select(i => i.FolderPath));
                        if (ordnerZumLoeschen.Count == 0 && USBDiskBackupsVM.SelectedUSBDiskBackups != null)
                        {
                            ordnerZumLoeschen.Add(Path.Combine(aktuelleListePfad, USBDiskBackupsVM.SelectedUSBDiskBackups.Name));
                        }
                    }
                    break;
            }

            if (ordnerZumLoeschen.Count == 0)
            {
                CustomMessageBox.Show("Please select an entry from the list first.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
                return;
            }

            var mainWin = Application.Current.MainWindow as Views.MainWindow;

            // ====================================================================
            // === NEU: VORAB-SCAN FÜR MODUS 1 (Aktive Disks UND Backups!) ===
            // ====================================================================
            // Greift jetzt sowohl beim Simulatoren-Tab (0) als auch beim Backup-Tab (2)
            if ((SelectedTabIndex == 0 || SelectedTabIndex == 2) && adminType == 1)
            {
                var gefundeneUsbDiskNamen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 1. Weiche: Wo liegt der Ziel-Ordner für die USB-Disks?
                string usbZielPfadModus1 = (SelectedTabIndex == 0)
                    ? Settings.Default.USBDisksPath
                    : Settings.Default.USBDiskBackupsPath;

                // 2. Weiche: Wie lautet der saubere Name im UI-Dialog?
                string uiWording = (SelectedTabIndex == 0)
                    ? "USB Disk(s)"
                    : "USB Disk Backup(s)";

                foreach (string simPath in ordnerZumLoeschen)
                {
                    if (Directory.Exists(simPath))
                    {
                        var gbFiles = Directory.GetFiles(simPath, "*.gb");
                        if (gbFiles.Length > 0)
                        {
                            string usbName = File.ReadAllText(gbFiles[0]).Trim();
                            if (!string.IsNullOrWhiteSpace(usbName) && !usbName.Contains("not found"))
                            {
                                gefundeneUsbDiskNamen.Add(usbName);
                            }
                        }
                    }
                }

                // Wenn wir verknüpfte USB-Disks oder Backups gefunden haben, fragen wir den User!
                if (gefundeneUsbDiskNamen.Count > 0)
                {
                    string diskNamesText = string.Join(", ", gefundeneUsbDiskNamen);
                    bool deleteUsbAsWell = CustomMessageBoxYesNo.Show(
                        $"Do you also want to delete the associated {uiWording} ({diskNamesText})?",
                        $"WARNING: Other simulators sharing these {uiWording} will lose their connection/data!",
                        mainWin,
                        Brushes.White);

                    if (deleteUsbAsWell)
                    {
                        foreach (string usbName in gefundeneUsbDiskNamen)
                        {
                            // Fügt dynamisch entweder den aktiven USB-Pfad oder den Backup-USB-Pfad hinzu!
                            usbOrdnerZumLoeschenModus1.Add(Path.Combine(usbZielPfadModus1, usbName));
                        }
                    }
                }
            }

            bool confirm = CustomMessageBoxYesNo.Show("Do you really want to delete the selected item(s)?", "Confirm deletion", mainWin, Brushes.Red);
            if (!confirm) return;

            if (mainWin != null) mainWin.IsHitTestVisible = false;

            var statusWin = new StatusWindow { Owner = mainWin };
            statusWin.Show();

            await Task.Delay(100);

            try
            {
                await Task.Run(() =>
                {
                    foreach (string folderPath in ordnerZumLoeschen)
                    {
                        // 1. Haupt-Ordner löschen
                        if (Directory.Exists(folderPath))
                        {
                            Directory.Delete(folderPath, true);
                        }

                        // 2. Modus 0: Automatisch spiegelbildlich löschen (Aktiv & Backup)
                        if (adminType == 0 && !string.IsNullOrEmpty(usbBasisPfadZumLoeschen))
                        {
                            string cleanPath = folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            string folderName = Path.GetFileName(cleanPath);

                            if (!string.IsNullOrEmpty(folderName))
                            {
                                string usbFolderPath = Path.Combine(usbBasisPfadZumLoeschen, folderName);
                                if (Directory.Exists(usbFolderPath))
                                {
                                    Directory.Delete(usbFolderPath, true);
                                }
                            }
                        }
                    }

                    // ====================================================================
                    // === 3. Bestätigte Modus-1 USB-Disks / Backups im Hintergrund löschen ===
                    // ====================================================================
                    foreach (string usbPath in usbOrdnerZumLoeschenModus1)
                    {
                        if (Directory.Exists(usbPath))
                        {
                            Directory.Delete(usbPath, true);
                        }
                    }
                });

                switch (SelectedTabIndex)
                {
                    case 0: SimulatorsVM.LoadListFromDirectory(aktuelleListePfad); break;
                    case 1: USBDisksVM.LoadListFromDirectory(aktuelleListePfad); break;
                    case 2: SimulatorBackupsVM.LoadListFromDirectory(aktuelleListePfad); break;
                    case 3: USBDiskBackupsVM.LoadListFromDirectory(aktuelleListePfad); break;
                }
                RestoreListFocus();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error during deletion: {ex.Message}", "Error", mainWin, Brushes.Red);
            }
            finally
            {
                statusWin.Close();
                if (mainWin != null) mainWin.IsHitTestVisible = true;
            }
        }

        private void ExecuteOpenItemFolder(object parameter)
        {
            string ordnerPfad = string.Empty;
            string markierterName = null;

            switch (SelectedTabIndex)
            {
                case 0:
                    ordnerPfad = Settings.Default.SimulatorsPath;
                    markierterName = SimulatorsVM.SelectedSimulator?.Name;
                    break;
                case 1:
                    ordnerPfad = Settings.Default.USBDisksPath;
                    markierterName = USBDisksVM.SelectedUSBDisk?.Name;
                    break;
                case 2:
                    ordnerPfad = Settings.Default.SimulatorBackupsPath;
                    markierterName = SimulatorBackupsVM.SelectedSimulatorBackups?.Name;
                    break;
                case 3:
                    ordnerPfad = Settings.Default.USBDiskBackupsPath;
                    markierterName = USBDiskBackupsVM.SelectedUSBDiskBackups?.Name;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(markierterName))
            {
                ordnerPfad = Path.Combine(ordnerPfad, markierterName);
            }

            if (Directory.Exists(ordnerPfad))
            {
                try
                {
                    Process.Start("explorer.exe", ordnerPfad);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Error opening folder: {ex.Message}", "Error", Application.Current.MainWindow, Brushes.Red);
                }
            }
            else
            {
                CustomMessageBox.Show("The folder does not exist or the path is empty.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
            }
        }

        private void ExecuteOpenActiveUSBDiskFolder(object parameter)
        {
            string ordnerPfad = Path.Combine(Settings.Default.SimulatorPath, "USBDisk");

            if (Directory.Exists(ordnerPfad))
            {
                try
                {
                    Process.Start("explorer.exe", ordnerPfad);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Error opening folder: {ex.Message}", "Error", Application.Current.MainWindow, Brushes.Red);
                }
            }
            else
            {
                CustomMessageBox.Show("The folder does not exist or the path is empty.", "Notice", Application.Current.MainWindow, Brushes.Yellow);
            }
        }

        private void UpdateLinkedUSBDiskName()
        {
            // Wenn wir auf Tab 1 (USB Disks) oder Tab 3 (USB Backups) sind, macht die Anzeige keinen Sinn
            if (SelectedTabIndex == 1 || SelectedTabIndex == 3)
            {
                LinkedUSBDiskName = "";
                return;
            }

            // Namen und Pfad des aktuell markierten Simulators ermitteln (Tab 0 oder Tab 2)
            string selectedSimName = (SelectedTabIndex == 0)
                ? SimulatorsVM?.SelectedSimulator?.Name
                : SimulatorBackupsVM?.SelectedSimulatorBackups?.Name;

            string basePath = (SelectedTabIndex == 0)
                ? Settings.Default.SimulatorsPath
                : Settings.Default.SimulatorBackupsPath;

            // Wenn nichts in der Liste markiert ist:
            if (string.IsNullOrWhiteSpace(selectedSimName) || string.IsNullOrWhiteSpace(basePath))
            {
                LinkedUSBDiskName = string.Empty;
                return;
            }

            int adminType = Settings.Default.TypesOfAdministrationNumber;

            //// === MODUS 0: 1:1 Verknüpfung ===
            //if (adminType == 0)
            //{
            //    // Der USB-Stick hat immer genau denselben Namen wie der Simulator
            //    LinkedUSBDiskName = selectedSimName;
            //    return;
            //}

            // === MODUS 2: Getrennte Verwaltung ===
            if (adminType == 2)
            {
                LinkedUSBDiskName = "- (Separated Mode) -";
                return;
            }

            // === MODUS 1: Die magische .gb-Datei auslesen ===
            if ((adminType == 1) || (adminType == 0))
            {
                try
                {
                    string simFolder = Path.Combine(basePath, selectedSimName);
                    if (Directory.Exists(simFolder))
                    {
                        var gbFiles = Directory.GetFiles(simFolder, "*.gb");
                        if (gbFiles.Length > 0)
                        {
                            string readUsbName = File.ReadAllText(gbFiles[0]).Trim();

                            // Prüfen, ob in der .gb-Datei überhaupt etwas drin steht
                            LinkedUSBDiskName = string.IsNullOrWhiteSpace(readUsbName)
                                ? "- (Empty .gb file!) -"
                                : readUsbName;
                            return;
                        }
                    }
                    LinkedUSBDiskName = "- (No .gb file found) -";
                }
                catch
                {
                    LinkedUSBDiskName = "- (Error reading link) -";
                }
            }
        }

        #endregion
    }
}
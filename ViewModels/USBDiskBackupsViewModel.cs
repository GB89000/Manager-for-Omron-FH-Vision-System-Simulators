using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using ManagerForOmronFHVisionSystemSimulators.Models;
using ManagerForOmronFHVisionSystemSimulators.Properties; // Wichtig für Settings.Default

namespace ManagerForOmronFHVisionSystemSimulators.ViewModels
{
    public class USBDiskBackupsViewModel : ObservableObject
    {
        // Die Liste, an die sich das XAML bindet
        public ObservableCollection<USBDiskBackupItem> USBDiskBackupsList { get; set; }
        private USBDiskBackupItem _selectedUSBDiskBackups;
        public USBDiskBackupItem SelectedUSBDiskBackups
        {
            get { return _selectedUSBDiskBackups; }
            set
            {
                if (_selectedUSBDiskBackups != value)
                {
                    _selectedUSBDiskBackups = value;
                    OnPropertyChanged(nameof(SelectedUSBDiskBackups));
                }
            }
        }

        private bool _isAllSelected;
        public bool IsAllSelected
        {
            get { return _isAllSelected; }
            set
            {
                if (_isAllSelected != value)
                {
                    _isAllSelected = value;
                    OnPropertyChanged(nameof(IsAllSelected));

                    // MAGIE: Wenn der Master-Haken gesetzt oder entfernt wird,
                    // übertragen wir das sofort auf alle Elemente in der Liste!
                    if (USBDiskBackupsList != null)
                    {
                        foreach (var item in USBDiskBackupsList)
                        {
                            item.IsChecked = value;
                        }
                    }
                }
            }
        }

        // 1. Variablen, um sich die letzte Sortierung zu merken
        private string _lastSortBy = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        public ICommand SortCommand { get; }

        public USBDiskBackupsViewModel() // Konstruktor
        {
            // Liste initialisieren (noch leer)
            USBDiskBackupsList = new ObservableCollection<USBDiskBackupItem>();

            SortCommand = new RelayCommand(ExecuteSort);
        }

        // --- HIER IST DEINE ALTE METHODE ---
        public bool LoadListFromDirectory(string rootFolderPath)
        {
            // 1. Prüfen, ob der Hauptordner überhaupt existiert
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
            {
                // this durch Application.Current.MainWindow ersetzt
                CustomMessageBox.Show($"The specified main folder does not exist.", "Error", Application.Current.MainWindow, Brushes.Red);
                return false;
            }

            // 2. Alte Liste leeren, bevor wir neu laden
            USBDiskBackupsList.Clear();

            try
            {
                // 3. Unterordner abrufen
                string[] unterordner = Directory.GetDirectories(rootFolderPath);

                foreach (string ordner in unterordner)
                {
                    // Nach .gb Dateien suchen
                    string[] gbFiles = Directory.GetFiles(ordner, "*.gb", SearchOption.TopDirectoryOnly);

                    foreach (string filePath in gbFiles)
                    {
                        string folderPath = Path.GetDirectoryName(filePath);
                        string fileName = Path.GetFileNameWithoutExtension(filePath);

                        string simulatorName = "Unknown";
                        DateTime backupDate = DateTime.MinValue;

                        if (fileName.Length >= 1)
                        {
                            simulatorName = fileName;
                            backupDate = File.GetCreationTime(filePath);
                        }

                        // 4. Das fertige Objekt zur Liste hinzufügen
                        USBDiskBackupsList.Add(new USBDiskBackupItem
                        {
                            IsChecked = false,
                            Name = simulatorName,
                            Date = backupDate,
                            FolderPath = folderPath
                        });
                    }
                }
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                CustomMessageBox.Show($"Access to a folder is denied. You may lack administrator rights.", "Error", Application.Current.MainWindow, Brushes.Red);
                return false;
            }
        }

        private void ExecuteSort(object parameter)
        {
            // Der Parameter ist z.B. "Name" oder "Date" (kommt gleich aus dem XAML)
            string sortBy = parameter as string;
            if (string.IsNullOrEmpty(sortBy)) return;

            ListSortDirection direction;

            // Klick auf eine NEUE Spalte -> aufsteigend
            if (sortBy != _lastSortBy)
            {
                direction = ListSortDirection.Ascending;
            }
            // Klick auf die SELBE Spalte -> Richtung umkehren
            else
            {
                direction = _lastDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            // Die Standard-Ansicht der Liste holen
            ICollectionView dataView = CollectionViewSource.GetDefaultView(USBDiskBackupsList);

            if (dataView != null)
            {
                // Alte Sortierungen löschen und neue hinzufügen
                dataView.SortDescriptions.Clear();
                dataView.SortDescriptions.Add(new SortDescription(sortBy, direction));
                dataView.Refresh();
            }

            // Für den nächsten Klick merken
            _lastSortBy = sortBy;
            _lastDirection = direction;
        }


    }
}
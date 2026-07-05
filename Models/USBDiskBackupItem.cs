using System;
using ManagerForOmronFHVisionSystemSimulators.ViewModels; // Damit ObservableObject gefunden wird

namespace ManagerForOmronFHVisionSystemSimulators.Models
{
    // Die Klasse erbt von ObservableObject, damit die UI bei Änderungen aktualisiert wird
    public class USBDiskBackupItem : ObservableObject
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(); // Meldet der UI: "Hallo, der Haken wurde gesetzt/entfernt!"
                }
            }
        }

        public DateTime Date { get; set; }
        public string Name { get; set; }

        // Der unsichtbare Pfad für spätere Aktionen (z.B. Restore)
        public string FolderPath { get; set; }
    }
}
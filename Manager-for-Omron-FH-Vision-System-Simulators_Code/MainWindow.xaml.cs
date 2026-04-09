using Microsoft.Win32;
using NamespaceSettingsXAML;
using ManagerForOmronFHVisionSystemSimulators;
using ManagerForOmronFHVisionSystemSimulators.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static NamespaceManagerForOmronFHVisionSystemSimulatorsXAML.MainWindow;
using static ManagerForOmronFHVisionSystemSimulators.FolderHelperNamespace.FolderHelper;
using static ManagerForOmronFHVisionSystemSimulators.StyleAndGrafics.ButtonColors;

namespace NamespaceManagerForOmronFHVisionSystemSimulatorsXAML
{
    public partial class MainWindow : Window
    {
        // Diese Liste hält alle deine Einträge
        private ObservableCollection<SimulatorItem> _simulatorListe;
        public string aktiveListPath;
        public string targetPath;
        private System.Windows.Controls.GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();
            ResizeWindow();
            ListOfSimulators.Background = ButtonGreen();
            List.Content = "List of Simulators";

            // 1. Liste initialisieren und mit Dummy-Daten füllen
            _simulatorListe = new ObservableCollection<SimulatorItem>
            {
            //new SimulatorItem { IsChecked = false, Date = DateTime.Now, Name = "Omron_Line_1_Backup" },
            //new SimulatorItem { IsChecked = false, Date = DateTime.Now.AddDays(-2), Name = "Roboter_Cell_A" },
            //new SimulatorItem { IsChecked = true,  Date = DateTime.Now.AddDays(-5), Name = "Main_Conveyor_Sysmac" }
            };

            // 2. Die Liste an die ListView in der UI binden
            SimulatorListView.ItemsSource = _simulatorListe;
            // 3. Liste mit echten Daten aus dem Simulators-Ordner füllen
            aktiveListPath = Settings.Default.SimulatorsPath;
            targetPath = Settings.Default.SimulatorBackupsPath;

            if (Settings.Default.DeleteOnOff)
            {
                DeleteOnOffAfterCopy.Background = ButtonGreen();
                DeleteOnOffAfterCopy.Content = "Delete after\n Copy: ON";
            }
            else
            {
                DeleteOnOffAfterCopy.Background = ButtonRed();
                DeleteOnOffAfterCopy.Content = "Delete after\n Copy: OFF";
            }
        }

        void MainWindow_Initialized(object sender, EventArgs e)
        {
            // Initialisierungscode hier vom WPF Fenster, brauche ich das???
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Hier ist das Fenster SICHTBAR und bereit für MessageBoxen!
            try
            {
                CheckForSimulatorFile();
                LoadSimulatorsListFromDirectory(Settings.Default.SimulatorsPath);
            }
            catch (Exception ex)
            {
                // Jetzt funktioniert "this" als Owner ohne Probleme
                CustomMessageBox.Show($"Initialisierungsfehler: {ex.Message}", "Fehler", this, Brushes.Red);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            // 1. F2 -> Fokus auf die TextBox
            if (e.Key == Key.F2)
            {
                ActiveSimulatorText.Focus();
                ActiveSimulatorText.SelectAll();
                e.Handled = true;
                return; // Direkt beenden, wenn Taste verarbeitet
            }

            // 2. STRG + S -> Speichern
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SaveChanges_Click(this, new RoutedEventArgs());
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter && ActiveSimulatorText.IsFocused)
            {
                RenameSimulator_Click(this, new RoutedEventArgs());

                // Wir schieben den Fokus auf das unsichtbare Element
                FocusDummy.Focus();

                e.Handled = true;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new ProgrammSettings(this);
            //settingsWindow.Show();
            bool? result = settingsWindow.ShowDialog();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void LoadSimulator_Click(object sender, RoutedEventArgs e)
        {
            if (SimulatorListView.SelectedItem is SimulatorItem markierterEintrag)
            {
                // SPERREN: Das ganze Fenster wird für Klicks gesperrt
                this.IsHitTestVisible = false;

                var statusWin = new StatusWindow { Owner = this };
                statusWin.Show();

                await Task.Delay(100);

                try
                {
                    string stringSimulatorPath = Settings.Default.SimulatorPath;
                    string destinationFolder = Path.Combine(aktiveListPath, markierterEintrag.Name);

                    await Task.Run(() =>
                    {
                        if (Directory.Exists(stringSimulatorPath))
                        {
                            EmptyDirectory(stringSimulatorPath, this);
                        }
                        CopyDirectoryContents(destinationFolder, stringSimulatorPath, true);
                    });

                    CheckForSimulatorFile();
                    //MessageBox.Show("Erfolgreich kopiert!", "Info");
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Error: {ex.Message}", "Error", this, Brushes.Red);
                }
                finally
                {
                    // FREIGEBEN: Egal was passiert, Fenster und Tasten gehen wieder
                    statusWin.Close();
                    this.IsHitTestVisible = true;
                }
            }
            else
            {
                CustomMessageBox.Show("Please click on a line first.", "Notice", this, Brushes.Yellow);
            }
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            RenameSimulatorM();
            // 1. Fenster sperren & Status anzeigen
            this.IsHitTestVisible = false;
            var statusWin = new StatusWindow { Owner = this };
            statusWin.Show();

            await Task.Delay(100);


            try
            {
                string stringSimulatorsPath = Settings.Default.SimulatorsPath;
                string stringSimulatorPath = Settings.Default.SimulatorPath;
                string fileNameOnly = ActiveSimulatorText.Text;
                string destinationFolder = Path.Combine(stringSimulatorsPath, fileNameOnly);


                // 1. Validierung: Ist der Pfad gesetzt und der Name gültig?
                if (string.IsNullOrWhiteSpace(stringSimulatorPath) || !Directory.Exists(stringSimulatorPath))
                {
                    return;
                }

                bool deleteSuccessful = true;
                // 2. Die schwere Arbeit in den Hintergrund!
                await Task.Run(() =>
                {

                    try
                    {
                        if (Directory.Exists(destinationFolder))
                        {
                            Directory.Delete(destinationFolder, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        deleteSuccessful = false;
                        // Wir schicken den UI-Befehl zurück an den Haupt-Thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CustomMessageBox.Show($"Error deleting folder: {ex.Message}", "Error", this, Brushes.Red);
                        });
                    }


                    if(deleteSuccessful)
                    {
                        string[] files = Directory.GetFiles(stringSimulatorPath, "*.gb", SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            File.SetLastWriteTime(files[0], DateTime.Now);
                        }

                        CopyDirectory(stringSimulatorPath, destinationFolder, true);
                    }
                });

                // 3. UI Update (Zurück im Haupt-Thread)
                LoadSimulatorsListFromDirectory(stringSimulatorsPath);
                //MessageBox.Show("Änderungen erfolgreich gespeichert!", "Info");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error saving: {ex.Message}", "Error", this, Brushes.Red);
            }
            finally
            {
                // 4. Alles wieder freigeben
                statusWin.Close();
                this.IsHitTestVisible = true;
            }
        }

        private void ListOfSimulators_Click(object sender, RoutedEventArgs e)
        {
            // Liste laden und grün markieren, wenn erfolgreich, ansonsten rot
            if (LoadSimulatorsListFromDirectory(Settings.Default.SimulatorsPath))
            {
                ListOfSimulators.Background = ButtonGreen();
                ListOfSimulatorBackups.Background = ButtonRed();
                aktiveListPath = Settings.Default.SimulatorsPath;
                targetPath = Settings.Default.SimulatorBackupsPath;
                MoveSelectedToXXX.Content = "Copy Selected\n   to Backups";
                List.Content = "List of Simulators";
            }
        }

        private void ListOfSimulatorBackups_Click(object sender, RoutedEventArgs e)
        {
            // Liste laden und grün markieren, wenn erfolgreich, ansonsten rot
            if (LoadSimulatorsListFromDirectory(Settings.Default.SimulatorBackupsPath))
            {
                ListOfSimulatorBackups.Background = ButtonGreen();
                ListOfSimulators.Background = ButtonRed();
                aktiveListPath = Settings.Default.SimulatorBackupsPath;
                targetPath = Settings.Default.SimulatorsPath;
                MoveSelectedToXXX.Content = "Copy Selected\n from Backups";
                List.Content = "List of Simulator Backups";
            }
        }


        private async void DeleteSelectedSimulatorsButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Vorab-Check: Werden Daten zum Löschen gefunden?
            var alleAusgewaehlten = _simulatorListe.Where(item => item.IsChecked).ToList();
            var markierterEintrag = SimulatorListView.SelectedItem as SimulatorItem;

            if (alleAusgewaehlten.Count == 0 && markierterEintrag == null)
            {
                CustomMessageBox.Show("Please select an entry first.", "Notice", this, Brushes.Yellow);
                return;
            }

            // Sicherheitsabfrage vor dem Löschen (optional, aber empfohlen)
            bool result = CustomMessageBoxYesNo.Show("Do you really want to delete this simulator?", "Confirm deletion", this, Brushes.Red);

            if (result)
            {
                // 2. UI SPERREN & STATUS ANZEIGEN
                this.IsHitTestVisible = false;
                var statusWin = new StatusWindow { Owner = this };
                statusWin.Show();

                await Task.Delay(100);

                try
                {
                    // 3. LÖSCHVORGANG IM HINTERGRUND
                    await Task.Run(() =>
                    {
                        if (alleAusgewaehlten.Count > 0)
                        {
                            // Fall A: Alle angehakten Checkboxen löschen
                            foreach (var item in alleAusgewaehlten)
                            {
                                if (Directory.Exists(item.FolderPath))
                                {
                                    Directory.Delete(item.FolderPath, true);
                                }
                            }
                        }
                        else if (markierterEintrag != null)
                        {
                            // Fall B: Nur den markierten Eintrag löschen
                            string destinationFolder = Path.Combine(aktiveListPath, markierterEintrag.Name);
                            if (Directory.Exists(destinationFolder))
                            {
                                Directory.Delete(destinationFolder, true);
                            }
                        }
                    });

                    // 4. UI AKTUALISIEREN (nach dem Löschen)
                    CheckForSimulatorFile();
                    LoadSimulatorsListFromDirectory(aktiveListPath);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Error during deletion: {ex.Message}", "Error", this, Brushes.Red);
                }
                finally
                {
                    // 5. IMMER FREIGEBEN
                    statusWin.Close();
                    this.IsHitTestVisible = true;
                }
            }
            else
            {
                return;
            }
        }

        private async void MoveSelectedToXXX_Click(object sender, RoutedEventArgs e)
        {
            // 1. Vorab prüfen: Gibt es überhaupt etwas zu tun?
            var alleAusgewaehlten = _simulatorListe.Where(item => item.IsChecked).ToList();
            var markierterEintrag = SimulatorListView.SelectedItem as SimulatorItem;

            if (alleAusgewaehlten.Count == 0 && markierterEintrag == null)
            {
                CustomMessageBox.Show("Please select at least one simulator (checkbox) or click on a row.", "Notice", this, Brushes.Yellow);
                return;
            }

            // 2. SPERREN & STATUS
            this.IsHitTestVisible = false;
            var statusWin = new StatusWindow { Owner = this };
            statusWin.Show();

            await Task.Delay(100); // UI Zeit zum Atmen geben

            try
            {
                // 3. Die schwere Arbeit in den Hintergrund verschieben
                await Task.Run(() =>
                {
                    // Priorität 1: Alle mit Checkbox verschieben
                    if (alleAusgewaehlten.Count > 0)
                    {
                        MoveDirectorys(targetPath, alleAusgewaehlten);
                    }
                    // Priorität 2: Wenn keine Checkbox, dann den markierten Eintrag
                    else if (markierterEintrag != null)
                    {
                        MoveDirectory(targetPath, markierterEintrag.Name);
                    }
                });

                // 4. Liste nach der Arbeit aktualisieren
                LoadSimulatorsListFromDirectory(aktiveListPath);
                //MessageBox.Show("Vorgang erfolgreich abgeschlossen.", "Info");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error during moving: {ex.Message}", "Error", this, Brushes.Red);
            }
            finally
            {
                // 5. IMMER FREIGEBEN
                statusWin.Close();
                this.IsHitTestVisible = true;
            }
        }

        
        private void DeleteOnOffAfterCopy_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.DeleteOnOff)
            {
                Settings.Default.DeleteOnOff = false;
                DeleteOnOffAfterCopy.Background = ButtonRed();
                DeleteOnOffAfterCopy.Content = "Delete after\n Copy: OFF";
            }
            else
            {
                Settings.Default.DeleteOnOff = true;
                DeleteOnOffAfterCopy.Background = ButtonGreen();
                DeleteOnOffAfterCopy.Content = "Delete after\n Copy: ON";
            }
            Settings.Default.Save();
        }

        private void RenameSimulator_Click(object sender, RoutedEventArgs e)
        {
            RenameSimulatorM();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Prüfen, ob der Auslöser (sender) eine CheckBox ist
            if (sender is System.Windows.Controls.CheckBox chk && chk.IsChecked.HasValue)
            {
                // 2. Den aktuellen Status der "Alle auswählen"-CheckBox auslesen (true oder false)
                bool isChecked = chk.IsChecked.Value;

                // 3. Durch alle Elemente in deiner Liste gehen und den Status übertragen
                if (_simulatorListe != null)
                {
                    foreach (var item in _simulatorListe)
                    {
                        item.IsChecked = isChecked;
                    }
                }
            }
        }


        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            // Herausfinden, auf welchen Header geklickt wurde
            var headerClicked = e.OriginalSource as System.Windows.Controls.GridViewColumnHeader;

            // Wenn ins Leere geklickt wurde oder es die Spalte mit der CheckBox ist (da der Header hier kein Text ist), abbrechen
            if (headerClicked == null || headerClicked.Column == null || !(headerClicked.Column.Header is string headerString))
            {
                return;
            }

            // Die Eigenschaft, nach der sortiert werden soll (entspricht dem Header-Text "Name" oder "Date")
            string sortBy = headerString;
            ListSortDirection direction;

            // Klick auf eine NEUE Spalte -> aufsteigend sortieren
            if (headerClicked != _lastHeaderClicked)
            {
                direction = ListSortDirection.Ascending;
            }
            // Klick auf die SELBE Spalte -> Richtung umkehren
            else
            {
                if (_lastDirection == ListSortDirection.Ascending)
                    direction = ListSortDirection.Descending;
                else
                    direction = ListSortDirection.Ascending;
            }

            // Sortierung anwenden
            SortList(sortBy, direction);

            // Variablen für den nächsten Klick merken
            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;
        }

        private void SortList(string sortBy, ListSortDirection direction)
        {
            // Die Standard-Ansicht der Liste holen
            System.ComponentModel.ICollectionView dataView =
                System.Windows.Data.CollectionViewSource.GetDefaultView(_simulatorListe);

            // Alte Sortierungen löschen und neue hinzufügen
            dataView.SortDescriptions.Clear();
            System.ComponentModel.SortDescription sd = new System.ComponentModel.SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);

            // Ansicht aktualisieren
            dataView.Refresh();
        }


        

        // Die Datenklasse repräsentiert exakt EINE Zeile in deiner Liste
        public class SimulatorItem : INotifyPropertyChanged
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
                        OnPropertyChanged(nameof(IsChecked));
                    }
                }
            }

            public DateTime Date { get; set; }
            public string Name { get; set; }

            // Der unsichtbare Pfad für spätere Aktionen(z.B.Restore)
            public string FolderPath { get; set; }

            // Standard-Event für WPF DataBinding
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool LoadSimulatorsListFromDirectory(string rootFolderPath)
        {
            // 1. Prüfen, ob der Hauptordner überhaupt existiert
            if (!Directory.Exists(rootFolderPath))
            {
                CustomMessageBox.Show($"The specified main folder does not exist.", "Error", this, Brushes.Red);
                return false;
            }

            // 2. Alte Liste leeren, bevor wir neu laden
            _simulatorListe.Clear();

            try
            {
                // 3. NEU: Wir holen uns NUR die direkten Unterordner im Hauptordner (Ebene 1)
                // SearchOption.TopDirectoryOnly ist hier der Standard, er geht also nicht tiefer.
                string[] unterordner = Directory.GetDirectories(rootFolderPath);

                // Wir gehen jeden Unterordner einzeln durch
                foreach (string ordner in unterordner)
                {
                    // Wir suchen NUR direkt in diesem Unterordner nach .gb Dateien
                    string[] gbFiles = Directory.GetFiles(ordner, "*.gb", SearchOption.TopDirectoryOnly);

                    foreach (string filePath in gbFiles)
                    {
                        // Pfad des Ordners extrahieren (das, was du für später brauchst)
                        string folderPath = Path.GetDirectoryName(filePath);

                        // Reinen Dateinamen ohne ".gb" extrahieren (z.B. "MeinSimulator_xxx_20231027")
                        string fileName = Path.GetFileNameWithoutExtension(filePath);

                        // --- DATEN EXTRAHIEREN ---
                        string simulatorName = "Unknown";
                        DateTime backupDate = DateTime.MinValue;

                        // Wir teilen den Namen am Unterstrich auf
                        string[] nameParts = fileName.Split('_');

                        if (nameParts.Length >= 3)
                        {
                            simulatorName = nameParts[0];
                            string dateString = nameParts[nameParts.Length - 1];

                            if (!DateTime.TryParse(dateString, out backupDate))
                            {
                                backupDate = File.GetCreationTime(filePath);
                            }
                        }
                        else
                        {
                            simulatorName = fileName;
                            backupDate = File.GetCreationTime(filePath);
                        }

                        // 4. Das fertige Objekt zur Liste hinzufügen
                        _simulatorListe.Add(new SimulatorItem
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
                CustomMessageBox.Show($"Access to a folder is denied. You may lack administrator rights.", "Error", this, Brushes.Red);
                return false;
            }
        }

        private void RenameSimulatorM()
        {
            string rootPath = Settings.Default.SimulatorPath;
            string newName = ActiveSimulatorText.Text.Trim();

            // 1. Validierung: Ist der Pfad gesetzt und der Name gültig?
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                CustomMessageBox.Show("The specified folder path does not exist.", "Notice", this, Brushes.Yellow);
                return;
            }

            if (string.IsNullOrWhiteSpace(newName) || newName.Contains("not found"))
            {
                CustomMessageBox.Show("Please enter a valid name for the simulator.", "Notice", this, Brushes.Yellow);
                return;
            }

            try
            {
                // 2. Suche nach einer existierenden .gb Datei (Top Level)
                var existingFiles = Directory.GetFiles(rootPath, "*.gb", SearchOption.TopDirectoryOnly);
                string newFilePath = Path.Combine(rootPath, newName + ".gb");

                if (existingFiles.Length > 0)
                {
                    // --- FALL A: DATEI EXISTIERT -> UMBENENNEN ---
                    string oldFilePath = existingFiles[0];

                    // Nur umbenennen, wenn der Name sich wirklich geändert hat
                    if (oldFilePath.ToLower() != newFilePath.ToLower())
                    {
                        File.Move(oldFilePath, newFilePath);
                        //MessageBox.Show($"Simulator erfolgreich von '{Path.GetFileName(oldFilePath)}' in '{newName}.gb' umbenannt.");
                    }
                }
                else
                {
                    // --- FALL B: KEINE DATEI DA -> NEU ERSTELLEN ---
                    // Wir erstellen eine leere Datei (0 Byte), damit das System etwas zum Arbeiten hat
                    using (FileStream fs = File.Create(newFilePath))
                    {
                        // Der 'using' Block sorgt dafür, dass die Datei sofort wieder 
                        // für andere Schreibvorgänge freigegeben wird.
                    }
                    //MessageBox.Show($"Neue Simulator-Datei '{newName}.gb' wurde erfolgreich erstellt.");
                }

                // 3. UI Aktualisieren
                ActiveSimulatorText.Foreground = Brushes.White;
                // Optional: Die Liste der Backups auch aktualisieren, falls nötig
                //LoadSimulatorsListFromDirectory(rootPath); // Brauche es bei einer anderen Taste, "Save Changes" oder so...????!!!
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error processing the file: {ex.Message}", "Error", this, Brushes.Red);
            }
        }



        private void MoveDirectorys(string targetPath, List<SimulatorItem> alleAusgewaehlten)
        {
            foreach (var item in alleAusgewaehlten)
            {
                string targetPathCombine = Path.Combine(targetPath, item.Name);
                try
                {
                    // PRÜFEN & LÖSCHEN
                    if (Directory.Exists(item.FolderPath))
                    {
                        // Delete the target folder if it already exists to avoid conflicts
                        if (Directory.Exists(targetPathCombine))
                        {
                            Directory.Delete(targetPathCombine, true);
                        }
                        // 4. DEN GANZEN ORDNER KOPIEREN
                        CopyDirectory(item.FolderPath, targetPathCombine, true);
                        // true bedeutet: Lösche auch alle Inhalte und Unterordner (rekursiv)
                        if(Settings.Default.DeleteOnOff)
                        {
                            Directory.Delete(item.FolderPath, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Fehler zurück an den UI-Thread schicken
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomMessageBox.Show($"Error deleting folder: {ex.Message}", "Error", this, Brushes.Red);
                    });
                }
            }
        }

        private void MoveDirectory(string targetPath, string markierterEintragName)
        {
            // 1. Pfade definieren
            string sourcePath = Path.Combine(aktiveListPath, markierterEintragName);
            string targetPathCombine = Path.Combine(targetPath, markierterEintragName);
            try
            {
                // PRÜFEN & LÖSCHEN
                if (Directory.Exists(sourcePath))
                {
                    // Delete the target folder if it already exists to avoid conflicts
                    if (Directory.Exists(targetPathCombine))
                    {
                        Directory.Delete(targetPathCombine, true);
                    }
                        // 4. DEN GANZEN ORDNER KOPIEREN
                        CopyDirectory(sourcePath, targetPathCombine, true);
                    // true bedeutet: Lösche auch alle Inhalte und Unterordner (rekursiv)
                    if (Settings.Default.DeleteOnOff)
                    {
                        Directory.Delete(sourcePath, true);
                    }
                }
                //CheckForSimulatorFile();
            }
            catch (Exception ex)
            {
                // Fehler zurück an den UI-Thread schicken
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CustomMessageBox.Show($"Error deleting folder: {ex.Message}", "Error", this, Brushes.Red);
                });
            }
        }

        private void CheckForSimulatorFile()
        {
            string rootPath = Settings.Default.SimulatorPath;

            try
            {
                if (!string.IsNullOrEmpty(rootPath) && Directory.Exists(rootPath))
                {
                    // TopDirectoryOnly stellt sicher, dass wir nur "oben" schauen
                    var files = Directory.GetFiles(rootPath, "*.gb", SearchOption.TopDirectoryOnly);

                    if (files.Length > 0)
                    {
                        // Die erste gefundene Datei nehmen
                        string fileNameOnly = Path.GetFileNameWithoutExtension(files[0]);
                        ActiveSimulatorText.Text = fileNameOnly;
                        ActiveSimulatorText.Foreground = Brushes.White;
                    }
                    else
                    {
                        SetWarningText();
                    }
                }
                else
                {
                    SetWarningText();
                }
            }
            catch (Exception)
            {
                SetWarningText();
            }
        }

        private void SetWarningText()
        {
            ActiveSimulatorText.Text = "File not found, enter text to create and press Rename";
            ActiveSimulatorText.Foreground = Brushes.Red; // Warnfarbe
        }

        private void ActiveSimulatorText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ActiveSimulatorText.Text.Contains("not found"))
            {
                ActiveSimulatorText.Text = "";
                ActiveSimulatorText.Foreground = Brushes.Black;
            }
        }

        private void ActiveSimulatorText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ActiveSimulatorText.Text))
            {
                SetWarningText();
            }
        }





        #region Fenstergröße anpassen  
        public void ResizeWindow()
        {
            // Fenstergröße anpassen  
            //this.Width = ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionWidth;
            //this.Height = ManagerForOmronFHVisionSystemSimulators.Properties.Settings.Default.ResolutionHeight;
            // Position des Fensters zentrieren  
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;

        }
        #endregion Fenstergröße anpassen
    }
}
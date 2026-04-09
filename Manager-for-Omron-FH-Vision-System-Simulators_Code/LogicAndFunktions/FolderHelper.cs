using System.IO;
using System.Windows.Media;
using System.Windows;

namespace ManagerForOmronFHVisionSystemSimulators.FolderHelperNamespace
{
    public class FolderHelper
    {
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Informationen über den Quellordner abrufen
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Quellordner nicht gefunden: {dir.FullName}");

            // Unterordner abrufen
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Zielordner erstellen
            Directory.CreateDirectory(destinationDir);

            // Dateien im aktuellen Ordner kopieren
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // Wenn rekursiv, dann auch alle Unterordner kopieren
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static void EmptyDirectory(string directoryPath, Window owner)
        {
            if (!Directory.Exists(directoryPath)) return;

            DirectoryInfo di = new DirectoryInfo(directoryPath);

            foreach (FileInfo file in di.GetFiles())
            {
                try { file.Delete(); }
                catch (IOException) { /* File in use */ }
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch (Exception ex)
                {
                    // Wir schicken den UI-Befehl zurück an den Haupt-Thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomMessageBox.Show($"Error deleting folder: {ex.Message}", "Error", owner, Brushes.Red);
                    });
                }
            }
        }

        public static void CopyDirectoryContents(string sourceDir, string destinationDir, bool recursive)
        {
            // Informationen über den Quellordner abrufen
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Quellordner nicht gefunden: {dir.FullName}");

            // Falls der Zielordner noch nicht existiert (nach dem Leeren), erstellen wir ihn
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // 1. Dateien direkt kopieren (ohne den Quellordner-Namen neu zu erstellen)
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true); // true = überschreiben, falls vorhanden
            }

            // 2. Wenn rekursiv, dann alle Unterordner kopieren
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    // WICHTIG: Wir hängen den Namen des Unterordners an das ZIEL an
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectoryContents(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}

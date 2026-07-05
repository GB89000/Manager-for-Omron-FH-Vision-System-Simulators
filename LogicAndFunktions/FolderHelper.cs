using System;
using System.IO;
using System.Windows.Media;
using System.Windows;

namespace ManagerForOmronFHVisionSystemSimulators.FolderHelperNamespace
{
    public class FolderHelper
    {
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Quellordner nicht gefunden: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static void EmptyDirectory(string directoryPath, Window owner, string folderToExclude = null)
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
                if (!string.IsNullOrEmpty(folderToExclude) &&
                    dir.Name.Equals(folderToExclude, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    dir.Delete(true);
                }
                catch (Exception ex)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomMessageBox.Show($"Error deleting folder: {ex.Message}", "Error", owner, Brushes.Red);
                    });
                }
            }
        }

        public static void CopyDirectoryContents(string sourceDir, string destinationDir, bool recursive, Window owner, string folderToExclude = null)
        {
            var dir = new DirectoryInfo(sourceDir);

            try
            {
                if (!dir.Exists)
                    throw new DirectoryNotFoundException($"{dir.FullName}");
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CustomMessageBox.Show($"Error not found folder: {ex.Message}", "Error", owner, Brushes.Red);
                });
                return;
            }

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // 1. Dateien direkt kopieren
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // 2. Wenn rekursiv, dann alle Unterordner kopieren
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    // MAGIE: Wenn dieser Unterordner ignoriert werden soll (z.B. "USBDisk"), überspringen!
                    if (!string.IsNullOrEmpty(folderToExclude) &&
                        subDir.Name.Equals(folderToExclude, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);

                    // WICHTIG: Den Parameter "folderToExclude" an den rekursiven Aufruf weiterreichen!
                    CopyDirectoryContents(subDir.FullName, newDestinationDir, true, owner, folderToExclude);
                }
            }
        }
    }
}
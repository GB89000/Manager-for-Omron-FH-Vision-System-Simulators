using System.Windows;
using System.Windows.Media;

namespace ManagerForOmronFHVisionSystemSimulators
{
    /// <summary>
    /// Interaktionslogik für CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        // Der Konstruktor bekommt jetzt optional eine Farbe
        public CustomMessageBox(string message, string title, Brush borderBrush = null)
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = title;

            // Wenn eine Farbe mitgegeben wurde, färben wir den Rahmen und den Titel
            if (borderBrush != null)
            {
                MessageBorder.BorderBrush = borderBrush;
                MessageBorder.Background = borderBrush;
                TitleText.Foreground = borderBrush;
            }
            // Setzt den Fokus direkt auf den Button, sobald das Fenster geladen ist
            this.Activated += (s, e) => {
                OK.Focus();
            };
        }



        public static void Show(string message, string title, Window owner, Brush color = null)
        {
            var msg = new CustomMessageBox(message, title, color);

            Window targetOwner = owner ?? Application.Current.MainWindow;

            if (targetOwner != null &&
                targetOwner.IsLoaded &&
                targetOwner.IsVisible &&
                targetOwner.WindowState != WindowState.Minimized &&
                targetOwner.ActualWidth > 0 &&
                targetOwner.ActualHeight > 0)
            {
                msg.Owner = targetOwner;
                msg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                // Wenn noch nichts zu sehen ist: Frei schwebend in der Mitte des Monitors!
                msg.Owner = null;
                msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                msg.Topmost = true;
            }

            msg.ShowDialog();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

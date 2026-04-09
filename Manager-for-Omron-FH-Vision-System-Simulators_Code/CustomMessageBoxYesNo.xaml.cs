using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ManagerForOmronFHVisionSystemSimulators
{
    /// <summary>
    /// Interaktionslogik für CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBoxYesNo : Window
    {
        // Der Konstruktor bekommt jetzt optional eine Farbe
        public CustomMessageBoxYesNo(string message, string title, Brush borderBrush = null)
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = title;

            // Wenn eine Farbe mitgegeben wurde, färben wir den Rahmen und den Titel
            if (borderBrush != null)
            {
                MessageBorder.BorderBrush = borderBrush;
                TitleText.Foreground = borderBrush;
            }
            // Setzt den Fokus direkt auf den Button, sobald das Fenster geladen ist
            this.Activated += (s, e) => {
                BtnYes.Focus();
            };
        }

        // Die statische Methode gibt jetzt einen bool zurück!
        public static bool Show(string message, string title, Window owner, Brush color = null)
        {
            var msg = new CustomMessageBoxYesNo(message, title, color);
            msg.Owner = owner;
            var result = msg.ShowDialog();

            // DialogResult ist true, wenn Ja geklickt wurde
            return result ?? false;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Schließt Fenster und gibt true zurück
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Schließt Fenster und gibt false zurück
        }
    }
}

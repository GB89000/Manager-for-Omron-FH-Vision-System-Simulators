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
                TitleText.Foreground = borderBrush;
            }
            // Setzt den Fokus direkt auf den Button, sobald das Fenster geladen ist
            this.Activated += (s, e) => {
                OK.Focus();
            };
        }

        // Die statische Methode zum einfachen Aufrufen
        public static void Show(string message, string title, Window owner, Brush color = null)
        {
            // Wir erstellen die Box und reichen die Farbe an den Konstruktor weiter
            var msg = new CustomMessageBox(message, title, color);
            msg.Owner = owner;
            msg.ShowDialog();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

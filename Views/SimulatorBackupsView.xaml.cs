using System.Windows.Controls;

namespace ManagerForOmronFHVisionSystemSimulators.Views
{
    public partial class SimulatorBackupsView : UserControl
    {
        public SimulatorBackupsView()
        {
            InitializeComponent();
        }
        public void SetFocusToActiveItem()
        {
            // 1. Fokus auf die ListView an sich setzen
            // (Tausche 'SimulatorListView' gegen den echten x:Name deiner Liste in diesem UserControl aus)
            SimulatorsListView.Focus();

            // 2. WPF zwingen, den Tastatur-Fokus auf das exakte Item zu setzen
            if (SimulatorsListView.SelectedItem != null)
            {
                // Zwingt das UI, sich fertig zu zeichnen
                SimulatorsListView.UpdateLayout();

                // Das visuelle Element (die Zeile) aus der Liste fischen
                var item = SimulatorsListView.ItemContainerGenerator.ContainerFromItem(SimulatorsListView.SelectedItem) as System.Windows.UIElement;

                // Den Tastatur-Cursor hart darauf festnageln!
                item?.Focus();
            }
        }
    }
}
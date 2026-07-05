using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ManagerForOmronFHVisionSystemSimulators.ViewModels // Passe den Namespace an dein Projekt an
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Diese Methode rufen wir auf, wenn sich ein Wert ändert
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
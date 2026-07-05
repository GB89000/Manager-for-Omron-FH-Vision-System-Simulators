using System;
using System.Windows.Input;

namespace ManagerForOmronFHVisionSystemSimulators.ViewModels // Passe den Namespace ggf. an
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        // Konstruktor
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Bestimmt, ob der Button gerade klickbar ist (z.B. ausgegraut, wenn keine Daten da sind)
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Meldet der UI, wenn sich der CanExecute-Status ändert
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Das passiert, wenn man klickt
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
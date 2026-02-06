using System.Windows.Input;

namespace BayWyn_Couriers.Utilities
{
    // This is the class that will be used to bind commands to the UI elements in the MVVM pattern. It implements the ICommand interface, allowing it to be used as a command source in WPF applications.
    class RelayCommand : ICommand
    {
        // Method that has a single parameter without a return value, which will be executed when the command is invoked.
        private readonly Action<object?> _execute;
        // Func is similar to Action, but it has a return value.
        // In this case, it takes an object? as a parameter and returns a bool,
        // which will be used to determine if the command can be executed (else the button will be disabled).
        private readonly Func<object?, bool>? _canExecute;

        // Auto code which automatically checks if the button should be enabled or not by subscribing to the CommandManager.RequerySuggested event,
        // which is raised whenever the command manager detects conditions that might change the ability of a command to execute.
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


        //Constructor that takes an Action and an optional Func. The Action is required, while the Func is optional and defaults to a function that always returns true if not provided.
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Function that determines whether the command can execute in its current state.
        // If no function is provided for _canExecute, it returns true, allowing the command to execute.
        // Otherwise, it evaluates the provided function with the given parameter to determine if the command can execute.
        public bool CanExecute(object? parameter)
        {
            // If no function is given for _canExecute, the command returns true (_canExecute == null). Button will not be disabled.
            // Otherwise, it evaluates the function with the given parameter to determine if the command can execute.
            return _canExecute == null || _canExecute(parameter);
        }

        // Method that executes the command by invoking the _execute action with the provided parameter.
        // The parameter can be null and it is mostly used to pass data from the UI to the command when it is executed.
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }

}

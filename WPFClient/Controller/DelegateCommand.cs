using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFClient.DelegateCommands
{
    /// <summary>
    /// Implementierung des ICommand-Interfaces, die es ermöglicht,
    /// Commands mit Delegates für Execute und CanExecute zu verwenden.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object>? canExecute;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="DelegateCommand"/>-Klasse mit CanExecute und Execute-Delegates.
        /// </summary>
        /// <param name="canExecute">Prüft, ob der Command ausgeführt werden kann.</param>
        /// <param name="execute">Auszuführende Aktion.</param>
        public DelegateCommand(Predicate<object>? canExecute, Action<object> execute) =>
            (this.canExecute, this.execute) = (canExecute, execute);

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="DelegateCommand"/>-Klasse mit nur einem Execute-Delegate.
        /// Der Command ist immer ausführbar.
        /// </summary>
        /// <param name="execute">Auszuführende Aktion.</param>
        public DelegateCommand(Action<object> execute) : this(null, execute) { }

        /// <summary>
        /// Wird ausgelöst, wenn sich der Ausführbarkeitsstatus des Commands ändert.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Löst das <see cref="CanExecuteChanged"/>-Event aus, um die UI über Änderungen zu informieren.
        /// </summary>
        public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Prüft, ob der Command aktuell ausgeführt werden kann.
        /// </summary>
        /// <param name="parameter">Parameter für die Prüfung.</param>
        /// <returns>True, wenn ausführbar; sonst false.</returns>
        public bool CanExecute(object? parameter) => this.canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Führt die hinterlegte Aktion aus.
        /// </summary>
        /// <param name="parameter">Parameter für die Ausführung.</param>
        public void Execute(object? parameter)
        {
            this.execute?.Invoke(parameter);
        }
    }
}

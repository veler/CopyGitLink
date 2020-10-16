#nullable enable

using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink.CodeLens.ViewModels
{
    public sealed class ActionCommand : ICommand
    {
        private readonly Action _execute;

        public event EventHandler? CanExecuteChanged;

        internal ActionCommand(Action execute)
        {
            _execute = Requires.NotNull(execute, nameof(execute));
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                _execute();
            }
            catch
            {
                // Fail silently.
            }

            RaiseCanExecuteChanged();
        }

        public async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Execute(null);
        }

        private void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

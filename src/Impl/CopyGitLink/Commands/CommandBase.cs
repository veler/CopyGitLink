#nullable enable

using CopyGitLink.Def;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink.Commands
{
    /// <summary>
    /// Represents a command in Visual Studio.
    /// </summary>
    public abstract class CommandBase : ICommandBase
    {
        /// <summary>
        /// Gets the command ID.
        /// </summary>
        protected abstract int CommandId { get; }

        /// <summary>
        /// Gets the command menu group (command set GUID).
        /// </summary>
        protected abstract Guid CommandSet { get; }

        public CommandBase()
        {
            RegisterCommandAsync().Forget();
        }

        /// <summary>
        /// The event handler called when the command's status changes.
        /// </summary>
        protected virtual void Change(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Event handler called when a lient asks for the command status.
        /// </summary>
        protected virtual void BeforeQueryStatus(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// The event handler called to execute the command.
        /// </summary>
        protected virtual void Execute(object sender, EventArgs e)
        {
        }

        private async Task RegisterCommandAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            OleMenuCommandService? commandService = CopyGitLinkPackage.ServiceProvider?.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(Execute, Change, BeforeQueryStatus, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }
    }
}

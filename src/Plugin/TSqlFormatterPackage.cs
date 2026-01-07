using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using TSqlFormatter.Core.Logging;
using Task = System.Threading.Tasks.Task;

namespace TSqlFormatter.Extension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class TSqlFormatterPackage : AsyncPackage
    {
        /// <summary>
        /// TSqlFormatterPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

        private DTE2? _dte;
        private Commands.FormatSqlCommand? _formatCommand;
        private Commands.SettingsCommand? _settingsCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="TSqlFormatterPackage"/> class.
        /// </summary>
        public TSqlFormatterPackage()
        {
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            try
            {
                Logger.Instance.Info("TSqlFormatterPackage initializing...");

                _dte = await GetServiceAsync(typeof(DTE)) as DTE2;
                if (_dte == null)
                {
                    Logger.Instance.Error("Failed to get DTE service");
                    return;
                }

                // Initialize commands using DTE CommandBars
                await InitializeMenusAsync();

                Logger.Instance.Info("TSqlFormatterPackage initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to initialize TSqlFormatterPackage", ex);
            }
        }

        private async Task InitializeMenusAsync()
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                // Get the Tools menu
                var commandBars = _dte!.CommandBars as CommandBars;
                if (commandBars == null)
                {
                    Logger.Instance.Error("Could not get CommandBars");
                    return;
                }

                var menuBar = commandBars["MenuBar"];
                CommandBarPopup? toolsMenu = null;

                // Find Tools menu (try multiple names for localization)
                foreach (CommandBarControl control in menuBar.Controls)
                {
                    if (control.Caption.Contains("Tools") || control.Caption.Contains("ツール"))
                    {
                        toolsMenu = control as CommandBarPopup;
                        break;
                    }
                }

                if (toolsMenu == null)
                {
                    Logger.Instance.Error("Could not find Tools menu");
                    return;
                }

                // Create T-SQL Formatter submenu
                var formatterPopup = (CommandBarPopup)toolsMenu.Controls.Add(
                    MsoControlType.msoControlPopup,
                    Type.Missing,
                    Type.Missing,
                    1,
                    true);
                formatterPopup.Caption = "T-SQL Formatter";

                // Add Format SQL command
                var formatButton = (CommandBarButton)formatterPopup.Controls.Add(
                    MsoControlType.msoControlButton,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing,
                    true);
                formatButton.Caption = "Format T-SQL";
                formatButton.TooltipText = "Format the current SQL query (Ctrl+Shift+K)";
                formatButton.Click += FormatButton_Click;

                // Add Settings command
                var settingsButton = (CommandBarButton)formatterPopup.Controls.Add(
                    MsoControlType.msoControlButton,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing,
                    true);
                settingsButton.Caption = "Settings...";
                settingsButton.TooltipText = "Open T-SQL Formatter Settings";
                settingsButton.Click += SettingsButton_Click;

                // Initialize command handlers
                _formatCommand = new Commands.FormatSqlCommand(this);
                _settingsCommand = new Commands.SettingsCommand(this);

                Logger.Instance.Info("Menus created successfully");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to create menus", ex);
            }
        }

        private void FormatButton_Click(CommandBarButton ctrl, ref bool cancelDefault)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _formatCommand?.Execute();
        }

        private void SettingsButton_Click(CommandBarButton ctrl, ref bool cancelDefault)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _settingsCommand?.Execute();
        }

        #endregion
    }
}

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using TSqlFormatter.Core.Logging;
using TSqlFormatter.Extension.Views;
using Task = System.Threading.Tasks.Task;

namespace TSqlFormatter.Extension.Commands
{
    /// <summary>
    /// Command handler for opening settings window.
    /// </summary>
    internal sealed class SettingsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// Logger instance.
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCommand"/> class.
        /// </summary>
        public SettingsCommand(AsyncPackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _logger = Logger.Instance;
            _logger.Info("SettingsCommand initialized");
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SettingsCommand? Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Execute the settings command.
        /// </summary>
        public void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                _logger.Debug("Opening settings window");
                var settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
                _logger.Debug("Settings window closed");
            }
            catch (Exception ex)
            {
                _logger.Error("Error opening settings window", ex);
                VsShellUtilities.ShowMessageBox(
                    _package,
                    $"設定画面を開けませんでした:\n{ex.Message}\n\n詳細はログファイルを確認してください。",
                    "T-SQL Formatter - エラー",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}

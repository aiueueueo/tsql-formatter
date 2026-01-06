using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using TSqlFormatter.Core;
using TSqlFormatter.Core.Logging;
using Task = System.Threading.Tasks.Task;

namespace TSqlFormatter.Extension.Commands
{
    /// <summary>
    /// Command handler for formatting SQL.
    /// </summary>
    internal sealed class FormatSqlCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// Settings manager.
        /// </summary>
        private readonly SettingsManager _settingsManager;

        /// <summary>
        /// Logger instance.
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatSqlCommand"/> class.
        /// </summary>
        public FormatSqlCommand(AsyncPackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _settingsManager = new SettingsManager();
            _logger = Logger.Instance;
            _logger.Info("FormatSqlCommand initialized");
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FormatSqlCommand? Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Execute the format command.
        /// </summary>
        public void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                _logger.Debug("Format command executed");

                // Reload settings in case they changed
                var settings = _settingsManager.LoadSettings();
                var formatter = new Formatter(settings);

                // Get the DTE service
                var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
                if (dte?.ActiveDocument == null)
                {
                    _logger.Warning("No active document found");
                    ShowMessage("アクティブなドキュメントがありません", "T-SQL Formatter", OLEMSGICON.OLEMSGICON_WARNING);
                    return;
                }

                var textDocument = dte.ActiveDocument.Object("TextDocument") as TextDocument;
                if (textDocument == null)
                {
                    _logger.Warning("Cannot access text document");
                    ShowMessage("テキストドキュメントにアクセスできません", "T-SQL Formatter", OLEMSGICON.OLEMSGICON_WARNING);
                    return;
                }

                string sqlToFormat;
                EditPoint startPoint;
                EditPoint endPoint;
                bool hasSelection = false;

                // Check if there's a selection
                var selection = dte.ActiveDocument.Selection as TextSelection;
                if (selection != null && !selection.IsEmpty)
                {
                    // Format selection only
                    sqlToFormat = selection.Text;
                    startPoint = selection.TopPoint.CreateEditPoint();
                    endPoint = selection.BottomPoint.CreateEditPoint();
                    hasSelection = true;
                    _logger.Debug($"Formatting selection ({sqlToFormat.Length} chars)");
                }
                else
                {
                    // Format entire document
                    startPoint = textDocument.StartPoint.CreateEditPoint();
                    endPoint = textDocument.EndPoint.CreateEditPoint();
                    sqlToFormat = startPoint.GetText(endPoint);
                    _logger.Debug($"Formatting entire document ({sqlToFormat.Length} chars)");
                }

                if (string.IsNullOrWhiteSpace(sqlToFormat))
                {
                    _logger.Info("No SQL to format (empty content)");
                    ShowMessage("フォーマットするSQLがありません", "T-SQL Formatter", OLEMSGICON.OLEMSGICON_INFO);
                    return;
                }

                // Format the SQL with detailed result
                var result = formatter.FormatWithDetails(sqlToFormat);

                if (!result.IsSuccess)
                {
                    // Show syntax error message
                    var errorMessage = BuildErrorMessage(result);
                    _logger.Warning($"Format failed: {result.Errors.Count} error(s)");
                    ShowMessage(errorMessage, "T-SQL Formatter - 構文エラー", OLEMSGICON.OLEMSGICON_WARNING);
                    return;
                }

                // Replace the text
                if (hasSelection)
                {
                    selection!.Delete();
                    selection.Insert(result.FormattedSql);
                }
                else
                {
                    startPoint.ReplaceText(endPoint, result.FormattedSql, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
                }

                _logger.Info("Format completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Unexpected error during format command", ex);
                ShowMessage(
                    $"予期しないエラーが発生しました:\n{ex.Message}\n\n詳細はログファイルを確認してください。",
                    "T-SQL Formatter - エラー",
                    OLEMSGICON.OLEMSGICON_CRITICAL);
            }
        }

        /// <summary>
        /// Builds a user-friendly error message from the formatter result.
        /// </summary>
        private string BuildErrorMessage(FormatterResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SQLに構文エラーがあるため、フォーマットできませんでした。");
            sb.AppendLine();

            var errorsToShow = result.Errors.Take(5).ToList();
            foreach (var error in errorsToShow)
            {
                if (error.Line > 0)
                {
                    sb.AppendLine($"  行 {error.Line}, 列 {error.Column}: {error.Message}");
                }
                else
                {
                    sb.AppendLine($"  {error.Message}");
                }
            }

            if (result.Errors.Count > 5)
            {
                sb.AppendLine($"  ...他 {result.Errors.Count - 5} 件のエラー");
            }

            sb.AppendLine();
            sb.AppendLine("構文エラーを修正してから再度実行してください。");

            return sb.ToString();
        }

        /// <summary>
        /// Shows a message box.
        /// </summary>
        private void ShowMessage(string message, string title, OLEMSGICON icon)
        {
            VsShellUtilities.ShowMessageBox(
                _package,
                message,
                title,
                icon,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}

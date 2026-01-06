using System;
using System.Collections.Generic;
using System.Linq;
using TSqlFormatter.Core.Logging;
using TSqlFormatter.Core.Parser;
using TSqlFormatter.Core.Rules;

namespace TSqlFormatter.Core
{
    /// <summary>
    /// Main entry point for T-SQL formatting operations.
    /// </summary>
    public class Formatter
    {
        private readonly SqlParser _parser;
        private readonly FormatterSettings _settings;
        private readonly Logger _logger;

        /// <summary>
        /// Creates a new Formatter with default settings.
        /// </summary>
        public Formatter() : this(FormatterSettings.Default)
        {
        }

        /// <summary>
        /// Creates a new Formatter with the specified settings.
        /// </summary>
        /// <param name="settings">The formatter settings to use.</param>
        public Formatter(FormatterSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _parser = new SqlParser();
            _logger = Logger.Instance;
        }

        /// <summary>
        /// Formats the given T-SQL string according to the configured rules.
        /// </summary>
        /// <param name="sql">The T-SQL string to format.</param>
        /// <returns>The formatted T-SQL string, or the original if parsing fails. Returns null if input is null.</returns>
        public string? Format(string? sql)
        {
            var result = FormatWithDetails(sql);
            return result.FormattedSql;
        }

        /// <summary>
        /// Formats the given T-SQL string and returns a detailed result.
        /// </summary>
        /// <param name="sql">The T-SQL string to format.</param>
        /// <returns>A FormatterResult containing the formatted SQL and any errors.</returns>
        public FormatterResult FormatWithDetails(string? sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                _logger.Debug("Format called with empty or whitespace SQL");
                return FormatterResult.Success(sql);
            }

            try
            {
                _logger.Debug($"Starting format operation for SQL ({sql!.Length} chars)");

                // Parse the SQL
                var parseResult = _parser.ParseWithResult(sql);

                // If parsing failed, return the original SQL with error info
                if (!parseResult.IsSuccess)
                {
                    var sqlErrors = _parser.GetSyntaxErrors(sql);
                    var formatterErrors = sqlErrors.Select(e => new FormatterError(
                        GetFriendlyErrorMessage(e.Message),
                        e.Line,
                        e.Column,
                        FormatterErrorType.SyntaxError)).ToList();

                    _logger.Warning($"SQL parsing failed with {formatterErrors.Count} error(s)");
                    foreach (var error in formatterErrors)
                    {
                        _logger.Warning($"  {error}");
                    }

                    return FormatterResult.SyntaxError(sql, formatterErrors);
                }

                // Use the formatting visitor to generate formatted output
                var visitor = new FormattingVisitor(_settings);
                parseResult.Fragment!.Accept(visitor);

                var formatted = visitor.GetFormattedSql();
                _logger.Info($"Format operation completed successfully ({formatted.Length} chars output)");

                return FormatterResult.Success(formatted);
            }
            catch (Exception ex)
            {
                _logger.Error("Unexpected error during formatting", ex);
                return FormatterResult.FromException(sql, ex);
            }
        }

        /// <summary>
        /// Formats the given T-SQL string and returns a detailed result (legacy method).
        /// </summary>
        /// <param name="sql">The T-SQL string to format.</param>
        /// <returns>A FormatResult containing the formatted SQL and any errors.</returns>
        [Obsolete("Use FormatWithDetails instead")]
        public FormatResult FormatWithResult(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return new FormatResult(sql, true, null);
            }

            // Parse the SQL
            var parseResult = _parser.ParseWithResult(sql);

            // If parsing failed, return the original SQL with error info
            if (!parseResult.IsSuccess)
            {
                var errors = _parser.GetSyntaxErrors(sql);
                return new FormatResult(sql, false, errors);
            }

            // Use the formatting visitor to generate formatted output
            var visitor = new FormattingVisitor(_settings);
            parseResult.Fragment!.Accept(visitor);

            return new FormatResult(visitor.GetFormattedSql(), true, null);
        }

        /// <summary>
        /// Checks if the given SQL has syntax errors.
        /// </summary>
        /// <param name="sql">The T-SQL string to check.</param>
        /// <returns>True if the SQL has syntax errors, false otherwise.</returns>
        public bool HasSyntaxErrors(string sql)
        {
            return _parser.HasSyntaxErrors(sql);
        }

        /// <summary>
        /// Gets the syntax errors in the given SQL.
        /// </summary>
        /// <param name="sql">The T-SQL string to check.</param>
        /// <returns>A list of syntax errors.</returns>
        public IList<FormatterError> GetSyntaxErrors(string sql)
        {
            var sqlErrors = _parser.GetSyntaxErrors(sql);
            return sqlErrors.Select(e => new FormatterError(
                GetFriendlyErrorMessage(e.Message),
                e.Line,
                e.Column,
                FormatterErrorType.SyntaxError)).ToList();
        }

        /// <summary>
        /// Converts technical error messages to user-friendly messages.
        /// </summary>
        private string GetFriendlyErrorMessage(string technicalMessage)
        {
            // Common SQL syntax error patterns and their friendly messages
            if (technicalMessage.Contains("Incorrect syntax near"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    technicalMessage, @"Incorrect syntax near '([^']+)'");
                if (match.Success)
                {
                    return $"'{match.Groups[1].Value}' の近くに構文エラーがあります";
                }
            }

            if (technicalMessage.Contains("Unexpected end of file"))
            {
                return "SQLが不完全です。閉じ括弧や句が不足している可能性があります";
            }

            if (technicalMessage.Contains("Missing"))
            {
                return technicalMessage.Replace("Missing", "不足しています:");
            }

            if (technicalMessage.Contains("Expected"))
            {
                return technicalMessage.Replace("Expected", "期待される構文:");
            }

            // Return original message if no pattern matches
            return technicalMessage;
        }
    }

    /// <summary>
    /// Represents the result of a format operation (legacy class).
    /// </summary>
    [Obsolete("Use FormatterResult instead")]
    public class FormatResult
    {
        /// <summary>
        /// Gets the formatted SQL (or original if formatting failed).
        /// </summary>
        public string FormattedSql { get; }

        /// <summary>
        /// Gets whether the formatting was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets any syntax errors that prevented formatting.
        /// </summary>
        public IList<SqlError>? Errors { get; }

        public FormatResult(string formattedSql, bool isSuccess, IList<SqlError>? errors)
        {
            FormattedSql = formattedSql;
            IsSuccess = isSuccess;
            Errors = errors;
        }
    }
}

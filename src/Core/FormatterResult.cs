using System.Collections.Generic;
using System.Linq;

namespace TSqlFormatter.Core
{
    /// <summary>
    /// Result of a formatting operation.
    /// </summary>
    public class FormatterResult
    {
        /// <summary>
        /// Gets the formatted SQL, or the original SQL if formatting failed.
        /// Null if the input was null.
        /// </summary>
        public string? FormattedSql { get; }

        /// <summary>
        /// Gets whether the formatting was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets any errors that occurred during formatting.
        /// </summary>
        public IReadOnlyList<FormatterError> Errors { get; }

        /// <summary>
        /// Gets any warnings from the formatting process.
        /// </summary>
        public IReadOnlyList<FormatterWarning> Warnings { get; }

        /// <summary>
        /// Gets whether there are any errors.
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Gets whether there are any warnings.
        /// </summary>
        public bool HasWarnings => Warnings.Count > 0;

        /// <summary>
        /// Gets a user-friendly error message, or null if no errors.
        /// </summary>
        public string? ErrorMessage => HasErrors
            ? string.Join("\n", Errors.Select(e => e.Message))
            : null;

        private FormatterResult(string? formattedSql, bool isSuccess,
            IEnumerable<FormatterError>? errors = null,
            IEnumerable<FormatterWarning>? warnings = null)
        {
            FormattedSql = formattedSql;
            IsSuccess = isSuccess;
            Errors = (errors?.ToList() ?? new List<FormatterError>()).AsReadOnly();
            Warnings = (warnings?.ToList() ?? new List<FormatterWarning>()).AsReadOnly();
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static FormatterResult Success(string? formattedSql, IEnumerable<FormatterWarning>? warnings = null)
        {
            return new FormatterResult(formattedSql, true, null, warnings);
        }

        /// <summary>
        /// Creates a failed result with syntax errors.
        /// </summary>
        public static FormatterResult SyntaxError(string originalSql, IEnumerable<FormatterError> errors)
        {
            return new FormatterResult(originalSql, false, errors);
        }

        /// <summary>
        /// Creates a failed result with an exception.
        /// </summary>
        public static FormatterResult FromException(string? originalSql, System.Exception ex)
        {
            var error = new FormatterError(
                $"An error occurred during formatting: {ex.Message}",
                0, 0, FormatterErrorType.InternalError);
            return new FormatterResult(originalSql, false, new[] { error });
        }
    }

    /// <summary>
    /// Represents a formatting error.
    /// </summary>
    public class FormatterError
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the line number where the error occurred (1-based).
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column number where the error occurred (1-based).
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the type of error.
        /// </summary>
        public FormatterErrorType ErrorType { get; }

        public FormatterError(string message, int line, int column, FormatterErrorType errorType)
        {
            Message = message;
            Line = line;
            Column = column;
            ErrorType = errorType;
        }

        public override string ToString()
        {
            if (Line > 0 && Column > 0)
            {
                return $"Line {Line}, Column {Column}: {Message}";
            }
            return Message;
        }
    }

    /// <summary>
    /// Types of formatter errors.
    /// </summary>
    public enum FormatterErrorType
    {
        /// <summary>
        /// SQL syntax error.
        /// </summary>
        SyntaxError,

        /// <summary>
        /// Internal formatting error.
        /// </summary>
        InternalError
    }

    /// <summary>
    /// Represents a formatting warning.
    /// </summary>
    public class FormatterWarning
    {
        /// <summary>
        /// Gets the warning message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the line number where the warning applies (1-based).
        /// </summary>
        public int Line { get; }

        public FormatterWarning(string message, int line = 0)
        {
            Message = message;
            Line = line;
        }

        public override string ToString()
        {
            if (Line > 0)
            {
                return $"Line {Line}: {Message}";
            }
            return Message;
        }
    }
}

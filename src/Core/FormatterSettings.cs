namespace TSqlFormatter.Core
{
    /// <summary>
    /// Configuration settings for the T-SQL formatter.
    /// </summary>
    public class FormatterSettings
    {
        /// <summary>
        /// Gets or sets whether to use tabs for indentation.
        /// </summary>
        public bool UseTab { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of spaces per indent level (when not using tabs).
        /// </summary>
        public int IndentSize { get; set; } = 4;

        /// <summary>
        /// Gets or sets the keyword casing style.
        /// </summary>
        public KeywordCasing KeywordCasing { get; set; } = KeywordCasing.Uppercase;

        /// <summary>
        /// Gets or sets the comma placement style.
        /// </summary>
        public CommaPlacement CommaPlacement { get; set; } = CommaPlacement.BeforeColumn;

        /// <summary>
        /// Gets or sets whether to add spaces around operators.
        /// </summary>
        public bool SpaceAroundOperators { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to force the AS keyword for aliases.
        /// </summary>
        public bool ForceAsKeyword { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to place each clause on a new line.
        /// </summary>
        public bool NewLinePerClause { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to place JOINs on separate lines.
        /// </summary>
        public bool JoinOnSeparateLine { get; set; } = true;

        /// <summary>
        /// Creates default settings based on the project specification.
        /// </summary>
        public static FormatterSettings Default => new FormatterSettings();
    }

    /// <summary>
    /// Specifies keyword casing options.
    /// </summary>
    public enum KeywordCasing
    {
        Uppercase,
        Lowercase,
        PascalCase
    }

    /// <summary>
    /// Specifies comma placement options.
    /// </summary>
    public enum CommaPlacement
    {
        BeforeColumn,  // , column
        AfterColumn    // column,
    }
}

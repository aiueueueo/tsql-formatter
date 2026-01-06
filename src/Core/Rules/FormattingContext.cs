using System.Text;

namespace TSqlFormatter.Core.Rules
{
    /// <summary>
    /// Context object passed to formatting rules during processing.
    /// </summary>
    public class FormattingContext
    {
        /// <summary>
        /// Gets the StringBuilder used to build the formatted output.
        /// </summary>
        public StringBuilder Output { get; } = new StringBuilder();

        /// <summary>
        /// Gets or sets the current indentation level.
        /// </summary>
        public int IndentLevel { get; set; }

        /// <summary>
        /// Gets the settings for this formatting operation.
        /// </summary>
        public FormatterSettings Settings { get; }

        public FormattingContext(FormatterSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Appends the current indentation to the output.
        /// </summary>
        public void AppendIndent()
        {
            for (int i = 0; i < IndentLevel; i++)
            {
                Output.Append(Settings.UseTab ? "\t" : new string(' ', Settings.IndentSize));
            }
        }
    }
}

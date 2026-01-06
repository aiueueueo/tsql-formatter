using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSqlFormatter.Core.Rules
{
    /// <summary>
    /// Interface for implementing formatting rules.
    /// </summary>
    public interface IFormattingRule
    {
        /// <summary>
        /// Gets the name of this formatting rule.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of what this rule does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets or sets whether this rule is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Applies this formatting rule to the given SQL fragment.
        /// </summary>
        /// <param name="fragment">The SQL fragment to format.</param>
        /// <param name="context">The formatting context.</param>
        void Apply(TSqlFragment fragment, FormattingContext context);
    }
}

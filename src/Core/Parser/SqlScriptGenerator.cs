using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using ScriptDomKeywordCasing = Microsoft.SqlServer.TransactSql.ScriptDom.KeywordCasing;

namespace TSqlFormatter.Core.Parser
{
    /// <summary>
    /// Generates formatted SQL from a TSqlFragment AST.
    /// </summary>
    public class SqlScriptGenerator
    {
        private readonly SqlScriptGeneratorOptions _options;

        public SqlScriptGenerator()
        {
            _options = CreateDefaultOptions();
        }

        public SqlScriptGenerator(FormatterSettings settings)
        {
            _options = CreateOptionsFromSettings(settings);
        }

        /// <summary>
        /// Generates SQL string from the given fragment using configured options.
        /// </summary>
        /// <param name="fragment">The TSqlFragment to generate SQL from.</param>
        /// <returns>The generated SQL string.</returns>
        public string Generate(TSqlFragment fragment)
        {
            var generator = new Sql160ScriptGenerator(_options);
            generator.GenerateScript(fragment, out var sql);
            return sql;
        }

        /// <summary>
        /// Generates SQL string to a TextWriter.
        /// </summary>
        /// <param name="fragment">The TSqlFragment to generate SQL from.</param>
        /// <param name="writer">The TextWriter to write to.</param>
        public void Generate(TSqlFragment fragment, TextWriter writer)
        {
            var generator = new Sql160ScriptGenerator(_options);
            generator.GenerateScript(fragment, writer);
        }

        private static SqlScriptGeneratorOptions CreateDefaultOptions()
        {
            return new SqlScriptGeneratorOptions
            {
                KeywordCasing = ScriptDomKeywordCasing.Uppercase,
                IncludeSemicolons = true,
                NewLineBeforeFromClause = true,
                NewLineBeforeWhereClause = true,
                NewLineBeforeGroupByClause = true,
                NewLineBeforeHavingClause = true,
                NewLineBeforeOrderByClause = true,
                NewLineBeforeJoinClause = true,
                NewLineBeforeOutputClause = true,
                NewLineBeforeOffsetClause = true,
                NewLineBeforeOpenParenthesisInMultilineList = false,
                NewLineBeforeCloseParenthesisInMultilineList = false,
                MultilineInsertSourcesList = true,
                MultilineInsertTargetsList = true,
                MultilineSelectElementsList = true,
                MultilineSetClauseItems = true,
                MultilineViewColumnsList = true,
                MultilineWherePredicatesList = false,
                AlignClauseBodies = false,
                AlignColumnDefinitionFields = false,
                AlignSetClauseItem = false,
                AsKeywordOnOwnLine = false,
                IndentationSize = 4,
                IndentSetClause = true,
                IndentViewBody = true,
                SqlVersion = SqlVersion.Sql160
            };
        }

        private static SqlScriptGeneratorOptions CreateOptionsFromSettings(FormatterSettings settings)
        {
            var options = CreateDefaultOptions();

            // Apply keyword casing
            options.KeywordCasing = settings.KeywordCasing switch
            {
                KeywordCasing.Uppercase => ScriptDomKeywordCasing.Uppercase,
                KeywordCasing.Lowercase => ScriptDomKeywordCasing.Lowercase,
                KeywordCasing.PascalCase => ScriptDomKeywordCasing.PascalCase,
                _ => ScriptDomKeywordCasing.Uppercase
            };

            // Apply indentation settings
            options.IndentationSize = settings.UseTab ? 1 : settings.IndentSize;

            // Apply newline settings
            if (settings.NewLinePerClause)
            {
                options.NewLineBeforeFromClause = true;
                options.NewLineBeforeWhereClause = true;
                options.NewLineBeforeGroupByClause = true;
                options.NewLineBeforeHavingClause = true;
                options.NewLineBeforeOrderByClause = true;
            }

            // Apply JOIN settings
            options.NewLineBeforeJoinClause = settings.JoinOnSeparateLine;

            // Multiline settings for comma placement
            options.MultilineSelectElementsList = true;
            options.MultilineInsertSourcesList = true;
            options.MultilineInsertTargetsList = true;
            options.MultilineSetClauseItems = true;

            return options;
        }
    }
}

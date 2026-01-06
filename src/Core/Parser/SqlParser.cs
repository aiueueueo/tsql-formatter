using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TSqlFormatter.Core.Parser
{
    /// <summary>
    /// Wrapper for Microsoft.SqlServer.TransactSql.ScriptDom parser.
    /// </summary>
    public class SqlParser
    {
        private readonly TSql160Parser _parser;

        public SqlParser()
        {
            _parser = new TSql160Parser(initialQuotedIdentifiers: false);
        }

        /// <summary>
        /// Parses the given T-SQL string into an AST.
        /// </summary>
        /// <param name="sql">The T-SQL string to parse.</param>
        /// <param name="errors">Any parsing errors encountered.</param>
        /// <returns>The parsed TSqlFragment, or null if parsing fails.</returns>
        public TSqlFragment? Parse(string sql, out IList<ParseError> errors)
        {
            using var reader = new StringReader(sql);
            var fragment = _parser.Parse(reader, out errors);
            return fragment;
        }

        /// <summary>
        /// Parses the given T-SQL string and returns a detailed result.
        /// </summary>
        /// <param name="sql">The T-SQL string to parse.</param>
        /// <returns>A ParseResult containing the fragment and any errors.</returns>
        public ParseResult ParseWithResult(string sql)
        {
            var fragment = Parse(sql, out var errors);
            return new ParseResult(fragment, errors, sql);
        }

        /// <summary>
        /// Checks if the given T-SQL string has syntax errors.
        /// </summary>
        /// <param name="sql">The T-SQL string to check.</param>
        /// <returns>True if the SQL has syntax errors, false otherwise.</returns>
        public bool HasSyntaxErrors(string sql)
        {
            Parse(sql, out var errors);
            return errors.Count > 0;
        }

        /// <summary>
        /// Gets the list of syntax errors for the given T-SQL string.
        /// </summary>
        /// <param name="sql">The T-SQL string to check.</param>
        /// <returns>A list of error messages.</returns>
        public IList<SqlError> GetSyntaxErrors(string sql)
        {
            Parse(sql, out var errors);
            return errors.Select(e => new SqlError(e)).ToList();
        }
    }

    /// <summary>
    /// Represents the result of parsing a T-SQL string.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Gets the parsed TSqlFragment, or null if parsing failed.
        /// </summary>
        public TSqlFragment? Fragment { get; }

        /// <summary>
        /// Gets the list of parse errors.
        /// </summary>
        public IList<ParseError> Errors { get; }

        /// <summary>
        /// Gets the original SQL string.
        /// </summary>
        public string OriginalSql { get; }

        /// <summary>
        /// Gets whether the parse was successful (no errors).
        /// </summary>
        public bool IsSuccess => Errors.Count == 0 && Fragment != null;

        /// <summary>
        /// Gets whether the SQL has syntax errors.
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Gets the script as a TSqlScript if it was successfully parsed.
        /// </summary>
        public TSqlScript? Script => Fragment as TSqlScript;

        public ParseResult(TSqlFragment? fragment, IList<ParseError> errors, string originalSql)
        {
            Fragment = fragment;
            Errors = errors;
            OriginalSql = originalSql;
        }

        /// <summary>
        /// Gets all statements from the parsed script.
        /// </summary>
        /// <returns>An enumerable of TSqlStatement objects.</returns>
        public IEnumerable<TSqlStatement> GetStatements()
        {
            if (Script == null) yield break;

            foreach (var batch in Script.Batches)
            {
                foreach (var statement in batch.Statements)
                {
                    yield return statement;
                }
            }
        }

        /// <summary>
        /// Gets statements of a specific type from the parsed script.
        /// </summary>
        /// <typeparam name="T">The type of statement to retrieve.</typeparam>
        /// <returns>An enumerable of statements of the specified type.</returns>
        public IEnumerable<T> GetStatements<T>() where T : TSqlStatement
        {
            return GetStatements().OfType<T>();
        }
    }

    /// <summary>
    /// Represents a SQL syntax error with formatted information.
    /// </summary>
    public class SqlError
    {
        /// <summary>
        /// Gets the line number where the error occurred.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column number where the error occurred.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the error number.
        /// </summary>
        public int ErrorNumber { get; }

        public SqlError(ParseError error)
        {
            Line = error.Line;
            Column = error.Column;
            Message = error.Message;
            ErrorNumber = error.Number;
        }

        public override string ToString()
        {
            return $"Line {Line}, Column {Column}: {Message}";
        }
    }
}

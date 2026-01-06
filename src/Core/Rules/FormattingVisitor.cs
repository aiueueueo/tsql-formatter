using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text;

namespace TSqlFormatter.Core.Rules
{
    /// <summary>
    /// A visitor that traverses the SQL AST and generates formatted output.
    /// </summary>
    public class FormattingVisitor : TSqlFragmentVisitor
    {
        private readonly StringBuilder _output;
        private readonly FormatterSettings _settings;
        private int _indentLevel;
        private bool _isFirstSelectElement;

        public FormattingVisitor(FormatterSettings settings)
        {
            _settings = settings;
            _output = new StringBuilder();
            _indentLevel = 0;
        }

        /// <summary>
        /// Gets the formatted SQL output.
        /// </summary>
        public string GetFormattedSql() => _output.ToString().TrimEnd();

        #region Helper Methods

        private void AppendKeyword(string keyword)
        {
            var formatted = _settings.KeywordCasing switch
            {
                KeywordCasing.Uppercase => keyword.ToUpperInvariant(),
                KeywordCasing.Lowercase => keyword.ToLowerInvariant(),
                KeywordCasing.PascalCase => ToPascalCase(keyword),
                _ => keyword.ToUpperInvariant()
            };
            _output.Append(formatted);
        }

        private void AppendIndent()
        {
            for (int i = 0; i < _indentLevel; i++)
            {
                _output.Append(_settings.UseTab ? "\t" : new string(' ', _settings.IndentSize));
            }
        }

        private void AppendNewLine()
        {
            _output.AppendLine();
        }

        private void AppendSpace()
        {
            _output.Append(' ');
        }

        private void AppendComma()
        {
            _output.Append(',');
        }

        private static string ToPascalCase(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return keyword;
            return char.ToUpperInvariant(keyword[0]) + keyword.Substring(1).ToLowerInvariant();
        }

        private void IncreaseIndent() => _indentLevel++;
        private void DecreaseIndent() => _indentLevel = System.Math.Max(0, _indentLevel - 1);

        #endregion

        #region Statement Visitors

        public override void ExplicitVisit(SelectStatement node)
        {
            // Handle CTEs if present
            if (node.WithCtesAndXmlNamespaces != null)
            {
                node.WithCtesAndXmlNamespaces.Accept(this);
                AppendNewLine();
            }

            // Visit the query expression
            node.QueryExpression?.Accept(this);
        }

        public override void ExplicitVisit(QuerySpecification node)
        {
            // SELECT keyword
            AppendKeyword("SELECT");

            // DISTINCT/ALL
            if (node.UniqueRowFilter == UniqueRowFilter.Distinct)
            {
                AppendSpace();
                AppendKeyword("DISTINCT");
            }

            // TOP clause
            if (node.TopRowFilter != null)
            {
                AppendSpace();
                node.TopRowFilter.Accept(this);
            }

            // SELECT elements
            AppendNewLine();
            IncreaseIndent();
            _isFirstSelectElement = true;
            foreach (var element in node.SelectElements)
            {
                if (!_isFirstSelectElement)
                {
                    AppendNewLine();
                }
                AppendIndent();
                if (!_isFirstSelectElement && _settings.CommaPlacement == CommaPlacement.BeforeColumn)
                {
                    AppendComma();
                    AppendSpace();
                }
                element.Accept(this);
                if (!_isFirstSelectElement || node.SelectElements.Count == 1)
                {
                    // Don't add comma after last element
                }
                else if (_settings.CommaPlacement == CommaPlacement.AfterColumn &&
                         node.SelectElements.IndexOf(element) < node.SelectElements.Count - 1)
                {
                    AppendComma();
                }
                _isFirstSelectElement = false;
            }
            DecreaseIndent();

            // FROM clause
            if (node.FromClause != null)
            {
                AppendNewLine();
                node.FromClause.Accept(this);
            }

            // WHERE clause
            if (node.WhereClause != null)
            {
                AppendNewLine();
                node.WhereClause.Accept(this);
            }

            // GROUP BY clause
            if (node.GroupByClause != null)
            {
                AppendNewLine();
                node.GroupByClause.Accept(this);
            }

            // HAVING clause
            if (node.HavingClause != null)
            {
                AppendNewLine();
                node.HavingClause.Accept(this);
            }
        }

        public override void ExplicitVisit(FromClause node)
        {
            AppendKeyword("FROM");
            AppendSpace();

            for (int i = 0; i < node.TableReferences.Count; i++)
            {
                if (i > 0)
                {
                    AppendComma();
                    AppendSpace();
                }
                node.TableReferences[i].Accept(this);
            }
        }

        public override void ExplicitVisit(WhereClause node)
        {
            AppendKeyword("WHERE");
            AppendSpace();
            node.SearchCondition?.Accept(this);
        }

        public override void ExplicitVisit(GroupByClause node)
        {
            AppendKeyword("GROUP BY");
            AppendSpace();
            for (int i = 0; i < node.GroupingSpecifications.Count; i++)
            {
                if (i > 0)
                {
                    AppendComma();
                    AppendSpace();
                }
                node.GroupingSpecifications[i].Accept(this);
            }
        }

        public override void ExplicitVisit(HavingClause node)
        {
            AppendKeyword("HAVING");
            AppendSpace();
            node.SearchCondition?.Accept(this);
        }

        public override void ExplicitVisit(OrderByClause node)
        {
            AppendKeyword("ORDER BY");
            AppendSpace();
            for (int i = 0; i < node.OrderByElements.Count; i++)
            {
                if (i > 0)
                {
                    AppendComma();
                    AppendSpace();
                }
                node.OrderByElements[i].Accept(this);
            }
        }

        #endregion

        #region Table Reference Visitors

        public override void ExplicitVisit(NamedTableReference node)
        {
            node.SchemaObject?.Accept(this);

            if (node.Alias != null)
            {
                AppendSpace();
                if (_settings.ForceAsKeyword)
                {
                    AppendKeyword("AS");
                    AppendSpace();
                }
                _output.Append(node.Alias.Value);
            }
        }

        public override void ExplicitVisit(QualifiedJoin node)
        {
            // First table
            node.FirstTableReference?.Accept(this);

            // JOIN keyword
            AppendNewLine();
            var joinType = node.QualifiedJoinType switch
            {
                QualifiedJoinType.Inner => "INNER JOIN",
                QualifiedJoinType.LeftOuter => "LEFT JOIN",
                QualifiedJoinType.RightOuter => "RIGHT JOIN",
                QualifiedJoinType.FullOuter => "FULL JOIN",
                _ => "JOIN"
            };
            AppendKeyword(joinType);
            AppendSpace();

            // Second table
            node.SecondTableReference?.Accept(this);

            // ON clause
            if (node.SearchCondition != null)
            {
                AppendNewLine();
                IncreaseIndent();
                AppendIndent();
                AppendKeyword("ON");
                AppendSpace();
                node.SearchCondition.Accept(this);
                DecreaseIndent();
            }
        }

        #endregion

        #region Expression Visitors

        public override void ExplicitVisit(SelectScalarExpression node)
        {
            node.Expression?.Accept(this);

            if (node.ColumnName != null)
            {
                AppendSpace();
                if (_settings.ForceAsKeyword)
                {
                    AppendKeyword("AS");
                    AppendSpace();
                }
                _output.Append(node.ColumnName.Value);
            }
        }

        public override void ExplicitVisit(SelectStarExpression node)
        {
            if (node.Qualifier != null)
            {
                node.Qualifier.Accept(this);
                _output.Append('.');
            }
            _output.Append('*');
        }

        public override void ExplicitVisit(ColumnReferenceExpression node)
        {
            node.MultiPartIdentifier?.Accept(this);
        }

        public override void ExplicitVisit(MultiPartIdentifier node)
        {
            for (int i = 0; i < node.Identifiers.Count; i++)
            {
                if (i > 0) _output.Append('.');
                var identifier = node.Identifiers[i];
                if (identifier.QuoteType == QuoteType.SquareBracket)
                {
                    _output.Append('[');
                    _output.Append(identifier.Value);
                    _output.Append(']');
                }
                else if (identifier.QuoteType == QuoteType.DoubleQuote)
                {
                    _output.Append('"');
                    _output.Append(identifier.Value);
                    _output.Append('"');
                }
                else
                {
                    _output.Append(identifier.Value);
                }
            }
        }

        public override void ExplicitVisit(SchemaObjectName node)
        {
            if (node.ServerIdentifier != null)
            {
                _output.Append(node.ServerIdentifier.Value);
                _output.Append('.');
            }
            if (node.DatabaseIdentifier != null)
            {
                _output.Append(node.DatabaseIdentifier.Value);
                _output.Append('.');
            }
            if (node.SchemaIdentifier != null)
            {
                _output.Append(node.SchemaIdentifier.Value);
                _output.Append('.');
            }
            if (node.BaseIdentifier != null)
            {
                _output.Append(node.BaseIdentifier.Value);
            }
        }

        public override void ExplicitVisit(IntegerLiteral node)
        {
            _output.Append(node.Value);
        }

        public override void ExplicitVisit(StringLiteral node)
        {
            _output.Append('\'');
            _output.Append(node.Value.Replace("'", "''"));
            _output.Append('\'');
        }

        public override void ExplicitVisit(NumericLiteral node)
        {
            _output.Append(node.Value);
        }

        public override void ExplicitVisit(NullLiteral node)
        {
            AppendKeyword("NULL");
        }

        #endregion

        #region Comparison and Boolean Visitors

        public override void ExplicitVisit(BooleanComparisonExpression node)
        {
            node.FirstExpression?.Accept(this);

            if (_settings.SpaceAroundOperators) AppendSpace();

            var op = node.ComparisonType switch
            {
                BooleanComparisonType.Equals => "=",
                BooleanComparisonType.NotEqualToBrackets => "<>",
                BooleanComparisonType.NotEqualToExclamation => "!=",
                BooleanComparisonType.LessThan => "<",
                BooleanComparisonType.LessThanOrEqualTo => "<=",
                BooleanComparisonType.GreaterThan => ">",
                BooleanComparisonType.GreaterThanOrEqualTo => ">=",
                _ => "="
            };
            _output.Append(op);

            if (_settings.SpaceAroundOperators) AppendSpace();

            node.SecondExpression?.Accept(this);
        }

        public override void ExplicitVisit(BooleanBinaryExpression node)
        {
            node.FirstExpression?.Accept(this);

            AppendNewLine();
            IncreaseIndent();
            AppendIndent();

            var op = node.BinaryExpressionType switch
            {
                BooleanBinaryExpressionType.And => "AND",
                BooleanBinaryExpressionType.Or => "OR",
                _ => "AND"
            };
            AppendKeyword(op);
            AppendSpace();

            DecreaseIndent();

            node.SecondExpression?.Accept(this);
        }

        public override void ExplicitVisit(BooleanParenthesisExpression node)
        {
            _output.Append('(');
            node.Expression?.Accept(this);
            _output.Append(')');
        }

        public override void ExplicitVisit(BooleanIsNullExpression node)
        {
            node.Expression?.Accept(this);
            AppendSpace();
            AppendKeyword("IS");
            if (node.IsNot)
            {
                AppendSpace();
                AppendKeyword("NOT");
            }
            AppendSpace();
            AppendKeyword("NULL");
        }

        public override void ExplicitVisit(InPredicate node)
        {
            node.Expression?.Accept(this);
            AppendSpace();
            if (node.NotDefined)
            {
                AppendKeyword("NOT");
                AppendSpace();
            }
            AppendKeyword("IN");
            AppendSpace();
            _output.Append('(');

            if (node.Subquery != null)
            {
                node.Subquery.Accept(this);
            }
            else
            {
                for (int i = 0; i < node.Values.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendComma();
                        AppendSpace();
                    }
                    node.Values[i].Accept(this);
                }
            }

            _output.Append(')');
        }

        public override void ExplicitVisit(LikePredicate node)
        {
            node.FirstExpression?.Accept(this);
            AppendSpace();
            if (node.NotDefined)
            {
                AppendKeyword("NOT");
                AppendSpace();
            }
            AppendKeyword("LIKE");
            AppendSpace();
            node.SecondExpression?.Accept(this);
        }

        public override void ExplicitVisit(BooleanTernaryExpression node)
        {
            node.FirstExpression?.Accept(this);
            AppendSpace();
            if (node.TernaryExpressionType == BooleanTernaryExpressionType.NotBetween)
            {
                AppendKeyword("NOT");
                AppendSpace();
            }
            AppendKeyword("BETWEEN");
            AppendSpace();
            node.SecondExpression?.Accept(this);
            AppendSpace();
            AppendKeyword("AND");
            AppendSpace();
            node.ThirdExpression?.Accept(this);
        }

        #endregion

        #region CASE Expression

        public override void ExplicitVisit(SearchedCaseExpression node)
        {
            AppendKeyword("CASE");
            IncreaseIndent();

            foreach (var whenClause in node.WhenClauses)
            {
                AppendNewLine();
                AppendIndent();
                whenClause.Accept(this);
            }

            if (node.ElseExpression != null)
            {
                AppendNewLine();
                AppendIndent();
                AppendKeyword("ELSE");
                AppendSpace();
                node.ElseExpression.Accept(this);
            }

            DecreaseIndent();
            AppendNewLine();
            AppendIndent();
            AppendKeyword("END");
        }

        public override void ExplicitVisit(SimpleCaseExpression node)
        {
            AppendKeyword("CASE");
            AppendSpace();
            node.InputExpression?.Accept(this);
            IncreaseIndent();

            foreach (var whenClause in node.WhenClauses)
            {
                AppendNewLine();
                AppendIndent();
                whenClause.Accept(this);
            }

            if (node.ElseExpression != null)
            {
                AppendNewLine();
                AppendIndent();
                AppendKeyword("ELSE");
                AppendSpace();
                node.ElseExpression.Accept(this);
            }

            DecreaseIndent();
            AppendNewLine();
            AppendIndent();
            AppendKeyword("END");
        }

        public override void ExplicitVisit(SearchedWhenClause node)
        {
            AppendKeyword("WHEN");
            AppendSpace();
            node.WhenExpression?.Accept(this);
            AppendSpace();
            AppendKeyword("THEN");
            AppendSpace();
            node.ThenExpression?.Accept(this);
        }

        public override void ExplicitVisit(SimpleWhenClause node)
        {
            AppendKeyword("WHEN");
            AppendSpace();
            node.WhenExpression?.Accept(this);
            AppendSpace();
            AppendKeyword("THEN");
            AppendSpace();
            node.ThenExpression?.Accept(this);
        }

        #endregion

        #region Function Calls

        public override void ExplicitVisit(FunctionCall node)
        {
            _output.Append(node.FunctionName.Value.ToUpperInvariant());
            _output.Append('(');

            for (int i = 0; i < node.Parameters.Count; i++)
            {
                if (i > 0)
                {
                    AppendComma();
                    AppendSpace();
                }
                node.Parameters[i].Accept(this);
            }

            _output.Append(')');
        }

        public override void ExplicitVisit(ScalarSubquery node)
        {
            _output.Append('(');
            AppendNewLine();
            IncreaseIndent();
            AppendIndent();
            node.QueryExpression?.Accept(this);
            DecreaseIndent();
            AppendNewLine();
            AppendIndent();
            _output.Append(')');
        }

        #endregion

        #region Arithmetic Expressions

        public override void ExplicitVisit(BinaryExpression node)
        {
            node.FirstExpression?.Accept(this);

            if (_settings.SpaceAroundOperators) AppendSpace();

            var op = node.BinaryExpressionType switch
            {
                BinaryExpressionType.Add => "+",
                BinaryExpressionType.Subtract => "-",
                BinaryExpressionType.Multiply => "*",
                BinaryExpressionType.Divide => "/",
                BinaryExpressionType.Modulo => "%",
                _ => "+"
            };
            _output.Append(op);

            if (_settings.SpaceAroundOperators) AppendSpace();

            node.SecondExpression?.Accept(this);
        }

        public override void ExplicitVisit(ParenthesisExpression node)
        {
            _output.Append('(');
            node.Expression?.Accept(this);
            _output.Append(')');
        }

        #endregion

        #region TOP Clause

        public override void ExplicitVisit(TopRowFilter node)
        {
            AppendKeyword("TOP");
            AppendSpace();
            if (node.WithTies)
            {
                _output.Append('(');
            }
            node.Expression?.Accept(this);
            if (node.WithTies)
            {
                _output.Append(')');
                AppendSpace();
                AppendKeyword("WITH TIES");
            }
            if (node.Percent)
            {
                AppendSpace();
                AppendKeyword("PERCENT");
            }
        }

        #endregion

        #region ORDER BY

        public override void ExplicitVisit(ExpressionWithSortOrder node)
        {
            node.Expression?.Accept(this);
            if (node.SortOrder == SortOrder.Descending)
            {
                AppendSpace();
                AppendKeyword("DESC");
            }
            else if (node.SortOrder == SortOrder.Ascending)
            {
                AppendSpace();
                AppendKeyword("ASC");
            }
        }

        #endregion

        #region Grouping

        public override void ExplicitVisit(ExpressionGroupingSpecification node)
        {
            node.Expression?.Accept(this);
        }

        #endregion

        #region INSERT Statement

        public override void ExplicitVisit(InsertStatement node)
        {
            AppendKeyword("INSERT INTO");
            AppendSpace();
            node.InsertSpecification?.Target?.Accept(this);

            // Column list
            if (node.InsertSpecification?.Columns?.Count > 0)
            {
                AppendSpace();
                _output.Append('(');
                AppendNewLine();
                IncreaseIndent();

                for (int i = 0; i < node.InsertSpecification.Columns.Count; i++)
                {
                    AppendIndent();
                    if (i > 0 && _settings.CommaPlacement == CommaPlacement.BeforeColumn)
                    {
                        AppendComma();
                        AppendSpace();
                    }
                    node.InsertSpecification.Columns[i].Accept(this);
                    if (_settings.CommaPlacement == CommaPlacement.AfterColumn &&
                        i < node.InsertSpecification.Columns.Count - 1)
                    {
                        AppendComma();
                    }
                    AppendNewLine();
                }

                DecreaseIndent();
                AppendIndent();
                _output.Append(')');
            }

            // VALUES or SELECT
            if (node.InsertSpecification?.InsertSource != null)
            {
                AppendNewLine();
                node.InsertSpecification.InsertSource.Accept(this);
            }
        }

        public override void ExplicitVisit(ValuesInsertSource node)
        {
            AppendKeyword("VALUES");
            for (int rowIndex = 0; rowIndex < node.RowValues.Count; rowIndex++)
            {
                if (rowIndex > 0)
                {
                    AppendComma();
                }
                AppendNewLine();
                IncreaseIndent();
                AppendIndent();
                _output.Append('(');

                var row = node.RowValues[rowIndex];
                for (int i = 0; i < row.ColumnValues.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendComma();
                        AppendSpace();
                    }
                    row.ColumnValues[i].Accept(this);
                }

                _output.Append(')');
                DecreaseIndent();
            }
        }

        public override void ExplicitVisit(SelectInsertSource node)
        {
            node.Select?.Accept(this);
        }

        #endregion

        #region UPDATE Statement

        public override void ExplicitVisit(UpdateStatement node)
        {
            AppendKeyword("UPDATE");
            AppendSpace();
            node.UpdateSpecification?.Target?.Accept(this);

            // SET clause
            if (node.UpdateSpecification?.SetClauses?.Count > 0)
            {
                AppendNewLine();
                AppendKeyword("SET");
                AppendNewLine();
                IncreaseIndent();

                for (int i = 0; i < node.UpdateSpecification.SetClauses.Count; i++)
                {
                    AppendIndent();
                    if (i > 0 && _settings.CommaPlacement == CommaPlacement.BeforeColumn)
                    {
                        AppendComma();
                        AppendSpace();
                    }
                    node.UpdateSpecification.SetClauses[i].Accept(this);
                    if (_settings.CommaPlacement == CommaPlacement.AfterColumn &&
                        i < node.UpdateSpecification.SetClauses.Count - 1)
                    {
                        AppendComma();
                    }
                    AppendNewLine();
                }

                DecreaseIndent();
            }

            // FROM clause
            if (node.UpdateSpecification?.FromClause != null)
            {
                node.UpdateSpecification.FromClause.Accept(this);
                AppendNewLine();
            }

            // WHERE clause
            if (node.UpdateSpecification?.WhereClause != null)
            {
                node.UpdateSpecification.WhereClause.Accept(this);
            }
        }

        public override void ExplicitVisit(AssignmentSetClause node)
        {
            node.Column?.Accept(this);
            if (_settings.SpaceAroundOperators) AppendSpace();
            _output.Append('=');
            if (_settings.SpaceAroundOperators) AppendSpace();
            node.NewValue?.Accept(this);
        }

        #endregion

        #region DELETE Statement

        public override void ExplicitVisit(DeleteStatement node)
        {
            AppendKeyword("DELETE");
            AppendSpace();

            // Target table
            if (node.DeleteSpecification?.Target != null)
            {
                node.DeleteSpecification.Target.Accept(this);
            }

            // FROM clause (for JOINs)
            if (node.DeleteSpecification?.FromClause != null)
            {
                AppendNewLine();
                node.DeleteSpecification.FromClause.Accept(this);
            }

            // WHERE clause
            if (node.DeleteSpecification?.WhereClause != null)
            {
                AppendNewLine();
                node.DeleteSpecification.WhereClause.Accept(this);
            }
        }

        #endregion

        #region CREATE TABLE

        public override void ExplicitVisit(CreateTableStatement node)
        {
            AppendKeyword("CREATE TABLE");
            AppendSpace();
            node.SchemaObjectName?.Accept(this);
            AppendNewLine();
            _output.Append('(');
            AppendNewLine();
            IncreaseIndent();

            for (int i = 0; i < node.Definition?.ColumnDefinitions?.Count; i++)
            {
                AppendIndent();
                if (i > 0 && _settings.CommaPlacement == CommaPlacement.BeforeColumn)
                {
                    AppendComma();
                    AppendSpace();
                }
                node.Definition.ColumnDefinitions[i].Accept(this);
                if (_settings.CommaPlacement == CommaPlacement.AfterColumn &&
                    i < node.Definition.ColumnDefinitions.Count - 1)
                {
                    AppendComma();
                }
                AppendNewLine();
            }

            DecreaseIndent();
            _output.Append(')');
        }

        public override void ExplicitVisit(ColumnDefinition node)
        {
            _output.Append(node.ColumnIdentifier?.Value);
            AppendSpace();
            node.DataType?.Accept(this);

            // Handle constraints
            if (node.Constraints != null)
            {
                foreach (var constraint in node.Constraints)
                {
                    if (constraint is NullableConstraintDefinition nullConstraint)
                    {
                        AppendSpace();
                        if (nullConstraint.Nullable)
                        {
                            AppendKeyword("NULL");
                        }
                        else
                        {
                            AppendKeyword("NOT NULL");
                        }
                    }
                }
            }

            if (node.DefaultConstraint != null)
            {
                AppendSpace();
                AppendKeyword("DEFAULT");
                AppendSpace();
                node.DefaultConstraint.Expression?.Accept(this);
            }

            if (node.IdentityOptions != null)
            {
                AppendSpace();
                AppendKeyword("IDENTITY");
                if (node.IdentityOptions.IdentitySeed != null || node.IdentityOptions.IdentityIncrement != null)
                {
                    _output.Append('(');
                    node.IdentityOptions.IdentitySeed?.Accept(this);
                    AppendComma();
                    AppendSpace();
                    node.IdentityOptions.IdentityIncrement?.Accept(this);
                    _output.Append(')');
                }
            }
        }

        public override void ExplicitVisit(SqlDataTypeReference node)
        {
            AppendKeyword(node.SqlDataTypeOption.ToString());
            if (node.Parameters?.Count > 0)
            {
                _output.Append('(');
                for (int i = 0; i < node.Parameters.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendComma();
                        AppendSpace();
                    }
                    node.Parameters[i].Accept(this);
                }
                _output.Append(')');
            }
        }

        #endregion

        #region Variables and Parameters

        public override void ExplicitVisit(VariableReference node)
        {
            _output.Append('@');
            _output.Append(node.Name);
        }

        public override void ExplicitVisit(GlobalVariableExpression node)
        {
            _output.Append("@@");
            _output.Append(node.Name);
        }

        #endregion

        #region Unary Expression

        public override void ExplicitVisit(UnaryExpression node)
        {
            var op = node.UnaryExpressionType switch
            {
                UnaryExpressionType.Negative => "-",
                UnaryExpressionType.Positive => "+",
                UnaryExpressionType.BitwiseNot => "~",
                _ => ""
            };
            _output.Append(op);
            node.Expression?.Accept(this);
        }

        #endregion
    }
}

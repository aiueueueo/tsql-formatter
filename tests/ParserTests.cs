using Microsoft.SqlServer.TransactSql.ScriptDom;
using TSqlFormatter.Core.Parser;
using Xunit;
using System.Linq;

namespace TSqlFormatter.Tests
{
    public class ParserTests
    {
        private readonly SqlParser _parser = new();

        #region Basic Parsing Tests

        [Fact]
        public void Parse_ValidSelectStatement_ReturnsFragment()
        {
            // Arrange
            var sql = "SELECT id, name FROM users WHERE id = 1";

            // Act
            var result = _parser.Parse(sql, out var errors);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Parse_InvalidSql_ReturnsErrors()
        {
            // Arrange
            var sql = "SELECT FROM WHERE";

            // Act
            var result = _parser.Parse(sql, out var errors);

            // Assert
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void HasSyntaxErrors_ValidSql_ReturnsFalse()
        {
            // Arrange
            var sql = "SELECT * FROM users";

            // Act
            var result = _parser.HasSyntaxErrors(sql);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasSyntaxErrors_InvalidSql_ReturnsTrue()
        {
            // Arrange
            var sql = "SELEC * FORM users";

            // Act
            var result = _parser.HasSyntaxErrors(sql);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region ParseResult Tests

        [Fact]
        public void ParseWithResult_ValidSql_ReturnsSuccessResult()
        {
            // Arrange
            var sql = "SELECT id FROM users";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Fragment);
            Assert.NotNull(result.Script);
            Assert.Equal(sql, result.OriginalSql);
        }

        [Fact]
        public void ParseWithResult_InvalidSql_ReturnsErrorResult()
        {
            // Arrange
            var sql = "SELEC * FORM";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public void ParseWithResult_GetStatements_ReturnsStatements()
        {
            // Arrange
            var sql = "SELECT 1; SELECT 2; SELECT 3";

            // Act
            var result = _parser.ParseWithResult(sql);
            var statements = result.GetStatements().ToList();

            // Assert
            Assert.Equal(3, statements.Count);
        }

        [Fact]
        public void ParseWithResult_GetStatementsOfType_ReturnsFilteredStatements()
        {
            // Arrange
            var sql = "SELECT 1; INSERT INTO t VALUES (1); SELECT 2";

            // Act
            var result = _parser.ParseWithResult(sql);
            var selectStatements = result.GetStatements<SelectStatement>().ToList();

            // Assert
            Assert.Equal(2, selectStatements.Count);
        }

        #endregion

        #region SqlError Tests

        [Fact]
        public void GetSyntaxErrors_ReturnsFormattedErrors()
        {
            // Arrange
            var sql = "SELEC * FROM users";

            // Act
            var errors = _parser.GetSyntaxErrors(sql);

            // Assert
            Assert.NotEmpty(errors);
            var error = errors[0];
            Assert.True(error.Line > 0);
            Assert.NotEmpty(error.Message);
        }

        [Fact]
        public void SqlError_ToString_ReturnsFormattedString()
        {
            // Arrange
            var sql = "SELEC * FROM users";

            // Act
            var errors = _parser.GetSyntaxErrors(sql);

            // Assert
            Assert.NotEmpty(errors);
            var errorString = errors[0].ToString();
            Assert.Contains("Line", errorString);
            Assert.Contains("Column", errorString);
        }

        #endregion

        #region Complex SQL Tests

        [Fact]
        public void Parse_SelectWithJoin_Succeeds()
        {
            // Arrange
            var sql = @"
                SELECT u.id, u.name, o.order_date
                FROM users u
                INNER JOIN orders o ON u.id = o.user_id
                WHERE u.status = 'active'";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Parse_SelectWithSubquery_Succeeds()
        {
            // Arrange
            var sql = @"
                SELECT *
                FROM users
                WHERE id IN (SELECT user_id FROM orders WHERE amount > 100)";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Parse_InsertStatement_Succeeds()
        {
            // Arrange
            var sql = "INSERT INTO users (id, name) VALUES (1, 'test')";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.GetStatements<InsertStatement>());
        }

        [Fact]
        public void Parse_UpdateStatement_Succeeds()
        {
            // Arrange
            var sql = "UPDATE users SET name = 'updated' WHERE id = 1";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.GetStatements<UpdateStatement>());
        }

        [Fact]
        public void Parse_DeleteStatement_Succeeds()
        {
            // Arrange
            var sql = "DELETE FROM users WHERE id = 1";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.GetStatements<DeleteStatement>());
        }

        [Fact]
        public void Parse_CreateTable_Succeeds()
        {
            // Arrange
            var sql = @"
                CREATE TABLE users (
                    id INT PRIMARY KEY,
                    name NVARCHAR(100) NOT NULL,
                    created_at DATETIME DEFAULT GETDATE()
                )";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Parse_StoredProcedure_Succeeds()
        {
            // Arrange
            var sql = @"
                CREATE PROCEDURE GetUserById
                    @UserId INT
                AS
                BEGIN
                    SELECT * FROM users WHERE id = @UserId
                END";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Parse_CaseExpression_Succeeds()
        {
            // Arrange
            var sql = @"
                SELECT
                    id,
                    CASE
                        WHEN status = 1 THEN 'Active'
                        WHEN status = 2 THEN 'Inactive'
                        ELSE 'Unknown'
                    END AS status_text
                FROM users";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Parse_MultipleStatements_Succeeds()
        {
            // Arrange
            var sql = @"
                SELECT 1;
                SELECT 2;
                SELECT 3;";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.GetStatements().Count());
        }

        [Fact]
        public void Parse_SqlWithComments_PreservesComments()
        {
            // Arrange
            var sql = @"
                -- This is a comment
                SELECT id, name /* inline comment */ FROM users
                -- Another comment";

            // Act
            var result = _parser.ParseWithResult(sql);

            // Assert
            Assert.True(result.IsSuccess);
        }

        #endregion
    }
}

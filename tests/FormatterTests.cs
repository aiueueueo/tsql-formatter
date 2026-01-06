using TSqlFormatter.Core;
using Xunit;

namespace TSqlFormatter.Tests
{
    public class FormatterTests
    {
        #region Basic Tests

        [Fact]
        public void Format_SimpleSelect_ReturnsFormattedSql()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select id, name from users";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("SELECT", result);
            Assert.Contains("FROM", result);
        }

        [Fact]
        public void Format_InvalidSql_ReturnsOriginal()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select from where";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Equal(input, result);
        }

        [Fact]
        public void Format_EmptyString_ReturnsEmpty()
        {
            // Arrange
            var formatter = new Formatter();

            // Act
            var result = formatter.Format("");

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Format_NullString_ReturnsNull()
        {
            // Arrange
            var formatter = new Formatter();

            // Act
            var result = formatter.Format(null!);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Keyword Casing Tests

        [Fact]
        public void Format_WithUppercaseKeywords_FormatsCorrectly()
        {
            // Arrange
            var settings = new FormatterSettings { KeywordCasing = KeywordCasing.Uppercase };
            var formatter = new Formatter(settings);
            var input = "select id from users where id = 1";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("SELECT", result);
            Assert.Contains("FROM", result);
            Assert.Contains("WHERE", result);
        }

        [Fact]
        public void Format_WithLowercaseKeywords_FormatsCorrectly()
        {
            // Arrange
            var settings = new FormatterSettings { KeywordCasing = KeywordCasing.Lowercase };
            var formatter = new Formatter(settings);
            var input = "SELECT id FROM users WHERE id = 1";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("select", result);
            Assert.Contains("from", result);
            Assert.Contains("where", result);
        }

        #endregion

        #region Comma Placement Tests

        [Fact]
        public void Format_WithCommaBeforeColumn_FormatsCorrectly()
        {
            // Arrange
            var settings = new FormatterSettings { CommaPlacement = CommaPlacement.BeforeColumn };
            var formatter = new Formatter(settings);
            var input = "select id, name, email from users";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains(", name", result);
            Assert.Contains(", email", result);
        }

        #endregion

        #region Operator Spacing Tests

        [Fact]
        public void Format_WithSpaceAroundOperators_FormatsCorrectly()
        {
            // Arrange
            var settings = new FormatterSettings { SpaceAroundOperators = true };
            var formatter = new Formatter(settings);
            var input = "select * from users where id=1";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("= 1", result);
        }

        [Fact]
        public void Format_WithoutSpaceAroundOperators_FormatsCorrectly()
        {
            // Arrange
            var settings = new FormatterSettings { SpaceAroundOperators = false };
            var formatter = new Formatter(settings);
            var input = "select * from users where id = 1";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("=1", result);
        }

        #endregion

        #region AS Keyword Tests

        [Fact]
        public void Format_WithForceAsKeyword_AddsAs()
        {
            // Arrange
            var settings = new FormatterSettings { ForceAsKeyword = true };
            var formatter = new Formatter(settings);
            var input = "select id user_id from users u";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("AS user_id", result);
            Assert.Contains("AS u", result);
        }

        #endregion

        #region JOIN Tests

        [Fact]
        public void Format_SelectWithInnerJoin_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select u.id, o.amount from users u inner join orders o on u.id = o.user_id";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("INNER JOIN", result);
            Assert.Contains("ON", result);
        }

        [Fact]
        public void Format_SelectWithLeftJoin_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from users u left join orders o on u.id = o.user_id";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("LEFT JOIN", result);
        }

        #endregion

        #region CASE Expression Tests

        [Fact]
        public void Format_SearchedCaseExpression_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select case when status = 1 then 'Active' when status = 2 then 'Inactive' else 'Unknown' end from users";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("CASE", result);
            Assert.Contains("WHEN", result);
            Assert.Contains("THEN", result);
            Assert.Contains("ELSE", result);
            Assert.Contains("END", result);
        }

        #endregion

        #region INSERT Tests

        [Fact]
        public void Format_InsertStatement_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "insert into users (id, name) values (1, 'test')";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("INSERT INTO", result);
            Assert.Contains("VALUES", result);
        }

        #endregion

        #region UPDATE Tests

        [Fact]
        public void Format_UpdateStatement_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "update users set name = 'test' where id = 1";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("UPDATE", result);
            Assert.Contains("SET", result);
            Assert.Contains("WHERE", result);
        }

        #endregion

        #region DELETE Tests

        [Fact]
        public void Format_DeleteStatement_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "delete from users where id = 1";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("DELETE", result);
            Assert.Contains("WHERE", result);
        }

        #endregion

        #region Subquery Tests

        [Fact]
        public void Format_SelectWithSubquery_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from users where id in (select user_id from orders)";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("IN", result);
            Assert.Contains("(", result);
        }

        #endregion

        #region FormatWithDetails Tests

        [Fact]
        public void FormatWithDetails_ValidSql_ReturnsSuccess()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from users";

            // Act
            var result = formatter.FormatWithDetails(input);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.FormattedSql);
        }

        [Fact]
        public void FormatWithDetails_InvalidSql_ReturnsErrors()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select from where";

            // Act
            var result = formatter.FormatWithDetails(input);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void FormatWithDetails_InvalidSql_ReturnsOriginalSql()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select from where";

            // Act
            var result = formatter.FormatWithDetails(input);

            // Assert
            Assert.Equal(input, result.FormattedSql);
        }

        [Fact]
        public void FormatWithDetails_EmptySql_ReturnsSuccess()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "";

            // Act
            var result = formatter.FormatWithDetails(input);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
        }

        [Fact]
        public void FormatWithDetails_ErrorMessage_IsUserFriendly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from";  // incomplete SQL

            // Act
            var result = formatter.FormatWithDetails(input);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.ErrorMessage);
        }

        #endregion

        #region Boolean Operators Tests

        [Fact]
        public void Format_WithAndOperator_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from users where status = 1 and type = 2";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("AND", result);
        }

        [Fact]
        public void Format_WithOrOperator_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from users where status = 1 or type = 2";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("OR", result);
        }

        [Fact]
        public void Format_WithIsNull_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from users where name is null";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("IS NULL", result);
        }

        [Fact]
        public void Format_WithIsNotNull_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from users where name is not null";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("IS NOT NULL", result);
        }

        [Fact]
        public void Format_WithLike_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select * from users where name like '%test%'";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("LIKE", result);
        }

        #endregion

        #region Function Tests

        [Fact]
        public void Format_WithFunction_FormatsCorrectly()
        {
            // Arrange
            var formatter = new Formatter();
            var input = "select count(*), sum(amount) from orders";

            // Act
            var result = formatter.Format(input);

            // Assert
            Assert.Contains("COUNT", result);
            Assert.Contains("SUM", result);
        }

        #endregion
    }
}

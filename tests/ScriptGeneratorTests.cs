using TSqlFormatter.Core;
using TSqlFormatter.Core.Parser;
using Xunit;

namespace TSqlFormatter.Tests
{
    public class ScriptGeneratorTests
    {
        private readonly SqlParser _parser = new();

        [Fact]
        public void Generate_SimpleSelect_ReturnsFormattedSql()
        {
            // Arrange
            var sql = "select id,name from users";
            var parseResult = _parser.ParseWithResult(sql);
            var generator = new SqlScriptGenerator();

            // Act
            var result = generator.Generate(parseResult.Fragment!);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("SELECT", result); // Keywords should be uppercase
        }

        [Fact]
        public void Generate_WithSettings_AppliesKeywordCasing()
        {
            // Arrange
            var sql = "select id from users";
            var parseResult = _parser.ParseWithResult(sql);
            var settings = new FormatterSettings { KeywordCasing = KeywordCasing.Uppercase };
            var generator = new SqlScriptGenerator(settings);

            // Act
            var result = generator.Generate(parseResult.Fragment!);

            // Assert
            Assert.Contains("SELECT", result);
            Assert.Contains("FROM", result);
        }

        [Fact]
        public void Generate_SelectWithJoin_FormatsCorrectly()
        {
            // Arrange
            var sql = "select u.id,o.amount from users u inner join orders o on u.id=o.user_id";
            var parseResult = _parser.ParseWithResult(sql);
            var settings = new FormatterSettings
            {
                KeywordCasing = KeywordCasing.Uppercase,
                JoinOnSeparateLine = true,
                NewLinePerClause = true
            };
            var generator = new SqlScriptGenerator(settings);

            // Act
            var result = generator.Generate(parseResult.Fragment!);

            // Assert
            Assert.Contains("SELECT", result);
            Assert.Contains("INNER JOIN", result);
        }

        [Fact]
        public void Generate_InsertStatement_FormatsCorrectly()
        {
            // Arrange
            var sql = "insert into users(id,name)values(1,'test')";
            var parseResult = _parser.ParseWithResult(sql);
            var generator = new SqlScriptGenerator();

            // Act
            var result = generator.Generate(parseResult.Fragment!);

            // Assert
            Assert.Contains("INSERT", result);
            Assert.Contains("INTO", result);
            Assert.Contains("VALUES", result);
        }

        [Fact]
        public void Generate_UpdateStatement_FormatsCorrectly()
        {
            // Arrange
            var sql = "update users set name='test'where id=1";
            var parseResult = _parser.ParseWithResult(sql);
            var generator = new SqlScriptGenerator();

            // Act
            var result = generator.Generate(parseResult.Fragment!);

            // Assert
            Assert.Contains("UPDATE", result);
            Assert.Contains("SET", result);
            Assert.Contains("WHERE", result);
        }

        [Fact]
        public void Generate_DeleteStatement_FormatsCorrectly()
        {
            // Arrange
            var sql = "delete from users where id=1";
            var parseResult = _parser.ParseWithResult(sql);
            var generator = new SqlScriptGenerator();

            // Act
            var result = generator.Generate(parseResult.Fragment!);

            // Assert
            Assert.Contains("DELETE", result);
            Assert.Contains("users", result);
            Assert.Contains("WHERE", result);
        }

        [Fact]
        public void Generate_CaseExpression_FormatsCorrectly()
        {
            // Arrange
            var sql = "select case when status=1 then 'A' else 'B' end from users";
            var parseResult = _parser.ParseWithResult(sql);
            var generator = new SqlScriptGenerator();

            // Act
            var result = generator.Generate(parseResult.Fragment!);

            // Assert
            Assert.Contains("CASE", result);
            Assert.Contains("WHEN", result);
            Assert.Contains("THEN", result);
            Assert.Contains("ELSE", result);
            Assert.Contains("END", result);
        }
    }
}

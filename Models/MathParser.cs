using TokenType = MathEditor.Models.MathLexer.TokenType;

namespace MathEditor.Models;

public class MathParser
{
    public record AssignExpr(string VarName, string RawValue);
    public record EvalExpr(string VarName);

    public record ParseResult(
        AssignExpr? Assignment,
        EvalExpr? Evaluation,
        List<string> Errors
    );

    
    /// <summary>
    /// Parses a LaTeX expression
    /// </summary>
    /// <param name="latex">Raw LaTeX</param>
    /// <returns>The parsing result</returns>
    public static ParseResult Parse(string latex)
    {
        var tokens = MathLexer.Tokenize(latex);
        var errors = new List<string>();

        var assignIdx = tokens.FindIndex(t => t.Type == TokenType.Assign);
        var equalsIdx = tokens.FindIndex(t => t.Type == TokenType.Equals);

        AssignExpr? assignment = null;
        EvalExpr? evaluation = null;

        if (assignIdx >= 0)
        {
            var lhs = tokens[..assignIdx];
            var rhs = tokens[(assignIdx + 1)..];
            
            // Strip the trailing = from rhs if present (e.g. a := 3 =)
            var rhsTokens = equalsIdx > assignIdx
                ? tokens[(assignIdx + 1)..equalsIdx]
                : rhs;

            var varToken = lhs.FirstOrDefault(t => t.Type == TokenType.Variable);
            var hasLeadingNum = lhs.Any(t => t.Type == TokenType.Number);

            if (varToken == null)
                errors.Add("Left side of := must contain a variable");
            else if(hasLeadingNum)
                errors.Add("Invalid variable name: cannot start with a number.");
            else
            {
                var rawValue = string.Join(" ", rhsTokens.Select(t => t.Value));
                assignment = new AssignExpr(varToken.Value, rawValue);
            }
        }

        if (equalsIdx >= 0)
        {
            // Tokens to the left of = (but after := rhs if present)
            var lhsStart = assignIdx >= 0 ? assignIdx + 1 : 0;
            var lhs = tokens[lhsStart..equalsIdx];

            var varToken = lhs.FirstOrDefault(t => t.Type == TokenType.Variable);
            if (varToken != null)
                evaluation = new EvalExpr(varToken.Value);
        }

        return new ParseResult(assignment, evaluation, errors);
    }
}
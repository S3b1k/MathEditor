namespace MathEditor.Models;

public class MathLexer
{
    public enum TokenType
    {
        Variable,
        Number,
        Assign,
        Equals,
        Other
    }

    public record Token(TokenType Type, string Value);


    /// <summary>
    /// Turns the raw LaTeX into a list of tokens that a Parser can work with
    /// </summary>
    public static List<Token> Tokenize(string latex)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < latex.Length)
        {
            var current = latex[i];
            
            // Skip whitespace
            if (char.IsWhiteSpace(current))
            {
                i++;
                continue;
            }
            
            // LaTeX commands starting with backslash
            if (current == '\\')
            {
                var cmd = ReadCommand(latex, ref i);
                tokens.Add(cmd == "\\coloneq"
                    ? new Token(TokenType.Assign, cmd)
                    : new Token(TokenType.Other, cmd));
                continue;
            }
            
            // Equals sign
            if (latex[i] == '=')
            {
                tokens.Add(new Token(TokenType.Equals, "="));
                i++;
                continue;
            }
            
            // Numbers
            if (char.IsDigit(current))
            {
                tokens.Add(new Token(TokenType.Number, ReadWhile(latex, ref i, char.IsDigit)));
                continue;
            }
            
            // Variables
            if (char.IsLetter(current))
            {
                var name = ReadWhile(latex, ref i, char.IsLetterOrDigit);
                if (i < latex.Length && latex[i] == '_')
                {
                    i++;    // Consume '_'
                    name += "_" + ReadSubscript(latex, ref i);
                }
                tokens.Add(new Token(TokenType.Variable, name));
                continue;
            }
            
            // Anything else
            tokens.Add(new Token(TokenType.Other, latex[i].ToString()));
            i++;
        }

        return tokens;
    }


    private static string ReadCommand(string latex, ref int i)
    {
        var start = i++;
        while (i < latex.Length && char.IsLetter(latex[i])) 
            i++;
        return latex[start..i];
    }

    private static string ReadWhile(string latex, ref int i, Func<char, bool> predicate)
    {
        var start = i;
        while (i < latex.Length && predicate(latex[i]))
            i++;
        return latex[start..i];
    }

    private static string ReadSubscript(string latex, ref int i)
    {
        if (i < latex.Length && latex[i] == '{')
        {
            i++; // consume '{'
            var content = ReadWhile(latex, ref i, c => c != '{');
            if (i < latex.Length)   // consume '}'
                i++;
            return content;
        }

        return i < latex.Length ? latex[i++].ToString() : "";
    }
}
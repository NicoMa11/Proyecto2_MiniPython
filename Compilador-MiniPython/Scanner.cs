using Antlr4.Runtime;

public class Scanner
{
    private readonly MiniPythonLexer _lexer;
    private IToken _currentToken;

    public Scanner(string input)
    {
        var inputStream = new AntlrInputStream(input);
        _lexer = new MiniPythonLexer(inputStream);
        _currentToken = _lexer.NextToken();

        while (_currentToken.Type != MiniPythonLexer.Eof)
        {
            Console.WriteLine($"Token: {_lexer.Vocabulary.GetSymbolicName(_currentToken.Type)}, Lexema: {_currentToken.Text}");
            _currentToken = _lexer.NextToken();
        }
    }

    public Token GetNextToken()
    {
        if (_currentToken.Type == MiniPythonLexer.Eof)
            return null;

        var token = new Token(
            _lexer.Vocabulary.GetSymbolicName(_currentToken.Type),
            _currentToken.Text,
            _currentToken.Line,
            _currentToken.Column
        );

        _currentToken = _lexer.NextToken();

        return token;
    }
}

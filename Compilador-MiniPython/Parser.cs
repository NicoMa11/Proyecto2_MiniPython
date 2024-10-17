using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiniPython
{
    public class MiParser
    {
        private readonly MiniPythonParser _parser;
        private readonly CustomErrorListener _errorListener;

        public MiParser(MiniPythonLexer lexer)
        {
            var tokens = new CommonTokenStream(lexer);
            _parser = new MiniPythonParser(tokens);
            _errorListener = new CustomErrorListener();
            _parser.RemoveErrorListeners();
            _parser.AddErrorListener(_errorListener);
        }

        public void Parse()
        {
            IParseTree tree = _parser.program();
        }
        
        public List<ErrorInfo> GetErrors()
        {
            return _errorListener.Errors;
        }
    }

    public class CustomErrorListener : BaseErrorListener
    {
        public List<ErrorInfo> Errors { get; private set; } = new List<ErrorInfo>();

        public bool HasErrors => Errors.Count > 0;

        public override void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            string cleanedMsg = msg.Replace("\\r\\n", "").Replace("\\n", "").Replace("\\r", "");

            if (cleanedMsg.Contains("missing") || cleanedMsg.Contains("no viable alternative"))
            {
                if (!Errors.Any(error => error.Line == line && error.Message == cleanedMsg))
                {
                    if (cleanedMsg.Contains("missing") && (cleanedMsg.Contains(")") || cleanedMsg.Contains(":")))
                    {
                        Errors.Add(new ErrorInfo
                        {
                            Line = line,
                            Column = charPositionInLine,
                            Message = $"Detalles: {cleanedMsg}"
                        });
                    }
                }
            }
        }
    }

    public class ErrorInfo
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; }
    }
}

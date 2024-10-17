using Antlr4.Runtime;
using System.Collections.Generic;
using MiniPython;

public class ContextAnalyzer : MiniPythonBaseVisitor<object>
{
    private SymbolTable symbolTable;  // Maneja identificadores y métodos
    private List<string> errors;  // Lista de errores encontrados

    public ContextAnalyzer()
    {
        this.symbolTable = new SymbolTable();  
        this.errors = new List<string>();  
    }

    public bool HasErrors()
    {
        return this.errors.Count > 0;
    }

    private void LogError(string message, IToken token)
    {
        var errorBuilder = new System.Text.StringBuilder();
        errorBuilder.Append(message)
                    .Append(" (")
                    .Append(token.Line)
                    .Append(":")
                    .Append(token.Column)
                    .Append(")");
        errors.Add(errorBuilder.ToString());
    }

    public override object VisitProgramAST(MiniPythonParser.ProgramASTContext context)
    {
        VisitChildren(context);  // Visita todos los nodos hijos
        return null;
    }

    public override object VisitFunctionDeclarationAST(MiniPythonParser.MethodDeclarationASTContext context)
    {
        IToken functionId = context.ID().Symbol;  
        int paramCount = context.argList()?.ID().Length ?? 0;  

        // Comprobar si la función ya está definida en el mismo ámbito
        if (symbolTable.LookupInCurrentScope(functionId.Text) != null)
        {
            LogError($"La función '{functionId.Text}' ya ha sido definida en este ámbito.", functionId);
        }
        else
        {
            symbolTable.Insert(functionId, paramCount);
        }

        symbolTable.OpenScope();

        if (context.argList() != null)
        {
            foreach (var param in context.argList().ID())
            {
                symbolTable.Insert(param.Symbol, -1);  
            }
        }

        Visit(context.sequence());
        symbolTable.CloseScope();  
        return null;
    }

    public override object VisitFunctionCallAST(MiniPythonParser.MethodCallASTContext context)
    {
        IToken functionId = context.ID().Symbol;
        int argumentCount = context.expressionList()?.expression().Length ?? 0;

        // Verificar si la función está definida
        var functionSymbol = symbolTable.Lookup(functionId.Text);
        if (functionSymbol == null)
        {
            LogError($"Función no definida: {functionId.Text}", functionId);
        }
        else
        {
            int definedParamCount = functionSymbol.ParameterCount;
            if (argumentCount != definedParamCount)
            {
                LogError($"La función '{functionId.Text}' esperaba {definedParamCount} argumentos, pero recibió {argumentCount}.", functionId);
            }
        }

        return null;
    }

    public override object VisitVariableDeclarationAST(MiniPythonParser.VarSDASTContext context)
    {
        IToken variableId = context.ID().Symbol;

        if (symbolTable.LookupInCurrentScope(variableId.Text) != null)
        {
            LogError($"La variable '{variableId.Text}' ya ha sido declarada en este ámbito.", variableId);
        }
        else
        {
            symbolTable.Insert(variableId, -1); 
        }

        return null;
    }

    public override object VisitIdentifierUsageAST(MiniPythonParser.IdPEASTContext context)
    {
        IToken variableId = context.ID().Symbol;

        if (symbolTable.Lookup(variableId.Text) == null)
        {
            LogError($"Identificador no definido: {variableId.Text}", variableId);
        }

        return null;
    }

    public override object VisitLetStatementAST(MiniPythonParser.LetSCASTContext context)
    {
        symbolTable.OpenScope(); 
        Visit(context.declaration()); 
        Visit(context.singleCommand());  
        symbolTable.CloseScope();  

        return null;
    }
    
    public override object VisitNumberExpressionAST(MiniPythonParser.NumPEASTContext context)
    {
        if (context.elementAccess() != null)
        {
            LogError("No se puede indexar un número.", context.NUM().Symbol);
        }

        return null;
    }

    public override object VisitCharExpressionAST(MiniPythonParser.CharPEASTContext context)
    {
        if (context.PIZQ() != null)
        {
            LogError("No se puede llamar un carácter como función.", context.CHAR().Symbol);
        }

        return null;
    }

    public override string ToString()
    {
        if (!HasErrors()) return "0 errores de contexto";
        var resultBuilder = new System.Text.StringBuilder();
        foreach (string error in errors)
        {
            resultBuilder.Append($"{error}\n");
        }
        return resultBuilder.ToString();
    }
}

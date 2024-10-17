using System;
using System.Collections.Generic;
using Antlr4.Runtime;

public class SymbolTable
{

    private List<Dictionary<string, Ident>> tabla;
    private int nivelActual;

    public enum SymbolType {
        Variable,
        Function,
        Parameter
    }

    public abstract class Ident {
        public IToken Tok { get; }
        public SymbolType Type { get; }
        public int Nivel { get; }
        public int Valor { get; set; }

        public Ident(IToken tok, SymbolType type, int nivel) {
            Tok = tok;
            Type = type;
            Nivel = nivel;
            Valor = 0; 
        }

        public abstract void PrintIdent();
    }

    public class VarIdent : Ident {
        public bool IsConstant { get; }

        public VarIdent(IToken tok, SymbolType type, int nivel, bool isConstant)
            : base(tok, type, nivel) {
            IsConstant = isConstant;
        }

        public override void PrintIdent() {
            Console.WriteLine($"Variable: {Tok.Text}, Tipo: {Type}, Nivel: {Nivel}, Constante: {IsConstant}");
        }
    }

    public class MethodIdent : Ident {
        public List<string> Params { get; }

        public MethodIdent(IToken tok, SymbolType type, int nivel, List<string> parameters)
            : base(tok, type, nivel) {
            Params = parameters;
        }

        public override void PrintIdent() {
            Console.WriteLine($"Funcion: {Tok.Text}, Tipo: {Type}, Nivel: {Nivel}, Parametros: {Params.Count}");
        }
    }

    // Constructor
    public SymbolTable() {
        tabla = new List<Dictionary<string, Ident>>();
        nivelActual = -1; // Nivel inicial
    }

    // Métodos para abrir y cerrar scopes 
    public void OpenScope() {
        nivelActual++;
        if (nivelActual >= tabla.Count) {
            tabla.Add(new Dictionary<string, Ident>());
        }
    }

    public void CloseScope() {
        if (nivelActual >= 0) {
            tabla[nivelActual].Clear();
            nivelActual--;
        }
    }

    public void InsertarVariable(IToken id, SymbolType tipo, bool isConstant = false)
    {
        if (nivelActual >= 0 && nivelActual < tabla.Count)
        {
            VarIdent varIdent = new VarIdent(id, tipo, nivelActual, isConstant);
            tabla[nivelActual][id.Text] = varIdent;
        }
    }

    public void InsertarFuncion(IToken id, SymbolType tipo, List<string> paramsList)
    {
        if (nivelActual >= 0 && nivelActual < tabla.Count)
        {
            Ident methodIdent = new MethodIdent(id, tipo, nivelActual, paramsList);
            tabla[nivelActual][id.Text] = methodIdent;
        }
    }

    // Buscar un símbolo 
    public Ident Buscar(string nombre)
    {
        for (int i = nivelActual; i >= 0; i--)
        {
            if (tabla[i].ContainsKey(nombre))
            {
                return tabla[i][nombre];
            }
        }
        return null;
    }

    public Ident BuscarEnNivelActual(string nombre)
    {
        if (nivelActual >= 0 && nivelActual < tabla.Count && tabla[nivelActual].ContainsKey(nombre))
        {
            return tabla[nivelActual][nombre];
        }
        return null;
    }

    // Imprimir la tabla
    public void Imprimir()
    {
        Console.WriteLine("----- INICIO TABLA ------");
        for (int i = 0; i <= nivelActual; i++)
        {
            Console.WriteLine($"Nivel {i}:");
            foreach (var id in tabla[i].Values)
            {
                id.PrintIdent();
            }
        }
        Console.WriteLine("----- FIN TABLA ------");
    }
}

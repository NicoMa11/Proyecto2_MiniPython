parser grammar MiniPythonParser;

options {
    tokenVocab = MiniPythonLexer; // Usa los tokens definidos en MiniPythonLexer
}

// Entry point of the program
program
    : (mainStatement | NEWLINE)* EOF
    ;

// Main statements
mainStatement
    : defStatement
    | assignStatement
    | ifStatement
    | whileStatement
    | forStatement
    | printStatement
    | returnStatement
    | functionCallStatement
    ;

// Function definition
defStatement
    : DEF ID PIZQ argList? PDER DOSPUN NEWLINE INDENT sequence NEWLINE
    ;

// Argument list
argList
    : ID (COMMA ID)*
    ;

// If statement
ifStatement
    : IF expression DOSPUN NEWLINE INDENT sequence (ELSE DOSPUN NEWLINE INDENT sequence)? 
    ;

// While statement
whileStatement
    : WHILE expression DOSPUN NEWLINE INDENT sequence 
    ;

// For statement
forStatement
    : FOR expression IN expressionList DOSPUN NEWLINE INDENT sequence 
    ;

// Return statement
returnStatement
    : RETURN expression NEWLINE
    ;

// Print statement
printStatement
    : PRINT expression NEWLINE
    ;

// Assignment
assignStatement
    : ID ASIGN expression NEWLINE
    ;

// Function call
functionCallStatement
    : ID PIZQ expressionList? PDER NEWLINE
    ;

// Sequence of statements
sequence
    : (statement NEWLINE?)*
    ;

// Statement
statement
    : defStatement
    | ifStatement
    | whileStatement
    | forStatement
    | returnStatement
    | printStatement
    | assignStatement
    | functionCallStatement
    ;

// Expression
expression
    : additionExpression (comparison)?
    ;

// Comparison
comparison
    : (GT | LT | GE | LE | EQEQ | NOTEQ) additionExpression
    ;

// Addition expression
additionExpression
    : multiplicationExpression (additionFactor)*
    ;

// Addition factor
additionFactor
    : (SUM | REST) multiplicationExpression
    ;

// Multiplication expression
multiplicationExpression
    : elementExpression (multiplicationFactor)*
    ;

// Multiplication factor
multiplicationFactor
    : (MUL | DIV | MOD) elementExpression
    ;

// Element expression
elementExpression
    : primitiveExpression (elementAccess)?
    ;

// Element access
elementAccess
    : LBRACKET expression RBRACKET
    ;

// List of expressions
expressionList
    : expression (COMMA expression)*
    ;

// Primitive expression
primitiveExpression
    : (REST? NUM)                       
    | (REST? FLOAT)
    | STRING
    | ID (PIZQ expressionList? PDER)?
    | PIZQ expression PDER
    | LBRACKET expressionList? RBRACKET
    | LEN PIZQ expression PDER
    ;

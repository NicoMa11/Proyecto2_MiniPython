using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Antlr4.Runtime;
using MiniPython;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.MapPost("/parse", async (HttpRequest request) =>
{
    try
    {

        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var json = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        var code = json?["code"] ?? string.Empty;

        var lexer = new MiniPythonLexer(CharStreams.fromString(code));
        var parser = new MiniPythonParser(new CommonTokenStream(lexer));

        
        var visitor = new ContextAnalyzer();  
        visitor.Visit(tree);

        var errorListener = new CustomErrorListener();
        parser.AddErrorListener(errorListener);

        parser.program(); 
        if (errorListener.HasErrors)
            {
                return BadRequest(new
                {
                    error = "Parsing failed",
                    details = errorListener.Errors.Select(err => new
                    {
                        line = err.Line,
                        column = err.Column,
                        message = err.Message
                    })
                });
            }

            // Si no hay errores de parsing, hacer el análisis de contexto
            ContextAnalyzer contextAnalyzer = new ContextAnalyzer();
            contextAnalyzer.Visit(tree);

            // Verificar si hubo errores en el análisis de contexto
            if (contextAnalyzer.HasErrors)
            {
                return BadRequest(new
                {
                    error = "Context analysis failed",
                    details = contextAnalyzer.GetErrors().Select(err => new
                    {
                        message = err
                    })
                });
            }

            // Si todo pasa correctamente, devolver éxito
            return Ok(new { message = "Compilation passed successfully." });
        }
    catch (Exception ex)
    {
        var errorResponse = new
        {
            error = ex.Message,
            details = ex.StackTrace
        };
        return Results.Problem(JsonSerializer.Serialize(errorResponse));
    }
});


app.Run();

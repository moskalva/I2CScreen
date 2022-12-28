
using System;
using System.Runtime.CompilerServices;

public class Logger
{

    public static Logger Get([CallerMemberName] string callerName = null)
    {
        return new Logger(callerName);
    }

    private readonly string context;

    private Logger(string context)
    {
        this.context = context;
    }

    public void Warning(string message)
    {
        Console.WriteLine($"[{context}] Warning: {message}");
    }
    public void Info(string message)
    {
        Console.WriteLine($"[{context}] Info: {message}");
    }
    public void Error(string message)
    {
        Console.WriteLine($"[{context}] Error: {message}");
    }
}
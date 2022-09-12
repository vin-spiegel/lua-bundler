using System;

public static class Logger
{
    public static void Warn(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warn - {msg}");
        Console.ForegroundColor = ConsoleColor.White;
    }
    public static void Success(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Success - {msg}");
        Console.ForegroundColor = ConsoleColor.White;
    }
    public static void Error(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error - {msg}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
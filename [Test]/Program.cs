using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Convention;
using Convention.EasySave;
using Convention.Symbolization;

public class Program
{
    static void Main(string[] args)
    {
        // Create a new SymbolizationContext
        var context = new SymbolizationContext();
        // Compile a script from a file
        var toolFile = new ToolFile("example_script.txt");
        try
        {
            context.Compile(toolFile);
            Console.WriteLine("Script compiled successfully.");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            toolFile.Create();
        }
    }
}
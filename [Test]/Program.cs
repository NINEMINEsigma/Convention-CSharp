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
        var runner = new SymbolizationRunner();
        try
        {
            runner.Compile("example_script.txt");
            Console.WriteLine("Script compiled successfully.");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
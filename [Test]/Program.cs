using Convention.RScript;
using Flee.PublicTypes;
using System;

public class Program
{
    static void Main(string[] args)
    {
        RScriptEngine engine = new();
        RScriptImportClass import = new();
        import.Add(typeof(Math));
        var result = engine.Run(@"
double x;
double y;
double z;
double result;
bool stats;
stats = true;
x = 10.0;
y = 20.0;
z = x + y;
// This is a comment;
# This is another comment;
result = z * 2;
result = Pow(result,2.0);
", import);
        Console.WriteLine($"Script executed successfully. Result: {result["result"].data}");
    }
}
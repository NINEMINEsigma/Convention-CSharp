using Convention.RScript;
using Flee.PublicTypes;
using System;

public class Program
{
    static void Main(string[] args)
    {
        RScriptEngine engine = new();
        RScriptImportClass import = new()
        {
            typeof(Math)
        };
        var result = engine.Run(@"
double i;
i = 2.0;
{
    label(test);
    i = Pow(i,2.0);
    goto(100,i,test);
}
string result;
result = i.ToString()+i.ToString();
", import);
        Console.WriteLine($"Script executed successfully. Result: {result["result"].data}");
    }
}
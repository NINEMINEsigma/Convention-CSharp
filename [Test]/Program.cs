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
label(test);
{
    goto(true,func1);
    goto(100>i,test);
}
double result;
result = i;

goto(true,end);

label(func1);
{
    i = Pow(i,2.0);
}
back(true);

label(end);
", import);
        Console.WriteLine($"Script executed successfully. Result: {result["result"].data}");
    }
}
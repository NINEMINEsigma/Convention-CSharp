using Convention.RScript;
using System;

public class Program
{
    static void Main(string[] args)
    {
        RScriptEngine engine = new();
        RScriptImportClass import = new()
        {
            typeof(Math),
           
        };
        import.Add(typeof(ExpressionMath));
        var result = engine.Run(@"
int i;
i = 2.2;
label(test);
goto(true,func1);
goto(100>i,test);

namespace(func1)
{
    i = Pow(i,2.0);
}
", import);
        Console.WriteLine($"Script executed successfully. Result: {result["i"].data}");
    }
}
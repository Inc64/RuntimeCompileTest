// See https://aka.ms/new-console-template for more information

using RuntimeCompileTest;

Console.WriteLine("now we try to run some script from file..");

(new RoslynTest()).Run();

Console.WriteLine("program finished");

Console.ReadKey();

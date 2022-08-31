using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.Loader;

namespace RuntimeCompileTest
{
    internal class RoslynTest
    {
        public void Run()
        {
            // Где наше файло со скриптом
            // Итак, путь к exe
            // var filePath = AppDomain.CurrentDomain.BaseDirectory;
            // C:\\Projects\\Robocommerce\\RuntimeCompileTest\\RuntimeCompileTest\\bin\\Debug\\net6.0\\

            // значит скрипт где-то тут:
            var filePath = "..\\..\\..\\SomeScript.cs";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            // читаем скрипт в строку
            var code = File.ReadAllText(filePath);

            // ищем где там наш .net живёт
            var basePath = Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location);

            // реализуем синтаксическое дерево с указанной версией языка в качестве параметра
            var syntaxTree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(LanguageVersion.CSharp8));

            var rootSyntaxTree = syntaxTree.GetRoot() as CompilationUnitSyntax;

            if (rootSyntaxTree == null)
            {
                throw new InvalidOperationException(nameof(rootSyntaxTree));
            }

            var references = rootSyntaxTree.Usings;

            // собираем все юзинги
            var referencePaths = new List<string>
            {
                typeof(object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                Path.Combine(basePath, "System.Runtime.dll"),
                Path.Combine(basePath, "System.Runtime.Extensions.dll"),
                Path.Combine(basePath, "mscorlib.dll"),
                Path.Combine(basePath, "Microsoft.CSharp.dll"),
                Path.Combine(basePath, "System.Linq.Expressions.dll"),
                Path.Combine(basePath, "netstandard.dll"),
            };

            referencePaths.AddRange(references.Select(x => Path.Combine(basePath, $"{x.Name}.dll")));

            var executableReferences = new List<PortableExecutableReference>();

            foreach (var reference in referencePaths)
            {
                if (File.Exists(reference))
                {
                    executableReferences.Add(MetadataReference.CreateFromFile(reference));
                }
            }

            // лепим файл с рандомным названием, цель - dll
            var compilation = CSharpCompilation.Create(Path.GetRandomFileName(),
                new[] { syntaxTree },
                executableReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // открываем стрим
            using (var memoryStream = new MemoryStream())
            {
                // уличная магия девида блейна (направляем скомпиленный IL в указанный стрим )
                EmitResult compilationResult = compilation.Emit(memoryStream);

                // Если неуспешно - соберём ошибки
                if (!compilationResult.Success)
                {
                    var errors = compilationResult.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError
                    || diagnostic.Severity == DiagnosticSeverity.Error)?
                    .ToList() ?? new List<Diagnostic>();
                }
                else
                {
                    //если устпешно, то goto на начало потока
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // ну а дальше обычная рефлпксия
                    AssemblyLoadContext assemblyContext = new AssemblyLoadContext(Path.GetRandomFileName(), isCollectible: true);

                    // установка обработчика выгрузки
                    assemblyContext.Unloading += ContextUnloading;

                    Assembly assembly = assemblyContext.LoadFromStream(memoryStream);

                    Console.WriteLine("script loaded");

                    // ищем нужный класс тип в загруженной сборке
                    var type = assembly.GetType("RuntimeCompileTest.SomeScript");

                    if (type != null)
                    {
                        // Тут надо ISomeScript вынести куданить в отдельную dll и чтоб на неё using был в скрипте, тогда 
                        // вместо  dynamic тут мы приводим объект к типу ISomeScript и запускем его Execute
                        dynamic someObj = Activator.CreateInstance(type);

                        // запускаем
                        Console.WriteLine("script working result:");
                        someObj?.Execute();

                    }

                    assemblyContext.Unload();

                    // TODO:
                    // Наверное надо ещё те рандомный файлы поудалять
                }
            }
        }

        private void ContextUnloading(AssemblyLoadContext obj)
        {
            // Выгрузились, юзинги все левые тоже должны были выгрузиться
            Console.WriteLine("script unloaded");

            //, но если что - можно проверить:
            // смотрим, какие сборки после выгрузки
            //foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    Console.WriteLine(asm.GetName().Name);
            //}
        }
    }
}

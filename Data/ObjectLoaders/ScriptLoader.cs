using System;
using System.Collections.Generic;
using Godot;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Linq;
using System.IO;

namespace Stellacrum.Data.ObjectLoaders
{
    public class ScriptLoader
    {
        //public static void CompileAndRunCode()
        //{
        //    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("class HelloWorld { static void Main() { System.Console.WriteLine(\"Hello World!\"); } }");
        //    CSharpCompilation compilation = CSharpCompilation.Create("HelloWorld")
        //        .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
        //        .AddSyntaxTrees(syntaxTree);
        //
        //    EmitResult result = compilation.Emit("HelloWorld.dll");
        //
        //    if (!result.Success)
        //    {
        //        foreach (Diagnostic diagnostic in result.Diagnostics)
        //        {
        //            Console.WriteLine(diagnostic);
        //        }
        //    }
        //
        //    if (result.Success)
        //    {
        //        Assembly assembly = Assembly.LoadFrom("HelloWorld.dll");
        //        Type type = assembly.GetType("HelloWorld");
        //        MethodInfo method = type.GetMethod("Main");
        //        method.Invoke(null, null);
        //    }
        //}

        //static readonly string code = @"
        //    using System;
        //    using Godot;
        //
        //    class Program
        //    {
        //        static void Main(string[] args)
        //        {
        //            //GD.Print(""Test"");
        //            Console.WriteLine(""Hello World!"");
        //            Console.ReadLine();
        //        }
        //    }
        //";
        //
        //public static void CompileAndRunCode()
        //{
        //    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        //
        //    string assemblyName = Path.GetRandomFileName();
        //    MetadataReference[] references = new MetadataReference[]
        //    {
        //        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        //        //MetadataReference.CreateFromFile(typeof(GD).Assembly.Location, new())
        //        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        //    };
        //    CSharpCompilation compilation = CSharpCompilation.Create(
        //        assemblyName,
        //        new[] { syntaxTree },
        //        new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
        //        options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        //    );
        //
        //    using (var ms = new MemoryStream())
        //    {
        //        EmitResult result = compilation.Emit(ms);
        //
        //        if (result.Success)
        //        {
        //            ms.Seek(0, SeekOrigin.Begin);
        //            Assembly assembly = Assembly.Load(ms.ToArray());
        //            MethodInfo entryPoint = assembly.EntryPoint;
        //
        //            if (entryPoint != null)
        //            {
        //                object[] args = new object[] { new string[] { } };
        //                entryPoint.Invoke(null, args);
        //            }
        //            else
        //            {
        //                GD.Print("No entry point found.");
        //            }
        //        }
        //        else
        //        {
        //            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
        //                diagnostic.IsWarningAsError ||
        //                diagnostic.Severity == DiagnosticSeverity.Error);
        //
        //            foreach (Diagnostic diagnostic in failures)
        //            {
        //                GD.PrintErr($"{diagnostic.Id}: {diagnostic.GetMessage()}");
        //            }
        //        }
        //    }
        //}
    }
}

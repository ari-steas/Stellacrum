using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.IO;
using Godot;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Threading;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Stellacrum.Data.ObjectLoaders
{
    public class ScriptLoader
    {
        readonly CSharpCompilation _compilation;
        Assembly _generatedAssembly;
        Type? _proxyType;
        string _assemblyName;
        string _typeName;

        public ScriptLoader(string typeName, string code, Type[] typesToReference)
        {
            _typeName = typeName;
            var refs = typesToReference.Select(h => MetadataReference.CreateFromFile(h.Assembly.Location) as MetadataReference).ToList();

            //some default refeerences
            refs.Add(MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")));
            refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            //generate syntax tree from code and config compilation options
            var syntax = CSharpSyntaxTree.ParseText(code);
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                optimizationLevel: OptimizationLevel.Release);

            _compilation = CSharpCompilation.Create(_assemblyName = Guid.NewGuid().ToString(), new List<SyntaxTree> { syntax }, refs, options);
        }

        public Assembly CompileScript()
        {
            CSharpCompilation c = CreateCompilation("", null, false);

            using MemoryStream pdbStream = new MemoryStream();
            using MemoryStream assemblyStream = new MemoryStream();

            EmitResult emitResult = c.Emit(assemblyStream, pdbStream, null, null, null, null, null, null, null, null, default(CancellationToken));
            bool success = emitResult.Success;

            if (success)
                return Assembly.Load(assemblyStream.ToArray());
            else
                return null;
        }

        // shamelessly ripped from SE's code
        internal CSharpCompilation CreateCompilation(string assemblyFileName, IEnumerable<Script> scripts, bool enableDebugInformation)
        {
            CSharpCompilationOptions m_debugCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, null, null, OptimizationLevel.Debug, false, false, null, null, default(ImmutableArray<byte>), null, Platform.X64, ReportDiagnostic.Default, 4, null, true, false, null, null, null, null, null, false, MetadataImportOptions.Public);
            CSharpCompilationOptions m_runtimeCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, null, null, OptimizationLevel.Release, false, false, null, null, default(ImmutableArray<byte>), null, Platform.X64, ReportDiagnostic.Default, 4, null, true, false, null, null, null, null, null, false, MetadataImportOptions.Public);

            CSharpCompilationOptions options = enableDebugInformation ? m_debugCompilationOptions : m_runtimeCompilationOptions;
            IEnumerable<SyntaxTree> syntaxTrees = null;


            CSharpParseOptions m_conditionalParseOptions = new CSharpParseOptions(LanguageVersion.CSharp6, DocumentationMode.None, SourceCodeKind.Regular, null);
            HashSet<string> ConditionalCompilationSymbols = new HashSet<string>();

            List<MetadataReference> m_metadataReferences = new List<MetadataReference>();

            if (scripts != null)
            {
                CSharpParseOptions parseOptions = m_conditionalParseOptions.WithPreprocessorSymbols(ConditionalCompilationSymbols);
                syntaxTrees = from s in scripts
                              select CSharpSyntaxTree.ParseText(s.Code, parseOptions, s.Name, Encoding.UTF8, default(CancellationToken));
            }
            return CSharpCompilation.Create(MakeAssemblyName(assemblyFileName), syntaxTrees, m_metadataReferences, options);
        }
        private static string MakeAssemblyName(string name)
        {
            if (name == null)
            {
                return "scripts.dll";
            }
            return Path.GetFileName(name);
        }









        public Type Compile()
        {

            if (_proxyType != null) return _proxyType;

            using (var ms = new MemoryStream())
            {
                var result = _compilation.Emit(ms);
                if (!result.Success)
                {
                    var compilationErrors = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error)
                        .ToList();
                    if (compilationErrors.Any())
                    {
                        var firstError = compilationErrors.First();
                        var errorNumber = firstError.Id;
                        var errorDescription = firstError.GetMessage();
                        var firstErrorMessage = $"{errorNumber}: {errorDescription};";
                        var exception = new Exception($"Compilation failed, first error is: {firstErrorMessage}");
                        compilationErrors.ForEach(e => { if (!exception.Data.Contains(e.Id)) exception.Data.Add(e.Id, e.GetMessage()); });
                        throw exception;
                    }
                }
                ms.Seek(0, SeekOrigin.Begin);

                _generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                _proxyType = _generatedAssembly.GetType(_typeName);
                return _proxyType;
            }
        }
    }
}

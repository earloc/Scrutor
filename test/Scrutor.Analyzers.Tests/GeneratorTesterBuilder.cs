using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Scrutor.Analyzers.Tests;

internal sealed class GeneratorTesterBuilder<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    internal static GeneratorTesterBuilder<TGenerator> Create(string baseDirectory, string? rootNamespace = null, string? useParamNamesInMethodNames = null) => new(baseDirectory, rootNamespace, useParamNamesInMethodNames);

    private readonly DirectoryInfo baseDirectory;
    private readonly List<FileInfo> sourceFiles = [];
    private readonly List<FileInfo> resxFiles = [];
    private readonly Dictionary<string, string> customToolNamespaces = [];
    private readonly Dictionary<string, string> useParamNamesInMethodNames = [];

    private readonly string? rootNamespace;
    private readonly string? useParamNamesInMethodNamesBuildProperty;

    public GeneratorTesterBuilder(string baseDirectory, string? rootNamespace = null, string? useParamNamesInMethodNames = null)
    {
        this.baseDirectory = new DirectoryInfo(baseDirectory);

        if (!this.baseDirectory.Exists)
        {
            throw new ArgumentException($"the specified directory {this.baseDirectory.FullName} does not exist", nameof(baseDirectory));
        }

        this.rootNamespace = rootNamespace;
        useParamNamesInMethodNamesBuildProperty = useParamNamesInMethodNames;
    }

    public GeneratorTesterBuilder<TGenerator> WithSourceFile(string fileName)
    {
        var path = Path.Combine(baseDirectory.FullName, fileName);

        var fileInfo = new FileInfo(path);

        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("source file not found", fileInfo.FullName);
        }
        sourceFiles.Add(fileInfo);
        return this;
    }

    public IVerifiable Build(bool withDateAndTimeOnly = true)
    {
        var syntaxTrees = sourceFiles
            .Select(x => new { Path = x.FullName, Content = File.ReadAllText(x.FullName) })
            .Select(x => CSharpSyntaxTree.ParseText(x.Content, path: x.Path))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName: "Scrutor.Analyzers.Tests.Dynamic",
            syntaxTrees: syntaxTrees
        );

        if (withDateAndTimeOnly)
        {
            compilation = compilation.AddReferences(
                MetadataReference.CreateFromFile(typeof(DateOnly).Assembly.Location)
            );
        }

        var generator = new TGenerator();
        var driver = CSharpGeneratorDriver.Create([generator]);

        var generatorDriver = driver.RunGenerators(compilation.AddReferences());

        return new GeneratorTester(generatorDriver, Path.Combine(baseDirectory.FullName, ".snapshots"));
    }
}


using Foo;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Scrutor.Analyzers.Tests;

internal sealed class GeneratorTesterBuilder<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    internal static GeneratorTesterBuilder<TGenerator> Create(string baseDirectory) => new(baseDirectory);

    private readonly DirectoryInfo baseDirectory;
    private readonly List<FileInfo> sourceFiles = [];

    public GeneratorTesterBuilder(string baseDirectory)
    {
        this.baseDirectory = new DirectoryInfo(baseDirectory);

        if (!this.baseDirectory.Exists)
        {
            throw new ArgumentException($"the specified directory {this.baseDirectory.FullName} does not exist", nameof(baseDirectory));
        }
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

    public IVerifiable Build()
    {
        var syntaxTrees = sourceFiles
            .Select(x => new { Path = x.FullName, Content = File.ReadAllText(x.FullName) })
            .Select(x => CSharpSyntaxTree.ParseText(x.Content, path: x.Path))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName: "Scrutor.Analyzers.Tests.Dynamic",
            syntaxTrees: syntaxTrees,
            references: [MetadataReference.CreateFromFile(typeof(GeneratorTesterBuilder<>).Assembly.Location)]
        );

        var generator = new TGenerator();
        var driver = CSharpGeneratorDriver.Create([generator]);

        var generatorDriver = driver.RunGenerators(compilation);

        return new GeneratorTester(generatorDriver, Path.Combine(baseDirectory.FullName, ".snapshots"));
    }
}


using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Illumination;

public static class PersistedAssemblyBuilderExtensions
{
	public static async Task Save(this PersistedAssemblyBuilder assemblyBuilder, String filePath, MethodInfo? entryPoint = null)
	{
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
		
		MetadataBuilder metadataBuilder = assemblyBuilder.GenerateMetadata(out BlobBuilder ilStream, out BlobBuilder fieldData);

		(MethodDefinitionHandle mdh, Characteristics c) = entryPoint switch
		{
			null => (default, Characteristics.Dll),
			_ => (MetadataTokens.MethodDefinitionHandle(entryPoint.MetadataToken), Characteristics.ExecutableImage)
		};
		
		ManagedPEBuilder peBuilder = new(
			header: new PEHeaderBuilder(imageCharacteristics: c),
			metadataRootBuilder: new MetadataRootBuilder(metadataBuilder),
			ilStream: ilStream,
			mappedFieldData: fieldData,
			entryPoint: mdh
		);

		BlobBuilder peBlob = new();
		peBuilder.Serialize(peBlob);

		using MemoryStream ms = new(peBlob.Count);
		peBlob.WriteContentTo(ms);

		await using FileStream fileStream = new(filePath, new FileStreamOptions
		{
			PreallocationSize = peBlob.Count,
			Access = FileAccess.Write,
			Mode = FileMode.Create
		});
		await fileStream.WriteAsync(ms.GetBuffer().AsMemory(0, (Int32)ms.Length));
	}

	public static async Task SaveToExecutable(
		this PersistedAssemblyBuilder assemblyBuilder,
		String directory,
		MethodInfo entryPoint)
	{
        if (!Directory.Exists(directory))
	        throw new DirectoryNotFoundException(directory);
        var absoluteDirectory = Path.GetFullPath(directory);
		
		var name = assemblyBuilder.GetName().Name ?? throw new("Assembly has no name, but a name is required");
		var filename = name + ".dll";
		using var tempDir = Raii.Create(() => Directory.CreateTempSubdirectory(), x => x.Delete(recursive: true));
		var savePath = Path.Combine(tempDir.Value.FullName, filename);
		await assemblyBuilder.Save(savePath, entryPoint);
		const String projectName = "Runner";
		var publishDir = Path.Combine(tempDir.Value.FullName, "publish");
		await File.WriteAllTextAsync(
			Path.Combine(tempDir.Value.FullName, $"{projectName}.csproj"),
			$"""
			<Project Sdk="Microsoft.NET.Sdk">

				<PropertyGroup>
					<OutputType>Exe</OutputType>
					<TargetFramework>net9.0</TargetFramework>
					<Nullable>enable</Nullable>
					<SelfContained>false</SelfContained>
					<PublishSingleFile>true</PublishSingleFile>
					<PublishDir>{publishDir}</PublishDir>
					<DebugType>none</DebugType>
					<DebugSymbols>false</DebugSymbols>
				</PropertyGroup>

				<ItemGroup>
					<Reference Include="{filename}" />
				</ItemGroup>

			</Project>
			""");
		var src =
			(entryPoint.ReturnType.FullName == typeof(void).FullName ? String.Empty : "return ") +
			(entryPoint.DeclaringType?.FullName is { } x ? x + '.' : String.Empty) +
			entryPoint.Name + "();";
		await File.WriteAllTextAsync(Path.Combine(tempDir.Value.FullName, "Program.cs"), src);
		var process = Process.Start(new ProcessStartInfo
			{
				WorkingDirectory = tempDir.Value.FullName,
				FileName = "dotnet",
				Arguments = $"publish ./{projectName}.csproj -c Release"
			}) ?? throw new InvalidOperationException("Failed to start dotnet publish process.");
		await process.WaitForExitAsync();
		if (process.ExitCode != 0)
			throw new InvalidOperationException($"dotnet publish failed with exit code {process.ExitCode}.");
		var publishedExe = Path.Combine(publishDir, projectName);
		var targetExe = Path.Combine(absoluteDirectory, name);
		if (OperatingSystem.IsWindows()) { publishedExe += ".exe"; targetExe += ".exe"; }
		if (File.Exists(targetExe)) File.Delete(targetExe);
		File.Move(publishedExe, targetExe);
	}
}
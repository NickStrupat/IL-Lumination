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
			Mode = FileMode.CreateNew
		});
		await fileStream.WriteAsync(ms.GetBuffer());
	}

	public static async Task SaveToExecutable(this PersistedAssemblyBuilder assemblyBuilder,
		String filePath,
		MethodInfo entryPoint)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
		
		var fileName = Path.GetFileNameWithoutExtension(filePath) ?? throw new InvalidOperationException();
		var tempDir = Directory.CreateTempSubdirectory();
		try
		{
			var filename = assemblyBuilder.GetName().Name + ".dll";
			var savePath = Path.Combine(tempDir.FullName, filename);
			await assemblyBuilder.Save(savePath, entryPoint);
			await File.WriteAllTextAsync(Path.Combine(tempDir.FullName, "Runner.csproj"), $"""
			                                                                                   <Project Sdk="Microsoft.NET.Sdk">
			                                                                                   
			                                                                                       <PropertyGroup>
			                                                                                           <OutputType>Exe</OutputType>
			                                                                                           <TargetFramework>net9.0</TargetFramework>
			                                                                                           <Nullable>enable</Nullable>
			                                                                                           <PublishSingleFile>true</PublishSingleFile>
			                                                                                           <SelfContained>false</SelfContained>
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
				;
			await File.WriteAllTextAsync(Path.Combine(tempDir.FullName, "Program.cs"), src);
			//await Process.Start("dotnet", "publish ./Runner.csproj -c Release").WaitForExitAsync();
		}
		finally
		{
			tempDir.Delete(recursive: true);
		}
	}
}
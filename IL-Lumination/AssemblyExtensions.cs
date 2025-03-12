using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Illumination;

public static class AssemblyExtensions
{
	public static async Task Save(this Assembly assembly, String path, MethodInfo? entryPoint = null)
	{
		var generator = new Lokad.ILPack.AssemblyGenerator();
		const String switchName = "Lokad.ILPack.AssemblyGenerator.ReplaceCoreLibWithNetStandard";
		var wasSwitchPreviouslySet = AppContext.TryGetSwitch(switchName, out var previousValue);
		AppContext.SetSwitch(switchName, true);
		try
		{
			var bytes = generator.GenerateAssemblyBytes(assembly, [], entryPoint);
			await File.WriteAllBytesAsync(path, bytes);
		}
		finally
		{
			if (wasSwitchPreviouslySet)
				AppContext.SetSwitch(switchName, previousValue);
		}
	}

	public static async Task SaveExecutable(this Assembly assembly, String directory, MethodInfo entryPoint)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(directory);
		ArgumentNullException.ThrowIfNull(entryPoint);

		var absoluteDirectory = Path.GetFullPath(directory);
		
		var tempDir = Directory.CreateTempSubdirectory();
		try
		{
			var name = assembly.GetName().Name ?? throw new("Assembly has no name, but a name is required");
			var filename = name + ".dll";
			var savePath = Path.Combine(tempDir.FullName, filename);
			// var path = Path.Combine(directory, name);
			await assembly.Save(savePath, entryPoint);
			const String projectName = "Runner";
			var publishDir = directory;//"publish";
			await File.WriteAllTextAsync(Path.Combine(tempDir.FullName, $"{projectName}.csproj"), $"""
				<Project Sdk="Microsoft.NET.Sdk">

					<PropertyGroup>
						<OutputType>Exe</OutputType>
						<TargetFramework>net9.0</TargetFramework>
						<Nullable>enable</Nullable>
						<PublishSingleFile>true</PublishSingleFile>
						<SelfContained>false</SelfContained>
						<PublishDir>{absoluteDirectory}</PublishDir>
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
				;
			await File.WriteAllTextAsync(Path.Combine(tempDir.FullName, "Program.cs"), src);
			await Process
				.Start(new ProcessStartInfo
				{
					WorkingDirectory = tempDir.FullName,
					FileName = "dotnet",
					Arguments = $"publish ./{projectName}.csproj -c Release"
				})!
				.WaitForExitAsync();
		}
		finally
		{
			tempDir.Delete(recursive:true);
		}
	}
}
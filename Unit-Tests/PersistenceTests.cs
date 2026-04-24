using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Illumination.Builders.Extensions;
using Xunit;

namespace Unit_Tests;

public class PersistenceTests : IDisposable
{
	private readonly String tempDir = Path.Combine(Path.GetTempPath(), "IL-Lumination-Tests-" + Guid.NewGuid().ToString("N"));

	public PersistenceTests()
	{
		Directory.CreateDirectory(tempDir);
	}

	public void Dispose()
	{
		if (Directory.Exists(tempDir))
			Directory.Delete(tempDir, recursive: true);
	}

	[Fact]
	public async Task SaveToDll_LoadAndInvoke_ReturnsCorrectValue()
	{
		var dllPath = Path.Combine(tempDir, "Test.dll");

		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("Test")
			.NewType(out var type, t => t
				.Name("Calculator")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("Double")
					.ReturnType<Int32>()
					.NewParameter<Int32>(out var x)
					.Body(b => b
						.Ldarg(x)
						.Ldc_I4_2()
						.Mul()
						.Ret()
					)
				)
			);

		await ab.Save(dllPath);

		Assert.True(File.Exists(dllPath));
		Assert.True(new FileInfo(dllPath).Length > 0);

		var loaded = System.Reflection.Assembly.LoadFrom(dllPath);
		var loadedType = loaded.GetType("Calculator")!;
		var method = loadedType.GetMethod("Double")!;
		var del = method.CreateDelegate<Func<Int32, Int32>>();

		Assert.Equal(10, del(5));
		Assert.Equal(0, del(0));
		Assert.Equal(-4, del(-2));
	}

	[Fact]
	public async Task SaveToDll_MultipleTypesAndMethods_AllAccessible()
	{
		var dllPath = Path.Combine(tempDir, "Multi.dll");

		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("Multi")
			.NewType(out var typeA, t => t
				.Name("TypeA")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("GetA")
					.ReturnType<Int32>()
					.Body(b => b.Ldc_I4(1).Ret())
				)
			)
			.NewType(out var typeB, t => t
				.Name("TypeB")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("GetB")
					.ReturnType<Int32>()
					.Body(b => b.Ldc_I4(2).Ret())
				)
			)
			.NewEnum<Int32>(e => e
				.Name("Color")
				.Public()
				.NewLiteral(l => l.Name("Red").Value(0))
				.NewLiteral(l => l.Name("Green").Value(1))
			);

		await ab.Save(dllPath);

		var loaded = System.Reflection.Assembly.LoadFrom(dllPath);

		Assert.Equal(1, loaded.GetType("TypeA")!.GetMethod("GetA")!.Invoke(null, null));
		Assert.Equal(2, loaded.GetType("TypeB")!.GetMethod("GetB")!.Invoke(null, null));

		var colorType = loaded.GetType("Color")!;
		Assert.True(colorType.IsEnum);
		Assert.Equal(typeof(Int32), Enum.GetUnderlyingType(colorType));
	}

	[Fact]
	public async Task SaveToDll_GlobalMethod_InvokableFromModule()
	{
		var dllPath = Path.Combine(tempDir, "GlobalMethods.dll");

		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("GlobalMethods")
			.NewMethod(m => m
				.Name("Add")
				.Public()
				.ReturnType<Int32>()
				.NewParameter<Int32>(out var a)
				.NewParameter<Int32>(out var b)
				.Body(body => body
					.Ldarg(a)
					.Ldarg(b)
					.Add()
					.Ret()
				)
			);

		await ab.Save(dllPath);

		var loaded = System.Reflection.Assembly.LoadFrom(dllPath);
		var method = loaded.Modules.First().GetMethod("Add")!;
		var del = method.CreateDelegate<Func<Int32, Int32, Int32>>();

		Assert.Equal(7, del(3, 4));
	}

	[Fact]
	public async Task SaveToExecutable_CreatesRunnableExecutable()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("ExeTest")
			.NewType(out var type, t => t
				.Name("Greeter")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("Main")
					.EntryPoint()
					.Body(b => b
						.Ldstr("IL-Lumination works!")
						.Call(typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(String)])!)
						.Ret()
					)
				)
			);

		await ab.SaveToExecutable(tempDir);

		var exeName = "ExeTest";
		if (OperatingSystem.IsWindows()) exeName += ".exe";
		var exePath = Path.Combine(tempDir, exeName);

		Assert.True(File.Exists(exePath), $"Executable not found at {exePath}");

		var filesInDir = Directory.GetFiles(tempDir);
		Assert.Single(filesInDir);

		var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
		{
			FileName = exePath,
			RedirectStandardOutput = true
		})!;
		await process.WaitForExitAsync();
		var output = await process.StandardOutput.ReadToEndAsync();

		Assert.Equal(0, process.ExitCode);
		Assert.Contains("IL-Lumination works!", output);
	}
}

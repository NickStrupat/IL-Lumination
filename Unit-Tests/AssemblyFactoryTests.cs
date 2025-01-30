using System;
using System.IO;
using Illumination;
using Lokad.ILPack;
using Xunit;

namespace Unit_Tests;

public class AssemblyFactoryTests
{
	[Fact]
	public void AssemblyFactory_Generate()
	{
		var assembly = new XAssembly("TestAssembly");
		var module = assembly.Module("TestModule");
		var m1 = assembly.AssemblyBuilder.Modules;
		var m2 = assembly.Modules;

		AppContext.SetSwitch("Lokad.ILPack.AssemblyGenerator.ReplaceCoreLibWithNetStandard", true);
		var generator = new AssemblyGenerator();
		var dir = Path.Combine(Environment.CurrentDirectory, "bin");
		Directory.CreateDirectory(dir);
		var path = Path.Combine(dir, assembly.AssemblyBuilder.GetName().Name + ".dll");
		generator.GenerateAssembly(assembly.AssemblyBuilder, path);
	}
}

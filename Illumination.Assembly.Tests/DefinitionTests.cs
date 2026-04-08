using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Illumination.Assembly.Tests;

public class DefinitionTests(ITestOutputHelper toh)
{
	[Fact]
	public void Test()
	{
		using var provider = CreateServiceProvider(toh);
		var factory = provider.GetRequiredService<IDbContextFactory<DbContext>>();
		using var context = factory.CreateDbContext();
		var methodDefs = context.Set<MethodDef>().ToList();
	}

	[Fact]
	public void Emit()
	{
		using var provider = CreateServiceProvider(toh);
		var factory = provider.GetRequiredService<IDbContextFactory<DbContext>>();
		
		using (var context = factory.CreateDbContext())
		{
			var assemblyDef = new AssemblyDef("Asm");
			assemblyDef.Modules.Add(new ModuleDef("Mod") { Parent = assemblyDef });
			context.Set<AssemblyDef>().Add(assemblyDef);
			context.SaveChanges();
		}

		using (var context = factory.CreateDbContext())
		{
			foreach (var assemblyDef in context.Set<AssemblyDef>().ToList())
			{
				//var asdf = assemblyDef.CreateAssembly(context);
			}
		}
	}

	[Fact]
	public void CreateAssembly()
	{
		using var provider = CreateServiceProvider(toh);
		var factory = provider.GetRequiredService<IDbContextFactory<DbContext>>();
		
		using (var context = factory.CreateDbContext())
		{
			var assemblyDef = new AssemblyDef("Asm");
			var moduleDef = new ModuleDef("Mod") { Parent = assemblyDef };
			assemblyDef.Modules.Add(moduleDef);
			var globalTypeDef = new TypeDef.GlobalTypeDef("Type") { Parent = moduleDef };
			moduleDef.Types.Add(globalTypeDef);
			globalTypeDef.Types.Add(new TypeDef.NestedTypeDef("NestedType") { Parent = globalTypeDef });
			context.Set<AssemblyDef>().Add(assemblyDef);
			context.SaveChanges();
		}
		
		using (var context = factory.CreateDbContext())
		{
			foreach (var assemblyDef in context.Set<AssemblyDef>().ToList())
			{
				var asdf = assemblyDef.CreateAssembly(context);
			}
		}
	}

	private static ServiceProvider CreateServiceProvider(ITestOutputHelper toh)
	{
		var services = new ServiceCollection();
		services.AddAssemblyStuff((sp, ob) => ob.LogTo(toh.WriteLine, LogLevel.Information));
		var provider =  services.BuildServiceProvider();
		var factory = provider.GetRequiredService<IDbContextFactory<DbContext>>();
		using var context = factory.CreateDbContext();
		context.Database.EnsureCreated();
		return provider;
	}
}
using System;
using System.Linq;
using Illumination;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Unit_Tests;

public class DefinitionTests(ITestOutputHelper toh)
{
	[Fact]
	public void Test()
	{
		using var provider = CreateServiceProvider(toh);
		var factory = provider.GetRequiredService<IDbContextFactory<Context>>();
		using var context = factory.CreateDbContext();
		var methodDefs = context.Set<MethodDef>().ToList();
	}

	[Fact]
	public void Emit()
	{
		using var provider = CreateServiceProvider(toh);
		var factory = provider.GetRequiredService<IDbContextFactory<Context>>();
		
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

	private static ServiceProvider CreateServiceProvider(ITestOutputHelper toh)
	{
		var services = new ServiceCollection();
		services.AddSingleton(_ =>
		{
			var conn = new SqliteConnection("Data Source=InMemorySample;Mode=Memory;Cache=Shared");
			conn.Open();
			return conn;
		});
		services.AddSingleton(toh);
		services.AddPooledDbContextFactory<Context>((sp, ob) => ob
			.UseSqlite(sp.GetRequiredService<SqliteConnection>())
			.LogTo(toh.WriteLine, LogLevel.Information)
		);
		var provider =  services.BuildServiceProvider();
		var factory = provider.GetRequiredService<IDbContextFactory<Context>>();
		using var context = factory.CreateDbContext();
		context.Database.EnsureCreated();
		return provider;
	}

	public class Context(DbContextOptions options) : DbContext(options)
	{
		protected override void OnModelCreating(ModelBuilder mb)
		{
			var defType = typeof(Definition);
			var det = mb.Entity(defType);
			// const String keyName = "Id";
			// det.Property(typeof(Int32), keyName);
			// det.HasKey(keyName);
			det.UseTphMappingStrategy();
			var defTypes = defType.Assembly.GetTypes().Where(x => x.IsSubclassOf(defType) && x != defType);
			foreach (var type in defTypes)
			{
				var etb = mb.Entity(type);
				//etb.UseTptMappingStrategy();
				// if (type.BaseType == defType)
				// {
				// 	etb.Property(typeof(Int32), keyName);
				// 	etb.HasKey(keyName);
				// }
			}

			var namedDefEtb = mb.Entity<Definition>();
			 namedDefEtb.HasIndex(x => x.Name);
			 namedDefEtb.HasIndex(x => new { x.ParentId, x.Name }).IsUnique();
			namedDefEtb.HasIndex(x => x.ParentId);
		}
	}
}
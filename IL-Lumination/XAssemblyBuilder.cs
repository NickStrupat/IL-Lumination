// using System;
// using System.Reflection;
// using System.Reflection.Emit;
// using NickStrupat;
//
// namespace Illumination;
//
// public abstract class XAssemblyBuilder
// {
// 	private XAssemblyBuilder(String name) => Name = name;
//
// 	public String Name { get; }
// 	public EquatableHashSet<XTypeBuilder> Types { get; } = new(EqCmp<XTypeBuilder>.Create(x => (x.Name, x.TypeParameters.Count)));
//
// 	public Assembly Build()
// 	{
// 		var assemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new(Name), AssemblyBuilderAccess.RunAndCollect);
// 		var moduleBuilder = assemblyBuilder.DefineDynamicModule(Name);
// 		foreach (var type in Types)
// 			_ = type.BuildInternal(moduleBuilder);
// 		return assemblyBuilder;
// 	}
//
// 	// public void GenerateAssembly(String assemblyFilePath)
// 	// {
// 	//     AppContext.SetSwitch("Lokad.ILPack.AssemblyGenerator.ReplaceCoreLibWithNetStandard", true);
// 	//     var generator = new AssemblyGenerator();
// 	//     var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new(Name), AssemblyBuilderAccess.RunAndCollect);
// 	//     generator.GenerateAssembly(assemblyBuilder, assemblyFilePath);
// 	// }
// }
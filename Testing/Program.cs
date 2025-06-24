using Illumination.Builders;

Action<String> println = Console.WriteLine;
var assemblyBuilder = new AssemblyBuilder()
	.Name("TestAssembly")
	.NewType(out var baseType, x => x.Name("BaseType").Public()
		.NewMethod(x => x.Public().Static().Name("Main").EntryPoint()
			.ReturnType(typeof(void))
			.Body(x => x.Ldstr("Hello").Call(println.Method).Ret())))
	.NewType(x => x.Name("TestType").BaseType(baseType))
	.NewEnum<Byte>(x => x.Name("enum").Public()
		.NewLiteral(x => x.Name("A").Value(0))
		.NewLiteral(x => x.Name("B").Value(1))
	)
	.NewMethod(x => x.Name("TestMethod").Public()
		.ReturnType(typeof(Int32))
		.NewParameter<Int32>(out var xParam)
		.Body(x => x.Ldarg(xParam).Ldc_I4_2().Mul_Ovf().Ret())
	)
	;
// baseType.Name("BaseType").Public().NewMethod(out var main);
// main.Public().Static().Name("Main").ReturnType(typeof(void)).Body(x => x.Ldstr("Hello").Call(println.Method).Ret());
// assemblyBuilder.EntryPoint(main);
// var assembly = assemblyBuilder.Create(out var entryPoint);
// var mainMeth = assembly.GetType("BaseType")!.GetMethod("Main");
// await assembly.SaveExecutable(Environment.CurrentDirectory, mainMeth);
await assemblyBuilder.Save("test5.dll");
await assemblyBuilder.SaveToExecutable(Environment.CurrentDirectory);
// var module = assembly.Modules.Single(); //assembly.GetModule("TestModule") ?? throw new InvalidOperationException("no module");
// var mainMi = module.GetMethod("Main") ?? throw new InvalidOperationException("no method");
// mainMi.Invoke(null, []);
// var method = module.GetMethod("TestMethod") ?? throw new InvalidOperationException("no method");
// var del = method.CreateDelegate<Func<Int32, Int32>>();
// var result = del(8);
// Console.WriteLine(result);

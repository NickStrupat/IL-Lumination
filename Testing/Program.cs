using System.Reflection;
using Illumination;

var assemblyBuilder = new AssemblyBuilder()
	.Name("TestAssembly")
	.NewType(out var baseType, x => x.Name("BaseType"))
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
	.NewMethod(out var main)
	.EntryPoint(main);
Action<String> println = Console.WriteLine;
main.Public().Name("Main").ReturnType(typeof(void)).Body(x => x.Ldstr("Hello").Call(println.Method));
var assembly = assemblyBuilder.Create();
assemblyBuilder.Save("test.dll");
var module = assembly.Modules.Single(); //assembly.GetModule("TestModule") ?? throw new InvalidOperationException("no module");
var method = module.GetMethod("TestMethod") ?? throw new InvalidOperationException("no method");
var del = method.CreateDelegate<Func<Int32, Int32>>();
var result = del(8);
Console.WriteLine(result);
	
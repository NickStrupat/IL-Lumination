using System.Reflection;
using Illumination;

var assembly = new AssemblyBuilder()
	.Name("TestAssembly")
	.NewType(x => x.Name("TestType"))
	.NewMethod(x => x
		.Name("TestMethod")
		.Public()
		.ReturnType(typeof(Int32))
		.NewParameter(out var xParam, x => x.Name("x").Type(typeof(Int32)))
		.Body(x => x.Ldarg(xParam).Ldc_I4_2().Mul_Ovf().Ret())
	)
	.Create();
var module = assembly.Modules.Single(); //assembly.GetModule("TestModule") ?? throw new InvalidOperationException("no module");
var method = module.GetMethod("TestMethod") ?? throw new InvalidOperationException("no method");
var del = method.CreateDelegate<Func<Int32, Int32>>();
var result = del(8);
Console.WriteLine(result);
	
using Illumination.Builders;
using Illumination.Builders.Extensions;

Action<String> println = Console.WriteLine;
var assemblyBuilder = new AssemblyBuilder()
	.Name("TestAssembly")
	.NewType(out var baseType, t => t.Name("BaseType").Public()
		.NewMethod(m => m.Public().Static().Name("Main").EntryPoint()
			.Body(b => b.Ldstr("Hello").Call(println.Method).Ret())
		)
		.NewMethod(m => m.Public().Static().Name("Log")
			.NewParameter<String>(out var text)
			.Body(b => b
				.Ldarg(text)
				.Call(println.Method)
				.Ret()
			)
		)
		.NewField(out var text, x => x.Public().Type<String>().Name("text"))
		.NewProperty(x => x.Type<String>().Name("Test")
			.Get(x => x.Public().Body(x => x.Ldarg_0().Ldfld(text).Ret()))
			.Set(x => x.Public().Body(x => x.Ldarg_0().Ldarg_1().Stfld(text).Ret()))
		)
	)
	.NewType(t => t.Name("TestType").BaseType(baseType))
	.NewEnum<Byte>(e => e.Name("enum").Public()
		.NewLiteral(l => l.Name("A").Value(0))
		.NewLiteral(l => l.Name("B").Value(1))
	)
	.NewMethod(x => x.Name("MultiplyBy2").Public()
		.ReturnType(typeof(Int32))
		.NewParameter<Int32>(out var xParam)
		.Body(x => x.Ldarg(xParam).Ldc_I4_2().Mul_Ovf().Ret())
	)
	;
// baseType.Name("BaseType").Public().NewMethod(out var main);
// main.Public().Static().Name("Main").ReturnType(typeof(void)).Body(x => x.Ldstr("Hello").Call(println.Method).Ret());
// assemblyBuilder.EntryPoint(main);
var assembly = assemblyBuilder.Create();
var type = assembly.GetType("BaseType")!;
type.GetMethod("Log")!.CreateDelegate<Action<String>>().Invoke("test");
var baseType2 = Activator.CreateInstance(type);
var testProp = type.GetProperty("Test")!;
testProp.SetValue(baseType2, "wow");
Console.WriteLine(testProp.GetValue(baseType2));

await assemblyBuilder.Save("TestAssembly.dll");
var loaded = System.Reflection.Assembly.LoadFrom(Path.GetFullPath("TestAssembly.dll"));
var loadedType = loaded.GetType("BaseType")!;
var mainMi = loadedType.GetMethod("Main") ?? throw new InvalidOperationException("no method");
mainMi.Invoke(null, []);
var method = loaded.Modules.First().GetMethod("MultiplyBy2") ?? throw new InvalidOperationException("no method");
var del = method.CreateDelegate<Func<Int32, Int32>>();
var result = del(8);
Console.WriteLine(result);

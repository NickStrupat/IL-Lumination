using System.Reflection;
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
		.NewProperty(x => x.Get(x => x.Public()))
		.NewAutoProperty(x => x.Public().Type<String>().Name("Text"))
		.NewAutoEvent(x => x.Public().Name("Notify").HandlerType<Action>())
		// .NewEvent(e => e.Name("OnNotify").HandlerType<Action>()
		// 	.AddMethod(m => m.Public().Name("add_OnNotify")
		// 		.NewParameter<Action>(p => p.Name("value"))
		// 		.NewLocal<Action>(out var addCurrent)
		// 		.NewLocal<Action>(out var addPrevious)
		// 		.Body(b => b
		// 			.Ldarg_0().Ldfld(onNotifyField).Stloc(addCurrent)
		// 			.DefineLabel(out var addLoop).MarkLabel(addLoop)
		// 			.Ldloc(addCurrent).Stloc(addPrevious)
		// 			.Ldarg_0().Ldflda(onNotifyField)
		// 			.Ldloc(addPrevious).Ldarg_1()
		// 			.Call(typeof(Delegate).GetMethod(nameof(Delegate.Combine), [typeof(Delegate), typeof(Delegate)])!)
		// 			.Castclass<Action>()
		// 			.Ldloc(addPrevious)
		// 			.Call(compareExchange)
		// 			.Stloc(addCurrent)
		// 			.Ldloc(addCurrent).Ldloc(addPrevious).Bne_Un(addLoop)
		// 			.Ret()
		// 		)
		// 	)
		// 	.RemoveMethod(m => m.Public().Name("remove_OnNotify")
		// 		.NewParameter<Action>(p => p.Name("value"))
		// 		.NewLocal<Action>(out var removeCurrent)
		// 		.NewLocal<Action>(out var removePrevious)
		// 		.Body(b => b
		// 			.Ldarg_0().Ldfld(onNotifyField).Stloc(removeCurrent)
		// 			.DefineLabel(out var removeLoop).MarkLabel(removeLoop)
		// 			.Ldloc(removeCurrent).Stloc(removePrevious)
		// 			.Ldarg_0().Ldflda(onNotifyField)
		// 			.Ldloc(removePrevious).Ldarg_1()
		// 			.Call(typeof(Delegate).GetMethod(nameof(Delegate.Remove), [typeof(Delegate), typeof(Delegate)])!)
		// 			.Castclass<Action>()
		// 			.Ldloc(removePrevious)
		// 			.Call(compareExchange)
		// 			.Stloc(removeCurrent)
		// 			.Ldloc(removeCurrent).Ldloc(removePrevious).Bne_Un(removeLoop)
		// 			.Ret()
		// 		)
		// 	)
		// )
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
var testProp = type.GetProperty("Text")!;
testProp.SetValue(baseType2, "wow");
Console.WriteLine(testProp.GetValue(baseType2));
var onNotifyEvent = type.GetEvent("Notify")!;
Action handler = () => Console.WriteLine("event fired");
onNotifyEvent.AddEventHandler(baseType2, handler);
((Action)type.GetField("Notify", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(baseType2)!).Invoke();

await assemblyBuilder.Save("TestAssembly.dll");
var loaded = System.Reflection.Assembly.LoadFrom(Path.GetFullPath("TestAssembly.dll"));
var loadedType = loaded.GetType("BaseType")!;
var mainMi = loadedType.GetMethod("Main") ?? throw new InvalidOperationException("no method");
mainMi.Invoke(null, []);
var method = loaded.Modules.First().GetMethod("MultiplyBy2") ?? throw new InvalidOperationException("no method");
var del = method.CreateDelegate<Func<Int32, Int32>>();
var result = del(8);
Console.WriteLine(result);

var outputDir = Path.Combine(Path.GetTempPath(), "IL-Lumination-exe-test");
Directory.CreateDirectory(outputDir);
await assemblyBuilder.SaveToExecutable(outputDir);
var exePath = Path.Combine(outputDir, "TestAssembly");
if (OperatingSystem.IsWindows()) exePath += ".exe";
Console.WriteLine($"Executable saved to: {exePath}");
var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
	FileName = exePath,
	RedirectStandardOutput = true
})!;
await process.WaitForExitAsync();
Console.WriteLine($"Executable output: {await process.StandardOutput.ReadToEndAsync()}");

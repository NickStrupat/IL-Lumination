using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Illumination;
using Illumination.Builders;
using Illumination.Builders.Extensions;
using Xunit;

namespace Unit_Tests;

public class BuilderTests
{
	[Fact]
	public void TypeCreation_DefineTypeWithMethod_InvokesSuccessfully()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("GetValue")
					.ReturnType<Int32>()
					.Body(b => b.Ldc_I4(42).Ret())
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var method = clrType.GetMethod("GetValue")!;
		var result = method.Invoke(null, null);

		Assert.Equal(42, result);
	}

	[Fact]
	public void AutoProperty_GetSet_WorksCorrectly()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewAutoProperty(p => p
					.Name("Value")
					.Type<String>()
					.Public()
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var instance = Activator.CreateInstance(clrType)!;
		var property = clrType.GetProperty("Value")!;

		property.SetValue(instance, "test");
		var result = property.GetValue(instance);

		Assert.Equal("test", result);
	}

	[Fact]
	public void AutoProperty_BackingField_FollowsConvention()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewAutoProperty(p => p
					.Name("Value")
					.Type<Int32>()
					.Public()
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var backingField = clrType.GetField("<Value>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(backingField);
		Assert.Equal(typeof(Int32), backingField.FieldType);
	}

	[Fact]
	public void AutoProperty_GetOnly_HasNoSetter()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewAutoProperty(p => p
					.Name("Value")
					.Type<Int32>()
					.Public()
					.GetOnly()
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var property = clrType.GetProperty("Value")!;

		Assert.NotNull(property.GetMethod);
		Assert.Null(property.SetMethod);
	}

	[Fact]
	public void AutoProperty_Static_WorksCorrectly()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewAutoProperty(p => p
					.Name("Value")
					.Type<Int32>()
					.Public()
					.Static()
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var property = clrType.GetProperty("Value", BindingFlags.Public | BindingFlags.Static)!;

		property.SetValue(null, 123);
		var result = property.GetValue(null);

		Assert.Equal(123, result);
	}

	[Fact]
	public void AutoProperty_PerAccessorVisibility_SetterIsPrivate()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewAutoProperty(p => p
					.Name("Value")
					.Type<Int32>()
					.Public()
					.SetVisibility(MethodAttributes.Private)
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var property = clrType.GetProperty("Value")!;

		Assert.True(property.GetMethod!.IsPublic);
		Assert.True(property.SetMethod!.IsPrivate);
	}

	[Fact]
	public void AutoEvent_AddRemoveInvoke_WorksCorrectly()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewAutoEvent(e => e
					.Name("Notify")
					.HandlerType<Action>()
					.Public()
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var instance = Activator.CreateInstance(clrType)!;
		var evt = clrType.GetEvent("Notify")!;

		var called = false;
		Action handler = () => called = true;
		evt.AddEventHandler(instance, handler);

		var backingField = clrType.GetField("Notify", BindingFlags.NonPublic | BindingFlags.Instance)!;
		var del = (Action)backingField.GetValue(instance)!;
		del.Invoke();

		Assert.True(called);
	}

	[Fact]
	public void AutoEvent_ConcurrentAdds_ThreadSafe()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewAutoEvent(e => e
					.Name("Notify")
					.HandlerType<Action>()
					.Public()
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var instance = Activator.CreateInstance(clrType)!;
		var evt = clrType.GetEvent("Notify")!;

		var count = 0;
		var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
		{
			Action handler = () => Interlocked.Increment(ref count);
			evt.AddEventHandler(instance, handler);
		})).ToArray();

		Task.WaitAll(tasks);

		var backingField = clrType.GetField("Notify", BindingFlags.NonPublic | BindingFlags.Instance)!;
		var del = (Action)backingField.GetValue(instance)!;
		del.Invoke();

		Assert.Equal(100, count);
	}

	[Fact]
	public void Field_Readonly_CannotBeModifiedAfterConstruction()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewField(f => f
					.Name("Value")
					.Type<Int32>()
					.Public()
					.Readonly()
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var field = clrType.GetField("Value")!;

		Assert.True(field.IsInitOnly);
	}

	[Fact]
	public void Field_Literal_HasConstantValue()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewField(f => f
					.Name("MaxValue")
					.Type<Int32>()
					.Public()
					.Literal(100)
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var field = clrType.GetField("MaxValue")!;

		Assert.True(field.IsLiteral);
		Assert.Equal(100, field.GetRawConstantValue());
	}

	[Fact]
	public void Field_Static_AccessibleWithoutInstance()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewField(out var field, f => f
					.Name("Counter")
					.Type<Int32>()
					.Public()
					.Static()
				)
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("Increment")
					.Body(b => b
						.Ldsfld(field)
						.Ldc_I4_1()
						.Add()
						.Stsfld(field)
						.Ret()
					)
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var staticField = clrType.GetField("Counter", BindingFlags.Public | BindingFlags.Static)!;
		var method = clrType.GetMethod("Increment")!;

		staticField.SetValue(null, 5);
		method.Invoke(null, null);
		var result = staticField.GetValue(null);

		Assert.Equal(6, result);
	}

	[Fact]
	public void Enum_TypedLiterals_RoundTripCorrectly()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewEnum<Byte>(e => e
				.Name("Status")
				.Public()
				.NewLiteral(l => l.Name("Active").Value(1))
				.NewLiteral(l => l.Name("Inactive").Value(2))
			);

		var assembly = ab.Create();
		var enumType = assembly.GetType("Status")!;

		Assert.True(enumType.IsEnum);
		Assert.Equal(typeof(Byte), Enum.GetUnderlyingType(enumType));
		Assert.Equal(1, (Byte)Enum.Parse(enumType, "Active"));
		Assert.Equal(2, (Byte)Enum.Parse(enumType, "Inactive"));
	}

	[Fact]
	public void GenericType_WithReferenceTypeConstraint_ReflectedCorrectly()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("Container`1")
				.Public()
				.NewTypeParameter(out var tParam, tp => tp
					.Name("T")
					.ReferenceTypeConstraint()
				)
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("GetDefault")
					.ReturnType<Object>()
					.Body(b => b.Ldnull().Ret())
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("Container`1")!;

		Assert.True(clrType.IsGenericTypeDefinition);
		var gArgs = clrType.GetGenericArguments();
		Assert.Single(gArgs);
		Assert.Equal("T", gArgs[0].Name);
		Assert.True((gArgs[0].GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0);

		// Verify it can be closed over a reference type
		var closed = clrType.MakeGenericType(typeof(String));
		var result = closed.GetMethod("GetDefault")!.Invoke(null, null);
		Assert.Null(result);
	}

	[Fact]
	public void GenericMethod_WithTypeParameter_ReflectedCorrectly()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("Utils")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("Create")
					.NewTypeParameter(out _, tp => tp
						.Name("T")
						.DefaultConstructorConstraint()
					)
					.ReturnType<Int32>()
					.Body(b => b.Ldc_I4(1).Ret())
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("Utils")!;
		var method = clrType.GetMethod("Create")!;

		Assert.True(method.IsGenericMethodDefinition);
		var gArgs = method.GetGenericArguments();
		Assert.Single(gArgs);
		Assert.Equal("T", gArgs[0].Name);
		Assert.True((gArgs[0].GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0);

		// Verify it can be called when closed
		var closed = method.MakeGenericMethod(typeof(Object));
		var result = closed.Invoke(null, null);
		Assert.Equal(1, result);
	}

	[Fact]
	public void MethodVirtuality_VirtualOverride_DispatchesCorrectly()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var baseType, t => t
				.Name("Base")
				.Public()
				.NewMethod(m => m
					.Public()
					.Virtual()
					.Name("GetValue")
					.ReturnType<Int32>()
					.Body(b => b.Ldc_I4(10).Ret())
				)
			)
			.NewType(out var derivedType, t => t
				.Name("Derived")
				.Public()
				.BaseType(baseType)
				.NewMethod(m => m
					.Public()
					.Override()
					.Name("GetValue")
					.ReturnType<Int32>()
					.Body(b => b.Ldc_I4(20).Ret())
				)
			);

		var assembly = ab.Create();
		var clrDerived = assembly.GetType("Derived")!;
		var instance = Activator.CreateInstance(clrDerived)!;
		var method = clrDerived.GetMethod("GetValue")!;

		var result = method.Invoke(instance, null);

		Assert.Equal(20, result);
	}

	[Fact]
	public void Constructor_Instance_RunsWhenCreated()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewField(out var field, f => f
					.Name("Value")
					.Type<Int32>()
					.Public()
				)
				.NewConstructor(c => c
					.Public()
					.Body(b => b
						.Ldarg_0()
						.Call(typeof(Object).GetConstructor(Type.EmptyTypes)!)
						.Ldarg_0()
						.Ldc_I4(99)
						.Stfld(field)
						.Ret()
					)
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var instance = Activator.CreateInstance(clrType)!;
		var fieldInfo = clrType.GetField("Value")!;

		var result = fieldInfo.GetValue(instance);

		Assert.Equal(99, result);
	}

	[Fact]
	public void Constructor_Static_RunsBeforeFirstUse()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewField(out var field, f => f
					.Name("Counter")
					.Type<Int32>()
					.Public()
					.Static()
				)
				.NewStaticConstructor(c => c
					.Body(b => b
						.Ldc_I4(777)
						.Stsfld(field)
						.Ret()
					)
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var staticField = clrType.GetField("Counter", BindingFlags.Public | BindingFlags.Static)!;

		var result = staticField.GetValue(null);

		Assert.Equal(777, result);
	}

	[Fact]
	public void CustomAttribute_OnType_RetrievableViaReflection()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.AddCustomAttribute(typeof(ObsoleteAttribute))
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var attrs = clrType.GetCustomAttributes(typeof(ObsoleteAttribute), false);

		Assert.Single(attrs);
	}

	[Fact]
	public void ParameterAttribute_HasDefault_ReflectedCorrectly()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("TestType")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("WithDefault")
					.ReturnType<Int32>()
					.NewParameter<Int32>(out var param, p => p
						.Name("value")
						.HasDefault(42)
					)
					.Body(b => b.Ldarg(param).Ret())
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("TestType")!;
		var method = clrType.GetMethod("WithDefault")!;
		var parameter = method.GetParameters()[0];

		Assert.True(parameter.HasDefaultValue);
		Assert.Equal(42, parameter.DefaultValue);
	}

	[Fact]
	public void NestedType_DefinedInsideParent_AccessibleViaGetNestedType()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var outer, t => t
				.Name("Outer")
				.Public()
				.NewType(inner => inner
					.Name("Inner")
					.Public()
					.NewMethod(m => m
						.Public()
						.Static()
						.Name("GetValue")
						.ReturnType<Int32>()
						.Body(b => b.Ldc_I4(123).Ret())
					)
				)
			);

		var assembly = ab.Create();
		var outerType = assembly.GetType("Outer")!;
		var innerType = outerType.GetNestedType("Inner")!;
		var method = innerType.GetMethod("GetValue")!;

		var result = method.Invoke(null, null);

		Assert.Equal(123, result);
	}

	[Fact]
	public void BaseTypeAndInterface_Implementation_VerifiedViaReflection()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var derived, t => t
				.Name("Derived")
				.Public()
				.BaseType<Exception>()
				.AddInterface<IDisposable>()
				.NewMethod(m => m
					.Public()
					.Virtual()
					.Name("Dispose")
					.Body(b => b.Ret())
				)
			);

		var assembly = ab.Create();
		var clrType = assembly.GetType("Derived")!;

		Assert.True(typeof(Exception).IsAssignableFrom(clrType));
		Assert.True(typeof(IDisposable).IsAssignableFrom(clrType));
	}

	[Theory]
	[InlineData(1, "match")]
	[InlineData(0, "no match")]
	public void ExceptionFilter_When_FiltersByCondition(Int32 expected, String message)
	{
		var getMessage = typeof(Exception).GetProperty("Message")!.GetMethod!;
		var stringEquals = typeof(String).GetMethod("op_Equality", [typeof(String), typeof(String)])!;
		var msgCtor = typeof(InvalidOperationException).GetConstructor([typeof(String)])!;

		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("FilterTest")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("Run")
					.ReturnType<Int32>()
					.NewParameter<String>(out var msg)
					.Body(b => b
						.DeclareLocal<Int32>(out var result)
						.Try(
							tb => tb
								.Ldarg(msg)
								.Newobj(msgCtor)
								.Throw(),
							t => t
								.CatchWhen<InvalidOperationException>(
									(wb, ex) => wb
										.Ldloc(ex)
										.Callvirt(getMessage)
										.Ldstr("match")
										.Call(stringEquals),
									cb => cb
										.Pop()
										.Ldc_I4_1()
										.Stloc(result)
								)
								.Catch<Exception>(cb => cb
									.Pop()
									.Ldc_I4_0()
									.Stloc(result)
								)
						)
						.Ldloc(result)
						.Ret()
					)
				)
			);

		var assembly = ab.Create();
		var method = assembly.GetType("FilterTest")!.GetMethod("Run")!;
		var result = method.Invoke(null, [message]);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void CatchAll_CatchesAnyException()
	{
		var ab = new Illumination.Builders.AssemblyBuilder()
			.Name("TestAssembly")
			.NewType(out var type, t => t
				.Name("CatchAllTest")
				.Public()
				.NewMethod(m => m
					.Public()
					.Static()
					.Name("Run")
					.ReturnType<Int32>()
					.Body(b => b
						.DeclareLocal<Int32>(out var result)
						.Try(
							tb => tb
								.Newobj(typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!)
								.Throw(),
							t => t
								.Catch(cb => cb
									.Pop()
									.Ldc_I4(42)
									.Stloc(result)
								)
						)
						.Ldloc(result)
						.Ret()
					)
				)
			);

		var assembly = ab.Create();
		var result = assembly.GetType("CatchAllTest")!.GetMethod("Run")!.Invoke(null, null);
		Assert.Equal(42, result);
	}

}

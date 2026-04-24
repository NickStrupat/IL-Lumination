using System;
using Illumination.Builders;
using Illumination.Builders.Extensions;
using Xunit;

namespace Unit_Tests;

public class CrossReferenceTests
{
	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, 1)]
	[InlineData(0, 4)]
	public void MutualMethodCalls_PingPong(Int32 expected, Int32 n)
	{
		var ab = new AssemblyBuilder().Name("Test");
		ab.NewType(out var typeA).NewType(out var typeB);

		typeA.Name("TypeA").Public().NewMethod(out var ping);
		typeB.Name("TypeB").Public().NewMethod(out var pong);

		ping.Name("Ping").Public().Static().ReturnType<Int32>()
			.NewParameter<Int32>(out var pingN)
			.Body(b => b
				.DefineLabel(out var notZero)
				.Ldarg(pingN).Brtrue(notZero)
				.Ldc_I4_0().Ret()
				.MarkLabel(notZero)
				.Ldarg(pingN).Ldc_I4_1().Sub().Call(pong).Ret()
			);

		pong.Name("Pong").Public().Static().ReturnType<Int32>()
			.NewParameter<Int32>(out var pongN)
			.Body(b => b.Ldarg(pongN).Call(ping).Ret());

		var result = ab.Create().GetType("TypeA")!.GetMethod("Ping")!.Invoke(null, [n]);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void CrossType_AsymmetricMethodCallChain()
	{
		var ab = new AssemblyBuilder().Name("Test");
		ab.NewType(out var typeA).NewType(out var typeB);

		typeA.Name("TypeA").Public().NewMethod(out var getBase).NewMethod(out var compute);
		typeB.Name("TypeB").Public().NewMethod(out var scale);

		getBase.Name("GetBase").Public().Static().ReturnType<Int32>()
			.Body(b => b.Ldc_I4(10).Ret());

		compute.Name("Compute").Public().Static().ReturnType<Int32>()
			.Body(b => b.Call(scale).Ret());

		scale.Name("Scale").Public().Static().ReturnType<Int32>()
			.Body(b => b.Call(getBase).Ldc_I4_2().Mul().Ret());

		var result = ab.Create().GetType("TypeA")!.GetMethod("Compute")!.Invoke(null, null);
		Assert.Equal(20, result);
	}

	[Fact]
	public void CrossType_FieldTypeReference()
	{
		var ab = new AssemblyBuilder().Name("Test");
		ab.NewType(out var typeA).NewType(out var typeB);

		typeA.Name("TypeA").Public().NewField(out var valueField);
		valueField.Name("Value").Type<Int32>().Public();

		typeB.Name("TypeB").Public().NewField(out var peerField).NewMethod(out var getPeerValue);
		peerField.Name("Peer").Type(typeA).Public();

		getPeerValue.Name("GetPeerValue").Public().Static().ReturnType<Int32>()
			.NewParameter(out var peerParam, p => p.Name("b").Type(typeB))
			.Body(b => b
				.Ldarg(peerParam)
				.Ldfld(peerField)
				.Ldfld(valueField)
				.Ret()
			);

		var assembly = ab.Create();
		var clrA = assembly.GetType("TypeA")!;
		var clrB = assembly.GetType("TypeB")!;

		Assert.Equal(clrA, clrB.GetField("Peer")!.FieldType);

		var aInstance = Activator.CreateInstance(clrA)!;
		clrA.GetField("Value")!.SetValue(aInstance, 77);
		var bInstance = Activator.CreateInstance(clrB)!;
		clrB.GetField("Peer")!.SetValue(bInstance, aInstance);
		var result = clrB.GetMethod("GetPeerValue")!.Invoke(null, [bInstance]);
		Assert.Equal(77, result);
	}

	[Fact]
	public void CrossType_MutualObjectCreation()
	{
		var ab = new AssemblyBuilder().Name("Test");
		ab.NewType(out var typeA).NewType(out var typeB);

		typeA.Name("TypeA").Public()
			.NewField(out var aTag).NewConstructor(out var ctorA).NewMethod(out var createB);
		typeB.Name("TypeB").Public()
			.NewField(out var bTag).NewConstructor(out var ctorB).NewMethod(out var createA);

		aTag.Name("Tag").Type<Int32>().Public();
		bTag.Name("Tag").Type<Int32>().Public();

		ctorA.Public().Body(b => b
			.Ldarg_0().Call(typeof(Object).GetConstructor(Type.EmptyTypes)!)
			.Ldarg_0().Ldc_I4(1).Stfld(aTag).Ret());

		ctorB.Public().Body(b => b
			.Ldarg_0().Call(typeof(Object).GetConstructor(Type.EmptyTypes)!)
			.Ldarg_0().Ldc_I4(2).Stfld(bTag).Ret());

		createB.Name("CreateB").Public().Static().ReturnType<Object>()
			.Body(b => b.Newobj(ctorB).Ret());

		createA.Name("CreateA").Public().Static().ReturnType<Object>()
			.Body(b => b.Newobj(ctorA).Ret());

		var assembly = ab.Create();
		var clrA = assembly.GetType("TypeA")!;
		var clrB = assembly.GetType("TypeB")!;

		var bInstance = clrA.GetMethod("CreateB")!.Invoke(null, null)!;
		Assert.IsType(clrB, bInstance);
		Assert.Equal(2, clrB.GetField("Tag")!.GetValue(bInstance));

		var aInstance = clrB.GetMethod("CreateA")!.Invoke(null, null)!;
		Assert.IsType(clrA, aInstance);
		Assert.Equal(1, clrA.GetField("Tag")!.GetValue(aInstance));
	}

	[Fact]
	public void CrossType_BaseMethodCreatesDerived()
	{
		var ab = new AssemblyBuilder().Name("Test");
		ab.NewType(out var animal).NewType(out var dog);

		animal.Name("Animal").Public().NewMethod(out var getNameBase).NewMethod(out var createDog);
		dog.Name("Dog").Public().BaseType(animal).NewConstructor(out var dogCtor).NewMethod(out var getNameDog);

		getNameBase.Name("GetName").Public().Virtual().ReturnType<String>()
			.Body(b => b.Ldstr("Animal").Ret());

		createDog.Name("CreateDog").Public().Static().ReturnType<Object>()
			.Body(b => b.Newobj(dogCtor).Ret());

		dogCtor.Public().Body(b => b
			.Ldarg_0().Call(typeof(Object).GetConstructor(Type.EmptyTypes)!).Ret());

		getNameDog.Name("GetName").Public().Override().ReturnType<String>()
			.Body(b => b.Ldstr("Dog").Ret());

		var assembly = ab.Create();
		var clrAnimal = assembly.GetType("Animal")!;
		var clrDog = assembly.GetType("Dog")!;

		var instance = clrAnimal.GetMethod("CreateDog")!.Invoke(null, null)!;
		Assert.IsType(clrDog, instance);
		Assert.Equal("Dog", clrDog.GetMethod("GetName")!.Invoke(instance, null));
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, 3)]
	[InlineData(0, 6)]
	public void CrossType_ThreeWayCircularCalls(Int32 expected, Int32 n)
	{
		var ab = new AssemblyBuilder().Name("Test");
		ab.NewType(out var ta).NewType(out var tb).NewType(out var tc);

		ta.Name("TA").Public().NewMethod(out var callA);
		tb.Name("TB").Public().NewMethod(out var callB);
		tc.Name("TC").Public().NewMethod(out var callC);

		callA.Name("F").Public().Static().ReturnType<Int32>()
			.NewParameter<Int32>(out var nA)
			.Body(b => b
				.DefineLabel(out var done)
				.Ldarg(nA).Brfalse(done)
				.Ldarg(nA).Ldc_I4_1().Sub().Call(callB).Ret()
				.MarkLabel(done).Ldc_I4_0().Ret()
			);

		callB.Name("F").Public().Static().ReturnType<Int32>()
			.NewParameter<Int32>(out var nB)
			.Body(b => b.Ldarg(nB).Call(callC).Ret());

		callC.Name("F").Public().Static().ReturnType<Int32>()
			.NewParameter<Int32>(out var nC)
			.Body(b => b.Ldarg(nC).Call(callA).Ret());

		var result = ab.Create().GetType("TA")!.GetMethod("F")!.Invoke(null, [n]);
		Assert.Equal(expected, result);
	}
}

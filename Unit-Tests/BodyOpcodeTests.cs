using System;
using System.Reflection.Emit;
using Illumination;
using Xunit;

namespace Unit_Tests;

public class BodyOpcodeTests
{
	// --- Arithmetic ---

	[Theory]
	[InlineData(3, 5, 2)]
	[InlineData(-1, 1, 2)]
	[InlineData(0, 0, 0)]
	public void Sub(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Sub().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Theory]
	[InlineData(6, 2, 3)]
	[InlineData(0, 0, 100)]
	[InlineData(-6, -2, 3)]
	public void Mul(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Mul().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Fact]
	public void Mul_Ovf_ThrowsOnOverflow()
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Mul_Ovf().Ret();
		Assert.Throws<OverflowException>(() => dm.CreateDelegate()(Int32.MaxValue, 2));
	}

	[Theory]
	[InlineData(1, 7, 3)]
	[InlineData(0, 6, 3)]
	[InlineData(2, 5, 3)]
	public void Rem(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Rem().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Theory]
	[InlineData(-5, 5)]
	[InlineData(5, -5)]
	[InlineData(0, 0)]
	public void Neg(Int32 expected, Int32 a)
	{
		var dm = new DynamicFunc<Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Neg().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a));
	}

	// --- Bitwise ---

	[Fact]
	public void And_Or_Xor_Not()
	{
		// And
		var dm1 = new DynamicFunc<Int32, Int32, Int32>();
		dm1.GetILGenerator().Body().Ldarg_0().Ldarg_1().And().Ret();
		Assert.Equal(0b1000, dm1.CreateDelegate()(0b1010, 0b1100));

		// Or
		var dm2 = new DynamicFunc<Int32, Int32, Int32>();
		dm2.GetILGenerator().Body().Ldarg_0().Ldarg_1().Or().Ret();
		Assert.Equal(0b1110, dm2.CreateDelegate()(0b1010, 0b1100));

		// Xor
		var dm3 = new DynamicFunc<Int32, Int32, Int32>();
		dm3.GetILGenerator().Body().Ldarg_0().Ldarg_1().Xor().Ret();
		Assert.Equal(0b0110, dm3.CreateDelegate()(0b1010, 0b1100));

		// Not
		var dm4 = new DynamicFunc<Int32, Int32>();
		dm4.GetILGenerator().Body().Ldarg_0().Not().Ret();
		Assert.Equal(~0b1010, dm4.CreateDelegate()(0b1010));
	}

	[Theory]
	[InlineData(8, 1, 3)]
	[InlineData(16, 4, 2)]
	public void Shl(Int32 expected, Int32 a, Int32 shift)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Shl().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, shift));
	}

	[Theory]
	[InlineData(1, 8, 3)]
	[InlineData(4, 16, 2)]
	public void Shr(Int32 expected, Int32 a, Int32 shift)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Shr().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, shift));
	}

	// --- Branching ---

	[Theory]
	[InlineData(1, 5, 5)]
	[InlineData(0, 5, 3)]
	public void Beq(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body()
			.DefineLabel(out var equal)
			.Ldarg_0().Ldarg_1().Beq(equal)
			.Ldc_I4_0().Ret()
			.MarkLabel(equal)
			.Ldc_I4_1().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Theory]
	[InlineData(1, 5, 3)]
	[InlineData(0, 5, 5)]
	public void Bne_Un(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body()
			.DefineLabel(out var notEqual)
			.Ldarg_0().Ldarg_1().Bne_Un(notEqual)
			.Ldc_I4_0().Ret()
			.MarkLabel(notEqual)
			.Ldc_I4_1().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Theory]
	[InlineData(1, 5, 3)]
	[InlineData(1, 5, 5)]
	[InlineData(0, 3, 5)]
	public void Bge(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body()
			.DefineLabel(out var geLabel)
			.Ldarg_0().Ldarg_1().Bge(geLabel)
			.Ldc_I4_0().Ret()
			.MarkLabel(geLabel)
			.Ldc_I4_1().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Theory]
	[InlineData(1, 3, 5)]
	[InlineData(0, 5, 5)]
	[InlineData(0, 5, 3)]
	public void Blt(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body()
			.DefineLabel(out var ltLabel)
			.Ldarg_0().Ldarg_1().Blt(ltLabel)
			.Ldc_I4_0().Ret()
			.MarkLabel(ltLabel)
			.Ldc_I4_1().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Theory]
	[InlineData(10, 0)]
	[InlineData(20, 1)]
	[InlineData(30, 2)]
	[InlineData(-1, 3)]
	[InlineData(-1, 99)]
	public void Switch(Int32 expected, Int32 index)
	{
		var dm = new DynamicFunc<Int32, Int32>();
		dm.GetILGenerator().Body()
			.DefineLabel(out var case0)
			.DefineLabel(out var case1)
			.DefineLabel(out var case2)
			.DefineLabel(out var defaultCase)
			.Ldarg_0()
			.Switch([case0, case1, case2])
			.Br(defaultCase)
			.MarkLabel(case0).Ldc_I4(10).Ret()
			.MarkLabel(case1).Ldc_I4(20).Ret()
			.MarkLabel(case2).Ldc_I4(30).Ret()
			.MarkLabel(defaultCase).Ldc_I4_M1().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(index));
	}

	// --- Comparison ---

	[Theory]
	[InlineData(1, 5, 5)]
	[InlineData(0, 5, 3)]
	public void Ceq(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Ceq().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Theory]
	[InlineData(1, 5, 3)]
	[InlineData(0, 3, 5)]
	[InlineData(0, 5, 5)]
	public void Cgt(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Cgt().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	[Theory]
	[InlineData(1, 3, 5)]
	[InlineData(0, 5, 3)]
	[InlineData(0, 5, 5)]
	public void Clt(Int32 expected, Int32 a, Int32 b)
	{
		var dm = new DynamicFunc<Int32, Int32, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Ldarg_1().Clt().Ret();
		Assert.Equal(expected, dm.CreateDelegate()(a, b));
	}

	// --- Field access (via DynamicMethod) ---

	public class FieldTarget
	{
		public Int32 Value;
		public static Int32 StaticValue;
	}

	[Fact]
	public void Ldfld_Stfld()
	{
		// Stfld
		var dmSet = new DynamicAction<FieldTarget, Int32>();
		dmSet.GetILGenerator().Body()
			.Ldarg_0()
			.Ldarg_1()
			.Stfld(typeof(FieldTarget).GetField("Value")!)
			.Ret();

		// Ldfld
		var dmGet = new DynamicFunc<FieldTarget, Int32>();
		dmGet.GetILGenerator().Body()
			.Ldarg_0()
			.Ldfld(typeof(FieldTarget).GetField("Value")!)
			.Ret();

		var target = new FieldTarget();
		dmSet.CreateDelegate()(target, 42);
		Assert.Equal(42, dmGet.CreateDelegate()(target));
	}

	[Fact]
	public void Ldsfld_Stsfld()
	{
		var dmSet = new DynamicAction<Int32>();
		dmSet.GetILGenerator().Body()
			.Ldarg_0()
			.Stsfld(typeof(FieldTarget).GetField("StaticValue")!)
			.Ret();

		var dmGet = new DynamicFunc<Int32>();
		dmGet.GetILGenerator().Body()
			.Ldsfld(typeof(FieldTarget).GetField("StaticValue")!)
			.Ret();

		dmSet.CreateDelegate()(99);
		Assert.Equal(99, dmGet.CreateDelegate()());
	}

	// --- Array operations ---

	[Fact]
	public void Array_Newarr_Stelem_Ldelem_Ldlen()
	{
		// Create int[3], set [0]=10, [1]=20, [2]=30, return [1]
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body()
			.Ldc_I4_3()
			.Newarr<Int32>()
			.Dup().Ldc_I4_0().Ldc_I4(10).Stelem_I4()
			.Dup().Ldc_I4_1().Ldc_I4(20).Stelem_I4()
			.Dup().Ldc_I4_2().Ldc_I4(30).Stelem_I4()
			.Ldc_I4_1()
			.Ldelem_I4()
			.Ret();
		Assert.Equal(20, dm.CreateDelegate()());
	}

	[Fact]
	public void Array_Ldlen_ReturnsLength()
	{
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body()
			.Ldc_I4(5)
			.Newarr<Int32>()
			.Ldlen()
			.Conv_I4()
			.Ret();
		Assert.Equal(5, dm.CreateDelegate()());
	}

	[Fact]
	public void Array_Stelem_Ldelem_ReferenceType()
	{
		// Create string[1], set [0]="hello", return [0]
		var dm = new DynamicFunc<String>();
		dm.GetILGenerator().Body()
			.Ldc_I4_1()
			.Newarr<String>()
			.Dup().Ldc_I4_0().Ldstr("hello").Stelem_Ref()
			.Ldc_I4_0()
			.Ldelem_Ref()
			.Ret();
		Assert.Equal("hello", dm.CreateDelegate()());
	}

	// --- Conversion ---

	[Fact]
	public void Conv_I4_FromInt64()
	{
		var dm = new DynamicFunc<Int64, Int32>();
		dm.GetILGenerator().Body().Ldarg_0().Conv_I4().Ret();
		Assert.Equal(42, dm.CreateDelegate()(42L));
	}

	[Fact]
	public void Conv_I8_FromInt32()
	{
		var dm = new DynamicFunc<Int32, Int64>();
		dm.GetILGenerator().Body().Ldarg_0().Conv_I8().Ret();
		Assert.Equal(42L, dm.CreateDelegate()(42));
	}

	[Fact]
	public void Conv_R8_FromInt32()
	{
		var dm = new DynamicFunc<Int32, Double>();
		dm.GetILGenerator().Body().Ldarg_0().Conv_R8().Ret();
		Assert.Equal(42.0, dm.CreateDelegate()(42));
	}

	[Fact]
	public void Conv_R4_FromInt32()
	{
		var dm = new DynamicFunc<Int32, Single>();
		dm.GetILGenerator().Body().Ldarg_0().Conv_R4().Ret();
		Assert.Equal(42.0f, dm.CreateDelegate()(42));
	}

	// --- Try/Catch/Finally helpers ---

	[Fact]
	public void TryCatch_CatchesException()
	{
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body()
			.DeclareLocal<Int32>(out var result)
			.Try(
				b => b
					.Newobj(typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!)
					.Throw(),
				t => t
					.Catch<InvalidOperationException>(b => b
						.Pop()
						.Ldc_I4(99)
						.Stloc(result)
					)
			)
			.Ldloc(result)
			.Ret();
		Assert.Equal(99, dm.CreateDelegate()());
	}

	[Fact]
	public void TryFinally_ExecutesFinallyBlock()
	{
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body()
			.DeclareLocal<Int32>(out var result)
			.Try(
				b => b
					.Ldc_I4(10)
					.Stloc(result),
				t => t
					.Finally(b => b
						.Ldc_I4(42)
						.Stloc(result)
					)
			)
			.Ldloc(result)
			.Ret();
		Assert.Equal(42, dm.CreateDelegate()());
	}

	[Theory]
	[InlineData(1, true)]
	[InlineData(2, false)]
	public void TryCatch_MultipleCatchBlocks(Int32 expected, Boolean throwInvalidOp)
	{
		var dm = new DynamicFunc<Boolean, Int32>();
		dm.GetILGenerator().Body()
			.DeclareLocal<Int32>(out var result)
			.Try(
				b => b
					.DefineLabel(out var throwArg)
					.Ldarg_0()
					.Brfalse(throwArg)
					.Newobj(typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!)
					.Throw()
					.MarkLabel(throwArg)
					.Newobj(typeof(ArgumentException).GetConstructor(Type.EmptyTypes)!)
					.Throw(),
				t => t
					.Catch<InvalidOperationException>(b => b.Pop().Ldc_I4_1().Stloc(result))
					.Catch<ArgumentException>(b => b.Pop().Ldc_I4_2().Stloc(result))
			)
			.Ldloc(result)
			.Ret();
		Assert.Equal(expected, dm.CreateDelegate()(throwInvalidOp));
	}

	[Theory]
	[InlineData(1, true)]
	[InlineData(2, false)]
	public void Try_Catch_MultipleCatchBlocks(Int32 expected, Boolean throwInvalidOp)
	{
		var dm = new DynamicFunc<Boolean, Int32>();
		dm.GetILGenerator().Body()
			.DeclareLocal<Int32>(out var result)
			.Try(
				b => b
					.DefineLabel(out var throwArg)
					.Ldarg_0()
					.Brfalse(throwArg)
					.Newobj(typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!)
					.Throw()
					.MarkLabel(throwArg)
					.Newobj(typeof(ArgumentException).GetConstructor(Type.EmptyTypes)!)
					.Throw(),
				t => t
					.Catch<InvalidOperationException>(b => b.Pop().Ldc_I4_1().Stloc(result))
					.Catch<ArgumentException>(b => b.Pop().Ldc_I4_2().Stloc(result))
			)
			.Ldloc(result)
			.Ret();
		Assert.Equal(expected, dm.CreateDelegate()(throwInvalidOp));
	}

	[Fact]
	public void Try_Catch_Finally()
	{
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body()
			.DeclareLocal<Int32>(out var result)
			.Try(
				b => b
					.Newobj(typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!)
					.Throw(),
				t => t
					.Catch<InvalidOperationException>(b => b
						.Pop()
						.Ldc_I4(10)
						.Stloc(result)
					)
					.Finally(b => b
						.Ldc_I4(42)
						.Stloc(result)
					)
			)
			.Ldloc(result)
			.Ret();
		Assert.Equal(42, dm.CreateDelegate()());
	}

	// Exception filters are not supported by DynamicMethod — see BuilderTests for filter tests.

	// --- Using ---

	public class TrackingDisposable : IDisposable
	{
		public static Boolean Disposed;
		public void Dispose() => Disposed = true;
	}

	[Fact]
	public void Using_CallsDispose()
	{
		TrackingDisposable.Disposed = false;
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body()
			.DeclareLocal<TrackingDisposable>(out var resource)
			.Newobj(typeof(TrackingDisposable).GetConstructor(Type.EmptyTypes)!)
			.Stloc(resource)
			.Using(resource, b => b.Nop())
			.Ldc_I4(42)
			.Ret();
		var result = dm.CreateDelegate()();
		Assert.Equal(42, result);
		Assert.True(TrackingDisposable.Disposed);
	}

	[Fact]
	public void Using_SkipsDisposeWhenNull()
	{
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body()
			.DeclareLocal<TrackingDisposable>(out var resource)
			.Using(resource, b => b.Nop())
			.Ldc_I4(99)
			.Ret();
		Assert.Equal(99, dm.CreateDelegate()());
	}

	// --- Box / Unbox ---

	[Fact]
	public void Box_Unbox_Any_RoundTrips()
	{
		var dm = new DynamicFunc<Int32, Int32>();
		dm.GetILGenerator().Body()
			.Ldarg_0()
			.Box<Int32>()
			.Unbox_Any<Int32>()
			.Ret();
		Assert.Equal(42, dm.CreateDelegate()(42));
	}

	[Fact]
	public void Box_ToObject_IsNotNull()
	{
		var dm = new DynamicFunc<Int32, Object>();
		dm.GetILGenerator().Body()
			.Ldarg_0()
			.Box<Int32>()
			.Ret();
		var result = dm.CreateDelegate()(42);
		Assert.NotNull(result);
		Assert.Equal(42, (Int32)result);
	}

	// --- Dup / Pop ---

	[Fact]
	public void Dup_DuplicatesTopOfStack()
	{
		// Dup the value, add to itself => n * 2
		var dm = new DynamicFunc<Int32, Int32>();
		dm.GetILGenerator().Body()
			.Ldarg_0()
			.Dup()
			.Add()
			.Ret();
		Assert.Equal(10, dm.CreateDelegate()(5));
	}

	// --- Load constants ---

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	[InlineData(7)]
	[InlineData(8)]
	public void Ldc_I4_ShortForms(Int32 expected)
	{
		var dm = new DynamicFunc<Int32>();
		var body = dm.GetILGenerator().Body();

		_ = expected switch
		{
			0 => body.Ldc_I4_0(),
			1 => body.Ldc_I4_1(),
			2 => body.Ldc_I4_2(),
			3 => body.Ldc_I4_3(),
			4 => body.Ldc_I4_4(),
			5 => body.Ldc_I4_5(),
			6 => body.Ldc_I4_6(),
			7 => body.Ldc_I4_7(),
			8 => body.Ldc_I4_8(),
			_ => throw new ArgumentOutOfRangeException()
		};

		body.Ret();
		Assert.Equal(expected, dm.CreateDelegate()());
	}

	[Fact]
	public void Ldc_I4_M1()
	{
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body().Ldc_I4_M1().Ret();
		Assert.Equal(-1, dm.CreateDelegate()());
	}

	[Fact]
	public void Ldc_I8()
	{
		var dm = new DynamicFunc<Int64>();
		dm.GetILGenerator().Body().Ldc_I8(Int64.MaxValue).Ret();
		Assert.Equal(Int64.MaxValue, dm.CreateDelegate()());
	}

	[Fact]
	public void Ldc_R4()
	{
		var dm = new DynamicFunc<Single>();
		dm.GetILGenerator().Body().Ldc_R4(3.14f).Ret();
		Assert.Equal(3.14f, dm.CreateDelegate()());
	}

	[Fact]
	public void Ldc_R8()
	{
		var dm = new DynamicFunc<Double>();
		dm.GetILGenerator().Body().Ldc_R8(3.14159).Ret();
		Assert.Equal(3.14159, dm.CreateDelegate()());
	}

	[Fact]
	public void Ldstr()
	{
		var dm = new DynamicFunc<String>();
		dm.GetILGenerator().Body().Ldstr("hello world").Ret();
		Assert.Equal("hello world", dm.CreateDelegate()());
	}

	[Fact]
	public void Ldnull()
	{
		var dm = new DynamicFunc<Object>();
		dm.GetILGenerator().Body().Ldnull().Ret();
		Assert.Null(dm.CreateDelegate()());
	}

	// --- Local variable store/load shortforms ---

	[Fact]
	public void Stloc_Ldloc_ShortForms()
	{
		var dm = new DynamicFunc<Int32>();
		dm.GetILGenerator().Body()
			.DeclareLocal<Int32>(out var loc0)
			.DeclareLocal<Int32>(out var loc1)
			.DeclareLocal<Int32>(out var loc2)
			.DeclareLocal<Int32>(out var loc3)
			.Ldc_I4_1().Stloc_0()
			.Ldc_I4_2().Stloc_1()
			.Ldc_I4_3().Stloc_2()
			.Ldc_I4_4().Stloc_3()
			.Ldloc_0().Ldloc_1().Add()
			.Ldloc_2().Add()
			.Ldloc_3().Add()
			.Ret();
		Assert.Equal(10, dm.CreateDelegate()());
	}

	// --- Castclass / Isinst ---

	[Fact]
	public void Castclass_ValidCast_Succeeds()
	{
		var dm = new DynamicFunc<Object, String>();
		dm.GetILGenerator().Body()
			.Ldarg_0()
			.Castclass<String>()
			.Ret();
		Assert.Equal("test", dm.CreateDelegate()("test"));
	}

	[Fact]
	public void Castclass_InvalidCast_Throws()
	{
		var dm = new DynamicFunc<Object, String>();
		dm.GetILGenerator().Body()
			.Ldarg_0()
			.Castclass<String>()
			.Ret();
		Assert.Throws<InvalidCastException>(() => dm.CreateDelegate()(42));
	}

	[Fact]
	public void Isinst_ReturnsNullOnMismatch()
	{
		var dm = new DynamicFunc<Object, String>();
		dm.GetILGenerator().Body()
			.Ldarg_0()
			.Isinst<String>()
			.Ret();
		Assert.Null(dm.CreateDelegate()(42));
		Assert.Equal("test", dm.CreateDelegate()("test"));
	}
}

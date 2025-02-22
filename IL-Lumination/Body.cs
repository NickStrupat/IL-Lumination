using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Illumination;

public static class BodyExtensions
{
	public static Body Body(this ILGenerator il) => new(il);
	public static Body Body(this DynamicMethod dm) => dm.GetILGenerator().Body();
	public static Body Body<TDel>(this DynamicMethod<TDel> dm) where TDel : Delegate => dm.GetILGenerator().Body();
}

public static class BodyAddExtensions
{
	public static Body Add(this Body body, Int32 value1, Int32 value2) => body.Ldc_I4(value1).Ldc_I4(value2).Add();
	public static Body Add_Ovf(this Body body, Int32 value1, Int32 value2) => body.Ldc_I4(value1).Ldc_I4(value2).Add_Ovf();
	public static Body Add_Ovf_Un(this Body body, Int32 value1, Int32 value2) => body.Ldc_I4(value1).Ldc_I4(value2).Add_Ovf_Un();
	
	public static Body Add(this Body body, Int64 value1, Int64 value2) => body.Ldc_I8(value1).Ldc_I8(value2).Add();
	public static Body Add_Ovf(this Body body, Int64 value1, Int64 value2) => body.Ldc_I8(value1).Ldc_I8(value2).Add_Ovf();
	public static Body Add_Ovf_Un(this Body body, Int64 value1, Int64 value2) => body.Ldc_I8(value1).Ldc_I8(value2).Add_Ovf_Un();
}

public static class BodyRetExtensions
{
	public static Body Ret(this Body body, Boolean value) => body.Ldc_I4(value ? 1 : 0).Ret();
	public static Body Ret(this Body body, Byte value) => body.Ldc_I4(value).Ret();
	public static Body Ret(this Body body, SByte value) => body.Ldc_I4(value).Ret();
	public static Body Ret(this Body body, UInt16 value) => body.Ldc_I4(value).Ret();
	public static Body Ret(this Body body, Int16 value) => body.Ldc_I4(value).Ret();
	public static Body Ret(this Body body, UInt32 value) => body.Ldc_I4(unchecked((Int32)value)).Ret();
	public static Body Ret(this Body body, Int32 value) => body.Ldc_I4(value).Ret();
	public static Body Ret(this Body body, UInt64 value) => body.Ldc_I8(unchecked((Int64)value)).Ret();
	public static Body Ret(this Body body, Int64 value) => body.Ldc_I8(value).Ret();
	public static Body Ret(this Body body, Single value) => body.Ldc_R4(value).Ret();
	public static Body Ret(this Body body, Double value) => body.Ldc_R8(value).Ret();
	public static Body Ret(this Body body, String value) => body.Ldstr(value).Ret();
	
	// public static Body Ret<T>(this Body body, T value)
	// {
	// 	if (typeof(T) == typeof(Boolean))
	// 		body.Ldc_I4(value is true ? 1 : 0);
	// 	return body.Ret();
	// }
}

public static class BodyLogicExtensions
{
	public static Body ForEach<T>(this Body body, IEnumerable<T> enumerable, Action<Body, T> action)
	{
		foreach (var item in enumerable)
			action(body, item);
		return body;
	}
}

public sealed class BodyBuilder : BodyBase<BodyBuilder>
{
	private readonly Func<String, Int16?> parameterNameToIndexLookup;
	internal BodyBuilder(ILGenerator ilGenerator, Func<String, Int16?> parameterNameToIndexLookup) : base(ilGenerator) => this.parameterNameToIndexLookup = parameterNameToIndexLookup;

	public BodyBuilder Ldarg(ParameterBuilderBase parameter) => Ldarg(parameter.index);
	public BodyBuilder Ldarg(String name) => Ldarg(parameterNameToIndexLookup(name) ?? throw new ArgumentException($"Parameter '{name}' not found", nameof(name)));
}

public sealed class Body(ILGenerator il) : BodyBase<Body>(il);

public class BodyBase<TBody> where TBody : BodyBase<TBody>
{
	private readonly ILGenerator il;
	private protected BodyBase(ILGenerator il) => this.il = il;

	private TBody This => (TBody)this;
	private TBody Emit(OpCode opcode) { il.Emit(opcode); return This; }
	private TBody Emit(OpCode opcode, Type type) { il.Emit(opcode, type); return This; }
	private TBody Emit(OpCode opcode, MethodInfo meth) { il.Emit(opcode, meth); return This; }
	private TBody Emit(OpCode opcode, ConstructorInfo constructor) { il.Emit(opcode, constructor); return This; }
	private TBody Emit(OpCode opcode, String str) { il.Emit(opcode, str); return This; }
	private TBody Emit(OpCode opcode, Byte arg) { il.Emit(opcode, arg); return This; }
	private TBody Emit(OpCode opcode, SByte arg) { il.Emit(opcode, arg); return This; }
	private TBody Emit(OpCode opcode, Int16 arg) { il.Emit(opcode, arg); return This; }
	private TBody Emit(OpCode opcode, Int32 arg) { il.Emit(opcode, arg); return This; }
	private TBody Emit(OpCode opcode, Int64 arg) { il.Emit(opcode, arg); return This; }
	private TBody Emit(OpCode opcode, Single arg) { il.Emit(opcode, arg); return This; }
	private TBody Emit(OpCode opcode, Double arg) { il.Emit(opcode, arg); return This; }
	private TBody Emit(OpCode opcode, Label label) { il.Emit(opcode, label); return This; }
	private TBody Emit(OpCode opcode, Label[] labels) { il.Emit(opcode, labels); return This; }
	private TBody Emit(OpCode opcode, FieldInfo field) { il.Emit(opcode, field); return This; }
	private TBody Emit(OpCode opcode, LocalBuilder local) { il.Emit(opcode, local); return This; }
	private TBody Emit(OpCode opcode, SignatureHelper signature) { il.Emit(opcode, signature); return This; }

	/// <inheritdoc cref="OpCodes.Add"/>
	public TBody Add() => Emit(OpCodes.Add);
	
	/// <inheritdoc cref="OpCodes.Add_Ovf"/>
	public TBody Add_Ovf() => Emit(OpCodes.Add_Ovf);
	
	/// <inheritdoc cref="OpCodes.Add_Ovf_Un"/>
	public TBody Add_Ovf_Un() => Emit(OpCodes.Add_Ovf_Un);
	
	/// <inheritdoc cref="OpCodes.And"/>
	public TBody And() => Emit(OpCodes.And);
	
	/// <inheritdoc cref="OpCodes.Arglist"/>
	public TBody Arglist() => Emit(OpCodes.Arglist);
	
	/// <inheritdoc cref="OpCodes.Beq"/>
	public TBody Beq(Label label) => Emit(OpCodes.Beq, label);
	
	/// <inheritdoc cref="OpCodes.Beq_S"/>
	public TBody Beq_S(Label label) => Emit(OpCodes.Beq_S, label);
	
	/// <inheritdoc cref="OpCodes.Bge"/>
	public TBody Bge(Label label) => Emit(OpCodes.Bge, label);
	
	/// <inheritdoc cref="OpCodes.Bge_S"/>
	public TBody Bge_S(Label label) => Emit(OpCodes.Bge_S, label);
	
	/// <inheritdoc cref="OpCodes.Bge_Un"/>
	public TBody Bge_Un(Label label) => Emit(OpCodes.Bge_Un, label);
	
	/// <inheritdoc cref="OpCodes.Bge_Un_S"/>
	public TBody Bge_Un_S(Label label) => Emit(OpCodes.Bge_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Bgt"/>
	public TBody Bgt(Label label) => Emit(OpCodes.Bgt, label);
	
	/// <inheritdoc cref="OpCodes.Bgt_S"/>
	public TBody Bgt_S(Label label) => Emit(OpCodes.Bgt_S, label);
	
	/// <inheritdoc cref="OpCodes.Bgt_Un"/>
	public TBody Bgt_Un(Label label) => Emit(OpCodes.Bgt_Un, label);
	
	/// <inheritdoc cref="OpCodes.Bgt_Un_S"/>
	public TBody Bgt_Un_S(Label label) => Emit(OpCodes.Bgt_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Ble"/>
	public TBody Ble(Label label) => Emit(OpCodes.Ble, label);
	
	/// <inheritdoc cref="OpCodes.Ble_S"/>
	public TBody Ble_S(Label label) => Emit(OpCodes.Ble_S, label);
	
	/// <inheritdoc cref="OpCodes.Ble_Un"/>
	public TBody Ble_Un(Label label) => Emit(OpCodes.Ble_Un, label);
	
	/// <inheritdoc cref="OpCodes.Ble_Un_S"/>
	public TBody Ble_Un_S(Label label) => Emit(OpCodes.Ble_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Blt"/>
	public TBody Blt(Label label) => Emit(OpCodes.Blt, label);
	
	/// <inheritdoc cref="OpCodes.Blt_S"/>
	public TBody Blt_S(Label label) => Emit(OpCodes.Blt_S, label);
	
	/// <inheritdoc cref="OpCodes.Blt_Un"/>
	public TBody Blt_Un(Label label) => Emit(OpCodes.Blt_Un, label);
	
	/// <inheritdoc cref="OpCodes.Blt_Un_S"/>
	public TBody Blt_Un_S(Label label) => Emit(OpCodes.Blt_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Bne_Un"/>
	/// <param name="label">The label to branch to if the two values are not equal.</param>
	public TBody Bne_Un(Label label) => Emit(OpCodes.Bne_Un, label);
	
	/// <inheritdoc cref="OpCodes.Bne_Un_S"/>
	/// <param name="label">The label to branch to if the two values are not equal.</param>
	public TBody Bne_Un_S(Label label) => Emit(OpCodes.Bne_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Box"/>
	public TBody Box(Type type) => Emit(OpCodes.Box, type);
	
	/// <inheritdoc cref="OpCodes.Box"/>
	public TBody Box<T>() where T : struct => Box(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Br"/>
	public TBody Br(Label label) => Emit(OpCodes.Br, label);
	
	/// <inheritdoc cref="OpCodes.Br_S"/>
	public TBody Br_S(Label label) => Emit(OpCodes.Br_S, label);
	
	/// <inheritdoc cref="OpCodes.Break"/>
	public TBody Break() => Emit(OpCodes.Break);
	
	/// <inheritdoc cref="OpCodes.Brfalse"/>
	public TBody Brfalse(Label label) => Emit(OpCodes.Brfalse, label);
	
	/// <inheritdoc cref="OpCodes.Brfalse_S"/>
	public TBody Brfalse_S(Label label) => Emit(OpCodes.Brfalse_S, label);
	
	/// <inheritdoc cref="OpCodes.Brtrue"/>
	public TBody Brtrue(Label label) => Emit(OpCodes.Brtrue, label);
	
	/// <inheritdoc cref="OpCodes.Brtrue_S"/>
	public TBody Brtrue_S(Label label) => Emit(OpCodes.Brtrue_S, label);
	
	/// <summary><inheritdoc cref="OpCodes.Call"/></summary>
	/// <inheritdoc cref="ILGenerator.Emit(OpCode, MethodInfo)" path="/param[@name='meth']"/>
	/// <exception><inheritdoc cref="ILGenerator.Emit(OpCode, MethodInfo)"/></exception>
	public TBody Call(MethodInfo meth) => Emit(OpCodes.Call, meth);
	
	/// <inheritdoc cref="ILGenerator.EmitCall"/>
	public TBody Call(MethodInfo method, Type[]? optionalParameterTypes) { il.EmitCall(OpCodes.Call, method, optionalParameterTypes); return This; }
	
	/// <inheritdoc cref="OpCodes.Call"/>
	public TBody Call(ConstructorInfo constructor) => Emit(OpCodes.Call, constructor);
	
	/// <inheritdoc cref="OpCodes.Calli"/>
	public TBody Calli(CallingConvention callingConvention, Type returnType, Type[] parameterTypes) { il.EmitCalli(OpCodes.Calli, callingConvention, returnType, parameterTypes); return This; }
	
	/// <inheritdoc cref="OpCodes.Calli"/>
	public TBody Calli(CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes) { il.EmitCalli(OpCodes.Calli, callingConvention, returnType, parameterTypes, optionalParameterTypes); return This; }
	
	/// <inheritdoc cref="OpCodes.Callvirt"/>
	public TBody Callvirt(MethodInfo method) => Emit(OpCodes.Callvirt, method);
	
	/// <inheritdoc cref="OpCodes.Castclass"/>
	public TBody Castclass(Type type) => Emit(OpCodes.Castclass, type);
	
	/// <inheritdoc cref="OpCodes.Castclass"/>
	public TBody Castclass<T>() => Castclass(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Ceq"/>
	public TBody Ceq() => Emit(OpCodes.Ceq);
	
	/// <inheritdoc cref="OpCodes.Cgt"/>
	public TBody Cgt() => Emit(OpCodes.Cgt);
	
	/// <inheritdoc cref="OpCodes.Cgt_Un"/>
	public TBody Cgt_Un() => Emit(OpCodes.Cgt_Un);
	
	/// <inheritdoc cref="OpCodes.Ckfinite"/>
	public TBody Ckfinite() => Emit(OpCodes.Ckfinite);
	
	/// <inheritdoc cref="OpCodes.Clt"/>
	public TBody Clt() => Emit(OpCodes.Clt);
	
	/// <inheritdoc cref="OpCodes.Clt_Un"/>
	public TBody Clt_Un() => Emit(OpCodes.Clt_Un);
	
	/// <inheritdoc cref="OpCodes.Constrained"/>
	public TBody Constrained(Type type) => Emit(OpCodes.Constrained, type);
	
	/// <inheritdoc cref="OpCodes.Constrained"/>
	public TBody Constrained<T>() => Constrained(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Conv_I"/>
	public TBody Conv_I() => Emit(OpCodes.Conv_I);
	
	/// <inheritdoc cref="OpCodes.Conv_I1"/>
	public TBody Conv_I1() => Emit(OpCodes.Conv_I1);
	
	/// <inheritdoc cref="OpCodes.Conv_I2"/>
	public TBody Conv_I2() => Emit(OpCodes.Conv_I2);
	
	/// <inheritdoc cref="OpCodes.Conv_I4"/>
	public TBody Conv_I4() => Emit(OpCodes.Conv_I4);
	
	/// <inheritdoc cref="OpCodes.Conv_I8"/>
	public TBody Conv_I8() => Emit(OpCodes.Conv_I8);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I"/>
	public TBody Conv_Ovf_I() => Emit(OpCodes.Conv_Ovf_I);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I_Un"/>
	public TBody Conv_Ovf_I_Un() => Emit(OpCodes.Conv_Ovf_I_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I1"/>
	public TBody Conv_Ovf_I1() => Emit(OpCodes.Conv_Ovf_I1);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I1_Un"/>
	public TBody Conv_Ovf_I1_Un() => Emit(OpCodes.Conv_Ovf_I1_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I2"/>
	public TBody Conv_Ovf_I2() => Emit(OpCodes.Conv_Ovf_I2);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I2_Un"/>
	public TBody Conv_Ovf_I2_Un() => Emit(OpCodes.Conv_Ovf_I2_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I4"/>
	public TBody Conv_Ovf_I4() => Emit(OpCodes.Conv_Ovf_I4);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I4_Un"/>
	public TBody Conv_Ovf_I4_Un() => Emit(OpCodes.Conv_Ovf_I4_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I8"/>
	public TBody Conv_Ovf_I8() => Emit(OpCodes.Conv_Ovf_I8);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I8_Un"/>
	public TBody Conv_Ovf_I8_Un() => Emit(OpCodes.Conv_Ovf_I8_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U"/>
	public TBody Conv_Ovf_U() => Emit(OpCodes.Conv_Ovf_U);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U_Un"/>
	public TBody Conv_Ovf_U_Un() => Emit(OpCodes.Conv_Ovf_U_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U1"/>
	public TBody Conv_Ovf_U1() => Emit(OpCodes.Conv_Ovf_U1);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U1_Un"/>
	public TBody Conv_Ovf_U1_Un() => Emit(OpCodes.Conv_Ovf_U1_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U2"/>
	public TBody Conv_Ovf_U2() => Emit(OpCodes.Conv_Ovf_U2);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U2_Un"/>
	public TBody Conv_Ovf_U2_Un() => Emit(OpCodes.Conv_Ovf_U2_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U4"/>
	public TBody Conv_Ovf_U4() => Emit(OpCodes.Conv_Ovf_U4);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U4_Un"/>
	public TBody Conv_Ovf_U4_Un() => Emit(OpCodes.Conv_Ovf_U4_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U8"/>
	public TBody Conv_Ovf_U8() => Emit(OpCodes.Conv_Ovf_U8);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U8_Un"/>
	public TBody Conv_Ovf_U8_Un() => Emit(OpCodes.Conv_Ovf_U8_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_R_Un"/>
	public TBody Conv_R_Un() => Emit(OpCodes.Conv_R_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_R4"/>
	public TBody Conv_R4() => Emit(OpCodes.Conv_R4);
	
	/// <inheritdoc cref="OpCodes.Conv_R8"/>
	public TBody Conv_R8() => Emit(OpCodes.Conv_R8);
	
	/// <inheritdoc cref="OpCodes.Conv_U"/>
	public TBody Conv_U() => Emit(OpCodes.Conv_U);
	
	/// <inheritdoc cref="OpCodes.Conv_U1"/>
	public TBody Conv_U1() => Emit(OpCodes.Conv_U1);
	
	/// <inheritdoc cref="OpCodes.Conv_U2"/>
	public TBody Conv_U2() => Emit(OpCodes.Conv_U2);
	
	/// <inheritdoc cref="OpCodes.Conv_U4"/>
	public TBody Conv_U4() => Emit(OpCodes.Conv_U4);
	
	/// <inheritdoc cref="OpCodes.Conv_U8"/>
	public TBody Conv_U8() => Emit(OpCodes.Conv_U8);
	
	/// <inheritdoc cref="OpCodes.Cpblk"/>
	public TBody Cpblk() => Emit(OpCodes.Cpblk);
	
	/// <inheritdoc cref="OpCodes.Cpobj"/>
	public TBody Cpobj(Type type) => Emit(OpCodes.Cpobj, type);
	
	/// <inheritdoc cref="OpCodes.Cpobj"/>
	public TBody Cpobj<T>() => Cpobj(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Div"/>
	public TBody Div() => Emit(OpCodes.Div);
	
	/// <inheritdoc cref="OpCodes.Div_Un"/>
	public TBody Div_Un() => Emit(OpCodes.Div_Un);
	
	/// <inheritdoc cref="OpCodes.Dup"/>
	public TBody Dup() => Emit(OpCodes.Dup);
	
	/// <inheritdoc cref="OpCodes.Endfilter"/>
	public TBody Endfilter() => Emit(OpCodes.Endfilter);
	
	/// <inheritdoc cref="OpCodes.Endfinally"/>
	public TBody Endfinally() => Emit(OpCodes.Endfinally);
	
	/// <inheritdoc cref="OpCodes.Initblk"/>
	public TBody Initblk() => Emit(OpCodes.Initblk);
	
	/// <inheritdoc cref="OpCodes.Initobj"/>
	public TBody Initobj(Type type) => Emit(OpCodes.Initobj, type);
	
	/// <inheritdoc cref="OpCodes.Initobj"/>
	public TBody Initobj<T>() => Initobj(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Isinst"/>
	public TBody Isinst(Type type) => Emit(OpCodes.Isinst, type);
	
	/// <inheritdoc cref="OpCodes.Isinst"/>
	public TBody Isinst<T>() => Isinst(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Jmp"/>
	public TBody Jmp(MethodInfo method) => Emit(OpCodes.Jmp, method);
	
	/// <inheritdoc cref="OpCodes.Ldarg_0"/>
	public TBody Ldarg_0() => Emit(OpCodes.Ldarg_0);
	
	/// <inheritdoc cref="OpCodes.Ldarg_1"/>
	public TBody Ldarg_1() => Emit(OpCodes.Ldarg_1);
	
	/// <inheritdoc cref="OpCodes.Ldarg_2"/>
	public TBody Ldarg_2() => Emit(OpCodes.Ldarg_2);
	
	/// <inheritdoc cref="OpCodes.Ldarg_3"/>
	public TBody Ldarg_3() => Emit(OpCodes.Ldarg_3);
	
	/// <inheritdoc cref="OpCodes.Ldarg"/>
	public TBody Ldarg(Int16 index) => Emit(OpCodes.Ldarg, index);
	
	/// <inheritdoc cref="OpCodes.Ldarg_S"/>
	public TBody Ldarg_S(Byte index) => Emit(OpCodes.Ldarg_S, index);
	
	/// <inheritdoc cref="OpCodes.Ldarga"/>
	public TBody Ldarga(Int32 index) => Emit(OpCodes.Ldarga, index);
	
	/// <inheritdoc cref="OpCodes.Ldarga_S"/>
	public TBody Ldarga_S(Byte index) => Emit(OpCodes.Ldarga_S, index);
	
	/// <inheritdoc cref="OpCodes.Ldc_I4"/>
	public TBody Ldc_I4(Int32 value) => Emit(OpCodes.Ldc_I4, value);

	/// <inheritdoc cref="OpCodes.Ldc_I4_S"/>
	public TBody Ldc_I4_S(Byte value) => Emit(OpCodes.Ldc_I4_S, value);

	/// <inheritdoc cref="OpCodes.Ldc_I4_S"/>
	public TBody Ldc_I4_S(SByte value) => Emit(OpCodes.Ldc_I4_S, value);

	/// <inheritdoc cref="OpCodes.Ldc_I4_0"/>
	public TBody Ldc_I4_0() => Emit(OpCodes.Ldc_I4_0);

	/// <inheritdoc cref="OpCodes.Ldc_I4_1"/>
	public TBody Ldc_I4_1() => Emit(OpCodes.Ldc_I4_1);

	/// <inheritdoc cref="OpCodes.Ldc_I4_2"/>
	public TBody Ldc_I4_2() => Emit(OpCodes.Ldc_I4_2);

	/// <inheritdoc cref="OpCodes.Ldc_I4_3"/>
	public TBody Ldc_I4_3() => Emit(OpCodes.Ldc_I4_3);

	/// <inheritdoc cref="OpCodes.Ldc_I4_4"/>
	public TBody Ldc_I4_4() => Emit(OpCodes.Ldc_I4_4);

	/// <inheritdoc cref="OpCodes.Ldc_I4_5"/>
	public TBody Ldc_I4_5() => Emit(OpCodes.Ldc_I4_5);

	/// <inheritdoc cref="OpCodes.Ldc_I4_6"/>
	public TBody Ldc_I4_6() => Emit(OpCodes.Ldc_I4_6);

	/// <inheritdoc cref="OpCodes.Ldc_I4_7"/>
	public TBody Ldc_I4_7() => Emit(OpCodes.Ldc_I4_7);

	/// <inheritdoc cref="OpCodes.Ldc_I4_8"/>
	public TBody Ldc_I4_8() => Emit(OpCodes.Ldc_I4_8);

	/// <inheritdoc cref="OpCodes.Ldc_I4_M1"/>
	public TBody Ldc_I4_M1() => Emit(OpCodes.Ldc_I4_M1);

	/// <inheritdoc cref="OpCodes.Ldc_I8"/>
	public TBody Ldc_I8(Int64 value) => Emit(OpCodes.Ldc_I8, value);

	/// <inheritdoc cref="OpCodes.Ldc_R4"/>
	public TBody Ldc_R4(Single value) => Emit(OpCodes.Ldc_R4, value);

	/// <inheritdoc cref="OpCodes.Ldc_R8"/>
	public TBody Ldc_R8(Double value) => Emit(OpCodes.Ldc_R8, value);

	/// <inheritdoc cref="OpCodes.Ldelem"/>
	public TBody Ldelem(Type type) => Emit(OpCodes.Ldelem, type);

	/// <inheritdoc cref="OpCodes.Ldelem"/>
	public TBody Ldelem<T>() => Ldelem(typeof(T));

	/// <inheritdoc cref="OpCodes.Ldelem_I"/>
	public TBody Ldelem_I() => Emit(OpCodes.Ldelem_I);

	/// <inheritdoc cref="OpCodes.Ldelem_I1"/>
	public TBody Ldelem_I1() => Emit(OpCodes.Ldelem_I1);

	/// <inheritdoc cref="OpCodes.Ldelem_I2"/>
	public TBody Ldelem_I2() => Emit(OpCodes.Ldelem_I2);

	/// <inheritdoc cref="OpCodes.Ldelem_I4"/>
	public TBody Ldelem_I4() => Emit(OpCodes.Ldelem_I4);

	/// <inheritdoc cref="OpCodes.Ldelem_I8"/>
	public TBody Ldelem_I8() => Emit(OpCodes.Ldelem_I8);

	/// <inheritdoc cref="OpCodes.Ldelem_R4"/>
	public TBody Ldelem_R4() => Emit(OpCodes.Ldelem_R4);

	/// <inheritdoc cref="OpCodes.Ldelem_R8"/>
	public TBody Ldelem_R8() => Emit(OpCodes.Ldelem_R8);

	/// <inheritdoc cref="OpCodes.Ldelem_Ref"/>
	public TBody Ldelem_Ref() => Emit(OpCodes.Ldelem_Ref);

	/// <inheritdoc cref="OpCodes.Ldelem_U1"/>
	public TBody Ldelem_U1() => Emit(OpCodes.Ldelem_U1);

	/// <inheritdoc cref="OpCodes.Ldelem_U2"/>
	public TBody Ldelem_U2() => Emit(OpCodes.Ldelem_U2);

	/// <inheritdoc cref="OpCodes.Ldelem_U4"/>
	public TBody Ldelem_U4() => Emit(OpCodes.Ldelem_U4);

	/// <inheritdoc cref="OpCodes.Ldelema"/>
	public TBody Ldelema(Type type) => Emit(OpCodes.Ldelema, type);

	/// <inheritdoc cref="OpCodes.Ldelema"/>
	public TBody Ldelema<T>() => Ldelema(typeof(T));

	/// <inheritdoc cref="OpCodes.Ldfld"/>
	public TBody Ldfld(FieldInfo field) => Emit(OpCodes.Ldfld, field);

	/// <inheritdoc cref="OpCodes.Ldflda"/>
	public TBody Ldflda(FieldInfo field) => Emit(OpCodes.Ldflda, field);

	/// <inheritdoc cref="OpCodes.Ldftn"/>
	public TBody Ldftn(MethodInfo method) => Emit(OpCodes.Ldftn, method);

	/// <inheritdoc cref="OpCodes.Ldind_I"/>
	public TBody Ldind_I() => Emit(OpCodes.Ldind_I);

	/// <inheritdoc cref="OpCodes.Ldind_I1"/>
	public TBody Ldind_I1() => Emit(OpCodes.Ldind_I1);

	/// <inheritdoc cref="OpCodes.Ldind_I2"/>
	public TBody Ldind_I2() => Emit(OpCodes.Ldind_I2);

	/// <inheritdoc cref="OpCodes.Ldind_I4"/>
	public TBody Ldind_I4() => Emit(OpCodes.Ldind_I4);

	/// <inheritdoc cref="OpCodes.Ldind_I8"/>
	public TBody Ldind_I8() => Emit(OpCodes.Ldind_I8);

	/// <inheritdoc cref="OpCodes.Ldind_R4"/>
	public TBody Ldind_R4() => Emit(OpCodes.Ldind_R4);

	/// <inheritdoc cref="OpCodes.Ldind_R8"/>
	public TBody Ldind_R8() => Emit(OpCodes.Ldind_R8);

	/// <inheritdoc cref="OpCodes.Ldind_Ref"/>
	public TBody Ldind_Ref() => Emit(OpCodes.Ldind_Ref);

	/// <inheritdoc cref="OpCodes.Ldind_U1"/>
	public TBody Ldind_U1() => Emit(OpCodes.Ldind_U1);

	/// <inheritdoc cref="OpCodes.Ldind_U2"/>
	public TBody Ldind_U2() => Emit(OpCodes.Ldind_U2);

	/// <inheritdoc cref="OpCodes.Ldind_U4"/>
	public TBody Ldind_U4() => Emit(OpCodes.Ldind_U4);

	/// <inheritdoc cref="OpCodes.Ldlen"/>
	public TBody Ldlen() => Emit(OpCodes.Ldlen);

	/// <inheritdoc cref="OpCodes.Ldloc"/>
	public TBody Ldloc(Int16 index) => Emit(OpCodes.Ldloc, index);

	/// <inheritdoc cref="OpCodes.Ldloc"/>
	public TBody Ldloc(LocalBuilder local) => Emit(OpCodes.Ldloc, local);

	/// <inheritdoc cref="OpCodes.Ldloc_0"/>
	public TBody Ldloc_0() => Emit(OpCodes.Ldloc_0);

	/// <inheritdoc cref="OpCodes.Ldloc_1"/>
	public TBody Ldloc_1() => Emit(OpCodes.Ldloc_1);

	/// <inheritdoc cref="OpCodes.Ldloc_2"/>
	public TBody Ldloc_2() => Emit(OpCodes.Ldloc_2);

	/// <inheritdoc cref="OpCodes.Ldloc_3"/>
	public TBody Ldloc_3() => Emit(OpCodes.Ldloc_3);

	/// <inheritdoc cref="OpCodes.Ldloc_S"/>
	public TBody Ldloc_S(Byte index) => Emit(OpCodes.Ldloc_S, index);

	/// <inheritdoc cref="OpCodes.Ldloc_S"/>
	public TBody Ldloc_S(LocalBuilder local) => Emit(OpCodes.Ldloc_S, local);

	/// <inheritdoc cref="OpCodes.Ldloca"/>
	public TBody Ldloca(Int16 index) => Emit(OpCodes.Ldloca, index);

	/// <inheritdoc cref="OpCodes.Ldloca"/>
	public TBody Ldloca(LocalBuilder local) => Emit(OpCodes.Ldloca, local);

	/// <inheritdoc cref="OpCodes.Ldloca_S"/>
	public TBody Ldloca_S(Byte index) => Emit(OpCodes.Ldloca_S, index);

	/// <inheritdoc cref="OpCodes.Ldloca_S"/>
	public TBody Ldloca_S(LocalBuilder local) => Emit(OpCodes.Ldloca_S, local);

	/// <inheritdoc cref="OpCodes.Ldnull"/>
	public TBody Ldnull() => Emit(OpCodes.Ldnull);

	/// <inheritdoc cref="OpCodes.Ldobj"/>
	public TBody Ldobj(Type type) => Emit(OpCodes.Ldobj, type);

	/// <inheritdoc cref="OpCodes.Ldobj"/>
	public TBody Ldobj<T>() => Ldobj(typeof(T));

	/// <inheritdoc cref="OpCodes.Ldsfld"/>
	public TBody Ldsfld(FieldInfo field) => Emit(OpCodes.Ldsfld, field);

	/// <inheritdoc cref="OpCodes.Ldsflda"/>
	public TBody Ldsflda(FieldInfo field) => Emit(OpCodes.Ldsflda, field);

	/// <inheritdoc cref="OpCodes.Ldstr"/>
	public TBody Ldstr(String value) => Emit(OpCodes.Ldstr, value);

	/// <inheritdoc cref="OpCodes.Ldtoken"/>
	public TBody Ldtoken(MethodInfo methodInfo) => Emit(OpCodes.Ldtoken, methodInfo);

	/// <inheritdoc cref="OpCodes.Ldtoken"/>
	public TBody Ldtoken(FieldInfo fieldInfo) => Emit(OpCodes.Ldtoken, fieldInfo);

	/// <inheritdoc cref="OpCodes.Ldtoken"/>
	public TBody Ldtoken(Type type) => Emit(OpCodes.Ldtoken, type);

	/// <inheritdoc cref="OpCodes.Ldtoken"/>
	public TBody Ldtoken<T>() => Ldtoken(typeof(T));

	/// <inheritdoc cref="OpCodes.Ldvirtftn"/>
	public TBody Ldvirtftn(MethodInfo method) => Emit(OpCodes.Ldvirtftn, method);

	/// <inheritdoc cref="OpCodes.Leave"/>
	public TBody Leave(Label label) => Emit(OpCodes.Leave, label);

	/// <inheritdoc cref="OpCodes.Leave_S"/>
	public TBody Leave_S(Label label) => Emit(OpCodes.Leave_S, label);

	/// <inheritdoc cref="OpCodes.Localloc"/>
	public TBody Localloc() => Emit(OpCodes.Localloc);

	/// <inheritdoc cref="OpCodes.Mkrefany"/>
	public TBody Mkrefany(Type type) => Emit(OpCodes.Mkrefany, type);

	/// <inheritdoc cref="OpCodes.Mkrefany"/>
	public TBody Mkrefany<T>() => Mkrefany(typeof(T));

	/// <inheritdoc cref="OpCodes.Mul"/>
	public TBody Mul() => Emit(OpCodes.Mul);

	/// <inheritdoc cref="OpCodes.Mul_Ovf"/>
	public TBody Mul_Ovf() => Emit(OpCodes.Mul_Ovf);

	/// <inheritdoc cref="OpCodes.Mul_Ovf_Un"/>
	public TBody Mul_Ovf_Un() => Emit(OpCodes.Mul_Ovf_Un);

	/// <inheritdoc cref="OpCodes.Neg"/>
	public TBody Neg() => Emit(OpCodes.Neg);

	/// <inheritdoc cref="OpCodes.Newarr"/>
	public TBody Newarr(Type type) => Emit(OpCodes.Newarr, type);

	/// <inheritdoc cref="OpCodes.Newarr"/>
	public TBody Newarr<T>() => Newarr(typeof(T));

	/// <inheritdoc cref="OpCodes.Newobj"/>
	public TBody Newobj(ConstructorInfo constructor) => Emit(OpCodes.Newobj, constructor);

	/// <inheritdoc cref="OpCodes.Newobj"/>
	public TBody Newobj<T>() where T : new() => Newobj(typeof(T).GetConstructor(Type.EmptyTypes)!);

	/// <inheritdoc cref="OpCodes.Nop"/>
	public TBody Nop() => Emit(OpCodes.Nop);

	/// <inheritdoc cref="OpCodes.Not"/>
	public TBody Not() => Emit(OpCodes.Not);

	/// <inheritdoc cref="OpCodes.Or"/>
	public TBody Or() => Emit(OpCodes.Or);

	/// <inheritdoc cref="OpCodes.Pop"/>
	public TBody Pop() => Emit(OpCodes.Pop);

	/// <inheritdoc cref="OpCodes.Readonly"/>
	public TBody Readonly() => Emit(OpCodes.Readonly);

	/// <inheritdoc cref="OpCodes.Refanytype"/>
	public TBody Refanytype() => Emit(OpCodes.Refanytype);

	/// <inheritdoc cref="OpCodes.Refanyval"/>
	public TBody Refanyval(Type type) => Emit(OpCodes.Refanyval, type);

	/// <inheritdoc cref="OpCodes.Refanyval"/>
	public TBody Refanyval<T>() => Refanyval(typeof(T));

	/// <inheritdoc cref="OpCodes.Rem"/>
	public TBody Rem() => Emit(OpCodes.Rem);

	/// <inheritdoc cref="OpCodes.Rem_Un"/>
	public TBody Rem_Un() => Emit(OpCodes.Rem_Un);

	/// <inheritdoc cref="OpCodes.Ret"/>
	public TBody Ret() => Emit(OpCodes.Ret);

	/// <inheritdoc cref="OpCodes.Rethrow"/>
	public TBody Rethrow() => Emit(OpCodes.Rethrow);

	/// <inheritdoc cref="OpCodes.Shl"/>
	public TBody Shl() => Emit(OpCodes.Shl);

	/// <inheritdoc cref="OpCodes.Shr"/>
	public TBody Shr() => Emit(OpCodes.Shr);

	/// <inheritdoc cref="OpCodes.Shr_Un"/>
	public TBody Shr_Un() => Emit(OpCodes.Shr_Un);

	/// <inheritdoc cref="OpCodes.Sizeof"/>
	public TBody Sizeof(Type type) => Emit(OpCodes.Sizeof, type);

	/// <inheritdoc cref="OpCodes.Sizeof"/>
	public TBody Sizeof<T>() => Sizeof(typeof(T));

	/// <inheritdoc cref="OpCodes.Starg"/>
	public TBody Starg(Int16 index) => Emit(OpCodes.Starg, index);

	/// <inheritdoc cref="OpCodes.Starg_S"/>
	public TBody Starg_S(Byte index) => Emit(OpCodes.Starg_S, index);

	/// <inheritdoc cref="OpCodes.Stelem"/>
	public TBody Stelem(Type type) => Emit(OpCodes.Stelem, type);

	/// <inheritdoc cref="OpCodes.Stelem"/>
	public TBody Stelem<T>() => Stelem(typeof(T));

	/// <inheritdoc cref="OpCodes.Stelem_I"/>
	public TBody Stelem_I() => Emit(OpCodes.Stelem_I);

	/// <inheritdoc cref="OpCodes.Stelem_I1"/>
	public TBody Stelem_I1() => Emit(OpCodes.Stelem_I1);

	/// <inheritdoc cref="OpCodes.Stelem_I2"/>
	public TBody Stelem_I2() => Emit(OpCodes.Stelem_I2);

	/// <inheritdoc cref="OpCodes.Stelem_I4"/>
	public TBody Stelem_I4() => Emit(OpCodes.Stelem_I4);

	/// <inheritdoc cref="OpCodes.Stelem_I8"/>
	public TBody Stelem_I8() => Emit(OpCodes.Stelem_I8);

	/// <inheritdoc cref="OpCodes.Stelem_R4"/>
	public TBody Stelem_R4() => Emit(OpCodes.Stelem_R4);

	/// <inheritdoc cref="OpCodes.Stelem_R8"/>
	public TBody Stelem_R8() => Emit(OpCodes.Stelem_R8);

	/// <inheritdoc cref="OpCodes.Stelem_Ref"/>
	public TBody Stelem_Ref() => Emit(OpCodes.Stelem_Ref);

	/// <inheritdoc cref="OpCodes.Stfld"/>
	public TBody Stfld(FieldInfo field) => Emit(OpCodes.Stfld, field);

	/// <inheritdoc cref="OpCodes.Stind_I"/>
	public TBody Stind_I() => Emit(OpCodes.Stind_I);

	/// <inheritdoc cref="OpCodes.Stind_I1"/>
	public TBody Stind_I1() => Emit(OpCodes.Stind_I1);

	/// <inheritdoc cref="OpCodes.Stind_I2"/>
	public TBody Stind_I2() => Emit(OpCodes.Stind_I2);

	/// <inheritdoc cref="OpCodes.Stind_I4"/>
	public TBody Stind_I4() => Emit(OpCodes.Stind_I4);

	/// <inheritdoc cref="OpCodes.Stind_I8"/>
	public TBody Stind_I8() => Emit(OpCodes.Stind_I8);

	/// <inheritdoc cref="OpCodes.Stind_R4"/>
	public TBody Stind_R4() => Emit(OpCodes.Stind_R4);

	/// <inheritdoc cref="OpCodes.Stind_R8"/>
	public TBody Stind_R8() => Emit(OpCodes.Stind_R8);

	/// <inheritdoc cref="OpCodes.Stind_Ref"/>
	public TBody Stind_Ref() => Emit(OpCodes.Stind_Ref);

	/// <inheritdoc cref="OpCodes.Stloc"/>
	public TBody Stloc(Int16 index) => Emit(OpCodes.Stloc, index);

	/// <inheritdoc cref="OpCodes.Stloc"/>
	public TBody Stloc(LocalBuilder local) => Emit(OpCodes.Stloc, local);

	/// <inheritdoc cref="OpCodes.Stloc_S"/>
	public TBody Stloc_S(Byte index) => Emit(OpCodes.Stloc_S, index);

	/// <inheritdoc cref="OpCodes.Stloc_S"/>
	public TBody Stloc_S(LocalBuilder local) => Emit(OpCodes.Stloc_S, local);

	/// <inheritdoc cref="OpCodes.Stobj"/>
	public TBody Stobj(Type type) => Emit(OpCodes.Stobj, type);

	/// <inheritdoc cref="OpCodes.Stobj"/>
	public TBody Stobj<T>() => Stobj(typeof(T));

	/// <inheritdoc cref="OpCodes.Stsfld"/>
	public TBody Stsfld(FieldInfo field) => Emit(OpCodes.Stsfld, field);

	/// <inheritdoc cref="OpCodes.Sub"/>
	public TBody Sub() => Emit(OpCodes.Sub);

	/// <inheritdoc cref="OpCodes.Sub_Ovf"/>
	public TBody Sub_Ovf() => Emit(OpCodes.Sub_Ovf);

	/// <inheritdoc cref="OpCodes.Sub_Ovf_Un"/>
	public TBody Sub_Ovf_Un() => Emit(OpCodes.Sub_Ovf_Un);

	/// <inheritdoc cref="OpCodes.Switch"/>
	public TBody Switch(Label[] labels) => Emit(OpCodes.Switch, labels);

	/// <inheritdoc cref="OpCodes.Tailcall"/>
	public TBody Tailcall() => Emit(OpCodes.Tailcall);

	/// <inheritdoc cref="OpCodes.Throw"/>
	public TBody Throw() => Emit(OpCodes.Throw);

	/// <inheritdoc cref="OpCodes.Unaligned"/>
	public TBody Unaligned(Byte alignment) => Emit(OpCodes.Unaligned, alignment);

	/// <inheritdoc cref="OpCodes.Unbox"/>
	public TBody Unbox(Type type) => Emit(OpCodes.Unbox, type);

	/// <inheritdoc cref="OpCodes.Unbox"/>
	public TBody Unbox<T>() => Unbox(typeof(T));

	/// <inheritdoc cref="OpCodes.Unbox_Any"/>
	public TBody Unbox_Any(Type type) => Emit(OpCodes.Unbox_Any, type);

	/// <inheritdoc cref="OpCodes.Unbox_Any"/>
	public TBody Unbox_Any<T>() => Unbox_Any(typeof(T));

	/// <inheritdoc cref="OpCodes.Volatile"/>
	public TBody Volatile() => Emit(OpCodes.Volatile);

	/// <inheritdoc cref="OpCodes.Xor"/>
	public TBody Xor() => Emit(OpCodes.Xor);

	/// <inheritdoc cref="ILGenerator.DeclareLocal(System.Type)"/>
	public TBody DeclareLocal(Type type, out LocalBuilder local) { local = il.DeclareLocal(type); return This; }
	
	/// <inheritdoc cref="ILGenerator.DeclareLocal(System.Type)"/>
	public TBody DeclareLocal<T>(out LocalBuilder local) => DeclareLocal(typeof(T), out local);
	
	/// <inheritdoc cref="ILGenerator.DefineLabel"/>
	public TBody DefineLabel(out Label label) { label = il.DefineLabel(); return This; }
	
	/// <inheritdoc cref="ILGenerator.MarkLabel(Label)"/>
	public TBody MarkLabel(Label label) { il.MarkLabel(label); return This; }
	
	/// <inheritdoc cref="ILGenerator.BeginExceptionBlock"/>
	public TBody BeginExceptionBlock() { il.BeginExceptionBlock(); return This; }
	
	/// <inheritdoc cref="ILGenerator.BeginExceptFilterBlock"/>
	public TBody BeginExceptFilterBlock() { il.BeginExceptFilterBlock(); return This; }
	
	/// <inheritdoc cref="ILGenerator.BeginCatchBlock"/>
	public TBody BeginCatchBlock() { il.BeginCatchBlock(null); return This; }
	
	/// <inheritdoc cref="ILGenerator.BeginCatchBlock(System.Type)"/>
	public TBody BeginCatchBlock(Type exceptionType) { il.BeginCatchBlock(exceptionType); return This; }
	
	/// <inheritdoc cref="ILGenerator.BeginCatchBlock(System.Type)"/>
	public TBody BeginCatchBlock<TException>() where TException : Exception => BeginCatchBlock(typeof(TException));
	
	/// <inheritdoc cref="ILGenerator.EndExceptionBlock"/>
	public TBody EndExceptionBlock() { il.EndExceptionBlock(); return This; }
	
}

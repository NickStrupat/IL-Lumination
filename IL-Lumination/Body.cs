using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace IL_Lumination;

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

public sealed class Body(ILGenerator il)
{
	private readonly ILGenerator il = il;

	private Body Emit(OpCode opcode) { il.Emit(opcode); return this; }
	private Body Emit(OpCode opcode, Type type) { il.Emit(opcode, type); return this; }
	private Body Emit(OpCode opcode, MethodInfo meth) { il.Emit(opcode, meth); return this; }
	private Body Emit(OpCode opcode, ConstructorInfo constructor) { il.Emit(opcode, constructor); return this; }
	private Body Emit(OpCode opcode, String str) { il.Emit(opcode, str); return this; }
	private Body Emit(OpCode opcode, Byte arg) { il.Emit(opcode, arg); return this; }
	private Body Emit(OpCode opcode, SByte arg) { il.Emit(opcode, arg); return this; }
	private Body Emit(OpCode opcode, Int16 arg) { il.Emit(opcode, arg); return this; }
	private Body Emit(OpCode opcode, Int32 arg) { il.Emit(opcode, arg); return this; }
	private Body Emit(OpCode opcode, Int64 arg) { il.Emit(opcode, arg); return this; }
	private Body Emit(OpCode opcode, Single arg) { il.Emit(opcode, arg); return this; }
	private Body Emit(OpCode opcode, Double arg) { il.Emit(opcode, arg); return this; }
	private Body Emit(OpCode opcode, Label label) { il.Emit(opcode, label); return this; }
	private Body Emit(OpCode opcode, Label[] labels) { il.Emit(opcode, labels); return this; }
	private Body Emit(OpCode opcode, FieldInfo field) { il.Emit(opcode, field); return this; }
	private Body Emit(OpCode opcode, LocalBuilder local) { il.Emit(opcode, local); return this; }
	private Body Emit(OpCode opcode, SignatureHelper signature) { il.Emit(opcode, signature); return this; }

	/// <inheritdoc cref="OpCodes.Add"/>
	public Body Add() => Emit(OpCodes.Add);
	
	/// <inheritdoc cref="OpCodes.Add_Ovf"/>
	public Body Add_Ovf() => Emit(OpCodes.Add_Ovf);
	
	/// <inheritdoc cref="OpCodes.Add_Ovf_Un"/>
	public Body Add_Ovf_Un() => Emit(OpCodes.Add_Ovf_Un);
	
	/// <inheritdoc cref="OpCodes.And"/>
	public Body And() => Emit(OpCodes.And);
	
	/// <inheritdoc cref="OpCodes.Arglist"/>
	public Body Arglist() => Emit(OpCodes.Arglist);
	
	/// <inheritdoc cref="OpCodes.Beq"/>
	public Body Beq(Label label) => Emit(OpCodes.Beq, label);
	
	/// <inheritdoc cref="OpCodes.Beq_S"/>
	public Body Beq_S(Label label) => Emit(OpCodes.Beq_S, label);
	
	/// <inheritdoc cref="OpCodes.Bge"/>
	public Body Bge(Label label) => Emit(OpCodes.Bge, label);
	
	/// <inheritdoc cref="OpCodes.Bge_S"/>
	public Body Bge_S(Label label) => Emit(OpCodes.Bge_S, label);
	
	/// <inheritdoc cref="OpCodes.Bge_Un"/>
	public Body Bge_Un(Label label) => Emit(OpCodes.Bge_Un, label);
	
	/// <inheritdoc cref="OpCodes.Bge_Un_S"/>
	public Body Bge_Un_S(Label label) => Emit(OpCodes.Bge_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Bgt"/>
	public Body Bgt(Label label) => Emit(OpCodes.Bgt, label);
	
	/// <inheritdoc cref="OpCodes.Bgt_S"/>
	public Body Bgt_S(Label label) => Emit(OpCodes.Bgt_S, label);
	
	/// <inheritdoc cref="OpCodes.Bgt_Un"/>
	public Body Bgt_Un(Label label) => Emit(OpCodes.Bgt_Un, label);
	
	/// <inheritdoc cref="OpCodes.Bgt_Un_S"/>
	public Body Bgt_Un_S(Label label) => Emit(OpCodes.Bgt_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Ble"/>
	public Body Ble(Label label) => Emit(OpCodes.Ble, label);
	
	/// <inheritdoc cref="OpCodes.Ble_S"/>
	public Body Ble_S(Label label) => Emit(OpCodes.Ble_S, label);
	
	/// <inheritdoc cref="OpCodes.Ble_Un"/>
	public Body Ble_Un(Label label) => Emit(OpCodes.Ble_Un, label);
	
	/// <inheritdoc cref="OpCodes.Ble_Un_S"/>
	public Body Ble_Un_S(Label label) => Emit(OpCodes.Ble_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Blt"/>
	public Body Blt(Label label) => Emit(OpCodes.Blt, label);
	
	/// <inheritdoc cref="OpCodes.Blt_S"/>
	public Body Blt_S(Label label) => Emit(OpCodes.Blt_S, label);
	
	/// <inheritdoc cref="OpCodes.Blt_Un"/>
	public Body Blt_Un(Label label) => Emit(OpCodes.Blt_Un, label);
	
	/// <inheritdoc cref="OpCodes.Blt_Un_S"/>
	public Body Blt_Un_S(Label label) => Emit(OpCodes.Blt_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Bne_Un"/>
	/// <param name="label">The label to branch to if the two values are not equal.</param>
	public Body Bne_Un(Label label) => Emit(OpCodes.Bne_Un, label);
	
	/// <inheritdoc cref="OpCodes.Bne_Un_S"/>
	/// <param name="label">The label to branch to if the two values are not equal.</param>
	public Body Bne_Un_S(Label label) => Emit(OpCodes.Bne_Un_S, label);
	
	/// <inheritdoc cref="OpCodes.Box"/>
	public Body Box(Type type) => Emit(OpCodes.Box, type);
	
	/// <inheritdoc cref="OpCodes.Box"/>
	public Body Box<T>() where T : struct => Box(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Br"/>
	public Body Br(Label label) => Emit(OpCodes.Br, label);
	
	/// <inheritdoc cref="OpCodes.Br_S"/>
	public Body Br_S(Label label) => Emit(OpCodes.Br_S, label);
	
	/// <inheritdoc cref="OpCodes.Break"/>
	public Body Break() => Emit(OpCodes.Break);
	
	/// <inheritdoc cref="OpCodes.Brfalse"/>
	public Body Brfalse(Label label) => Emit(OpCodes.Brfalse, label);
	
	/// <inheritdoc cref="OpCodes.Brfalse_S"/>
	public Body Brfalse_S(Label label) => Emit(OpCodes.Brfalse_S, label);
	
	/// <inheritdoc cref="OpCodes.Brtrue"/>
	public Body Brtrue(Label label) => Emit(OpCodes.Brtrue, label);
	
	/// <inheritdoc cref="OpCodes.Brtrue_S"/>
	public Body Brtrue_S(Label label) => Emit(OpCodes.Brtrue_S, label);
	
	/// <summary><inheritdoc cref="OpCodes.Call"/></summary>
	/// <inheritdoc cref="ILGenerator.Emit(OpCode, MethodInfo)" path="/param[@name='meth']"/>
	/// <exception><inheritdoc cref="ILGenerator.Emit(OpCode, MethodInfo)"/></exception>
	public Body Call(MethodInfo meth) => Emit(OpCodes.Call, meth);
	
	/// <inheritdoc cref="ILGenerator.EmitCall"/>
	public Body Call(MethodInfo method, Type[]? optionalParameterTypes) { il.EmitCall(OpCodes.Call, method, optionalParameterTypes); return this; }
	
	/// <inheritdoc cref="OpCodes.Call"/>
	public Body Call(ConstructorInfo constructor) => Emit(OpCodes.Call, constructor);
	
	/// <inheritdoc cref="OpCodes.Calli"/>
	public Body Calli(CallingConvention callingConvention, Type returnType, Type[] parameterTypes) { il.EmitCalli(OpCodes.Calli, callingConvention, returnType, parameterTypes); return this; }
	
	/// <inheritdoc cref="OpCodes.Calli"/>
	public Body Calli(CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes) { il.EmitCalli(OpCodes.Calli, callingConvention, returnType, parameterTypes, optionalParameterTypes); return this; }
	
	/// <inheritdoc cref="OpCodes.Callvirt"/>
	public Body Callvirt(MethodInfo method) => Emit(OpCodes.Callvirt, method);
	
	/// <inheritdoc cref="OpCodes.Castclass"/>
	public Body Castclass(Type type) => Emit(OpCodes.Castclass, type);
	
	/// <inheritdoc cref="OpCodes.Castclass"/>
	public Body Castclass<T>() => Castclass(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Ceq"/>
	public Body Ceq() => Emit(OpCodes.Ceq);
	
	/// <inheritdoc cref="OpCodes.Cgt"/>
	public Body Cgt() => Emit(OpCodes.Cgt);
	
	/// <inheritdoc cref="OpCodes.Cgt_Un"/>
	public Body Cgt_Un() => Emit(OpCodes.Cgt_Un);
	
	/// <inheritdoc cref="OpCodes.Ckfinite"/>
	public Body Ckfinite() => Emit(OpCodes.Ckfinite);
	
	/// <inheritdoc cref="OpCodes.Clt"/>
	public Body Clt() => Emit(OpCodes.Clt);
	
	/// <inheritdoc cref="OpCodes.Clt_Un"/>
	public Body Clt_Un() => Emit(OpCodes.Clt_Un);
	
	/// <inheritdoc cref="OpCodes.Constrained"/>
	public Body Constrained(Type type) => Emit(OpCodes.Constrained, type);
	
	/// <inheritdoc cref="OpCodes.Constrained"/>
	public Body Constrained<T>() => Constrained(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Conv_I"/>
	public Body Conv_I() => Emit(OpCodes.Conv_I);
	
	/// <inheritdoc cref="OpCodes.Conv_I1"/>
	public Body Conv_I1() => Emit(OpCodes.Conv_I1);
	
	/// <inheritdoc cref="OpCodes.Conv_I2"/>
	public Body Conv_I2() => Emit(OpCodes.Conv_I2);
	
	/// <inheritdoc cref="OpCodes.Conv_I4"/>
	public Body Conv_I4() => Emit(OpCodes.Conv_I4);
	
	/// <inheritdoc cref="OpCodes.Conv_I8"/>
	public Body Conv_I8() => Emit(OpCodes.Conv_I8);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I"/>
	public Body Conv_Ovf_I() => Emit(OpCodes.Conv_Ovf_I);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I_Un"/>
	public Body Conv_Ovf_I_Un() => Emit(OpCodes.Conv_Ovf_I_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I1"/>
	public Body Conv_Ovf_I1() => Emit(OpCodes.Conv_Ovf_I1);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I1_Un"/>
	public Body Conv_Ovf_I1_Un() => Emit(OpCodes.Conv_Ovf_I1_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I2"/>
	public Body Conv_Ovf_I2() => Emit(OpCodes.Conv_Ovf_I2);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I2_Un"/>
	public Body Conv_Ovf_I2_Un() => Emit(OpCodes.Conv_Ovf_I2_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I4"/>
	public Body Conv_Ovf_I4() => Emit(OpCodes.Conv_Ovf_I4);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I4_Un"/>
	public Body Conv_Ovf_I4_Un() => Emit(OpCodes.Conv_Ovf_I4_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I8"/>
	public Body Conv_Ovf_I8() => Emit(OpCodes.Conv_Ovf_I8);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_I8_Un"/>
	public Body Conv_Ovf_I8_Un() => Emit(OpCodes.Conv_Ovf_I8_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U"/>
	public Body Conv_Ovf_U() => Emit(OpCodes.Conv_Ovf_U);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U_Un"/>
	public Body Conv_Ovf_U_Un() => Emit(OpCodes.Conv_Ovf_U_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U1"/>
	public Body Conv_Ovf_U1() => Emit(OpCodes.Conv_Ovf_U1);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U1_Un"/>
	public Body Conv_Ovf_U1_Un() => Emit(OpCodes.Conv_Ovf_U1_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U2"/>
	public Body Conv_Ovf_U2() => Emit(OpCodes.Conv_Ovf_U2);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U2_Un"/>
	public Body Conv_Ovf_U2_Un() => Emit(OpCodes.Conv_Ovf_U2_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U4"/>
	public Body Conv_Ovf_U4() => Emit(OpCodes.Conv_Ovf_U4);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U4_Un"/>
	public Body Conv_Ovf_U4_Un() => Emit(OpCodes.Conv_Ovf_U4_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U8"/>
	public Body Conv_Ovf_U8() => Emit(OpCodes.Conv_Ovf_U8);
	
	/// <inheritdoc cref="OpCodes.Conv_Ovf_U8_Un"/>
	public Body Conv_Ovf_U8_Un() => Emit(OpCodes.Conv_Ovf_U8_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_R_Un"/>
	public Body Conv_R_Un() => Emit(OpCodes.Conv_R_Un);
	
	/// <inheritdoc cref="OpCodes.Conv_R4"/>
	public Body Conv_R4() => Emit(OpCodes.Conv_R4);
	
	/// <inheritdoc cref="OpCodes.Conv_R8"/>
	public Body Conv_R8() => Emit(OpCodes.Conv_R8);
	
	/// <inheritdoc cref="OpCodes.Conv_U"/>
	public Body Conv_U() => Emit(OpCodes.Conv_U);
	
	/// <inheritdoc cref="OpCodes.Conv_U1"/>
	public Body Conv_U1() => Emit(OpCodes.Conv_U1);
	
	/// <inheritdoc cref="OpCodes.Conv_U2"/>
	public Body Conv_U2() => Emit(OpCodes.Conv_U2);
	
	/// <inheritdoc cref="OpCodes.Conv_U4"/>
	public Body Conv_U4() => Emit(OpCodes.Conv_U4);
	
	/// <inheritdoc cref="OpCodes.Conv_U8"/>
	public Body Conv_U8() => Emit(OpCodes.Conv_U8);
	
	/// <inheritdoc cref="OpCodes.Cpblk"/>
	public Body Cpblk() => Emit(OpCodes.Cpblk);
	
	/// <inheritdoc cref="OpCodes.Cpobj"/>
	public Body Cpobj(Type type) => Emit(OpCodes.Cpobj, type);
	
	/// <inheritdoc cref="OpCodes.Cpobj"/>
	public Body Cpobj<T>() => Cpobj(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Div"/>
	public Body Div() => Emit(OpCodes.Div);
	
	/// <inheritdoc cref="OpCodes.Div_Un"/>
	public Body Div_Un() => Emit(OpCodes.Div_Un);
	
	/// <inheritdoc cref="OpCodes.Dup"/>
	public Body Dup() => Emit(OpCodes.Dup);
	
	/// <inheritdoc cref="OpCodes.Endfilter"/>
	public Body Endfilter() => Emit(OpCodes.Endfilter);
	
	/// <inheritdoc cref="OpCodes.Endfinally"/>
	public Body Endfinally() => Emit(OpCodes.Endfinally);
	
	/// <inheritdoc cref="OpCodes.Initblk"/>
	public Body Initblk() => Emit(OpCodes.Initblk);
	
	/// <inheritdoc cref="OpCodes.Initobj"/>
	public Body Initobj(Type type) => Emit(OpCodes.Initobj, type);
	
	/// <inheritdoc cref="OpCodes.Initobj"/>
	public Body Initobj<T>() => Initobj(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Isinst"/>
	public Body Isinst(Type type) => Emit(OpCodes.Isinst, type);
	
	/// <inheritdoc cref="OpCodes.Isinst"/>
	public Body Isinst<T>() => Isinst(typeof(T));
	
	/// <inheritdoc cref="OpCodes.Jmp"/>
	public Body Jmp(MethodInfo method) => Emit(OpCodes.Jmp, method);
	
	/// <inheritdoc cref="OpCodes.Ldarg_0"/>
	public Body Ldarg_0() => Emit(OpCodes.Ldarg_0);
	
	/// <inheritdoc cref="OpCodes.Ldarg_1"/>
	public Body Ldarg_1() => Emit(OpCodes.Ldarg_1);
	
	/// <inheritdoc cref="OpCodes.Ldarg_2"/>
	public Body Ldarg_2() => Emit(OpCodes.Ldarg_2);
	
	/// <inheritdoc cref="OpCodes.Ldarg_3"/>
	public Body Ldarg_3() => Emit(OpCodes.Ldarg_3);
	
	/// <inheritdoc cref="OpCodes.Ldarg"/>
	public Body Ldarg(Int16 index) => Emit(OpCodes.Ldarg, index);
	
	/// <inheritdoc cref="OpCodes.Ldarg_S"/>
	public Body Ldarg_S(Byte index) => Emit(OpCodes.Ldarg_S, index);
	
	/// <inheritdoc cref="OpCodes.Ldarga"/>
	public Body Ldarga(Int32 index) => Emit(OpCodes.Ldarga, index);
	
	/// <inheritdoc cref="OpCodes.Ldarga_S"/>
	public Body Ldarga_S(Byte index) => Emit(OpCodes.Ldarga_S, index);
	
	/// <inheritdoc cref="OpCodes.Ldc_I4"/>
	public Body Ldc_I4(Int32 value) => Emit(OpCodes.Ldc_I4, value);

	/// <inheritdoc cref="OpCodes.Ldc_I4_S"/>
	public Body Ldc_I4_S(Byte value) => Emit(OpCodes.Ldc_I4_S, value);

	/// <inheritdoc cref="OpCodes.Ldc_I4_S"/>
	public Body Ldc_I4_S(SByte value) => Emit(OpCodes.Ldc_I4_S, value);

	/// <inheritdoc cref="OpCodes.Ldc_I4_0"/>
	public Body Ldc_I4_0() => Emit(OpCodes.Ldc_I4_0);

	/// <inheritdoc cref="OpCodes.Ldc_I4_1"/>
	public Body Ldc_I4_1() => Emit(OpCodes.Ldc_I4_1);

	/// <inheritdoc cref="OpCodes.Ldc_I4_2"/>
	public Body Ldc_I4_2() => Emit(OpCodes.Ldc_I4_2);

	/// <inheritdoc cref="OpCodes.Ldc_I4_3"/>
	public Body Ldc_I4_3() => Emit(OpCodes.Ldc_I4_3);

	/// <inheritdoc cref="OpCodes.Ldc_I4_4"/>
	public Body Ldc_I4_4() => Emit(OpCodes.Ldc_I4_4);

	/// <inheritdoc cref="OpCodes.Ldc_I4_5"/>
	public Body Ldc_I4_5() => Emit(OpCodes.Ldc_I4_5);

	/// <inheritdoc cref="OpCodes.Ldc_I4_6"/>
	public Body Ldc_I4_6() => Emit(OpCodes.Ldc_I4_6);

	/// <inheritdoc cref="OpCodes.Ldc_I4_7"/>
	public Body Ldc_I4_7() => Emit(OpCodes.Ldc_I4_7);

	/// <inheritdoc cref="OpCodes.Ldc_I4_8"/>
	public Body Ldc_I4_8() => Emit(OpCodes.Ldc_I4_8);

	/// <inheritdoc cref="OpCodes.Ldc_I4_M1"/>
	public Body Ldc_I4_M1() => Emit(OpCodes.Ldc_I4_M1);

	/// <inheritdoc cref="OpCodes.Ldc_I8"/>
	public Body Ldc_I8(Int64 value) => Emit(OpCodes.Ldc_I8, value);

	/// <inheritdoc cref="OpCodes.Ldc_R4"/>
	public Body Ldc_R4(Single value) => Emit(OpCodes.Ldc_R4, value);

	/// <inheritdoc cref="OpCodes.Ldc_R8"/>
	public Body Ldc_R8(Double value) => Emit(OpCodes.Ldc_R8, value);

	/// <inheritdoc cref="OpCodes.Ldelem"/>
	public Body Ldelem(Type type) => Emit(OpCodes.Ldelem, type);

	/// <inheritdoc cref="OpCodes.Ldelem"/>
	public Body Ldelem<T>() => Ldelem(typeof(T));

	/// <inheritdoc cref="OpCodes.Ldelem_I"/>
	public Body Ldelem_I() => Emit(OpCodes.Ldelem_I);

	/// <inheritdoc cref="OpCodes.Ldelem_I1"/>
	public Body Ldelem_I1() => Emit(OpCodes.Ldelem_I1);

	/// <inheritdoc cref="OpCodes.Ldelem_I2"/>
	public Body Ldelem_I2() => Emit(OpCodes.Ldelem_I2);

	/// <inheritdoc cref="OpCodes.Ldelem_I4"/>
	public Body Ldelem_I4() => Emit(OpCodes.Ldelem_I4);

	/// <inheritdoc cref="OpCodes.Ldelem_I8"/>
	public Body Ldelem_I8() => Emit(OpCodes.Ldelem_I8);

	/// <inheritdoc cref="OpCodes.Ldelem_R4"/>
	public Body Ldelem_R4() => Emit(OpCodes.Ldelem_R4);

	/// <inheritdoc cref="OpCodes.Ldelem_R8"/>
	public Body Ldelem_R8() => Emit(OpCodes.Ldelem_R8);

	/// <inheritdoc cref="OpCodes.Ldelem_Ref"/>
	public Body Ldelem_Ref() => Emit(OpCodes.Ldelem_Ref);

	/// <inheritdoc cref="OpCodes.Ldelem_U1"/>
	public Body Ldelem_U1() => Emit(OpCodes.Ldelem_U1);

	/// <inheritdoc cref="OpCodes.Ldelem_U2"/>
	public Body Ldelem_U2() => Emit(OpCodes.Ldelem_U2);

	/// <inheritdoc cref="OpCodes.Ldelem_U4"/>
	public Body Ldelem_U4() => Emit(OpCodes.Ldelem_U4);

	/// <inheritdoc cref="OpCodes.Ldelema"/>
	public Body Ldelema(Type type) => Emit(OpCodes.Ldelema, type);

	/// <inheritdoc cref="OpCodes.Ldelema"/>
	public Body Ldelema<T>() => Ldelema(typeof(T));

	/// <inheritdoc cref="OpCodes.Ldfld"/>
	public Body Ldfld(FieldInfo field) => Emit(OpCodes.Ldfld, field);

	/// <inheritdoc cref="OpCodes.Ldflda"/>
	public Body Ldflda(FieldInfo field) => Emit(OpCodes.Ldflda, field);

	/// <inheritdoc cref="OpCodes.Ldftn"/>
	public Body Ldftn(MethodInfo method) => Emit(OpCodes.Ldftn, method);

	/// <inheritdoc cref="OpCodes.Ldind_I"/>
	public Body Ldind_I() => Emit(OpCodes.Ldind_I);

	/// <inheritdoc cref="OpCodes.Ldind_I1"/>
	public Body Ldind_I1() => Emit(OpCodes.Ldind_I1);

	/// <inheritdoc cref="OpCodes.Ldind_I2"/>
	public Body Ldind_I2() => Emit(OpCodes.Ldind_I2);

	/// <inheritdoc cref="OpCodes.Ldind_I4"/>
	public Body Ldind_I4() => Emit(OpCodes.Ldind_I4);

	/// <inheritdoc cref="OpCodes.Ldind_I8"/>
	public Body Ldind_I8() => Emit(OpCodes.Ldind_I8);

	/// <inheritdoc cref="OpCodes.Ldind_R4"/>
	public Body Ldind_R4() => Emit(OpCodes.Ldind_R4);

	/// <inheritdoc cref="OpCodes.Ldind_R8"/>
	public Body Ldind_R8() => Emit(OpCodes.Ldind_R8);

	/// <inheritdoc cref="OpCodes.Ldind_Ref"/>
	public Body Ldind_Ref() => Emit(OpCodes.Ldind_Ref);

	/// <inheritdoc cref="OpCodes.Ldind_U1"/>
	public Body Ldind_U1() => Emit(OpCodes.Ldind_U1);

	/// <inheritdoc cref="OpCodes.Ldind_U2"/>
	public Body Ldind_U2() => Emit(OpCodes.Ldind_U2);

	/// <inheritdoc cref="OpCodes.Ldind_U4"/>
	public Body Ldind_U4() => Emit(OpCodes.Ldind_U4);

	/// <inheritdoc cref="OpCodes.Ldlen"/>
	public Body Ldlen() => Emit(OpCodes.Ldlen);

	/// <inheritdoc cref="OpCodes.Ldloc"/>
	public Body Ldloc(Int16 index) => Emit(OpCodes.Ldloc, index);

	/// <inheritdoc cref="OpCodes.Ldloc"/>
	public Body Ldloc(LocalBuilder local) => Emit(OpCodes.Ldloc, local);

	/// <inheritdoc cref="OpCodes.Ldloc_0"/>
	public Body Ldloc_0() => Emit(OpCodes.Ldloc_0);

	/// <inheritdoc cref="OpCodes.Ldloc_1"/>
	public Body Ldloc_1() => Emit(OpCodes.Ldloc_1);

	/// <inheritdoc cref="OpCodes.Ldloc_2"/>
	public Body Ldloc_2() => Emit(OpCodes.Ldloc_2);

	/// <inheritdoc cref="OpCodes.Ldloc_3"/>
	public Body Ldloc_3() => Emit(OpCodes.Ldloc_3);

	/// <inheritdoc cref="OpCodes.Ldloc_S"/>
	public Body Ldloc_S(Byte index) => Emit(OpCodes.Ldloc_S, index);

	/// <inheritdoc cref="OpCodes.Ldloc_S"/>
	public Body Ldloc_S(LocalBuilder local) => Emit(OpCodes.Ldloc_S, local);

	/// <inheritdoc cref="OpCodes.Ldloca"/>
	public Body Ldloca(Int16 index) => Emit(OpCodes.Ldloca, index);

	/// <inheritdoc cref="OpCodes.Ldloca"/>
	public Body Ldloca(LocalBuilder local) => Emit(OpCodes.Ldloca, local);

	/// <inheritdoc cref="OpCodes.Ldloca_S"/>
	public Body Ldloca_S(Byte index) => Emit(OpCodes.Ldloca_S, index);

	/// <inheritdoc cref="OpCodes.Ldloca_S"/>
	public Body Ldloca_S(LocalBuilder local) => Emit(OpCodes.Ldloca_S, local);

	/// <inheritdoc cref="OpCodes.Ldnull"/>
	public Body Ldnull() => Emit(OpCodes.Ldnull);

	/// <inheritdoc cref="OpCodes.Ldobj"/>
	public Body Ldobj(Type type) => Emit(OpCodes.Ldobj, type);

	/// <inheritdoc cref="OpCodes.Ldobj"/>
	public Body Ldobj<T>() => Ldobj(typeof(T));

	/// <inheritdoc cref="OpCodes.Ldsfld"/>
	public Body Ldsfld(FieldInfo field) => Emit(OpCodes.Ldsfld, field);

	/// <inheritdoc cref="OpCodes.Ldsflda"/>
	public Body Ldsflda(FieldInfo field) => Emit(OpCodes.Ldsflda, field);

	/// <inheritdoc cref="OpCodes.Ldstr"/>
	public Body Ldstr(String value) => Emit(OpCodes.Ldstr, value);

	/// <inheritdoc cref="OpCodes.Ldtoken"/>
	public Body Ldtoken(MethodInfo methodInfo) => Emit(OpCodes.Ldtoken, methodInfo);

	/// <inheritdoc cref="OpCodes.Ldtoken"/>
	public Body Ldtoken(FieldInfo fieldInfo) => Emit(OpCodes.Ldtoken, fieldInfo);

	/// <inheritdoc cref="OpCodes.Ldtoken"/>
	public Body Ldtoken(Type type) => Emit(OpCodes.Ldtoken, type);

	/// <inheritdoc cref="OpCodes.Ldtoken"/>
	public Body Ldtoken<T>() => Ldtoken(typeof(T));

	/// <inheritdoc cref="OpCodes.Ldvirtftn"/>
	public Body Ldvirtftn(MethodInfo method) => Emit(OpCodes.Ldvirtftn, method);

	/// <inheritdoc cref="OpCodes.Leave"/>
	public Body Leave(Label label) => Emit(OpCodes.Leave, label);

	/// <inheritdoc cref="OpCodes.Leave_S"/>
	public Body Leave_S(Label label) => Emit(OpCodes.Leave_S, label);

	/// <inheritdoc cref="OpCodes.Localloc"/>
	public Body Localloc() => Emit(OpCodes.Localloc);

	/// <inheritdoc cref="OpCodes.Mkrefany"/>
	public Body Mkrefany(Type type) => Emit(OpCodes.Mkrefany, type);

	/// <inheritdoc cref="OpCodes.Mkrefany"/>
	public Body Mkrefany<T>() => Mkrefany(typeof(T));

	/// <inheritdoc cref="OpCodes.Mul"/>
	public Body Mul() => Emit(OpCodes.Mul);

	/// <inheritdoc cref="OpCodes.Mul_Ovf"/>
	public Body Mul_Ovf() => Emit(OpCodes.Mul_Ovf);

	/// <inheritdoc cref="OpCodes.Mul_Ovf_Un"/>
	public Body Mul_Ovf_Un() => Emit(OpCodes.Mul_Ovf_Un);

	/// <inheritdoc cref="OpCodes.Neg"/>
	public Body Neg() => Emit(OpCodes.Neg);

	/// <inheritdoc cref="OpCodes.Newarr"/>
	public Body Newarr(Type type) => Emit(OpCodes.Newarr, type);

	/// <inheritdoc cref="OpCodes.Newarr"/>
	public Body Newarr<T>() => Newarr(typeof(T));

	/// <inheritdoc cref="OpCodes.Newobj"/>
	public Body Newobj(ConstructorInfo constructor) => Emit(OpCodes.Newobj, constructor);

	/// <inheritdoc cref="OpCodes.Newobj"/>
	public Body Newobj<T>() where T : new() => Newobj(typeof(T).GetConstructor(Type.EmptyTypes)!);

	/// <inheritdoc cref="OpCodes.Nop"/>
	public Body Nop() => Emit(OpCodes.Nop);

	/// <inheritdoc cref="OpCodes.Not"/>
	public Body Not() => Emit(OpCodes.Not);

	/// <inheritdoc cref="OpCodes.Or"/>
	public Body Or() => Emit(OpCodes.Or);

	/// <inheritdoc cref="OpCodes.Pop"/>
	public Body Pop() => Emit(OpCodes.Pop);

	/// <inheritdoc cref="OpCodes.Readonly"/>
	public Body Readonly() => Emit(OpCodes.Readonly);

	/// <inheritdoc cref="OpCodes.Refanytype"/>
	public Body Refanytype() => Emit(OpCodes.Refanytype);

	/// <inheritdoc cref="OpCodes.Refanyval"/>
	public Body Refanyval(Type type) => Emit(OpCodes.Refanyval, type);

	/// <inheritdoc cref="OpCodes.Refanyval"/>
	public Body Refanyval<T>() => Refanyval(typeof(T));

	/// <inheritdoc cref="OpCodes.Rem"/>
	public Body Rem() => Emit(OpCodes.Rem);

	/// <inheritdoc cref="OpCodes.Rem_Un"/>
	public Body Rem_Un() => Emit(OpCodes.Rem_Un);

	/// <inheritdoc cref="OpCodes.Ret"/>
	public Body Ret() => Emit(OpCodes.Ret);

	/// <inheritdoc cref="OpCodes.Rethrow"/>
	public Body Rethrow() => Emit(OpCodes.Rethrow);

	/// <inheritdoc cref="OpCodes.Shl"/>
	public Body Shl() => Emit(OpCodes.Shl);

	/// <inheritdoc cref="OpCodes.Shr"/>
	public Body Shr() => Emit(OpCodes.Shr);

	/// <inheritdoc cref="OpCodes.Shr_Un"/>
	public Body Shr_Un() => Emit(OpCodes.Shr_Un);

	/// <inheritdoc cref="OpCodes.Sizeof"/>
	public Body Sizeof(Type type) => Emit(OpCodes.Sizeof, type);

	/// <inheritdoc cref="OpCodes.Sizeof"/>
	public Body Sizeof<T>() => Sizeof(typeof(T));

	/// <inheritdoc cref="OpCodes.Starg"/>
	public Body Starg(Int16 index) => Emit(OpCodes.Starg, index);

	/// <inheritdoc cref="OpCodes.Starg_S"/>
	public Body Starg_S(Byte index) => Emit(OpCodes.Starg_S, index);

	/// <inheritdoc cref="OpCodes.Stelem"/>
	public Body Stelem(Type type) => Emit(OpCodes.Stelem, type);

	/// <inheritdoc cref="OpCodes.Stelem"/>
	public Body Stelem<T>() => Stelem(typeof(T));

	/// <inheritdoc cref="OpCodes.Stelem_I"/>
	public Body Stelem_I() => Emit(OpCodes.Stelem_I);

	/// <inheritdoc cref="OpCodes.Stelem_I1"/>
	public Body Stelem_I1() => Emit(OpCodes.Stelem_I1);

	/// <inheritdoc cref="OpCodes.Stelem_I2"/>
	public Body Stelem_I2() => Emit(OpCodes.Stelem_I2);

	/// <inheritdoc cref="OpCodes.Stelem_I4"/>
	public Body Stelem_I4() => Emit(OpCodes.Stelem_I4);

	/// <inheritdoc cref="OpCodes.Stelem_I8"/>
	public Body Stelem_I8() => Emit(OpCodes.Stelem_I8);

	/// <inheritdoc cref="OpCodes.Stelem_R4"/>
	public Body Stelem_R4() => Emit(OpCodes.Stelem_R4);

	/// <inheritdoc cref="OpCodes.Stelem_R8"/>
	public Body Stelem_R8() => Emit(OpCodes.Stelem_R8);

	/// <inheritdoc cref="OpCodes.Stelem_Ref"/>
	public Body Stelem_Ref() => Emit(OpCodes.Stelem_Ref);

	/// <inheritdoc cref="OpCodes.Stfld"/>
	public Body Stfld(FieldInfo field) => Emit(OpCodes.Stfld, field);

	/// <inheritdoc cref="OpCodes.Stind_I"/>
	public Body Stind_I() => Emit(OpCodes.Stind_I);

	/// <inheritdoc cref="OpCodes.Stind_I1"/>
	public Body Stind_I1() => Emit(OpCodes.Stind_I1);

	/// <inheritdoc cref="OpCodes.Stind_I2"/>
	public Body Stind_I2() => Emit(OpCodes.Stind_I2);

	/// <inheritdoc cref="OpCodes.Stind_I4"/>
	public Body Stind_I4() => Emit(OpCodes.Stind_I4);

	/// <inheritdoc cref="OpCodes.Stind_I8"/>
	public Body Stind_I8() => Emit(OpCodes.Stind_I8);

	/// <inheritdoc cref="OpCodes.Stind_R4"/>
	public Body Stind_R4() => Emit(OpCodes.Stind_R4);

	/// <inheritdoc cref="OpCodes.Stind_R8"/>
	public Body Stind_R8() => Emit(OpCodes.Stind_R8);

	/// <inheritdoc cref="OpCodes.Stind_Ref"/>
	public Body Stind_Ref() => Emit(OpCodes.Stind_Ref);

	/// <inheritdoc cref="OpCodes.Stloc"/>
	public Body Stloc(Int16 index) => Emit(OpCodes.Stloc, index);

	/// <inheritdoc cref="OpCodes.Stloc"/>
	public Body Stloc(LocalBuilder local) => Emit(OpCodes.Stloc, local);

	/// <inheritdoc cref="OpCodes.Stloc_S"/>
	public Body Stloc_S(Byte index) => Emit(OpCodes.Stloc_S, index);

	/// <inheritdoc cref="OpCodes.Stloc_S"/>
	public Body Stloc_S(LocalBuilder local) => Emit(OpCodes.Stloc_S, local);

	/// <inheritdoc cref="OpCodes.Stobj"/>
	public Body Stobj(Type type) => Emit(OpCodes.Stobj, type);

	/// <inheritdoc cref="OpCodes.Stobj"/>
	public Body Stobj<T>() => Stobj(typeof(T));

	/// <inheritdoc cref="OpCodes.Stsfld"/>
	public Body Stsfld(FieldInfo field) => Emit(OpCodes.Stsfld, field);

	/// <inheritdoc cref="OpCodes.Sub"/>
	public Body Sub() => Emit(OpCodes.Sub);

	/// <inheritdoc cref="OpCodes.Sub_Ovf"/>
	public Body Sub_Ovf() => Emit(OpCodes.Sub_Ovf);

	/// <inheritdoc cref="OpCodes.Sub_Ovf_Un"/>
	public Body Sub_Ovf_Un() => Emit(OpCodes.Sub_Ovf_Un);

	/// <inheritdoc cref="OpCodes.Switch"/>
	public Body Switch(Label[] labels) => Emit(OpCodes.Switch, labels);

	/// <inheritdoc cref="OpCodes.Tailcall"/>
	public Body Tailcall() => Emit(OpCodes.Tailcall);

	/// <inheritdoc cref="OpCodes.Throw"/>
	public Body Throw() => Emit(OpCodes.Throw);

	/// <inheritdoc cref="OpCodes.Unaligned"/>
	public Body Unaligned(Byte alignment) => Emit(OpCodes.Unaligned, alignment);

	/// <inheritdoc cref="OpCodes.Unbox"/>
	public Body Unbox(Type type) => Emit(OpCodes.Unbox, type);

	/// <inheritdoc cref="OpCodes.Unbox"/>
	public Body Unbox<T>() => Unbox(typeof(T));

	/// <inheritdoc cref="OpCodes.Unbox_Any"/>
	public Body Unbox_Any(Type type) => Emit(OpCodes.Unbox_Any, type);

	/// <inheritdoc cref="OpCodes.Unbox_Any"/>
	public Body Unbox_Any<T>() => Unbox_Any(typeof(T));

	/// <inheritdoc cref="OpCodes.Volatile"/>
	public Body Volatile() => Emit(OpCodes.Volatile);

	/// <inheritdoc cref="OpCodes.Xor"/>
	public Body Xor() => Emit(OpCodes.Xor);

	/// <inheritdoc cref="ILGenerator.DeclareLocal(System.Type)"/>
	public Body DeclareLocal(Type type, out LocalBuilder local) { local = il.DeclareLocal(type); return this; }
	
	/// <inheritdoc cref="ILGenerator.DeclareLocal(System.Type)"/>
	public Body DeclareLocal<T>(out LocalBuilder local) => DeclareLocal(typeof(T), out local);
	
	/// <inheritdoc cref="ILGenerator.DefineLabel"/>
	public Body DefineLabel(out Label label) { label = il.DefineLabel(); return this; }
	
	/// <inheritdoc cref="ILGenerator.MarkLabel(Label)"/>
	public Body MarkLabel(Label label) { il.MarkLabel(label); return this; }
	
	/// <inheritdoc cref="ILGenerator.BeginExceptionBlock"/>
	public Body BeginExceptionBlock() { il.BeginExceptionBlock(); return this; }
	
	/// <inheritdoc cref="ILGenerator.BeginExceptFilterBlock"/>
	public Body BeginExceptFilterBlock() { il.BeginExceptFilterBlock(); return this; }
	
	/// <inheritdoc cref="ILGenerator.BeginCatchBlock"/>
	public Body BeginCatchBlock() { il.BeginCatchBlock(null); return this; }
	
	/// <inheritdoc cref="ILGenerator.BeginCatchBlock(System.Type)"/>
	public Body BeginCatchBlock(Type exceptionType) { il.BeginCatchBlock(exceptionType); return this; }
	
	/// <inheritdoc cref="ILGenerator.BeginCatchBlock(System.Type)"/>
	public Body BeginCatchBlock<TException>() where TException : Exception => BeginCatchBlock(typeof(TException));
	
	/// <inheritdoc cref="ILGenerator.EndExceptionBlock"/>
	public Body EndExceptionBlock() { il.EndExceptionBlock(); return this; }
	
}

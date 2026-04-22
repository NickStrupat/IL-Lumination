using System;
using SreLocalBuilder = System.Reflection.Emit.LocalBuilder;

namespace Illumination.Builders;

public sealed class TryBuilder<TBody> where TBody : BodyBase<TBody>
{
	private readonly TBody body;

	internal TryBuilder(TBody body) => this.body = body;

	public TryBuilder<TBody> Catch(Action<TBody> catchBody)
		=> Catch(typeof(Object), catchBody);

	public TryBuilder<TBody> Catch<TException>(Action<TBody> catchBody) where TException : Exception
		=> Catch(typeof(TException), catchBody);

	public TryBuilder<TBody> Catch(Type exceptionType, Action<TBody> catchBody)
	{
		body.BeginCatchBlock(exceptionType).Apply(catchBody);
		return this;
	}

	/// <param name="when">
	/// Filter expression. Receives the body and a local containing the exception.
	/// Must leave an int32 on the stack (1 = handle, 0 = skip).
	/// </param>
	public TryBuilder<TBody> CatchWhen(Action<TBody, SreLocalBuilder> when, Action<TBody> catchBody)
	{
		body.DeclareLocal<Object>(out var exLocal)
			.BeginExceptFilterBlock()
			.Stloc(exLocal)
			.Apply(b => when(b, exLocal))
			.BeginCatchBlock()
			.Apply(catchBody);
		return this;
	}

	/// <param name="when">
	/// Filter expression. Receives the body and a local containing the typed exception.
	/// Must leave an int32 on the stack (1 = handle, 0 = skip).
	/// </param>
	public TryBuilder<TBody> CatchWhen<TException>(Action<TBody, SreLocalBuilder> when, Action<TBody> catchBody) where TException : Exception
	{
		body.DeclareLocal<TException>(out var exLocal)
			.BeginExceptFilterBlock()
			.Isinst<TException>()
			.Dup()
			.Stloc(exLocal)
			.DefineLabel(out var match)
			.DefineLabel(out var endFilter)
			.Brtrue(match)
			.Ldc_I4_0()
			.Br(endFilter)
			.MarkLabel(match)
			.Apply(b => when(b, exLocal))
			.MarkLabel(endFilter)
			.BeginCatchBlock()
			.Apply(catchBody);
		return this;
	}

	public void Finally(Action<TBody> finallyBody)
	{
		body.BeginFinallyBlock()
			.Apply(finallyBody);
	}
}

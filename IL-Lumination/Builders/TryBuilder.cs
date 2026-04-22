using System;
using SreLocalBuilder = System.Reflection.Emit.LocalBuilder;

namespace Illumination.Builders;

public sealed class TryBuilder<TBody> where TBody : BodyBase<TBody>
{
	private readonly TBody body;

	internal TryBuilder(TBody body) => this.body = body;

	public TryBuilder<TBody> Catch<TException>(Action<TBody> catchBody) where TException : Exception
		=> Catch(typeof(TException), catchBody);

	public TryBuilder<TBody> Catch(Type exceptionType, Action<TBody> catchBody)
	{
		body.BeginCatchBlock(exceptionType);
		catchBody(body);
		return this;
	}

	/// <param name="when">
	/// Filter expression. Receives the body and a local containing the typed exception.
	/// Must leave an int32 on the stack (1 = handle, 0 = skip).
	/// </param>
	public TryBuilder<TBody> CatchWhen<TException>(Action<TBody, SreLocalBuilder> when, Action<TBody> catchBody) where TException : Exception
	{
		body.DeclareLocal<TException>(out var exLocal);
		body.BeginExceptFilterBlock();
		body.Isinst<TException>();
		body.Dup();
		body.Stloc(exLocal);
		body.DefineLabel(out var match);
		body.DefineLabel(out var endFilter);
		body.Brtrue(match);
		body.Ldc_I4_0();
		body.Br(endFilter);
		body.MarkLabel(match);
		when(body, exLocal);
		body.MarkLabel(endFilter);
		body.BeginCatchBlock();
		catchBody(body);
		return this;
	}

	public void Finally(Action<TBody> finallyBody)
	{
		body.BeginFinallyBlock();
		finallyBody(body);
	}
}

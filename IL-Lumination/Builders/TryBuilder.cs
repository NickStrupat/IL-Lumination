using System;

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

	public void Finally(Action<TBody> finallyBody)
	{
		body.BeginFinallyBlock();
		finallyBody(body);
	}
}

using System;

namespace Illumination;

public readonly struct Raii<T> : IDisposable
{
	public T Value { get; }
	private readonly Action<T> deinit;

	internal Raii(Func<T> init, Action<T> deinit) => (Value, this.deinit) = (init(), deinit);

	void IDisposable.Dispose() => deinit(Value);
}

public static class Raii
{
	public static Raii<T> Create<T>(Func<T> init, Action<T> deinit) => new(init, deinit);
}
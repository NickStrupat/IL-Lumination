using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Illumination.Builders;

public interface IBuilder<T> where T : class;

public sealed class BuilderMap
{
	private readonly Dictionary<Object, Object> map = new();

	public void Add<T>(IBuilder<T> builder, T sreBuilder) where T : class
	{
		try
		{
			map.Add(builder, sreBuilder);
		}
		catch (ArgumentException)
		{
			throw new ArgumentException("This builder has already been added.");
		}
	}
	
	public void Remove<T>(IBuilder<T> builder) where T : class
	{
		if (!map.Remove(builder))
			throw new KeyNotFoundException("Builder not found.");
	}

	public Boolean TryRemove<T>(IBuilder<T> builder) where T : class
	{
		return map.Remove(builder);
	}

	public T Get<T>(IBuilder<T> builder) where T : class
	{
		return TryGetBuilder(builder, out var value) ? value : throw new KeyNotFoundException("Builder not found.");
	}
	
	public Boolean TryGetBuilder<T>(IBuilder<T> builder, [MaybeNullWhen(false)] out T value) where T : class
	{
		if (map.TryGetValue(builder, out var x))
		{
			value = (T)x;
			return true;
		}
		value = null!;
		return false;
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Illumination.Builders;

public interface IBuilder<T> where T : class;

internal sealed class BuilderMap
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
		return GetOrNull(builder) ?? throw new KeyNotFoundException("Builder not found.");
	}
	
	public T? GetOrNull<T>(IBuilder<T> builder) where T : class
	{
		return TryGetBuilder(builder, out var value) ? value : null;
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

internal sealed class DefinerMap
{
	private readonly Dictionary<Object, Definer> map = new();
	
	public void Add<T>(IBuilder<T> builder, Definer definer) where T : class
	{
		try
		{
			map.Add(builder, definer);
		}
		catch (ArgumentException)
		{
			throw new ArgumentException("This builder has already been added.");
		}
	}

	public Definer Get<T>(IBuilder<T> builder) where T : class
	{
		return GetOrNull(builder) ?? throw new KeyNotFoundException("Definer not found.");
	}
	
	public Definer? GetOrNull<T>(IBuilder<T> builder) where T : class
	{
		return TryGetBuilder(builder, out var value) ? value : null;
	}
	
	public Boolean TryGetBuilder<T>(IBuilder<T> builder, [MaybeNullWhen(false)] out Definer value) where T : class
	{
		if (map.TryGetValue(builder, out var x))
		{
			value = x;
			return true;
		}
		value = null!;
		return false;
	}
}
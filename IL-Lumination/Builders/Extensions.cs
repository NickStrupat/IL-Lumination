using System;
using System.Collections.Generic;

namespace Illumination.Builders;

internal static class Extensions
{
	public static T AddAction<T, TItem>(this T @this, List<TItem> list, TItem item, Action<TItem> action)
	{
		list.Add(item);
		action(item);
		return @this;
	}

	public static T AddAction<T, TItem>(this T @this, IContravariantListWrapper<TItem> list, TItem item, Action<TItem> action)
	{
		list.Add(item);
		action(item);
		return @this;
	}
	
	public static IContravariantListWrapper<T> AsContravariant<T>(this List<T> list) => new ContravariantListWrapper<T>(list);
}

public interface IContravariantListWrapper<in T>
{
	void Add(T item);
}

public sealed class ContravariantListWrapper<T>(List<T> list) : IContravariantListWrapper<T>
{
	void IContravariantListWrapper<T>.Add(T item) => list.Add(item);
}

public readonly record struct Maybe<T>(T Value)
{
	public readonly Boolean HasValue = true;
	public override String? ToString() => HasValue ? Value?.ToString() : null;
}
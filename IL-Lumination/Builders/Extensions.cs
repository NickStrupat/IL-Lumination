using System;
using System.Collections.Generic;

namespace Illumination.Builders;

internal static class Extensions
{
	public static T AddAction<T, TItem>(this T @this, List<TItem> list, TItem item, Action<TItem> action)
	{
		action(item);
		list.Add(item);
		return @this;
	}

	public static T AddAction<T, TItem>(this T @this, IContravariantList<TItem> list, TItem item, Action<TItem> action)
	{
		action(item);
		list.Add(item);
		return @this;
	}
	
	public static IContravariantList<T> AsContravariant<T>(this List<T> list)
	{
		return new ContravariantListWrapper<T>(list);
	}

	public static T AddAction<T, TItem>(this T @this, IContravariantSet<TItem> set, TItem item, Action<TItem> action)
	{
		action(item);
		if (!set.Add(item))
			throw new ArgumentException("An equal element (according to the equality comparer) already exists in the set.");
		return @this;
	}
	
	public static IContravariantSet<T> AsContravariant<T>(this ISet<T> set)
	{
		return new ContravariantSetWrapper<T>(set);
	}

	public static T AddAction<T, K, V>(this T @this, IContravariantDictionary<K, V> dictionary, K key, V value, Action<V> action)
	{
		action(value);
		dictionary.Add(key, value);
		return @this;
	}
	
	public static IContravariantDictionary<K, V> AsContravariant<K, V>(this IDictionary<K, V> dictionary)
	{
		return new ContravariantDictionaryWrapper<K, V>(dictionary);
	}
}

public interface IContravariantList<in T>
{
	void Add(T item);
}

public sealed class ContravariantListWrapper<T>(List<T> list) : IContravariantList<T>
{
	private readonly List<T> list = list;
	void IContravariantList<T>.Add(T item) => list.Add(item);
}

public interface IContravariantSet<in T>
{
	bool Add(T item);
}

public sealed class ContravariantSetWrapper<T>(ISet<T> set) : IContravariantSet<T>
{
	private readonly ISet<T> set = set;
	public Boolean Add(T item) => set.Add(item);
}

public interface IContravariantDictionary<in TKey, in TValue>
{
	void Add(TKey key, TValue value);
}

public sealed class ContravariantDictionaryWrapper<K, V>(IDictionary<K, V> dictionary) : IContravariantDictionary<K, V>
{
	private readonly IDictionary<K, V> dictionary = dictionary;
	public void Add(K key, V value) => dictionary.Add(key, value);
}

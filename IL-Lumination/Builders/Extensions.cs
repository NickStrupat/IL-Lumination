using System;
using System.Collections.Generic;

namespace Illumination;

internal static class Extensions
{
	public static T AddTo<T, TList, TItem>(this T @this, TList list, TItem item) where TList : List<TItem>
	{
		list.Add(item);
		return @this;
	}
	
	// public static T AddTo<T, TList, TItem>(this T @this, TList list, Func<Int32, TItem> itemFunc) where TList : List<TItem>
	// {
	// 	var item = itemFunc(list.Count);
	// 	list.Add(item);
	// 	return @this;
	// }

	public static T AddAction<T, TList, TItem>(this T @this, TList list, TItem item, Action<TItem> action) where TList : List<TItem>
	{
		@this.AddTo(list, item);
		action(item);
		return @this;
	}
}
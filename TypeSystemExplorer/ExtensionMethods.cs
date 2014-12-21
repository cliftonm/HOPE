/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Clifton.Tools.Data
{
	public static class ExtensionMethods
	{
		// Type is...

		public static bool Is<T>(this object obj, Action<T> action)
		{
			bool ret = obj is T;

			if (ret)
			{
				action((T)obj);
			}

			return ret;
		}

		// ---------- if-then-else as lambda expressions --------------

		/// <summary>
		/// Returns true if the object is null.
		/// </summary>
		public static bool IfNull<T>(this T obj)
		{
			return obj == null;
		}

		/// <summary>
		/// If the object is null, performs the action and returns true.
		/// </summary>
		public static bool IfNull<T>(this T obj, Action action)
		{
			bool ret = obj == null;

			if (ret) { action(); }

			return ret;
		}

		/// <summary>
		/// Returns true if the object is not null.
		/// </summary>
		public static bool IfNotNull<T>(this T obj)
		{
			return obj != null;
		}

		public static R IfNotNullReturn<T, R>(this T obj, Func<T, R> func)
		{
			if (obj != null)
			{
				return func(obj);
			}
			else
			{
				return default(R);
			}
		}

		public static R ElseIfNullReturn<T, R>(this T obj, Func<R> func)
		{
			if (obj == null)
			{
				return func();
			}
			else
			{
				return default(R);
			}
		}

		/// <summary>
		/// If the object is not null, performs the action and returns true.
		/// </summary>
		public static bool IfNotNull<T>(this T obj, Action<T> action)
		{
			bool ret = obj != null;

			if (ret) { action(obj); }

			return ret;
		}

		/// <summary>
		/// If the boolean is true, performs the specified action.
		/// </summary>
		public static bool Then(this bool b, Action f)
		{
			if (b) { f(); }

			return b;
		}

		/// <summary>
		/// If the boolean is false, performs the specified action and returns the complement of the original state.
		/// </summary>
		public static void Else(this bool b, Action f)
		{
			if (!b) { f(); }
		}

		// ---------- Dictionary --------------

		/// <summary>
		/// Return the key for the dictionary value or throws an exception if more than one value matches.
		/// </summary>
		public static TKey KeyFromValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue val)
		{
			// from: http://stackoverflow.com/questions/390900/cant-operator-be-applied-to-generic-types-in-c
			// "Instead of calling Equals, it's better to use an IComparer<T> - and if you have no more information, EqualityComparer<T>.Default is a good choice: Aside from anything else, this avoids boxing/casting."
			return dict.Single(t => EqualityComparer<TValue>.Default.Equals(t.Value, val)).Key;
		}

		// ---------- DBNull value --------------

		// Note the "where" constraint, only value types can be used as Nullable<T> types.
		// Otherwise, we get a bizzare error that doesn't really make it clear that T needs to be restricted as a value type.
		public static object AsDBNull<T>(this Nullable<T> item) where T : struct
		{
			// If the item is null, return DBNull.Value, otherwise return the item.
			return item as object ?? DBNull.Value;
		}

		// ---------- ForEach iterators --------------

		/// <summary>
		/// Implements a ForEach for generic enumerators.
		/// </summary>
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (var item in collection)
			{
				action(item);
			}
		}

		/// <summary>
		/// Implements ForEach for non-generic enumerators.
		/// </summary>
		// Usage: Controls.ForEach<Control>(t=>t.DoSomething());
		public static void ForEach<T>(this IEnumerable collection, Action<T> action)
		{
			foreach (T item in collection)
			{
				action(item);
			}
		}

		public static void ForEach(this DataView dv, Action<DataRowView> action)
		{
			foreach (DataRowView drv in dv)
			{
				action(drv);
			}
		}

		// ---------- collection management --------------

		// From the comments of the blog entry http://blog.jordanterrell.com/post/LINQ-Distinct()-does-not-work-as-expected.aspx regarding why Distinct doesn't work right.
		public static IEnumerable<T> RemoveDuplicates<T>(this IEnumerable<T> source)
		{
			return RemoveDuplicates(source, (t1, t2) => t1.Equals(t2));
		}

		public static IEnumerable<T> RemoveDuplicates<T>(this IEnumerable<T> source, Func<T, T, bool> equater)
		{
			// copy the source array 
			List<T> result = new List<T>();

			foreach (T item in source)
			{
				if (result.All(t => !equater(item, t)))
				{
					// Doesn't exist already: Add it 
					result.Add(item);
				}
			}

			return result;
		}

		public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T newItem, Func<T, T, bool> equater)
		{
			List<T> result = new List<T>();

			foreach (T item in source)
			{
				if (!equater(item, newItem))
				{
					result.Add(item);
				}
			}

			result.Add(newItem);

			return result;
		}

		public static void AddIfUnique<T>(this IList<T> list, T item)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}

		public static void RemoveLast<T>(this IList<T> list)
		{
			list.RemoveAt(list.Count - 1);
		}

		// ---------- List to DataTable --------------

		// From http://stackoverflow.com/questions/564366/generic-list-to-datatable
		// which also suggests, for better performance, HyperDescriptor: http://www.codeproject.com/Articles/18450/HyperDescriptor-Accelerated-dynamic-property-acces
		public static DataTable AsDataTable<T>(this IList<T> data)
		{
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
			DataTable table = new DataTable();

			for (int i = 0; i < props.Count; i++)
			{
				PropertyDescriptor prop = props[i];
				table.Columns.Add(prop.Name, prop.PropertyType);
			}

			object[] values = new object[props.Count];

			foreach (T item in data)
			{
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = props[i].GetValue(item);
				}
				table.Rows.Add(values);
			}

			return table;
		}
	}
}

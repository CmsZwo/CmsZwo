using System;
using System.Linq;
using System.Collections.Generic;

namespace CmsZwo
{
	public static class IEnumerableExtensions
	{
		#region Calc

		public static float AverageSafe<T>(this IEnumerable<T> instance, Func<T, int> func)
		{
			if (!instance.HasContent()) return 0f;

			var sum = instance.Sum(func);
			if (sum == 0) return 0f;

			var result = (float)sum / instance.Count();
			return result;
		}

		#endregion

		#region Null

		public static bool HasContent<T>(this IEnumerable<T> instance)
			=> instance != null && instance.Count() > 0;

		public static int CountSafe<T>(this IEnumerable<T> instance)
		{
			if (!instance.HasContent()) return 0;
			var result = instance.Count();
			return result;
		}

		#endregion

		#region Join

		public static string JoinSafe(this IEnumerable<string> instance, string separator = ", ", bool removeNoContents = true)
		{
			if (!instance.HasContent()) return null;
			if (removeNoContents) instance = instance.WhereHasContent();
			return string.Join(separator, instance);
		}

		public static string JoinSafe(this IEnumerable<int> instance, string separator = ", ")
		{
			if (!instance.HasContent()) return null;
			return string.Join(separator, instance);
		}

		#endregion

		#region Selecting

		public static bool ContainsIgnoreCase(this IEnumerable<string> instance, string content)
		{
			if (!instance.HasContent())
				return false;

			return
				instance.Any(x => x.EqualsIgnoreCase(content));
		}

		public static string FirstWithContent(this IEnumerable<string> instance)
		{
			if (!instance.HasContent())
				return null;

			return
				instance.FirstOrDefault(x => x.HasContent());
		}

		public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> instance, int count)
			=>
			instance
				.Skip(Math.Max(0, instance.Count() - count))
				.Take(count);

		public static T PickRandom<T>(this IEnumerable<T> instance)
		{
			if (!instance.HasContent()) return default(T);
			var random = new Random();
			var index = random.Next(0, instance.Count());
			var result = instance.ElementAt(index);
			return result;
		}

		public static int IndexOf<T>(this IEnumerable<T> instance, Func<T, bool> predicate)
		{
			int result = 0;
			var found = false;

			foreach (var item in instance)
			{
				if (predicate(item))
				{
					found = true;
					break;
				};
				result++;
			}

			return found ? result : -1;
		}

		public static List<T> Copy<T>(this IEnumerable<T> instance, Func<T, bool> predicate = null)
		{
			List<T> result = null;

			if (predicate != null) result = new List<T>(instance.Where(predicate));
			else result = new List<T>(instance);

			return result;
		}

		public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
		{
			if (source == null) yield break;

			var seenKeys = new HashSet<TKey>();
			foreach (T element in source)
			{
				if (seenKeys.Add(keySelector(element)))
					yield return element;
			}
		}

		public static HashSet<string> ToHashSet(this IEnumerable<string> instance)
		{
			if (instance == null) return null;
			return new HashSet<string>(instance);
		}

		#endregion

		#region Filtering

		public static IEnumerable<string> WhereHasContent(this IEnumerable<string> instance)
		{
			if (instance == null) return instance;
			var result = instance.Where(x => x.HasContent());
			return result;
		}

		#endregion

		#region Looping

		public static IEnumerable<T> Safe<T>(this IEnumerable<T> instance)
			=> instance ?? Enumerable.Empty<T>();

		#endregion

		#region Equals

		public static bool SequenceEqualSafe<T>(this IEnumerable<T> instance, IEnumerable<T> compareTo)
		{
			if (instance == null && compareTo == null) return true;

			if (instance == null && compareTo != null) return false;
			if (instance != null && compareTo == null) return false;

			var result = instance.SequenceEqual(compareTo);

			return result;
		}

		#endregion
	}
}

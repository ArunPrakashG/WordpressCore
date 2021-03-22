using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordpressCore.Models.Responses;

namespace WordpressCore {
	/// <summary>
	/// Contains extensions to ease the use of this library.
	/// </summary>
	public static class LibraryExtensions {
		/// <summary>
		/// Iterate through each value in the response if the response result is a success.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="container"></param>
		/// <param name="onEachElement"></param>
		/// <returns></returns>
		public static bool ForEachIfSuccess<T>(this Response<IEnumerable<T>> container, Action<T> onEachElement) {
			if(container == null || !container.Status || container.Value == null || onEachElement == null) {
				return false;
			}

			foreach(T val in container.Value) {
				onEachElement.Invoke(val);
			}

			return true;
		}

		/// <summary>
		/// Returns the underlying <see cref="IEnumerable{T}"/> if the result is success.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="container"></param>
		/// <returns></returns>
		public static IEnumerable<T> ForEachIfSuccess<T>(this Response<IEnumerable<T>> container) {
			if (container == null || !container.Status || container.Value == null) {
				yield break;
			}

			foreach (T val in container.Value) {
				yield return val;
			}
		}

		/// <summary>
		/// Appends the target dictionary to the source dictionary.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="Y"></typeparam>
		/// <param name="sourceDic"></param>
		/// <param name="targetDic"></param>
		/// <returns>Combined result of append result of each index of the target dictionary. if all values appened successfully, then result is True, else False</returns>
		public static bool Append<T,Y>(this IDictionary<T,Y> sourceDic, IDictionary<T,Y> targetDic) {
			if(sourceDic == null || targetDic == null || targetDic.Count <= 0) {
				return false;
			}
			
			bool[] results = new bool[targetDic.Count];
			int counter = 0;
			foreach(KeyValuePair<T, Y> pair in targetDic) {
				results[counter++] = sourceDic.TryAdd(pair.Key, pair.Value);
			}

			return results.All(x => x);
		}
	}
}

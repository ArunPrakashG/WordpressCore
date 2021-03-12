using System;
using System.Collections.Generic;
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
	}
}

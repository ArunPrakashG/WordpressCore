using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressCore.Models.Requests {
	/// <summary>
	/// Base class for all QueryBuilder models.
	/// <para>(Used for internal purposes only.)</para>
	/// </summary>
	/// <typeparam name="TBuilerType"></typeparam>
	public abstract class QueryBuilder<TBuilerType> where TBuilerType: new() {

		/// <summary>
		/// Static override to new <see cref="TBuilerType"/>() call.
		/// </summary>
		/// <returns></returns>
		public static TBuilerType WithBuilder() => new TBuilerType();
	}
}

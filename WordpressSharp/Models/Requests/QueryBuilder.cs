using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressSharp.Models.Requests {
	public abstract class QueryBuilder<TBuilerType> where TBuilerType: new() {

		public static TBuilerType WithBuilder() => new TBuilerType();
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressCore.Models.Requests {
	public abstract class QueryBuilder<TBuilerType> where TBuilerType: new() {

		public static TBuilerType WithBuilder() => new TBuilerType();
	}
}

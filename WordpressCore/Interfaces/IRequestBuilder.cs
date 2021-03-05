using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace WordpressCore.Interfaces {
	/// <summary>
	/// Base interface which is implemented by all QueryBuilders.
	/// <para>(Used for internal purposes)</para>
	/// </summary>
	/// <typeparam name="YRequestType">The Request Type</typeparam>
	/// <typeparam name="TReturnType">The Return Type, normally, <see cref="HttpContent"/></typeparam>
	public interface IRequestBuilder<YRequestType, TReturnType> {
		/// <summary>
		/// Initialize the Builder with default values.
		/// </summary>
		/// <returns></returns>
		YRequestType InitializeWithDefaultValues();

		/// <summary>
		/// Creates <see cref="HttpContent"/> which is to be send with the request.
		/// </summary>
		/// <returns></returns>
		TReturnType Create();
	}
}

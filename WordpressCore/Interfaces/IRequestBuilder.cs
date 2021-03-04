using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressCore.Interfaces {
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

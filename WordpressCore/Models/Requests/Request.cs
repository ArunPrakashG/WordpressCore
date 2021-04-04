using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace WordpressCore.Models.Requests {
	/// <summary>
	/// Contains the actual request which is created using <see cref="RequestBuilder"/>.
	/// </summary>
	public class Request {
		/// <summary>
		/// The resultant Uri which is build using the <see cref="RequestBuilder"/>
		/// </summary>
		public readonly Uri RequestUri;
		internal readonly string Endpoint;
		internal readonly Callback Callback;
		internal readonly CancellationToken Token;
		internal readonly WordpressAuthorization Authorization;
		internal readonly Func<string, bool> ValidationDelegate;
		internal readonly HttpMethod RequestMethod;
		internal readonly int PerPageCount;
		internal readonly IDictionary<string, string> Headers;
		internal readonly HttpContent FormBody;

		/// <summary>
		/// Gets if there are any headers specified with the request.
		/// </summary>
		public bool HasHeaders => Headers != null && Headers.Count > 0;

		/// <summary>
		/// Gets if the request has form body content.
		/// </summary>
		public bool HasFormContent => FormBody != null;

		/// <summary>
		/// Gets if the request should be authorized using the specified authorization method.
		/// </summary>
		public bool ShouldAuthorize => Authorization != null && !Authorization.IsDefault;

		/// <summary>
		/// Gets if the request should be validated using the caller defined <see cref="ValidationDelegate"/>.
		/// </summary>
		public bool ShouldValidateResponse => ValidationDelegate != null;

		/// <summary>
		/// Gets if the request has a valid Exception callback configured.
		/// </summary>
		public bool HasValidExceptionCallback => Callback != null && Callback.UnhandledExceptionCallback != null;

		/// <summary>
		/// Gets if the request has a valid callbacks configured.
		/// </summary>
		public bool HasValidCallbacks => Callback != null && Callback.RequestCallback != null && Callback.ResponseCallback != null && Callback.UnhandledExceptionCallback != null;

		/// <summary>
		/// Gets if the request is executable.
		/// </summary>
		public bool IsRequestExecutable => RequestUri != null;

		internal Request(Uri requestUri, Func<string, bool> validationDelegate, string endpoint, CancellationToken token, WordpressAuthorization auth, HttpMethod method, IDictionary<string, string> headers, HttpContent formBody, int perPageCount = 10, Callback callback = null) {
			RequestUri = requestUri;
			Callback = callback;
			Endpoint = endpoint;
			Token = token;
			RequestMethod = method;
			Headers = headers;
			FormBody = formBody;
			Authorization = auth;
			PerPageCount = perPageCount;
			ValidationDelegate = validationDelegate;
		}
	}
}

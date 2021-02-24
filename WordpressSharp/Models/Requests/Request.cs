using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace WordpressSharp.Models.Requests {
	public class Request {
		public readonly Uri RequestUri;
		internal readonly string Endpoint;
		internal readonly Callback Callback;
		internal readonly CancellationToken Token;
		internal readonly Authorization Authorization;
		internal readonly Func<string, bool> ValidationDelegate;
		internal readonly HttpMethod RequestMethod;
		internal readonly Dictionary<string, string> Headers;
		internal readonly Dictionary<string, string> FormBody;

		public bool HasHeaders => Headers != null && Headers.Count > 0;

		public bool HasFormContent => FormBody != null && FormBody.Count > 0;

		public bool ShouldAuthorize => !Authorization.IsDefault;

		public bool ShouldValidateResponse => ValidationDelegate != null;

		public bool HasValidExceptionCallback => Callback != null && Callback.UnhandledExceptionCallback != null;

		public bool HasValidCallbacks => Callback != null && Callback.RequestCallback != null && Callback.ResponseCallback != null && Callback.UnhandledExceptionCallback != null;

		public bool IsRequestExecutable => RequestUri != null;

		public Request(Uri requestUri, Func<string, bool> validationDelegate, string endpoint, CancellationToken token, Authorization auth, HttpMethod method, Dictionary<string,string> headers, Dictionary<string,string> formBody, Callback callback = null) {
			RequestUri = requestUri;
			Callback = callback;
			Endpoint = endpoint;
			Token = token;
			RequestMethod = method;
			Headers = headers;
			FormBody = formBody;
			Authorization = auth;
			ValidationDelegate = validationDelegate;
		}
	}
}

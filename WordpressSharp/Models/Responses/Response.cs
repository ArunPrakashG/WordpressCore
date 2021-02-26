using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WordpressSharp.Models.Responses {
	/// <summary>
	/// A container for all responses returned from the api.
	/// <para>Wraps responses and provides status and error messages to the caller.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Response<T> {
		/// <summary>
		/// The response Value.
		/// </summary>
		public T Value { get; private set; }

		/// <summary>
		/// The headers of the response.
		/// </summary>
		public Dictionary<string, string> Headers { get; private set; }

		/// <summary>
		/// The status of the request.
		/// <para>True when success, false when failed.</para>
		/// </summary>
		public bool Status { get; private set; }

		/// <summary>
		/// The <see cref="HttpStatusCode"/> of the response.
		/// </summary>
		public HttpStatusCode StatusCode { get; private set; }

		/// <summary>
		/// The time taken to receive the response from the API. as <see cref="TimeSpan"/> instance.
		/// </summary>
		public TimeSpan Duration { get; private set; }

		/// <summary>
		/// Stores any exception which occured during the request or while parsing the response.
		/// </summary>
		public Exception RequestException { get; private set; }

		/// <summary>
		/// The error message, if any.
		/// </summary>
		public string Message { get; private set; }

		internal Response(T value) => Value = value;

		internal Response() { }

		internal Response<T> SetValue(T value) {
			Value = value;
			return this;
		}

		internal Response<T> SetException(Exception e) {
			RequestException = e;
			return this;
		}

		internal Response<T> SetMessage(params string[] message) {
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < message.Length; i++) {
				string val = message[i];
				builder.AppendLine(val);
			}

			Message = builder.ToString();
			return this;
		}

		internal Response<T> SetHeaders(Dictionary<string, string> headers) {
			Headers = headers;
			return this;
		}

		internal Response<T> SetStatus(bool value) {
			Status = value;
			return this;
		}

		internal Response<T> SetStatusCode(HttpStatusCode code) {
			StatusCode = code;
			return this;
		}

		internal Response<T> SetDuration(TimeSpan duration) {
			Duration = duration;
			return this;
		}
	}
}

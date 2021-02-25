using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WordpressSharp.Models.Responses {
	internal static class Response {
		internal static Response<T> CloneFrom<T>(T responseValue, Response<T[]> result) {
			if (responseValue == null) {
				return default;
			}

			Response<T> resultContainer = new Response<T>(responseValue);
			resultContainer.SetHeaders(result.Headers);
			resultContainer.SetDuration(result.Duration);
			resultContainer.SetStatus(result.Status);
			resultContainer.SetStatusCode(result.StatusCode);
			resultContainer.SetMessage(result.Message);
			return resultContainer;
		}
	}

	public class ResponseWrapper {
		public Dictionary<string, string> Headers;
		public bool Status;
		public HttpStatusCode StatusCode;
		public TimeSpan Duration;
		public Exception RequestException;
		public string Message;
	}

	public class Response<T> {
		public T Value { get; private set; }

		public Dictionary<string, string> Headers { get; private set; }

		public bool Status { get; private set; }

		public HttpStatusCode StatusCode { get; private set; }

		public TimeSpan Duration { get; private set; }

		public Exception RequestException { get; private set; }

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

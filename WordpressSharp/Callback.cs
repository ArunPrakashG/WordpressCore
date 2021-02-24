using System;

namespace WordpressSharp
{
	public struct RequestStatus {
		public readonly bool Status;
		public readonly string ErrorMessage;

		public RequestStatus(bool status, string errorMessage) {
			Status = status;
			ErrorMessage = errorMessage;
		}
	}

	public class Callback {
		public readonly Action<Exception> UnhandledExceptionCallback;
		public readonly Action<string> ResponseCallback;
		public readonly Action<RequestStatus> RequestCallback;

		public Callback(Action<Exception> exceptionCallback, Action<string> responseCallback, Action<RequestStatus> requestCallback) {
			UnhandledExceptionCallback = exceptionCallback;
			ResponseCallback = responseCallback;
			RequestCallback = requestCallback;
		}
	}
}

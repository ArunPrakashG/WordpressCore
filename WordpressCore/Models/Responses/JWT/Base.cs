using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressCore.Models.Responses.JWT {
	public class Base {
		[JsonProperty("success")]
		public bool IsSuccess { get; set; }

		[JsonProperty("statusCode")]
		public int StatusCode { get; set; }

		[JsonProperty("code")]
		public string Code { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }
	}
}

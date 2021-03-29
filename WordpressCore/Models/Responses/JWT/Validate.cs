using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressCore.Models.Responses.JWT {
	public class Validate : Base {
		[JsonProperty("code")]
		public string JwtStatusCode { get; set; }

		[JsonProperty("data")]
		public Data DataContainer { get; set; }

		public class Data {
			[JsonProperty("status")]
			public int ResultStatusCode { get; set; }
		}

	}
}

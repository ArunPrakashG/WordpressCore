using Newtonsoft.Json;
using PathanamthittaMedia.Library.Models.Responses.JWT;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathanamthittaMedia.Library.Models.Responses.JWT {
	public class Token : Base {
		[JsonProperty("data")]
		public TokenContainer Container { get; set; }

		public class TokenContainer {
			[JsonProperty("token")]
			public string Token { get; set; }

			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("email")]
			public string Email { get; set; }

			[JsonProperty("nicename")]
			public string NiceName { get; set; }

			[JsonProperty("firstName")]
			public string FirstName { get; set; }

			[JsonProperty("lastName")]
			public string LastName { get; set; }

			[JsonProperty("displayName")]
			public string DisplayName { get; set; }
		}
	}
}

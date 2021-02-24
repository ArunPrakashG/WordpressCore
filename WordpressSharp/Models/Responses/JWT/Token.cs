using Newtonsoft.Json;

namespace WordpressSharp.Models.Responses.JWT {
	internal class Token : Base {
		[JsonProperty("data")]
		internal TokenContainer Container { get; set; }

		internal class TokenContainer {
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

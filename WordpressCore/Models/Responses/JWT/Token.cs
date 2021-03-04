using Newtonsoft.Json;

namespace WordpressCore.Models.Responses.JWT {
	internal class Token : Base {
		[JsonProperty("data")]
		internal TokenContainer Container { get; set; }

		internal class TokenContainer {
			[JsonProperty("token")]
			internal string Token { get; set; }

			[JsonProperty("id")]
			internal int Id { get; set; }

			[JsonProperty("email")]
			internal string Email { get; set; }

			[JsonProperty("nicename")]
			internal string NiceName { get; set; }

			[JsonProperty("firstName")]
			internal string FirstName { get; set; }

			[JsonProperty("lastName")]
			internal string LastName { get; set; }

			[JsonProperty("displayName")]
			internal string DisplayName { get; set; }
		}
	}
}

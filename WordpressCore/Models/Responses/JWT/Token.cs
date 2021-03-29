using Newtonsoft.Json;

namespace WordpressCore.Models.Responses.JWT {
	internal class Token : Base {
		[JsonProperty("token")]
		public string TokenValue { get; set; }

		[JsonProperty("user_email")]
		public string UserEmail { get; set; }

		[JsonProperty("user_nicename")]
		public string UserNiceName { get; set; }

		[JsonProperty("user_display_name")]
		public string UserDisplayName { get; set; }
	}
}

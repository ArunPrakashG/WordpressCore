using Newtonsoft.Json;

namespace WordpressSharp.Models.Responses {
	public class User : BaseResponse {
		[JsonProperty("id")]
		public int Identifier { get; set; }

		[JsonProperty("name")]
		public string UserName { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("link")]
		public string ProfileLink { get; set; }

		[JsonProperty("slug")]
		public string Slug { get; set; }

		[JsonProperty("avatar_urls")]
		public AvatarUrls AvatarContainer { get; set; }
	}
}

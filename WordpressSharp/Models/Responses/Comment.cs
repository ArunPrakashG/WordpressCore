using Newtonsoft.Json;
using System;

namespace WordpressSharp.Models.Responses {
	public class Comment : BaseResponse {
		[JsonProperty("id")]
		public int Identifier { get; set; }

		[JsonProperty("post")]
		public int PostIdentifier { get; set; }

		[JsonProperty("parent")]
		public int ParentIdentifier { get; set; }

		[JsonProperty("author")]
		public int AuthorIdentifier { get; set; }

		[JsonProperty("author_name")]
		public string AuthorName { get; set; }

		[JsonProperty("author_url")]
		public string AuthorUrl { get; set; }

		[JsonProperty("date")]
		public DateTime Date { get; set; }

		[JsonProperty("date_gmt")]
		public DateTime DateGmt { get; set; }

		[JsonProperty("content")]
		public ContentContainer Content { get; set; }

		[JsonProperty("link")]
		public string Link { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("author_avatar_urls")]
		public AvatarUrls AuthorAvatarUrl { get; set; }
	}
}

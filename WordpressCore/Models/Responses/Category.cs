using Newtonsoft.Json;

namespace WordpressCore.Models.Responses {
	public class Category : BaseResponse {
		[JsonProperty("id")]
		public int Identifier { get; set; }

		[JsonProperty("count")]
		public int PublishedPostsCount { get; set; }

		[JsonProperty("descriptions")]
		public string Description { get; set; }

		[JsonProperty("link")]
		public string AbsoluteUrl { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("slug")]
		public string Slug { get; set; }

		[JsonProperty("taxonomy")]
		public string Taxonomy { get; set; }

		[JsonProperty("parent")]
		public int ParentCategoryId { get; set; }
	}
}

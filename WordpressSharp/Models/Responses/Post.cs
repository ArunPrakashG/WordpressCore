using Newtonsoft.Json;
using System;

namespace WordpressSharp.Models.Responses {
	public class Post : BaseResponse {
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("date")]
		public DateTime Date { get; set; }

		[JsonProperty("date_gmt")]
		public DateTime DateGmt { get; set; }

		[JsonProperty("guid")]
		public GuidContainer Guid { get; set; }

		[JsonProperty("modified")]
		public DateTime Modified { get; set; }

		[JsonProperty("modified_gmt")]
		public DateTime ModifiedGmt { get; set; }

		[JsonProperty("slug")]
		public string Slug { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("link")]
		public string Link { get; set; }

		[JsonProperty("title")]
		public TitleContainer Title { get; set; }

		[JsonProperty("content")]
		public ContentContainer Content { get; set; }

		[JsonProperty("excerpt")]
		public ExcerptContainer Excerpt { get; set; }

		[JsonProperty("author")]
		public int Author { get; set; }

		[JsonProperty("featured_media")]
		public int FeaturedMedia { get; set; }

		[JsonProperty("comment_status")]
		public string CommentStatus { get; set; }

		[JsonProperty("ping_status")]
		public string PingStatus { get; set; }

		[JsonProperty("sticky")]
		public bool Sticky { get; set; }

		[JsonProperty("template")]
		public string Template { get; set; }

		[JsonProperty("format")]
		public string Format { get; set; }

		[JsonProperty("categories")]
		public int[] Categories { get; set; }

		[JsonProperty("tags")]
		public int[] Tags { get; set; }

		[JsonProperty("author_meta")]
		public AuthorMeta AuthorMetaData { get; set; }

		[JsonProperty("_embedded")]
		public Embed Embed { get; set; }

		public class GuidContainer {
			[JsonProperty("rendered")]
			public string Rendered { get; set; }
		}

		public class AuthorMeta {
			[JsonProperty("ID")]
			public string AuthorId { get; set; }

			[JsonProperty("user_nicename")]
			public string UserNiceName { get; set; }

			[JsonProperty("user_registered")]
			public DateTime UserRegisteredOn { get; set; }

			[JsonProperty("display_name")]
			public string NickName { get; set; }
		}

		public class TitleContainer {
			[JsonProperty("rendered")]
			public string Rendered { get; set; }

			[JsonIgnore]
			public string Parsed => !string.IsNullOrEmpty(Rendered) ? Rendered.CleanContent() : Rendered;
		}

		public class ContentContainer {
			[JsonProperty("rendered")]
			public string Rendered { get; set; }

			[JsonIgnore]
			public string Parsed => !string.IsNullOrEmpty(Rendered) ? Rendered.CleanContent() : Rendered;

			[JsonProperty("_protected")]
			public bool Protected { get; set; }
		}

		public class ExcerptContainer {
			[JsonProperty("rendered")]
			public string Rendered { get; set; }

			[JsonProperty("_protected")]
			public bool Protected { get; set; }
		}
	}
}

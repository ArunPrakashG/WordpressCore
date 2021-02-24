using Newtonsoft.Json;

namespace PathanamthittaMedia.Library.Models.Responses {
	public abstract class BaseResponse {
		[JsonProperty("featured_image_url")]
		public string FeaturedImageUrl { get; set; }

		[JsonProperty("_links")]
		public LinkContainer Links { get; set; }

		[JsonProperty("meta")]
		public object[] Meta { get; set; }
	}

	public class ContentContainer {
		[JsonProperty("rendered")]
		public string Rendered { get; set; }

		[JsonIgnore]
		public string Parsed => !string.IsNullOrEmpty(Rendered) ? Rendered.CleanContent() : Rendered;

		[JsonProperty("_protected")]
		public bool Protected { get; set; }
	}

	public class InReplyTo {
		[JsonProperty("embeddable")]
		public bool IsEmbeddable { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class AvatarUrls {
		[JsonProperty("_24")]
		public string Size24 { get; set; }

		[JsonProperty("_48")]
		public string Size48 { get; set; }

		[JsonProperty("_96")]
		public string Size96 { get; set; }
	}

	public class LinkContainer {
		[JsonProperty("self")]
		public Self[] SelfLinks { get; set; }

		[JsonProperty("collection")]
		public Collection[] Collection { get; set; }

		[JsonProperty("about")]
		public About[] About { get; set; }

		[JsonProperty("wppost_type")]
		public WordpressPostType[] PostType { get; set; }

		[JsonProperty("curies")]
		public Cury[] Curies { get; set; }

		[JsonProperty("ParentLink")]
		public Parent[] ParentLink { get; set; }

		[JsonProperty("author")]
		public Author[] Author { get; set; }

		[JsonProperty("replies")]
		public Reply[] Replies { get; set; }

		[JsonProperty("versionhistory")]
		public VersionHistory[] VersionHistory { get; set; }

		[JsonProperty("predecessorversion")]
		public PredecessorVersion[] PredecessorVersion { get; set; }

		[JsonProperty("wpfeaturedmedia")]
		public FeaturedMedia[] FeaturedMedia { get; set; }

		[JsonProperty("wpattachment")]
		public Attachment[] Attachments { get; set; }

		[JsonProperty("wpterm")]
		public WpTerm[] Term { get; set; }
	}

	public class Author {
		[JsonProperty("embeddable")]
		public bool Embeddable { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class Reply {
		[JsonProperty("embeddable")]
		public bool Embeddable { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class VersionHistory {
		[JsonProperty("count")]
		public int Count { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class PredecessorVersion {
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class FeaturedMedia {
		[JsonProperty("embeddable")]
		public bool Embeddable { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class Attachment {
		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class WpTerm {
		[JsonProperty("taxonomy")]
		public string Taxonomy { get; set; }

		[JsonProperty("embeddable")]
		public bool Embeddable { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class Self {
		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class Collection {
		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class About {
		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class WordpressPostType {
		[JsonProperty("href")]
		public string Url { get; set; }
	}

	public class Cury {
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }

		[JsonProperty("templated")]
		public bool IsTemplated { get; set; }
	}

	public class Parent {
		[JsonProperty("embeddable")]
		public bool IsEmbeddable { get; set; }

		[JsonProperty("href")]
		public string Url { get; set; }
	}
}

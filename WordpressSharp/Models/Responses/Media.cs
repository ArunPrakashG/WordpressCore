using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathanamthittaMedia.Library.Models.Responses {
	public class Media : BaseResponse {
		[JsonProperty("id")]
		public int Identifier { get; set; }

		[JsonProperty("date")]
		public DateTime UploadedDate { get; set; }

		[JsonProperty("date_gmt")]
		public DateTime DateGmt { get; set; }

		[JsonProperty("guid")]
		public ContentContainer Guid { get; set; }

		[JsonProperty("modified")]
		public DateTime ModifiedOn { get; set; }

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
		public ContentContainer Title { get; set; }

		[JsonProperty("author")]
		public int AuthorId { get; set; }

		[JsonProperty("comment_status")]
		public string CommentStatus { get; set; }

		[JsonProperty("ping_status")]
		public string PingStatus { get; set; }

		[JsonProperty("template")]
		public string Template { get; set; }

		[JsonProperty("description")]
		public ContentContainer Description { get; set; }

		[JsonProperty("caption")]
		public ContentContainer Caption { get; set; }

		[JsonProperty("alt_text")]
		public string ImageAltText { get; set; }

		[JsonProperty("media_type")]
		public string MediaType { get; set; }

		[JsonProperty("mime_type")]
		public string MimeType { get; set; }

		[JsonProperty("media_details")]
		public MediaDetailsContainer MediaDetails { get; set; }

		[JsonProperty("post")]
		public int PostId { get; set; }

		[JsonProperty("source_url")]
		public string SourceUrl { get; set; }

		public class MediaDetailsContainer {
			[JsonProperty("width")]
			public int Width { get; set; }

			[JsonProperty("height")]
			public int Height { get; set; }

			[JsonProperty("file")]
			public string FileName { get; set; }

			[JsonProperty("sizes")]
			public Sizes Sizes { get; set; }

			[JsonProperty("image_meta")]
			public ImageMetaContainer ImageMeta { get; set; }
		}

		public class Sizes {
			[JsonProperty("medium")]
			public ImageSize Medium { get; set; }

			[JsonProperty("thumbnail")]
			public ImageSize Thumbnail { get; set; }

			[JsonProperty("td_218x150")]
			public ImageSize td_218x150 { get; set; }

			[JsonProperty("td_324x400")]
			public ImageSize td_324x400 { get; set; }

			[JsonProperty("td_485x360")]
			public ImageSize td_485x360 { get; set; }

			[JsonProperty("td_696x0")]
			public ImageSize td_696x0 { get; set; }

			[JsonProperty("td_80x60")]
			public ImageSize td_80x60 { get; set; }

			[JsonProperty("td_100x70")]
			public ImageSize td_100x70 { get; set; }

			[JsonProperty("td_265x198")]
			public ImageSize td_265x198 { get; set; }

			[JsonProperty("td_324x160")]
			public ImageSize td_324x160 { get; set; }

			[JsonProperty("td_324x235")]
			public ImageSize td_324x235 { get; set; }

			[JsonProperty("td_356x220")]
			public ImageSize td_356x220 { get; set; }

			[JsonProperty("td_356x364")]
			public ImageSize td_356x364 { get; set; }

			[JsonProperty("td_533x261")]
			public ImageSize td_533x261 { get; set; }

			[JsonProperty("td_534x462")]
			public ImageSize td_534x462 { get; set; }

			[JsonProperty("td_696x385")]
			public ImageSize td_696x385 { get; set; }

			[JsonProperty("td_741x486")]
			public ImageSize td_741x486 { get; set; }

			[JsonProperty("full")]
			public ImageSize Full { get; set; }
		}

		public class ImageSize {
			[JsonProperty("file")]
			public string FileName { get; set; }

			[JsonProperty("width")]
			public int Width { get; set; }

			[JsonProperty("height")]
			public int Height { get; set; }

			[JsonProperty("mime_type")]
			public string MimeType { get; set; }

			[JsonProperty("source_url")]
			public string SourceUrl { get; set; }
		}

		public class ImageMetaContainer {
			[JsonProperty("aperture")]
			public string Aperture { get; set; }
			[JsonProperty("credit")]
			public string Credit { get; set; }
			[JsonProperty("camera")]
			public string Camera { get; set; }
			[JsonProperty("caption")]
			public string Caption { get; set; }
			[JsonProperty("created_timestamp")]
			public string CreatedTimestamp { get; set; }
			[JsonProperty("copyright")]
			public string Copyright { get; set; }
			[JsonProperty("focal_length")]
			public string FocalLength { get; set; }
			[JsonProperty("iso")]
			public string Iso { get; set; }
			[JsonProperty("shutter_speed")]
			public string ShutterSpeed { get; set; }
			[JsonProperty("title")]
			public string Title { get; set; }
			[JsonProperty("orientation")]
			public string Orientation { get; set; }
		}
	}
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathanamthittaMedia.Library.Models.Responses {
	public class Tag : BaseResponse {
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("count")]
		public int Count { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("link")]
		public string Link { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("slug")]
		public string Slug { get; set; }

		[JsonProperty("taxonomy")]
		public string Taxonomy { get; set; }
	}
}

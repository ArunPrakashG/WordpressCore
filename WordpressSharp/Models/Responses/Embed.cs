using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressSharp.Models.Responses {
	public class Embed {
		[JsonProperty("author")]
		public User[] Author { get; set; }

		[JsonProperty("wpfeaturedmedia")]
		public Media[] FeaturedMedia { get; set; }

		[JsonProperty("wpterm")]
		public Category[] Category { get; set; }
	}
}

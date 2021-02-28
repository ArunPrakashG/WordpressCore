using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressSharp.Models.Requests {
	public class Enums {
		public enum PostStatus {
			Publish,
			Future,
			Draft,
			Pending,
			Private
		}

		public enum CommandStatusValue {
			Open,
			Closed
		}

		public enum PingStatusValue {
			Open,
			Closed
		}

		public enum PostFormat {
			Standard,
			Aside,
			Chat,
			Gallery,
			Link,
			Image,
			Quote,
			Status,
			Video,
			Audio
		}

		public enum Scope {
			View,
			Embed,
			Edit
		}

		public enum TaxonomyRelation {
			And,
			Or
		}

		public enum OrderBy {
			Date,
			Author,
			Id,
			Include,
			Modified,
			Parent,
			Relevance,
			Slug,
			IncludeSlugs,
			Title,
			Email,
			Url,
			Name
		}

		public enum Status {
			Published,
			Draft,
			Trash
		}

		public enum Order {
			Ascending,
			Descending
		}
	}
}

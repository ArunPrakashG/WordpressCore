using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WordpressCore.Interfaces;

namespace WordpressCore.Models.Requests {
	public class PopularPostsBuilder : QueryBuilder<PopularPostsBuilder>, IRequestBuilder<PopularPostsBuilder, HttpContent> {
		private string PostType;
		private int Limit;
		private bool Freshness;
		private int Offset;
		private OrderPopularPostsBy OrderBy;
		private TimeRange Range;
		private TimeUnit Unit;
		private int TimeQuanity;
		private int[] ExcludedPosts;
		private string TaxonomyFilter;
		private int[] TaxonomyIds;
		private int[] AllowedAuthors;

		public HttpContent Create() {
			var formContent = new Dictionary<string, string>();
			formContent.Add("post_type", PostType);
			formContent.Add("limit", Limit.ToString());
			formContent.Add("freshness", Freshness ? "1" : "0");
			formContent.Add("offset", Offset.ToString());
			formContent.Add("order_by", OrderBy.ToString().ToLower());
			formContent.Add("range", Range.ToString().ToLower());
			formContent.Add("time_unit", Unit.ToString().ToLower());
			formContent.Add("time_quantity", TimeQuanity.ToString());

			if(ExcludedPosts.Length > 0) {
				formContent.Add("pid", string.Join(',', ExcludedPosts));
			}

			if (!string.IsNullOrEmpty(TaxonomyFilter)) {
				formContent.Add("taxonomy", TaxonomyFilter);

				if(TaxonomyIds.Length > 0) {
					formContent.Add("term_id", string.Join(',', TaxonomyIds));
				}				
			}

			if(AllowedAuthors.Length > 0) {
				formContent.Add("author", string.Join(',', AllowedAuthors));
			}

			return new FormUrlEncodedContent(formContent);
		}

		public PopularPostsBuilder InitializeWithDefaultValues() {
			PostType = "post";
			Limit = 10;
			Freshness = false;
			Offset = 0;
			OrderBy = OrderPopularPostsBy.Views;
			Range = TimeRange.Last24Hours;
			Unit = TimeUnit.Hour;
			TimeQuanity = 24;
			TaxonomyFilter = "category";
			return this;
		}

		public PopularPostsBuilder WithPostType(string postType) {
			PostType = postType;
			return this;
		}

		public PopularPostsBuilder WithLimit(int limit) {
			Limit = limit;
			return this;
		}

		public PopularPostsBuilder WithFreshness(bool freshness) {
			Freshness = freshness;
			return this;
		}

		public PopularPostsBuilder WithOffset(int offset) {
			Offset = offset;
			return this;
		}

		public PopularPostsBuilder WithPopularPostsOrder(OrderPopularPostsBy order) {
			OrderBy = order;
			return this;
		}

		public PopularPostsBuilder WithRange(TimeRange range) {
			Range = range;
			return this;
		}

		public PopularPostsBuilder WithUnit(TimeUnit unit) {
			Unit = unit;
			return this;
		}

		public PopularPostsBuilder WithTimeQuantity(int quantity) {
			TimeQuanity = quantity;
			return this;
		}

		public PopularPostsBuilder WithExcludedPosts(params int[] excludedPosts) {
			ExcludedPosts = excludedPosts;
			return this;
		}

		public PopularPostsBuilder WithTaxonomyFilter(string taxonomyType) {
			TaxonomyFilter = taxonomyType;
			return this;
		}

		public PopularPostsBuilder WithTaxonomyIds(params int[] taxonomyIds) {
			TaxonomyIds = taxonomyIds;
			return this;
		}

		public PopularPostsBuilder WithAllowedAuthors(params int[] authorIds) {
			AllowedAuthors = authorIds;
			return this;
		}

		public enum OrderPopularPostsBy {
			Views,
			Comments
		}

		public enum TimeRange {
			Last24Hours,
			Last7Days,
			Last30Days,
			All,
			Custom
		}

		public enum TimeUnit {
			Hour,
			Minute,
			Day,
			Week,
			Month
		}

		
	}
}

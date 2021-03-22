using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WordpressCore.Interfaces;

namespace WordpressCore.Models.Requests {
	/// <summary>
	/// Builder used to build GetPopularPosts request
	/// </summary>
	public class PopularPostsBuilder : QueryBuilder<PopularPostsBuilder>, IRequestBuilder<PopularPostsBuilder, HttpContent> {
		private string[] PostType;
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

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public HttpContent Create() {
			Dictionary<string, string> formContent = new Dictionary<string, string> {				
				{ "limit", Limit.ToString() },
				{ "freshness", Freshness ? "1" : "0" },
				{ "offset", Offset.ToString() },
				{ "order_by", OrderBy.ToString().ToLower() },
				{ "range", Range.ToString().ToLower() },
				{ "time_unit", Unit.ToString().ToLower() },
				{ "time_quantity", TimeQuanity.ToString() }
			};

			if(PostType != null && PostType.Length > 0) {
				formContent.Add("post_type", string.Join(',', PostType));
			}

			if (ExcludedPosts != null && ExcludedPosts.Length > 0) {
				formContent.Add("pid", string.Join(',', ExcludedPosts));
			}

			if (!string.IsNullOrEmpty(TaxonomyFilter)) {
				formContent.Add("taxonomy", TaxonomyFilter);

				if(TaxonomyIds != null && TaxonomyIds.Length > 0) {
					formContent.Add("term_id", string.Join(',', TaxonomyIds));
				}				
			}

			if(AllowedAuthors != null && AllowedAuthors.Length > 0) {
				formContent.Add("author", string.Join(',', AllowedAuthors));
			}

			return new FormUrlEncodedContent(formContent);
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public PopularPostsBuilder InitializeWithDefaultValues() {
			PostType = new string[] { "post" };
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

		/// <summary>
		/// Specifys the post types to be included in the response.
		/// </summary>
		/// <param name="postType"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithPostType(params string[] postType) {
			PostType = postType;
			return this;
		}

		/// <summary>
		/// The maximum number of popular posts to return.
		/// </summary>
		/// <param name="limit"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithLimit(int limit) {
			Limit = limit;
			return this;
		}

		/// <summary>
		/// Retrieve the most popular entries published within the specified time range.
		/// </summary>
		/// <param name="freshness"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithFreshness(bool freshness) {
			Freshness = freshness;
			return this;
		}

		/// <summary>
		/// An offset point for the collection.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithOffset(int offset) {
			Offset = offset;
			return this;
		}

		/// <summary>
		/// Set the sorting option of the popular posts.
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithPopularPostsOrder(OrderPopularPostsBy order) {
			OrderBy = order;
			return this;
		}

		/// <summary>
		/// Return posts from a specified time range.
		/// <para>When using the value "custom" in conjunction with the parameters time_unit and time_value (see below) you can retrieve popular posts from a custom defined time range (eg. last 12 hours).</para>
		/// </summary>
		/// <param name="range"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithRange(TimeRange range) {
			Range = range;
			return this;
		}

		/// <summary>
		/// Specifies the time unit of the custom time range.
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithUnit(TimeUnit unit) {
			Unit = unit;
			return this;
		}

		/// <summary>
		/// Specifies the number of time units of the custom time range.
		/// </summary>
		/// <param name="quantity"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithTimeQuantity(int quantity) {
			TimeQuanity = quantity;
			return this;
		}

		/// <summary>
		/// Post IDs to exclude from the listing (comma separated).
		/// </summary>
		/// <param name="excludedPosts"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithExcludedPosts(params int[] excludedPosts) {
			ExcludedPosts = excludedPosts;
			return this;
		}

		/// <summary>
		/// Include posts in a specified taxonomy.
		/// </summary>
		/// <param name="taxonomyType"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithTaxonomyFilter(string taxonomyType) {
			TaxonomyFilter = taxonomyType;
			return this;
		}

		/// <summary>
		/// Taxonomy IDs, separated by comma (prefix a minus sign to exclude).
		/// </summary>
		/// <param name="taxonomyIds"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithTaxonomyIds(params int[] taxonomyIds) {
			TaxonomyIds = taxonomyIds;
			return this;
		}

		/// <summary>
		/// Include popular posts from author ID(s).
		/// </summary>
		/// <param name="authorIds"></param>
		/// <returns></returns>
		public PopularPostsBuilder WithAllowedAuthors(params int[] authorIds) {
			AllowedAuthors = authorIds;
			return this;
		}

		/// <summary>
		/// Popular posts order
		/// </summary>
		public enum OrderPopularPostsBy {
			/// <summary>
			/// Order by views received.
			/// </summary>
			Views,

			/// <summary>
			/// Order by comments received.
			/// </summary>
			Comments
		}

		/// <summary>
		/// Time range for the posts
		/// </summary>
		public enum TimeRange {
			/// <summary>
			/// Gets posts from last 24 hours
			/// </summary>
			Last24Hours,

			/// <summary>
			/// Get posts from last 7 days
			/// </summary>
			Last7Days,

			/// <summary>
			/// Get posts from last 30 days
			/// </summary>
			Last30Days,

			/// <summary>
			/// Gets all posts
			/// </summary>
			All,

			/// <summary>
			/// Custom, set Timeunit and interval
			/// </summary>
			Custom
		}

		/// <summary>
		/// The time unit. To be used with interval.
		/// </summary>
		public enum TimeUnit {
			/// <summary>
			/// Hour
			/// </summary>
			Hour,

			/// <summary>
			/// Minute
			/// </summary>
			Minute,

			/// <summary>
			/// Day
			/// </summary>
			Day,

			/// <summary>
			/// Week
			/// </summary>
			Week,

			/// <summary>
			/// Month
			/// </summary>
			Month
		}

		
	}
}

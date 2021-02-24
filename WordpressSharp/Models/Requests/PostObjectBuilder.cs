using System;
using System.Collections.Generic;
using System.Text;

namespace PathanamthittaMedia.Library.Models.Requests {
	public class PostObjectBuilder {
		private Dictionary<string, string> FormData;

		private string Content;
		private string Title;
		private DateTime PostDate;
		private string Slug;
		private PostStatus Status = PostStatus.Pending;
		private string Password;
		private int AuthorId;
		private string Excerpt;
		private int FeaturedImageId;
		private CommandStatusValue CommandStatus = CommandStatusValue.Open;
		private PingStatusValue PingStatus = PingStatusValue.Open;
		private PostFormat Format = PostFormat.Standard;
		private bool Sticky;
		private int[] Categories;
		private int[] Tags;

		public PostObjectBuilder() { }

		internal Dictionary<string, string> Create() {
			FormData = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(Content)) {
				FormData.Add("content", Content);
			}

			if (!string.IsNullOrEmpty(Title)) {
				FormData.Add("title", Title);
			}

			if (!string.IsNullOrEmpty(Slug)) {
				FormData.Add("slug", Slug);
			}

			if (!string.IsNullOrEmpty(Password)) {
				FormData.Add("password", Password);
			}

			if(AuthorId > 0) {
				FormData.Add("author", AuthorId.ToString());
			}

			if (!string.IsNullOrEmpty(Excerpt)) {
				FormData.Add("excerpt", Excerpt);
			}

			if(FeaturedImageId > 0) {
				FormData.Add("featured_media", FeaturedImageId.ToString());
			}			

			if (Sticky) {
				FormData.Add("sticky", "1");
			}

			if(Categories != null && Categories.Length > 0) {
				FormData.Add("categories", string.Join(',', Categories));
			}

			if(Tags != null && Tags.Length > 0) {
				FormData.Add("tags", string.Join(',', Tags));
			}

			if(PostDate != DateTime.MinValue) {
				FormData.Add("date", PostDate.ToString());
			}

			FormData.Add("comment_status", CommandStatus.ToString().ToLower());
			FormData.Add("ping_status", PingStatus.ToString().ToLower());
			FormData.Add("format", Format.ToString().ToLower());			
			FormData.Add("status", Status.ToString().ToLower());
			return FormData;
		}

		public PostObjectBuilder WithTitle(string title) {
			Title = title;
			return this;
		}

		public PostObjectBuilder WithContent(string content) {
			Content = content;
			return this;
		}

		public PostObjectBuilder WithDate(DateTime dateTime) {
			PostDate = dateTime;
			return this;
		}

		public PostObjectBuilder WithSlug(string slug) {
			Slug = slug;
			return this;
		}

		public PostObjectBuilder WithStatus(PostStatus status) {
			Status = status;
			return this;
		}

		// TODO: Implement automatic password generator and return the generated password to caller using out parameter
		public PostObjectBuilder WithPassword(string password) {
			Password = password;
			return this;
		}

		public PostObjectBuilder WithAuthor(int authorId) {
			AuthorId = authorId;
			return this;
		}

		public PostObjectBuilder WithExcerpt(string excerpt) {
			Excerpt = excerpt;
			return this;
		}

		public PostObjectBuilder WithFeaturedImage(int featuredImageId) {
			FeaturedImageId = featuredImageId;
			return this;
		}

		public PostObjectBuilder WithCommandStatus(CommandStatusValue commandStatus) {
			CommandStatus = commandStatus;
			return this;
		}

		public PostObjectBuilder WithPingStatus(PingStatusValue pingStatus) {
			PingStatus = pingStatus;
			return this;
		}

		public PostObjectBuilder WithFormat(PostFormat format) {
			Format = format;
			return this;
		}

		public PostObjectBuilder WithStickyBehaviour(bool value) {
			Sticky = value;
			return this;
		}

		public PostObjectBuilder WithCategories(params int[] categories) {
			Categories = categories;
			return this;
		}

		public PostObjectBuilder WithTags(params int[] tags) {
			Tags = tags;
			return this;
		}

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
	}
}

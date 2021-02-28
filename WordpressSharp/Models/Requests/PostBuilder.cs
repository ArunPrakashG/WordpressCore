using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WordpressSharp.Interfaces;
using static WordpressSharp.Models.Requests.Enums;

namespace WordpressSharp.Models.Requests {
	/// <summary>
	/// Builder used to build CreatePost request
	/// </summary>
	public class PostBuilder : QueryBuilder<PostBuilder>, IRequestBuilder<PostBuilder, HttpContent> {
		private string Content;
		private string Title;
		private DateTime PostDate;
		private string Slug;
		private PostStatus Status;
		private string Password;
		private int AuthorId;
		private string Excerpt;
		private int FeaturedImageId;
		private CommandStatusValue CommandStatus;
		private PingStatusValue PingStatus;
		private PostFormat Format;
		private bool Sticky;
		private int[] Categories;
		private int[] Tags;

		public PostBuilder() { }

		public PostBuilder InitializeWithDefaultValues() {
			CommandStatus = CommandStatusValue.Open;
			PingStatus = PingStatusValue.Open;
			Format = PostFormat.Standard;
			Status = PostStatus.Pending;
			return this;
		}

		public HttpContent Create() {
			Dictionary<string, string> formData = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(Content)) {
				formData.Add("content", Content);
			}

			if (!string.IsNullOrEmpty(Title)) {
				formData.Add("title", Title);
			}

			if (!string.IsNullOrEmpty(Slug)) {
				formData.Add("slug", Slug);
			}

			if (!string.IsNullOrEmpty(Password)) {
				formData.Add("password", Password);
			}

			if (AuthorId > 0) {
				formData.Add("author", AuthorId.ToString());
			}

			if (!string.IsNullOrEmpty(Excerpt)) {
				formData.Add("excerpt", Excerpt);
			}

			if (FeaturedImageId > 0) {
				formData.Add("featured_media", FeaturedImageId.ToString());
			}

			if (Sticky) {
				formData.Add("sticky", "1");
			}

			if (Categories != null && Categories.Length > 0) {
				formData.Add("categories", string.Join(',', Categories));
			}

			if (Tags != null && Tags.Length > 0) {
				formData.Add("tags", string.Join(',', Tags));
			}

			if (PostDate != DateTime.MinValue) {
				formData.Add("date", PostDate.ToString());
			}

			formData.Add("comment_status", CommandStatus.ToString().ToLower());
			formData.Add("ping_status", PingStatus.ToString().ToLower());
			formData.Add("format", Format.ToString().ToLower());
			formData.Add("status", Status.ToString().ToLower());
			return new FormUrlEncodedContent(formData);
		}

		/// <summary>
		/// Sets the title of the post
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public PostBuilder WithTitle(string title) {
			Title = title;
			return this;
		}

		/// <summary>
		/// Sets the content of the post
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public PostBuilder WithContent(string content) {
			Content = content;
			return this;
		}

		[Obsolete]
		/// <summary>
		/// Sets the published date of the post (Bugged)
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public PostBuilder WithDate(DateTime dateTime) {
			PostDate = dateTime;
			return this;
		}

		/// <summary>
		/// Sets the slug of the post
		/// </summary>
		/// <param name="slug"></param>
		/// <returns></returns>
		public PostBuilder WithSlug(string slug) {
			Slug = slug;
			return this;
		}

		/// <summary>
		/// Sets the status of the post
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		public PostBuilder WithStatus(PostStatus status) {
			Status = status;
			return this;
		}

		// TODO: Implement automatic password generator and return the generated password to caller using out parameter
		/// <summary>
		/// Sets the password for the post
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		public PostBuilder WithPassword(string password) {
			Password = password;
			return this;
		}

		/// <summary>
		/// Generates an random password of specified length and returns it using out parameter.
		/// </summary>
		/// <param name="generatedPassword">The generated password</param>
		/// <param name="passwordLength">The password length. Default is 13</param>
		/// <returns></returns>
		public PostBuilder WithPassword(out string generatedPassword, int passwordLength = 13) {
			Password = Utilites.GenerateToken(passwordLength);
			generatedPassword = Password;
			return this;
		}

		/// <summary>
		/// Sets the author of the post
		/// </summary>
		/// <param name="authorId"></param>
		/// <returns></returns>
		public PostBuilder WithAuthor(int authorId) {
			AuthorId = authorId;
			return this;
		}

		/// <summary>
		/// Sets the excerpt of the post
		/// </summary>
		/// <param name="excerpt"></param>
		/// <returns></returns>
		public PostBuilder WithExcerpt(string excerpt) {
			Excerpt = excerpt;
			return this;
		}

		/// <summary>
		/// Sets the featured image to be used for the post
		/// </summary>
		/// <param name="featuredImageId"></param>
		/// <returns></returns>
		public PostBuilder WithFeaturedImage(int featuredImageId) {
			FeaturedImageId = featuredImageId;
			return this;
		}

		/// <summary>
		/// Sets the featured image of the post by first uploading the image on the path and then using its returned imageId
		/// </summary>
		/// <param name="client"></param>
		/// <param name="imagePath"></param>
		/// <returns></returns>
		public async Task<PostBuilder> WithFeaturedImage(WordpressClient client, string imagePath) {
			if (client == null) {
				throw new ArgumentNullException(nameof(client));
			}

			if (string.IsNullOrEmpty(imagePath)) {
				throw new ArgumentNullException(nameof(imagePath));
			}

			if (!File.Exists(imagePath)) {
				throw new FileNotFoundException(nameof(imagePath));
			}

			Responses.Response<Responses.Media> featuredMedia = await client.CreateMediaAsync((builder) => builder.WithBody<MediaBuilder, HttpContent>((media) => media.WithFile(imagePath).Create()).Create());
			FeaturedImageId = featuredMedia.Status ? featuredMedia.Value.Id : 0;
			return this;
		}

		public PostBuilder WithCommandStatus(CommandStatusValue commandStatus) {
			CommandStatus = commandStatus;
			return this;
		}

		public PostBuilder WithPingStatus(PingStatusValue pingStatus) {
			PingStatus = pingStatus;
			return this;
		}

		public PostBuilder WithFormat(PostFormat format) {
			Format = format;
			return this;
		}

		public PostBuilder WithStickyBehaviour(bool value) {
			Sticky = value;
			return this;
		}

		public PostBuilder WithCategories(params int[] categories) {
			Categories = categories;
			return this;
		}

		public PostBuilder WithTags(params int[] tags) {
			Tags = tags;
			return this;
		}
	}
}

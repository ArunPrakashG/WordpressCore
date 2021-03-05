using System;
using System.Collections.Generic;
using System.Net.Http;
using WordpressCore.Interfaces;

namespace WordpressCore.Models.Requests {
	/// <summary>
	/// Builder used to build CreateComment request
	/// </summary>
	public class CommentBuilder : QueryBuilder<CommentBuilder>, IRequestBuilder<CommentBuilder, HttpContent> {
		private int AuthorId;
		private string Email;
		private string Ip;
		private string Name;
		private string Url;
		private string UserAgent;
		private string Content;
		private int ParentId;
		private int PostId;
		private DateTime Date;

		/// <summary>
		/// The date the object was published, in the site's timezone.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public CommentBuilder WithDate(DateTime date) {
			Date = date;
			return this;
		}

		/// <summary>
		/// The ID of the user object, if author was a user.
		/// </summary>
		/// <param name="authorId"></param>
		/// <returns></returns>
		public CommentBuilder WithAuthor(int authorId) {
			AuthorId = authorId;
			return this;
		}

		/// <summary>
		/// Email address for the object author.
		/// </summary>
		/// <param name="authorEmail"></param>
		/// <returns></returns>
		public CommentBuilder WithEmail(string authorEmail) {
			Email = authorEmail;
			return this;
		}

		/// <summary>
		/// IP address for the object author.
		/// </summary>
		/// <param name="authorIp"></param>
		/// <returns></returns>
		public CommentBuilder WithIp(string authorIp) {
			Ip = authorIp;
			return this;
		}

		/// <summary>
		/// Display name for the object author.
		/// </summary>
		/// <param name="authorName"></param>
		/// <returns></returns>
		public CommentBuilder WithName(string authorName) {
			Name = authorName;
			return this;
		}

		/// <summary>
		/// URL for the object author.
		/// </summary>
		/// <param name="authorUrl"></param>
		/// <returns></returns>
		public CommentBuilder WithUrl(string authorUrl) {
			Url = authorUrl;
			return this;
		}

		/// <summary>
		/// User agent for the object author.
		/// </summary>
		/// <param name="userAgent"></param>
		/// <returns></returns>
		public CommentBuilder WithUserAgent(string userAgent) {
			UserAgent = userAgent;
			return this;
		}

		/// <summary>
		/// The content for the object.
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public CommentBuilder WithContent(string content) {
			Content = content;
			return this;
		}

		/// <summary>
		/// The ID for the parent of the object.
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public CommentBuilder WithParentId(int parentId) {
			ParentId = parentId;
			return this;
		}

		/// <summary>
		/// The ID of the associated post object.
		/// </summary>
		/// <param name="postId"></param>
		/// <returns></returns>
		public CommentBuilder WithPostId(int postId) {
			PostId = postId;
			return this;
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public HttpContent Create() {
			var formData = new Dictionary<string, string>();

			if (AuthorId >= 0) {
				formData.TryAdd("author", AuthorId.ToString());
			}

			if (!string.IsNullOrEmpty(Email)) {
				formData.TryAdd("author_email", Email);
			}

			if (!string.IsNullOrEmpty(Ip)) {
				formData.TryAdd("author_ip", Ip);
			}

			if (!string.IsNullOrEmpty(Name)) {
				formData.TryAdd("author_name", Name);
			}

			if (!string.IsNullOrEmpty(Url)) {
				formData.TryAdd("author_url", Url);
			}

			if (!string.IsNullOrEmpty(UserAgent)) {
				formData.TryAdd("author_user_agent", UserAgent);
			}

			if (!string.IsNullOrEmpty(Content)) {
				formData.TryAdd("content", Content);
			}

			if (ParentId >= 0) {
				formData.TryAdd("parent", ParentId.ToString());
			}

			if (PostId >= 0) {
				formData.TryAdd("post", PostId.ToString());
			}

			if (Date != DateTime.MinValue) {
				formData.TryAdd("date", Date.ToString());
			}

			return new FormUrlEncodedContent(formData);
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public CommentBuilder InitializeWithDefaultValues() {
			AuthorId = -1;
			ParentId = -1;
			PostId = -1;
			Date = DateTime.MinValue;
			return this;
		}
	}
}

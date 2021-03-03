using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using WordpressSharp.Interfaces;
using static WordpressSharp.Models.Requests.Enums;

namespace WordpressSharp.Models.Requests {
	public class MediaBuilder : QueryBuilder<MediaBuilder>, IRequestBuilder<MediaBuilder, HttpContent> {
		private StreamContent HttpStreamContent;
		private string AltText;
		private string Caption;
		private string Description;
		private int AssociatedPostId = -1;
		private string Title;
		private int AuthorId = -1;
		private CommentStatusValue CommandStatus;
		private PingStatusValue PingStatus;

		public MediaBuilder() { }

		public MediaBuilder InitializeWithDefaultValues() {
			AssociatedPostId = -1;
			CommandStatus = CommentStatusValue.Open;
			PingStatus = PingStatusValue.Open;
			AuthorId = -1;			
			return this;
		}

		public MediaBuilder WithFile(Stream fileStream, string filePath) {
			if(fileStream == null) {
				throw new ArgumentNullException(nameof(fileStream));
			}

			if (string.IsNullOrEmpty(filePath)) {
				throw new ArgumentNullException(filePath);
			}

			if (!File.Exists(filePath)) {
				throw new FileNotFoundException(filePath);
			}

			HttpStreamContent = new StreamContent(fileStream);

			string fileName = Path.GetFileName(filePath);
			string extension = fileName.Split('.').Last();
			HttpStreamContent.Headers.TryAddWithoutValidation("Content-Type", Utilites.GetMIMETypeFromExtension(extension));
			HttpStreamContent.Headers.TryAddWithoutValidation("Content-Disposition", $"attachment; filename={fileName}");
			return this;
		}

		public MediaBuilder WithFile(string filePath) {
			if(string.IsNullOrEmpty(filePath)) {
				throw new ArgumentNullException(filePath);
			}

			if (!File.Exists(filePath)) {
				throw new FileNotFoundException(filePath);
			}

			HttpStreamContent = new StreamContent(File.OpenRead(filePath));
			string fileName = Path.GetFileName(filePath);
			string extension = fileName.Split('.').Last();
			HttpStreamContent.Headers.TryAddWithoutValidation("Content-Type", Utilites.GetMIMETypeFromExtension(extension));
			HttpStreamContent.Headers.TryAddWithoutValidation("Content-Disposition", $"attachment; filename={fileName}");
			return this;
		}

		public HttpContent Create() {
			if(HttpStreamContent == null) {
				throw new NullReferenceException($"{nameof(HttpStreamContent)} cannot be null.");
			}

			if (!string.IsNullOrEmpty(AltText)) {
				HttpStreamContent.Headers.TryAddWithoutValidation("alt_text", AltText);
			}

			if (!string.IsNullOrEmpty(Caption)) {
				HttpStreamContent.Headers.TryAddWithoutValidation("caption", Caption);
			}

			if (!string.IsNullOrEmpty(Description)) {
				HttpStreamContent.Headers.TryAddWithoutValidation("description", Description);
			}

			if (AssociatedPostId >= 0) {
				HttpStreamContent.Headers.TryAddWithoutValidation("post", AssociatedPostId.ToString());
			}

			if (!string.IsNullOrEmpty(Title)) {
				HttpStreamContent.Headers.TryAddWithoutValidation("title", Title);
			}

			if (AuthorId >= 0) {
				HttpStreamContent.Headers.TryAddWithoutValidation("author", AuthorId.ToString());
			}

			HttpStreamContent.Headers.TryAddWithoutValidation("comment_status", CommandStatus.ToString().ToLower());
			HttpStreamContent.Headers.TryAddWithoutValidation("ping_status", PingStatus.ToString().ToLower());
			return HttpStreamContent;
		}

		public MediaBuilder WithTitle(string title) {
			Title = title;
			return this;
		}

		public MediaBuilder WithCaption(string caption) {
			Caption = caption;
			return this;
		}

		public MediaBuilder WithDescription(string description) {
			Description = description;
			return this;
		}

		public MediaBuilder WithAlternateText(string text) {
			AltText = text;
			return this;
		}

		public MediaBuilder WithAuthor(int authorId) {
			AuthorId = authorId;
			return this;
		}

		public MediaBuilder WithAssociatedPost(int postId) {
			AssociatedPostId = postId;
			return this;
		}

		public MediaBuilder WithCommandStatus(CommentStatusValue commandStatus) {
			CommandStatus = commandStatus;
			return this;
		}

		public MediaBuilder WithPingStatus(PingStatusValue pingStatus) {
			PingStatus = pingStatus;
			return this;
		}


	}
}

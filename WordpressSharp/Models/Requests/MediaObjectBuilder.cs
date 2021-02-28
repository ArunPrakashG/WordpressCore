using System;
using System.Collections.Generic;
using System.Text;
using static WordpressSharp.Models.Requests.Enums;

namespace WordpressSharp.Models.Requests {
	public class MediaObjectBuilder {
		private string AltText;
		private string Caption;
		private string Description;
		private int AssociatedPostId = -1;
		private string Title;
		private int AuthorId = -1;
		private CommandStatusValue CommandStatus = CommandStatusValue.Open;
		private PingStatusValue PingStatus = PingStatusValue.Open;

		internal MediaObjectBuilder() { }

		internal Dictionary<string, string> Create() {
			Dictionary<string, string> formData = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(AltText)) {
				formData.Add("alt_text", AltText);
			}

			if (!string.IsNullOrEmpty(Caption)) {
				formData.Add("caption", Caption);
			}

			if (!string.IsNullOrEmpty(Description)) {
				formData.Add("description", Description);
			}

			if(AssociatedPostId >= 0) {
				formData.Add("post", AssociatedPostId.ToString());
			}

			if (!string.IsNullOrEmpty(Title)) {
				formData.Add("title", Title);
			}

			if(AuthorId >= 0) {
				formData.Add("author", AuthorId.ToString());
			}

			formData.Add("comment_status", CommandStatus.ToString().ToLower());
			formData.Add("ping_status", PingStatus.ToString().ToLower());
			return formData;
		}

		public MediaObjectBuilder WithTitle(string title) {
			Title = title;
			return this;
		}

		public MediaObjectBuilder WithCaption(string caption) {
			Caption = caption;
			return this;
		}

		public MediaObjectBuilder WithDescription(string description) {
			Description = description;
			return this;
		}

		public MediaObjectBuilder WithAlternateText(string text) {
			AltText = text;
			return this;
		}

		public MediaObjectBuilder WithAuthor(int authorId) {
			AuthorId = authorId;
			return this;
		}

		public MediaObjectBuilder WithAssociatedPost(int postId) {
			AssociatedPostId = postId;
			return this;
		}

		public MediaObjectBuilder WithCommandStatus(CommandStatusValue commandStatus) {
			CommandStatus = commandStatus;
			return this;
		}

		public MediaObjectBuilder WithPingStatus(PingStatusValue pingStatus) {
			PingStatus = pingStatus;
			return this;
		}
	}
}

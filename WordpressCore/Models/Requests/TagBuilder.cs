using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WordpressCore.Interfaces;

namespace WordpressCore.Models.Requests {
	/// <summary>
	/// Builder used to build CreateTag request
	/// </summary>
	public class TagBuilder : QueryBuilder<PostBuilder>, IRequestBuilder<TagBuilder, HttpContent> {
		private string Description;
		private string Name;
		private string Slug;

		/// <summary>
		/// Constructor
		/// </summary>
		public TagBuilder() { }

		/// <summary>
		/// Adds a description to the tag. (Can contain HTML)
		/// </summary>
		/// <param name="description">The Description</param>
		/// <returns></returns>
		public TagBuilder WithDescription(string description) {
			Description = description;
			return this;
		}

		/// <summary>
		/// The Name of the Tag. (REQUIRED)
		/// </summary>
		/// <param name="name">The Name</param>
		/// <returns></returns>
		public TagBuilder WithName(string name) {
			Name = name;
			return this;
		}

		/// <summary>
		/// The Slug for the Tag. (Alphanumeric charecters only, 0-9 a-Z)
		/// </summary>
		/// <param name="slug"></param>
		/// <returns></returns>
		public TagBuilder WithSlug(string slug) {
			Slug = slug;
			return this;
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public HttpContent Create() {
			Dictionary<string, string> values = new Dictionary<string, string>();

			if (string.IsNullOrEmpty(Name)) {
				throw new InvalidOperationException($"{nameof(Name)} field can't be an empty value.");
			}

			values.TryAdd("name", Name);

			if (!string.IsNullOrEmpty(Description)) {
				values.TryAdd("description", Description);
			}

			if (!string.IsNullOrEmpty(Slug) && Utilites.IsAlphanumeric(Slug)) {
				values.TryAdd("slug", Slug);
			}

			return new FormUrlEncodedContent(values);
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public TagBuilder InitializeWithDefaultValues() => this;
	}
}

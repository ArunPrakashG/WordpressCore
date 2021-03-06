using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WordpressCore.Interfaces;

namespace WordpressCore.Models.Requests {
	/// <summary>
	/// Builder used to build CreateUser request
	/// </summary>
	public class CategoryBuilder : QueryBuilder<CategoryBuilder>, IRequestBuilder<CategoryBuilder, HttpContent> {
		private string Description;
		private string Name;
		private string Slug;
		private int ParentId;

		/// <summary>
		/// Sets the Description for the category.
		/// </summary>
		/// <param name="description"></param>
		/// <returns></returns>
		public CategoryBuilder WithDescription(string description) {
			Description = description;
			return this;
		}

		/// <summary>
		/// Sets the Name for the category. (REQUIRED)
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public CategoryBuilder WithName(string name) {
			Name = name;
			return this;
		}

		/// <summary>
		/// Sets the Slug for the category.
		/// </summary>
		/// <param name="slug"></param>
		/// <returns></returns>
		public CategoryBuilder WithSlug(string slug) {
			if (!Utilites.IsAlphanumeric(slug)) {
				throw new ArgumentException($"{nameof(slug)} can only contain alphanumeric charecters. (a-Z, 0-9)");
			}

			Slug = slug;
			return this;
		}

		/// <summary>
		/// Sets the Parent category of this category.
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public CategoryBuilder WithParentId(int parentId) {
			ParentId = parentId;
			return this;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		/// <returns></returns>
		public HttpContent Create() {
			var formData = new Dictionary<string, string>();

			if (string.IsNullOrEmpty(Name)) {
				throw new ArgumentNullException($"{nameof(Name)} can't be an empty value.");
			}

			formData.TryAdd("name", Name);

			if (!string.IsNullOrEmpty(Description)) {
				formData.TryAdd("description", Description);
			}

			if (!string.IsNullOrEmpty(Slug)) {
				formData.TryAdd("slug", Slug);
			}

			if(ParentId > 0) {
				formData.TryAdd("parent", ParentId.ToString());
			}

			return new StringContent(JsonConvert.SerializeObject(formData));
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		/// <returns></returns>
		public CategoryBuilder InitializeWithDefaultValues() {
			ParentId = -1;
			return this;
		}
	}
}

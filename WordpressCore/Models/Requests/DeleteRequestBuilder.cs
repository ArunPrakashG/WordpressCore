using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WordpressCore.Interfaces;

namespace WordpressCore.Models.Requests {
	public class DeleteRequestBuilder : IRequestBuilder<DeleteRequestBuilder, string> {
		private string Endpoint;
		private int Id;
		private bool ForceDelete;

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public string Create() => $"{(!string.IsNullOrEmpty(Endpoint) ? Endpoint : "")}/{Id}?force={ForceDelete.ToString().ToLower()}";

		public DeleteRequestBuilder WithId(int id) {
			if(id < 0) {
				throw new ArgumentOutOfRangeException(nameof(id));
			}

			Id = id;
			return this;
		}

		public DeleteRequestBuilder WithEndpoint(string endPoint) {
			Endpoint = endPoint;
			return this;
		}

		public DeleteRequestBuilder WithForceDeleteStatus(bool value) {
			ForceDelete = value;
			return this;
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public DeleteRequestBuilder InitializeWithDefaultValues() {
			ForceDelete = false;
			return this;
		}
	}
}

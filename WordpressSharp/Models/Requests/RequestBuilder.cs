using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using static WordpressSharp.Models.Requests.Enums;

namespace WordpressSharp.Models.Requests {
	public class RequestBuilder {
		private Uri BaseUri;
		private Uri RequestUri;
		private CancellationToken Token;
		private string Endpoint;
		private string Context;
		private int PageNumber;
		private int PerPageCount;
		private string SearchQuery;
		private DateTime After;
		private DateTime Before;
		private List<int> AllowedAuthors;
		private List<int> ExcludedAuthors;
		private List<int> ExcludedIds;
		private List<int> AllowedIds;
		private int ResultOffset;
		private string ResultOrder;
		private string SortOrder;
		private List<string> LimitBySlug;
		private string LimitByStatus;
		private string LimitByTaxonomyRelation;
		private List<int> AllowedTags;
		private List<int> ExcludedTags;
		private List<int> AllowedCategories;
		private List<int> ExcludedCategories;
		private bool OnlySticky;
		private bool Embeded;
		private WordpressAuthorization Authorization;
		private Func<string, bool> ResponseValidationDelegate;
		private HttpMethod Method;
		private Dictionary<string, string> Headers;
		private Dictionary<string, string> FormBody;

		public RequestBuilder(string requestUrlBase, string endpoint) {
			if (string.IsNullOrEmpty(requestUrlBase) || string.IsNullOrEmpty(endpoint)) {
				throw new ArgumentNullException(nameof(requestUrlBase));
			}

			if (!Uri.TryCreate(Path.Combine(requestUrlBase, endpoint), UriKind.RelativeOrAbsolute, out Uri requestUri)) {
				throw new InvalidOperationException(nameof(requestUri));
			}

			BaseUri = requestUri;
			Endpoint = endpoint;
		}

		public RequestBuilder() { }

		public static RequestBuilder WithBuilder() => new RequestBuilder();

		private static bool ContainsQueryValues(string url, out bool hasMultiple) {
			hasMultiple = false;

			if (string.IsNullOrEmpty(url)) {
				return false;
			}

			bool hasFirstQuery = url.Contains('?');
			int queryCount = 0;

			if (url.Contains('&')) {
				queryCount = url.Split('&').Length;
			}

			hasMultiple = hasFirstQuery && queryCount > 0;
			return hasMultiple || hasFirstQuery || queryCount > 0;
		}

		private bool CreateUri() {
			string baseUrl = BaseUri.OriginalString;
			char joiningChar = ContainsQueryValues(baseUrl, out bool hasMultiple) && hasMultiple ? '&' : '?';

			// because context value is ignored mostly on those pages which doesn't require it.
			if (!string.IsNullOrEmpty(Context)) {
				baseUrl += $"{joiningChar}context={Context}";
			}			

			if (PageNumber >= 1) {
				baseUrl += $"{joiningChar}page={PageNumber}";
			}

			if(PerPageCount >= 1) {
				baseUrl += $"{joiningChar}per_page={(PerPageCount <= 0 ? 10 : PerPageCount)}";
			}			

			if (!string.IsNullOrEmpty(SearchQuery)) {
				baseUrl += $"{joiningChar}search={SearchQuery}";
			}

			if (Embeded) {
				baseUrl += $"{joiningChar}_embed=1";
			}

			if (After != DateTime.MinValue) {
				baseUrl += $"{joiningChar}after={After.ToString("o", CultureInfo.InvariantCulture)}";
			}

			if (Before != DateTime.MinValue) {
				baseUrl += $"{joiningChar}before={Before.ToString("o", CultureInfo.InvariantCulture)}";
			}

			if (AllowedAuthors != null && AllowedAuthors.Count > 0) {
				baseUrl += $"{joiningChar}author={string.Join(",", AllowedAuthors)}";
			}

			if (ExcludedAuthors != null && ExcludedAuthors.Count > 0) {
				baseUrl += $"{joiningChar}author_exclude={string.Join(",", ExcludedAuthors)}";
			}

			if (AllowedIds != null && AllowedIds.Count > 0) {
				baseUrl += $"{joiningChar}include={string.Join(",", AllowedIds)}";
			}

			if (ExcludedIds != null && ExcludedIds.Count > 0) {
				baseUrl += $"{joiningChar}exclude={string.Join(",", ExcludedIds)}";
			}

			if (ResultOffset > 0) {
				baseUrl += $"{joiningChar}offset={ResultOffset}";
			}

			if (!string.IsNullOrEmpty(SortOrder)) {
				baseUrl += $"{joiningChar}order={SortOrder}";
			}			

			if (!string.IsNullOrEmpty(ResultOrder)) {
				baseUrl += $"{joiningChar}orderby={ResultOrder}";
			}			

			if (LimitBySlug != null && LimitBySlug.Count > 0) {
				baseUrl += $"{joiningChar}slug={string.Join(",", LimitBySlug)}";
			}

			if (!string.IsNullOrEmpty(LimitByStatus)) {
				baseUrl += $"{joiningChar}status={LimitByStatus}";
			}

			if (!string.IsNullOrEmpty(LimitByTaxonomyRelation)) {
				baseUrl += $"{joiningChar}tax_relation={LimitByTaxonomyRelation}";
			}

			if (AllowedCategories != null && AllowedCategories.Count > 0) {
				baseUrl += $"{joiningChar}categories={string.Join(",", AllowedCategories)}";
			}

			if (ExcludedCategories != null && ExcludedCategories.Count > 0) {
				baseUrl += $"{joiningChar}categories_exclude={string.Join(",", ExcludedCategories)}";
			}

			if (AllowedTags != null && AllowedTags.Count > 0) {
				baseUrl += $"{joiningChar}tags={string.Join(",", AllowedTags)}";
			}

			if (ExcludedTags != null && ExcludedTags.Count > 0) {
				baseUrl += $"{joiningChar}tags_exclude={string.Join(",", ExcludedTags)}";
			}

			if (OnlySticky) {
				baseUrl += $"{joiningChar}sticky=1";
			}

			if (!Uri.TryCreate(baseUrl, UriKind.RelativeOrAbsolute, out Uri requestUri)) {
				return false;
			}

			RequestUri = requestUri;
			return true;
		}

		internal RequestBuilder WithBaseAndEndpoint(string requestUrlBase, string endpoint) {
			if (string.IsNullOrEmpty(requestUrlBase) || string.IsNullOrEmpty(endpoint)) {
				throw new ArgumentNullException(nameof(requestUrlBase));
			}

			if (!Uri.TryCreate(Path.Combine(requestUrlBase, endpoint), UriKind.RelativeOrAbsolute, out Uri requestUri)) {
				throw new InvalidOperationException(nameof(requestUri));
			}

			Endpoint = endpoint;
			BaseUri = requestUri;
			return this;
		}

		internal RequestBuilder WithUri(Uri requestUri) {
			BaseUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
			return this;
		}

		public Request CreateWithCallback(Callback callback) {
			if (CreateUri()) {
#if DEBUG
				Debug.WriteLine(RequestUri.ToString());
#endif
				return new Request(RequestUri, ResponseValidationDelegate, Endpoint, Token, Authorization, Method ?? HttpMethod.Get, Headers, FormBody, PerPageCount, callback);
			}

			return default;
		}

		public Request Create() {
			if (CreateUri()) {
#if DEBUG
				Debug.WriteLine(RequestUri.ToString());
#endif
				return new Request(RequestUri, null, Endpoint, Token, Authorization, Method ?? HttpMethod.Get, Headers, FormBody, PerPageCount);
			}

			return default;
		}

		public RequestBuilder WithAuthorization(WordpressAuthorization auth) {
			if (auth.IsDefault) {
				return this;
			}

			Authorization = auth;
			return this;
		}

		public RequestBuilder WithPostBody(Func<PostObjectBuilder, PostObjectBuilder> formBodyBuilder) {
			FormBody = formBodyBuilder.Invoke(new PostObjectBuilder()).Create();
			return this;
		}

		public RequestBuilder WithMediaBody(Func<MediaObjectBuilder, MediaObjectBuilder> formBodyBuilder) {
			FormBody = formBodyBuilder.Invoke(new MediaObjectBuilder()).Create();
			return this;
		}

		public RequestBuilder WithHeaders(Dictionary<string, string> headers) {
			Headers = headers;
			return this;
		}

		internal RequestBuilder WithHttpMethod(HttpMethod method) {
			Method = method;
			return this;
		}

		public RequestBuilder WithResponseValidationOverride(Func<string, bool> responseDelegate) {
			ResponseValidationDelegate = responseDelegate;
			return this;
		}

		public RequestBuilder WithCancellationToken(CancellationToken token) {
			Token = token;
			return this;
		}

		public RequestBuilder WithSearchQuery(string queryValue) {
			SearchQuery = queryValue;
			return this;
		}

		public RequestBuilder SetEmbeded(bool value) {
			Embeded = value;
			return this;
		}

		public RequestBuilder WithPerPage(int count) {
			PerPageCount = count;
			return this;
		}

		public RequestBuilder WithPageNumber(int pageNumber) {
			PageNumber = pageNumber;
			return this;
		}

		public RequestBuilder ValuesBefore(DateTime dateTime) {
			Before = dateTime;
			return this;
		}

		public RequestBuilder ValuesAfter(DateTime dateTime) {
			After = dateTime;
			return this;
		}

		public RequestBuilder AllowAuthors(params int[] ids) {
			if (AllowedAuthors == null) {
				AllowedAuthors = new List<int>();
			}

			AllowedAuthors.AddRange(ids);
			return this;
		}

		public RequestBuilder ExcludeAuthors(params int[] ids) {
			if (ExcludedAuthors == null) {
				ExcludedAuthors = new List<int>();
			}

			ExcludedAuthors.AddRange(ids);
			return this;
		}

		public RequestBuilder IncludeIds(params int[] ids) {
			if (AllowedIds == null) {
				AllowedIds = new List<int>();
			}

			AllowedIds.AddRange(ids);
			return this;
		}

		public RequestBuilder ExcludeIds(params int[] ids) {
			if (ExcludedIds == null) {
				ExcludedIds = new List<int>();
			}

			ExcludedIds.AddRange(ids);
			return this;
		}

		public RequestBuilder WithResultOffset(int offset) {
			ResultOffset = offset;
			return this;
		}

		public RequestBuilder AllowSlugs(params string[] slug) {
			if (LimitBySlug == null) {
				LimitBySlug = new List<string>();
			}

			LimitBySlug.AddRange(slug);
			return this;
		}

		public RequestBuilder OrderResultBy(Order resultOrder) {
			switch (resultOrder) {
				case Order.Ascending:
					ResultOrder = "asc";
					break;
				case Order.Descending:
					ResultOrder = "desc";
					break;
			}

			return this;
		}

		public RequestBuilder OrderResultBy(OrderBy orderBy) {
			string endPoint = BaseUri.AbsoluteUri.Substring(BaseUri.AbsoluteUri.LastIndexOf('/'));
			Debug.WriteLine("Endpoint: " + endPoint);

			switch (orderBy) {				
				case OrderBy.Date when endPoint.Equals("users", StringComparison.OrdinalIgnoreCase):
					SortOrder = "registered_date";
					break;
				case OrderBy.Date:
				case OrderBy.Author:
				case OrderBy.Id:
				case OrderBy.Include:
				case OrderBy.Modified:
				case OrderBy.Parent:
				case OrderBy.Relevance:
				case OrderBy.Slug:
				case OrderBy.Title:
				case OrderBy.Email when endPoint.Equals("users", StringComparison.OrdinalIgnoreCase):
				case OrderBy.Name when endPoint.Equals("users", StringComparison.OrdinalIgnoreCase):
				case OrderBy.Url when endPoint.Equals("users", StringComparison.OrdinalIgnoreCase):
					SortOrder = orderBy.ToString().ToLower();
					break;
				case OrderBy.IncludeSlugs:
					SortOrder = "include_slugs";
					break;
			}

			return this;
		}

		public RequestBuilder SetScope(Scope scope) {
			switch (scope) {
				case Scope.View:
					Context = "view";
					break;
				case Scope.Embed:
					Context = "embed";
					break;
				case Scope.Edit:
					Context = "edit";
					break;
			}

			return this;
		}

		public RequestBuilder LimitToSticky(bool shouldLimit) {
			OnlySticky = shouldLimit;
			return this;
		}

		public RequestBuilder AllowTags(params int[] tags) {
			if (AllowedTags == null) {
				AllowedTags = new List<int>();
			}

			AllowedTags.AddRange(tags);
			return this;
		}

		public RequestBuilder ExcludeTags(params int[] tags) {
			if (ExcludedTags == null) {
				ExcludedTags = new List<int>();
			}

			ExcludedTags.AddRange(tags);
			return this;
		}

		public RequestBuilder AllowCategories(params int[] categories) {
			if (categories.Contains(-1)) {
				return this;
			}

			if (AllowedCategories == null) {
				AllowedCategories = new List<int>();
			}

			AllowedCategories.AddRange(categories);
			return this;
		}

		public RequestBuilder ExcludeCategories(params int[] categories) {
			if (ExcludedCategories == null) {
				ExcludedCategories = new List<int>();
			}

			ExcludedCategories.AddRange(categories);
			return this;
		}

		public RequestBuilder SetAllowedTaxonomyRelation(TaxonomyRelation relation) {
			switch (relation) {
				case TaxonomyRelation.And:
					LimitByTaxonomyRelation = "AND";
					break;
				case TaxonomyRelation.Or:
					LimitByTaxonomyRelation = "OR";
					break;
			}

			return this;
		}

		public RequestBuilder SetAllowedStatus(Status status) {
			switch (status) {
				case Status.Published:
					LimitByStatus = "published";
					break;
				case Status.Draft:
					LimitByStatus = "draft";
					break;
				case Status.Trash:
					LimitByStatus = "trash";
					break;
			}

			return this;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using WordpressCore.Interfaces;
using static WordpressCore.Models.Requests.Enums;

namespace WordpressCore.Models.Requests {
	/// <summary>
	/// Allows you to construct a request in a fluent pattern.
	/// </summary>
	public class RequestBuilder : IRequestBuilder<RequestBuilder, Request> {
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
		private IDictionary<string, string> Headers;
		private HttpContent FormBody;

		/// <summary>
		/// Constructor to set Request Base Url and the Endpoint to use.
		/// </summary>
		/// <param name="requestUrlBase">The Base Url</param>
		/// <param name="endpoint">The Endpoint</param>
		internal RequestBuilder(string requestUrlBase, string endpoint) {
			if (string.IsNullOrEmpty(requestUrlBase) || string.IsNullOrEmpty(endpoint)) {
				throw new ArgumentNullException(nameof(requestUrlBase));
			}

			if (!Uri.TryCreate(Path.Combine(requestUrlBase, endpoint), UriKind.RelativeOrAbsolute, out Uri requestUri)) {
				throw new InvalidOperationException(nameof(requestUri));
			}

			BaseUri = requestUri;
			Endpoint = endpoint;
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		internal RequestBuilder() { }

		/// <summary>
		/// Static overload of new RequestBuilder() call.
		/// </summary>
		/// <returns>A new instance of <see cref="RequestBuilder"/></returns>
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

			if (FormBody == null || FormBody.Headers.Any()) {
				char joiningChar = ContainsQueryValues(baseUrl, out bool hasMultiple) && hasMultiple ? '&' : '?';

				// because context value is ignored mostly on those pages which doesn't require it.
				if (!string.IsNullOrEmpty(Context)) {
					baseUrl += $"{joiningChar}context={Context}";
				}

				if (PageNumber >= 1) {
					baseUrl += $"{joiningChar}page={PageNumber}";
				}

				if (PerPageCount >= 1) {
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

		/// <summary>
		/// Initializes current instance of <see cref="RequestBuilder"/> with Default values.
		/// </summary>
		/// <returns></returns>
		public RequestBuilder InitializeWithDefaultValues() => this;

		/// <summary>
		/// Creates the <see cref="Request"/> with supplies <see cref="Callback"/> methods.
		/// </summary>
		/// <param name="callback">The Callback</param>
		/// <returns>The <see cref="Request"/></returns>
		public Request CreateWithCallback(Callback callback) {
			if (CreateUri()) {
#if DEBUG
				Debug.WriteLine(RequestUri.ToString());
#endif
				return new Request(RequestUri, ResponseValidationDelegate, Endpoint, Token, Authorization, Method ?? HttpMethod.Get, Headers, FormBody, PerPageCount, callback);
			}

			return default;
		}

		/// <summary>
		/// Creates a <see cref="Request"/>.
		/// </summary>
		/// <returns>The <see cref="Request"/></returns>
		public Request Create() {
			if (CreateUri()) {
#if DEBUG
				Debug.WriteLine(RequestUri.ToString());
#endif
				return new Request(RequestUri, null, Endpoint, Token, Authorization, Method ?? HttpMethod.Get, Headers, FormBody, PerPageCount);
			}

			return default;
		}

		/// <summary>
		/// Adds <see cref="WordpressAuthorization"/> to this request.
		/// <para>(If you are using JWT, getting the token is handled internally)</para>
		/// </summary>
		/// <param name="auth">The <see cref="WordpressAuthorization"/> container</param>
		/// <returns></returns>
		public RequestBuilder WithAuthorization(WordpressAuthorization auth) {
			if (auth.IsDefault) {
				return this;
			}

			Authorization = auth;
			return this;
		}

		/// <summary>
		/// Transforms the request to support a Post body.
		/// <para>(used for CreatePost() Requests)</para>
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public RequestBuilder WithPostBody(Func<PostBuilder, HttpContent> builder) {
			FormBody = builder.Invoke(new PostBuilder().InitializeWithDefaultValues());
			return this;
		}

		/// <summary>
		/// Transforms the request to support a Media body.
		/// <para>(used for CreateMedia() Requests)</para>
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public RequestBuilder WithMediaBody(Func<MediaBuilder, HttpContent> builder) {
			FormBody = builder.Invoke(new MediaBuilder().InitializeWithDefaultValues());
			return this;
		}

		/// <summary>
		/// Transforms the request to support a Tag body.
		/// <para>(used for CreateTag() Requests)</para>
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public RequestBuilder WithTagBody(Func<TagBuilder, HttpContent> builder) {
			FormBody = builder.Invoke(new TagBuilder().InitializeWithDefaultValues());
			return this;
		}

		/// <summary>
		/// Transforms the request to support a Comment body.
		/// <para>(used for CreateComment() Requests)</para>
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public RequestBuilder WithCommentBody(Func<CommentBuilder, HttpContent> builder) {
			FormBody = builder.Invoke(new CommentBuilder().InitializeWithDefaultValues());
			return this;
		}

		/// <summary>
		/// Transforms the request to support a User body.
		/// <para>(used for CreateUser() Requests)</para>
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public RequestBuilder WithUserBody(Func<UserBuilder, HttpContent> builder) {
			FormBody = builder.Invoke(new UserBuilder().InitializeWithDefaultValues());
			return this;
		}

		/// <summary>
		/// Transforms the request to support a Category body.
		/// <para>(used for CreateCategory() Requests)</para>
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public RequestBuilder WithCategoryBody(Func<CategoryBuilder, HttpContent> builder) {
			FormBody = builder.Invoke(new CategoryBuilder().InitializeWithDefaultValues());
			return this;
		}

		/// <summary>
		/// Generic overload for WithBody() Requests.
		/// Transforms the request to support an <see cref="HttpContent"/> body.
		/// <para>(used for Create() Requests)</para>
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public RequestBuilder WithHttpBody<TBuilderType, YBuilderReturnType>(Func<TBuilderType, YBuilderReturnType> builder)
			where TBuilderType : IRequestBuilder<TBuilderType, YBuilderReturnType>, new()
			where YBuilderReturnType : HttpContent {
			FormBody = builder.Invoke(new TBuilderType().InitializeWithDefaultValues());
			return this;
		}

		/// <summary>
		/// Adds additional headers to the request.
		/// </summary>
		/// <param name="headers">The headers</param>
		/// <returns></returns>
		public RequestBuilder WithHeaders(IDictionary<string, string> headers) {
			Headers = headers;
			return this;
		}

		/// <summary>
		/// Specifys the <see cref="HttpMethod"/> to use.
		/// </summary>
		/// <param name="method">The method</param>
		/// <returns></returns>
		internal RequestBuilder WithHttpMethod(HttpMethod method) {
			Method = method;
			return this;
		}

		/// <summary>
		/// Adds a response validation callback.
		/// <para>When a response is received, the response string is obtained, then supplied to this delegate. If the returned value is true, request is continued, else the request is terminated with a error code.</para>
		/// </summary>
		/// <param name="responseDelegate">The delegate</param>
		/// <returns></returns>
		public RequestBuilder WithResponseValidationOverride(Func<string, bool> responseDelegate) {
			ResponseValidationDelegate = responseDelegate;
			return this;
		}

		/// <summary>
		/// Supply a <see cref="CancellationToken"/> to this request.
		/// </summary>
		/// <param name="token">The token</param>
		/// <returns></returns>
		public RequestBuilder WithCancellationToken(CancellationToken token) {
			Token = token;
			return this;
		}

		/// <summary>
		/// Specify a Search Query to search for the result in server side.
		/// </summary>
		/// <param name="queryValue">The query</param>
		/// <returns></returns>
		public RequestBuilder WithSearchQuery(string queryValue) {
			SearchQuery = queryValue;
			return this;
		}

		/// <summary>
		/// Set to true to allow embed result in the response.
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns></returns>
		public RequestBuilder SetEmbeded(bool value) {
			Embeded = value;
			return this;
		}

		/// <summary>
		/// Specify the maximum number of elements in the returned page.
		/// </summary>
		/// <param name="count">The count</param>
		/// <remarks>Defaults to 10</remarks>
		/// <returns></returns>
		public RequestBuilder WithPerPage(int count) {
			PerPageCount = count;
			return this;
		}

		/// <summary>
		/// Sets the page number to request.
		/// </summary>
		/// <param name="pageNumber">The page number</param>
		/// <returns></returns>
		public RequestBuilder WithPageNumber(int pageNumber) {
			PageNumber = pageNumber;
			return this;
		}

		/// <summary>
		/// Set to allow response elements to be of values before this particular date.
		/// </summary>
		/// <param name="dateTime">The date</param>
		/// <returns></returns>
		public RequestBuilder ValuesBefore(DateTime dateTime) {
			Before = dateTime;
			return this;
		}

		/// <summary>
		/// Set to allow response elements to be of values after this particular date.
		/// </summary>
		/// <param name="dateTime">The date</param>
		/// <returns></returns>
		public RequestBuilder ValuesAfter(DateTime dateTime) {
			After = dateTime;
			return this;
		}

		/// <summary>
		/// Set to only allow elements published by these authors in the response.
		/// </summary>
		/// <param name="ids">The author ids</param>
		/// <returns></returns>
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

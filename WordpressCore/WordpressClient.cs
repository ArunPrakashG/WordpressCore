using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WordpressCore.Models.Requests;
using WordpressCore.Models.Responses;

namespace WordpressCore {
	/// <summary>
	/// Initializes an instance of <see cref="WordpressClient"/>
	/// </summary>
	public class WordpressClient {
		private readonly int TIMEOUT = 60;
		private readonly int MAX_CONCURRENT_CONNECTION_PER_INSTANCE = 10;
		private readonly bool Threadsafe = false;
		private readonly string UrlPath = "wp/v2/";
		private readonly string BaseUrl = "http://demo.wp-api.org/wp-json/";
		private HttpClient _client;
		private HttpClientHandler _clientHandler;
		private WordpressAuthorization LoginDetails;
		private CookieContainer Cookies;
		private JsonSerializerSettings JsonSerializerSettings;
		private SemaphoreSlim RequestSync;
		private static Action<string, int> EndpointRequestCountChangedCallback;
		private static Func<string, bool> GlobalResponsePreprocessorCallback;
		internal static Func<string, string> HtmlResponseCleanerCallback;

		/// <summary>
		/// Stores statistical data of requests send through this <see cref="WordpressClient"/> Instance.
		/// <para> Key => Endpoint</para>
		/// <para>Value => Total number of times the specified endpoint has been called.</para>
		/// </summary>
		public static readonly Dictionary<string, int> EndpointRequestCount;

		static WordpressClient() => EndpointRequestCount = new Dictionary<string, int>();

		private HttpClient Client {
			get {
				if (_client == null) {
					_client = GenerateDisposableClient();
				}

				return _client;
			}
		}

		private HttpClientHandler ClientHandler {
			get {
				if (_clientHandler == null) {
					_clientHandler = GenerateDisposableClientHandler();
				}

				return _clientHandler;
			}
		}

		/// <summary>
		/// Default constructor to initialize <see cref="WordpressClient"/>
		/// <para><paramref name="baseUrl"/> should not be null or an invalid url.</para>
		/// </summary>
		/// <param name="baseUrl">The base url for Wordpress REST api endpoint. Example: http://demo.wp-api.org/wp-json/</param>
		/// <param name="path">REST API Path</param>
		/// <param name="maxConcurrentRequestsPerInstance">The maximum concurrent connections for this instance. Set as 0 to disable request limits</param>
		/// <param name="timeout">The timeout period. After this timeout is passed, the request will be considered as a failure and terminated with a error code.</param>
		public WordpressClient(string baseUrl, string path = "wp/v2", int maxConcurrentRequestsPerInstance = 10, int timeout = 60) {
			BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
			UrlPath = path ?? throw new ArgumentNullException(nameof(path));
			TIMEOUT = timeout;
			MAX_CONCURRENT_CONNECTION_PER_INSTANCE = maxConcurrentRequestsPerInstance;
			Threadsafe = maxConcurrentRequestsPerInstance > 0;
			RequestSync = Threadsafe ? new SemaphoreSlim(1, MAX_CONCURRENT_CONNECTION_PER_INSTANCE) : null;
			Cookies = new CookieContainer();

			if (!Uri.TryCreate(baseUrl, UriKind.RelativeOrAbsolute, out Uri _)) {
				throw new ArgumentException($"{nameof(baseUrl)} url is invalid.");
			}

			JsonSerializerSettings = new JsonSerializerSettings() {
				MissingMemberHandling = MissingMemberHandling.Ignore
			};
		}

		/// <summary>
		/// Override to use custom <see cref="HttpClientHandler"/>
		/// </summary>
		/// <returns><see cref="HttpClientHandler"/></returns>
		protected virtual HttpClientHandler GenerateDisposableClientHandler() => new HttpClientHandler() {
			AllowAutoRedirect = true,
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			CookieContainer = Cookies,
			MaxConnectionsPerServer = MAX_CONCURRENT_CONNECTION_PER_INSTANCE,
			UseCookies = true
		};

		/// <summary>
		/// Override to use custom <see cref="HttpClient"/>
		/// </summary>
		/// <returns><see cref="HttpClient"/></returns>
		protected virtual HttpClient GenerateDisposableClient() {
			HttpClient client = new HttpClient(ClientHandler, false) {
				BaseAddress = new Uri(BaseUrl),
				Timeout = TimeSpan.FromSeconds(TIMEOUT)
			};

			client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true, MaxAge = TimeSpan.FromSeconds(60) };
			return client;
		}

		/// <summary>
		/// Adds a Authorization header to all request from this <see cref="HttpClient"/> instance by using the specified authorization method.
		/// <para>Supports JWT authentication as well as BasicAuth (requires plugin)</para>
		/// </summary>
		/// <param name="loginDetails">Authorization configuration</param>
		/// <returns><see cref="WordpressClient"/></returns>
		public virtual async Task<WordpressClient> WithDefaultAuthorization(WordpressAuthorization loginDetails) {
			if (loginDetails.IsDefault) {
				return this;
			}

			if (!LoginDetails.IsDefault) {
				return this;
			}

			LoginDetails = loginDetails;

			if (loginDetails.AuthorizationType == AuthorizationType.Jwt && !await loginDetails.HandleJwtAuthentication(BaseUrl, Client).ConfigureAwait(false)) {
				return this;
			}

			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(loginDetails.Scheme, loginDetails.EncryptedAccessToken);
			return this;
		}

		/// <summary>
		/// Add a preprocessor for all responses received on this instance.
		/// <para>When a response is received internally, it first passes through the specified processor function.</para>
		/// <para>If the function returns true, the response is then parsed to the a valid response object, else it is disgarded with an error message.</para>
		/// </summary>
		/// <param name="processorFunc">The preprocessor function</param>
		/// <returns><see cref="WordpressClient"/></returns>
		public WordpressClient WithGlobalResponseProcessor(Func<string, bool> processorFunc) {
			GlobalResponsePreprocessorCallback = processorFunc;
			return this;
		}

		/// <summary>
		/// Adds a UserAgent header to all request from this <see cref="HttpClient"/> instance.
		/// </summary>
		/// <param name="userAgent">The UserAgent</param>
		/// <returns><see cref="WordpressClient"/></returns>
		public  WordpressClient WithDefaultUserAgent(string userAgent) {
			Client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
			return this;
		}

		/// <summary>
		/// Replace internal semaphore used for controlling all requests send from this instance.
		/// </summary>
		/// <param name="sync">The <see cref="SemaphoreSlim"/></param>
		/// <returns><see cref="WordpressClient"/></returns>
		public WordpressClient WithRequestSemaphoreSlim(SemaphoreSlim sync) {
			if (!Threadsafe) {
				return this;
			}

			RequestSync = sync ?? new SemaphoreSlim(1, MAX_CONCURRENT_CONNECTION_PER_INSTANCE);
			return this;
		}

		/// <summary>
		/// Replace internal serializer settings with the specified settings.
		/// </summary>
		/// <param name="settings">The settings</param>
		/// <returns></returns>
		public WordpressClient WithJsonSerializerSetting(JsonSerializerSettings settings) {
			JsonSerializerSettings = settings;
			return this;
		}

		/// <summary>
		/// Replace internal cookie container with the referenced cookie container
		/// </summary>
		/// <param name="container">The cookie container</param>
		/// <returns></returns>
		public WordpressClient WithCookieContainer(ref CookieContainer container) {
			Cookies = container ?? new CookieContainer();
			return this;
		}

		/// <summary>
		/// Replace internal cookie container with the specified cookie container
		/// </summary>
		/// <param name="container">The cookie container</param>
		/// <returns></returns>
		public WordpressClient WithCookieContainer(CookieContainer container) {
			Cookies = container ?? new CookieContainer();
			return this;
		}

		/// <summary>
		/// Adds default request headers to pass on all requests passed through this client
		/// </summary>
		/// <param name="pairs"></param>
		/// <returns></returns>
		public WordpressClient WithDefaultRequestHeaders(params KeyValuePair<string, string>[] pairs) {
			for (int i = 0; i < pairs.Length; i++) {
				Client.DefaultRequestHeaders.Add(pairs[i].Key, pairs[i].Value);
			}

			return this;
		}

		/// <summary>
		/// Register an event to receive endpoint statistics.
		/// </summary>
		/// <param name="statisticDelegate"></param>
		/// <returns></returns>
		public WordpressClient WithEndpointStatisticDelegate(Action<string, int> statisticDelegate) {
			EndpointRequestCountChangedCallback = statisticDelegate;
			return this;
		}

		/// <summary>
		/// Adds a function to be invoked to strip html tags and sanitize the received response content. such as title, body etc.
		/// </summary>
		/// <param name="cleanerDelegate">The delegate</param>
		/// <returns></returns>
		public WordpressClient WithHtmlResponseCleaner(Func<string, string> cleanerDelegate) {
			HtmlResponseCleanerCallback = cleanerDelegate;
			return this;
		}

		/// <summary>
		/// Gets all Categories formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<IEnumerable<Category>>> GetCategoriesAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("categories")));
			return await ExecuteAsync<IEnumerable<Category>>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all Posts formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<IEnumerable<Post>>> GetPostsAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("posts")));
			return await ExecuteAsync<IEnumerable<Post>>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all Users formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<IEnumerable<User>>> GetUsersAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("users")));
			return await ExecuteAsync<IEnumerable<User>>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all Comments formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<IEnumerable<Comment>>> GetCommentsAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("comments")));
			return await ExecuteAsync<IEnumerable<Comment>>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all Media formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<IEnumerable<Media>>> GetMediasAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("media")));
			return await ExecuteAsync<IEnumerable<Media>>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all Tags formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<IEnumerable<Tag>>> GetTagsAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("tags")));
			return await ExecuteAsync<IEnumerable<Tag>>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets Post with the specified postId formatted using the request builder.
		/// </summary>
		/// <param name="postId">The Post ID</param>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Post>> GetPostAsync(long postId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("posts", postId.ToString())));
			return await ExecuteAsync<Post>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets User with the specified userId formatted using the request builder.
		/// </summary>
		/// <param name="userId">The User ID</param>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<User>> GetUserAsync(int userId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("users", userId.ToString())));
			return await ExecuteAsync<User>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets Comment with the specified commentId formatted using the request builder.
		/// </summary>
		/// <param name="commentId">The Comment ID</param>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Comment>> GetCommentAsync(int commentId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("comments", commentId.ToString())));
			return await ExecuteAsync<Comment>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets Media with the specified mediaId formatted using the request builder.
		/// </summary>
		/// <param name="mediaId">The Media ID</param>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Media>> GetMediaAsync(int mediaId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("media", mediaId.ToString())));
			return await ExecuteAsync<Media>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets Tag with the specified tagId formatted using the request builder.
		/// </summary>
		/// <param name="tagId">The Tag ID</param>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Tag>> GetTagAsync(int tagId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("tags", tagId.ToString())));
			return await ExecuteAsync<Tag>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a Most formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Post>> CreatePostAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), "posts").WithHttpMethod(HttpMethod.Post));
			return await ExecuteAsync<Post>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a Media formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Media>> CreateMediaAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), "media").WithHttpMethod(HttpMethod.Post));
			return await ExecuteAsync<Media>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a Tag formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Tag>> CreateTagAsync(Func<RequestBuilder, Request> request){
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), "tags").WithHttpMethod(HttpMethod.Post));
			return await ExecuteAsync<Tag>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a Comment formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Comment>> CreateCommentAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), "comments").WithHttpMethod(HttpMethod.Post));
			return await ExecuteAsync<Comment>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a User formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<User>> CreateUserAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), "users").WithHttpMethod(HttpMethod.Post));
			return await ExecuteAsync<User>(requestContainer).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a Category formatted using the request builder.
		/// </summary>
		/// <param name="request">The request builder</param>
		/// <returns></returns>
		public virtual async Task<Response<Category>> CreateCategoryAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(RequestBuilder.WithBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), "categories").WithHttpMethod(HttpMethod.Post));
			return await ExecuteAsync<Category>(requestContainer).ConfigureAwait(false);
		}

		private static void EndpointStatistics(string requestEndpoint) {
			try {
				if (requestEndpoint.Contains('/')) {
					requestEndpoint = requestEndpoint.Split('/')[0];
				}

				if (!EndpointRequestCount.TryGetValue(requestEndpoint, out int value)) {
					EndpointRequestCount.TryAdd(requestEndpoint, 1);
				}
				else {
					EndpointRequestCount[requestEndpoint]++;
				}

				Task.Run(() => EndpointRequestCountChangedCallback?.Invoke(requestEndpoint, EndpointRequestCount[requestEndpoint]));
			}
			catch { }
		}

		/// <summary>
		/// Wrapper for internal execution functions.
		/// </summary>
		/// <param name="request">The Request object</param>
		/// <param name="cancellationToken">The Cancellation Token (if any)</param>
		/// <returns></returns>
		protected virtual Task<Response<T>> ExecuteAsync<T>(Request request, CancellationToken cancellationToken = default) where T : class {
			if (request == null) {
				return default;
			}

			if (request.RequestMethod == HttpMethod.Post) {
				return PostRequestAsync<T>(request, cancellationToken);
			}

			if (request.RequestMethod == HttpMethod.Put) {
				return PutRequestAsync<T>(request, cancellationToken);
			}

			if (request.RequestMethod == HttpMethod.Delete) {
				return DeleteRequestAsync<T>(request, cancellationToken);
			}

			return GetRequestAsync<T>(request, cancellationToken);
		}

		/// <summary>
		/// Internal PUT Request method. (Handles all PUT requests)
		/// </summary>
		/// <param name="request">The Request object</param>
		/// <param name="cancellationToken">The Cancellation Token (if any)</param>
		/// <returns></returns>
		protected virtual async Task<Response<T>> PutRequestAsync<T>(Request request, CancellationToken cancellationToken = default) where T : class {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Internal DELETE Request method. (Handles all DELETE requests)
		/// </summary>
		/// <param name="request">The Request object</param>
		/// <param name="cancellationToken">The Cancellation Token (if any)</param>
		/// <returns></returns>
		protected virtual async Task<Response<T>> DeleteRequestAsync<T>(Request request, CancellationToken cancellationToken = default) where T : class {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Internal POST Request method. (Handles all POST requests)
		/// </summary>
		/// <param name="request">The Request object</param>
		/// <param name="cancellationToken">The Cancellation Token (if any)</param>
		/// <returns></returns>
		protected virtual async Task<Response<T>> PostRequestAsync<T>(Request request, CancellationToken cancellationToken = default) {
			if (request == null || !request.IsRequestExecutable) {
				return default;
			}

			if (Threadsafe) {
				await RequestSync.WaitAsync().ConfigureAwait(false);
			}

			Response<T> responseContainer = new Response<T>();
			Stopwatch watch = new Stopwatch();

			try {
				using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, request.RequestUri)) {
					if (request.Token != default) {
						cancellationToken = request.Token;
					}

					if (cancellationToken == default) {
						cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT)).Token;
					}

					if (request.HasHeaders) {
						httpRequest.TryAddHeaders(request.Headers);
					}

					if (request.HasFormContent) {
						httpRequest.Content = request.FormBody;
					}

					if (request.ShouldAuthorize && !await Utilites.AuthorizeRequest(httpRequest, Client, BaseUrl, request.Authorization, request.Callback).ConfigureAwait(false)) {
						SetResponseContainerValues(ref watch, ref responseContainer, null);
						responseContainer.SetValue(default);
						responseContainer.SetMessage("Authorization failed.");
						return responseContainer;
					}

					watch.Start();
					using (HttpResponseMessage response = await Client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false)) {
						watch.Stop();

						string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(responseContent) || responseContent.Length <= 4) {
							SetResponseContainerValues(ref watch, ref responseContainer, response);
							responseContainer.SetMessage($"Request failed with ({(int) response.StatusCode}) [{response.StatusCode}] status.", "----------------------------", responseContent, "----------------------------");
							return responseContainer.SetValue(default);
						}

						await Task.Run(() => EndpointStatistics(request.Endpoint)).ConfigureAwait(false);

						if (GlobalResponsePreprocessorCallback != null && !GlobalResponsePreprocessorCallback.Invoke(responseContent)) {
							SetResponseContainerValues(ref watch, ref responseContainer, response);
							responseContainer.SetMessage($"Request aborted with ({(int) response.StatusCode}) [Globally defined validation restricted] status.", "----------------------------", responseContent, "----------------------------");
							return responseContainer.SetValue(default);
						}

						request.Callback?.ResponseCallback?.Invoke(responseContent);

						if (request.ShouldValidateResponse && !request.ValidationDelegate.Invoke(responseContent)) {
							SetResponseContainerValues(ref watch, ref responseContainer, response);
							responseContainer.SetMessage($"Request aborted with ({(int) response.StatusCode}) [User defined validation restricted] status.", "----------------------------", responseContent, "----------------------------");
							return responseContainer.SetValue(default);
						}

						SetResponseContainerValues(ref watch, ref responseContainer, response);
						responseContainer.SetMessage($"Request success with ({(int) response.StatusCode}) [{response.StatusCode}] status.", "----------------------------", responseContent, "----------------------------");
						return responseContainer.SetValue(JsonConvert.DeserializeObject<T>(responseContent, JsonSerializerSettings));
					}
				}
			}
			catch (OperationCanceledException oc) {
				request.Callback?.RequestCallback?.Invoke(new RequestStatus(false, "Operation cancelled. (passed timeout limit)"));
				SetResponseContainerValues(ref watch, ref responseContainer, null);
				responseContainer.SetMessage($"Request exceptioned occured. ({oc.HResult}) [Passed timeout limit]", "----------------------------", oc.Message, "----------------------------");
				return responseContainer.SetException(oc).SetValue(default);
			}
			catch (Exception e) {
				request.Callback?.UnhandledExceptionCallback?.Invoke(e);
				request.Callback?.RequestCallback?.Invoke(new RequestStatus(false, e.Message));
				SetResponseContainerValues(ref watch, ref responseContainer, null);
				responseContainer.SetMessage($"Request exceptioned occured. ({e.HResult}) [{e.Message}]", "----------------------------", e.Message, "----------------------------");
				return responseContainer.SetException(e).SetValue(default);
			}
			finally {
				if (Threadsafe) {
					RequestSync.Release();
				}
			}
		}

		/// <summary>
		/// Internal GET Request method. (Handles all GET requests)
		/// </summary>
		/// <param name="request">The Request object</param>
		/// <param name="cancellationToken">The Cancellation Token (if any)</param>
		/// <returns></returns>
		protected virtual async Task<Response<T>> GetRequestAsync<T>(Request request, CancellationToken cancellationToken = default) {
			if (request == null || !request.IsRequestExecutable) {
				return default;
			}

			if (Threadsafe) {
				await RequestSync.WaitAsync().ConfigureAwait(false);
			}

			Response<T> responseContainer = new Response<T>();
			Stopwatch watch = new Stopwatch();

			try {
				using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, request.RequestUri)) {
					if (request.Token != default) {
						cancellationToken = request.Token;
					}

					if (cancellationToken == default) {
						cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT)).Token;
					}

					watch.Start();
					using (HttpResponseMessage response = await Client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false)) {
						string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(responseContent) || responseContent.Length <= 4) {
							SetResponseContainerValues(ref watch, ref responseContainer, response);
							responseContainer.SetMessage($"Request failed with ({(int) response.StatusCode}) [{response.StatusCode}] status.", "----------------------------", responseContent, "----------------------------");
							return responseContainer.SetValue(default);
						}

						await Task.Run(() => EndpointStatistics(request.Endpoint)).ConfigureAwait(false);

						if (GlobalResponsePreprocessorCallback != null && !GlobalResponsePreprocessorCallback.Invoke(responseContent)) {
							SetResponseContainerValues(ref watch, ref responseContainer, response);
							responseContainer.SetMessage($"Request aborted with ({(int) response.StatusCode}) [Globally defined validation restricted] status.", "----------------------------", responseContent, "----------------------------");
							return responseContainer.SetValue(default);
						}

						request.Callback?.ResponseCallback?.Invoke(responseContent);

						if (request.ShouldValidateResponse && !request.ValidationDelegate.Invoke(responseContent)) {
							SetResponseContainerValues(ref watch, ref responseContainer, response);
							responseContainer.SetMessage($"Request aborted with ({(int) response.StatusCode}) [User defined validation restricted] status.", "----------------------------", responseContent, "----------------------------");
							return responseContainer.SetValue(default);
						}

						SetResponseContainerValues(ref watch, ref responseContainer, response);
						responseContainer.SetMessage($"Request success with ({(int) response.StatusCode}) [{response.StatusCode}] status.", "----------------------------", responseContent, "----------------------------");
						return responseContainer.SetValue(JsonConvert.DeserializeObject<T>(responseContent, JsonSerializerSettings));
					}
				}
			}
			catch (OperationCanceledException oc) {
				request.Callback?.RequestCallback?.Invoke(new RequestStatus(false, "Operation cancelled. (passed timeout limit)"));
				SetResponseContainerValues(ref watch, ref responseContainer, null);
				responseContainer.SetMessage($"Request exception occured. ({oc.HResult}) [Passed timeout limit]", "----------------------------", oc.Message, "----------------------------");
				return responseContainer.SetException(oc).SetValue(default);
			}
			catch (Exception e) {
				request.Callback?.UnhandledExceptionCallback?.Invoke(e);
				request.Callback?.RequestCallback?.Invoke(new RequestStatus(false, e.Message));
				SetResponseContainerValues(ref watch, ref responseContainer, null);
				responseContainer.SetMessage($"Request exception occured. ({e.HResult}) [{e.Message}]", "----------------------------", e.Message, "----------------------------");
				return responseContainer.SetException(e).SetValue(default);
			}
			finally {
				if (Threadsafe) {
					RequestSync.Release();
				}
			}
		}

		private static void SetResponseContainerValues<T>(ref Stopwatch watch, ref Response<T> responseContainer, HttpResponseMessage response) {
			if (watch == null || responseContainer == null) {
				return;
			}

			if (watch.IsRunning) {
				watch.Stop();
			}

			try {
				responseContainer.SetDuration(watch.Elapsed);
				Dictionary<string, string> headerCollection = new Dictionary<string, string>();

				if (response != null) {
					if (response.Headers != null && response.Headers.Count() > 0) {
						foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers) {
							string headerName = header.Key;
							string headerContent = string.Join(",", header.Value.ToArray());
							headerCollection.TryAdd(headerName, headerContent);
						}
					}

					responseContainer.SetStatus(response.IsSuccessStatusCode);
					responseContainer.SetStatusCode(response.StatusCode);
				}
				else {
					responseContainer.SetStatus(false);
					responseContainer.SetStatusCode(HttpStatusCode.Forbidden);
				}

				responseContainer.SetHeaders(headerCollection);
			}
			catch { return; }
		}

		/// <summary>
		/// Different types of authorization methods supported by this library.
		/// <para>More authorization methods will be supported in the future!</para>
		/// </summary>
		public enum AuthorizationType {

			/// <summary>
			/// Basic authorization by wordpress api team. (see <see href="https://github.com/WP-API/Basic-Auth"/>)
			/// </summary>
			Basic,

			/// <summary>
			/// JWT authorization by usefull team. (see <see href="https://github.com/usefulteam/jwt-auth"/>)
			/// </summary>
			Jwt,

			/// <summary>
			/// No authorization. (Default)
			/// </summary>
			NoAuth
		}
	}
}

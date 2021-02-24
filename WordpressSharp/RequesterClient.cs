using Newtonsoft.Json;
using PathanamthittaMedia.Library.Models.Requests;
using PathanamthittaMedia.Library.Models.Responses;
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

namespace PathanamthittaMedia.Library {
	public class RequesterClient {
		private readonly int TIMEOUT = 60;
		private readonly int MAX_CONCURRENT_CONNECTION_PER_INSTANCE = 10;
		private readonly bool Threadsafe = false;
		private readonly string UrlPath = "wp/v2/";
		private readonly string BaseUrl = "http://demo.wp-api.org/wp-json/";
		private HttpClient _client;
		private HttpClientHandler _clientHandler;
		private Authorization LoginDetails;
		private CookieContainer Cookies;
		private SemaphoreSlim RequestSync;
		private static Action<string, int> EndpointRequestCountDelegate;
		private static Func<string, bool> GlobalResponsePreprocessorFunc;
		public static readonly Dictionary<string, int> EndpointRequestCount;		

		static RequesterClient() => EndpointRequestCount = new Dictionary<string, int>();

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

		public RequesterClient(string baseUrl = "http://demo.wp-api.org/wp-json/", string path = "wp/v2", bool threadSafe = true, int maxConcurrentRequestsPerInstance = 10, int timeout = 60) {
			BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
			UrlPath = path ?? throw new ArgumentNullException(nameof(path));
			TIMEOUT = timeout;
			Threadsafe = threadSafe;
			MAX_CONCURRENT_CONNECTION_PER_INSTANCE = maxConcurrentRequestsPerInstance;
			RequestSync = Threadsafe ? new SemaphoreSlim(1, MAX_CONCURRENT_CONNECTION_PER_INSTANCE) : null;			
			Cookies = new CookieContainer();

			if(!Uri.TryCreate(baseUrl, UriKind.RelativeOrAbsolute, out Uri _)) {
				throw new ArgumentException($"{nameof(baseUrl)} url is invalid.");
			}
		}

		protected virtual HttpClientHandler GenerateDisposableClientHandler() => new HttpClientHandler() {
			AllowAutoRedirect = true,
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			CookieContainer = Cookies,
			MaxConnectionsPerServer = MAX_CONCURRENT_CONNECTION_PER_INSTANCE,
			UseCookies = true
		};

		protected virtual HttpClient GenerateDisposableClient() {
			HttpClient client = new HttpClient(ClientHandler, false) {
				BaseAddress = new Uri(BaseUrl),
				Timeout = TimeSpan.FromSeconds(TIMEOUT)
			};

			client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true, MaxAge = TimeSpan.FromSeconds(60) };
			return client;
		}

		public virtual async Task<RequesterClient> WithDefaultAuthorization(Authorization loginDetails) {
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

		public virtual RequesterClient WithGlobalResponseProcessor(Func<string, bool> processorFunc) {
			GlobalResponsePreprocessorFunc = processorFunc;
			return this;
		}

		public virtual RequesterClient WithDefaultUserAgent(string userAgent) {
			Client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
			return this;
		}

		public virtual RequesterClient WithRequestSemaphoreSlim(SemaphoreSlim sync) {
			if (!Threadsafe) {
				return this;
			}

			RequestSync = sync ?? new SemaphoreSlim(1, MAX_CONCURRENT_CONNECTION_PER_INSTANCE);
			return this;
		}

		public virtual RequesterClient WithCookieContainer(ref CookieContainer container) {
			Cookies = container ?? new CookieContainer();
			return this;
		}

		public virtual RequesterClient WithCookieContainer(CookieContainer container) {
			Cookies = container ?? new CookieContainer();
			return this;
		}

		public virtual RequesterClient WithDefaultRequestHeaders(params KeyValuePair<string, string>[] pairs) {
			for (int i = 0; i < pairs.Length; i++) {
				Client.DefaultRequestHeaders.Add(pairs[i].Key, pairs[i].Value);
			}

			return this;
		}

		public virtual RequesterClient WithEndpointStatisticDelegate(Action<string, int> statisticDelegate) {
			EndpointRequestCountDelegate = statisticDelegate;
			return this;
		}

		public virtual async IAsyncEnumerable<Response<Category>> GetCategoriesAsync(Func<RequestBuilder, Request> request, IProgress<int> progressReport = default) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("categories")));
			Response<Category[]> result = await ExecuteAsync<Category[]>(requestContainer).ConfigureAwait(false);

			if (!result.Status) {
				yield break;
			}

			for (int i = 0; i < result.Value.Length; i++) {
				progressReport?.Report(i);
				yield return Response.CloneFrom<Category>(result.Value[i], result);
			}
		}

		public virtual async IAsyncEnumerable<Response<Post>> GetPostsAsync(Func<RequestBuilder, Request> request, IProgress<double> progressReport = default) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("posts")));
			Response<Post[]> result = await ExecuteAsync<Post[]>(requestContainer).ConfigureAwait(false);

			if (!result.Status) {
				yield break;
			}

			for (int i = 0; i < result.Value.Length; i++) {
				if (progressReport != null) {
					progressReport.Report(CalculateProgress(i, result.Value.Length));
				}

				yield return Response.CloneFrom<Post>(result.Value[i], result);
			}
		}

		public virtual async Task<Response<Post>> GetPostAsync(long postId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("posts", postId.ToString())));
			return await ExecuteAsync<Post>(requestContainer).ConfigureAwait(false);
		}

		public virtual async Task<Response<Post>> CreatePostAsync(Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), "posts"));
			return await ExecuteAsync<Post>(requestContainer).ConfigureAwait(false);
		}

		public virtual async IAsyncEnumerable<Response<User>> GetUsersAsync(Func<RequestBuilder, Request> request, IProgress<double> progressReport = default) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("users")));
			Response<User[]> result = await ExecuteAsync<User[]>(requestContainer).ConfigureAwait(false);

			if (!result.Status) {
				yield break;
			}

			for (int i = 0; i < result.Value.Length; i++) {
				if (progressReport != null) {
					progressReport.Report(CalculateProgress(i, result.Value.Length));
				}

				yield return Response.CloneFrom<User>(result.Value[i], result);
			}
		}

		public virtual async Task<Response<User>> GetUserAsync(int userId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("users", userId.ToString())));
			return await ExecuteAsync<User>(requestContainer).ConfigureAwait(false);
		}

		public virtual async IAsyncEnumerable<Response<Comment>> GetCommentsAsync(Func<RequestBuilder, Request> request, IProgress<double> progressReport = default) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("comments")));
			Response<Comment[]> result = await ExecuteAsync<Comment[]>(requestContainer).ConfigureAwait(false);

			if (!result.Status) {
				yield break;
			}

			for (int i = 0; i < result.Value.Length; i++) {
				if (progressReport != null) {
					progressReport.Report(CalculateProgress(i, result.Value.Length));
				}

				yield return Response.CloneFrom<Comment>(result.Value[i], result);
			}
		}

		public virtual async Task<Response<Comment>> GetCommentAsync(int commentId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("comments", commentId.ToString())));
			return await ExecuteAsync<Comment>(requestContainer).ConfigureAwait(false);
		}

		public virtual async IAsyncEnumerable<Response<Media>> GetMediasAsync(Func<RequestBuilder, Request> request, IProgress<double> progressReport = default) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("media")));
			Response<Media[]> result = await ExecuteAsync<Media[]>(requestContainer).ConfigureAwait(false);

			if (!result.Status) {
				yield break;
			}

			for (int i = 0; i < result.Value.Length; i++) {
				if(progressReport != null) {
					progressReport.Report(CalculateProgress(i, result.Value.Length));
				}

				yield return Response.CloneFrom<Media>(result.Value[i], result);
			}
		}

		public virtual async Task<Response<Media>> GetMediaAsync(int mediaId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("media", mediaId.ToString())));
			return await ExecuteAsync<Media>(requestContainer).ConfigureAwait(false);
		}

		public virtual async IAsyncEnumerable<Response<Tag>> GetTagsAsync(Func<RequestBuilder, Request> request, IProgress<double> progressReport = default) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("tags")));
			Response<Tag[]> result = await ExecuteAsync<Tag[]>(requestContainer).ConfigureAwait(false);

			if (!result.Status) {
				yield break;
			}

			for (int i = 0; i < result.Value.Length; i++) {
				if (progressReport != null) {
					progressReport.Report(CalculateProgress(i, result.Value.Length));
				}

				yield return Response.CloneFrom<Tag>(result.Value[i], result);
			}
		}

		public virtual async Task<Response<Tag>> GetTagAsync(int tagId, Func<RequestBuilder, Request> request) {
			Request requestContainer = request.Invoke(new RequestBuilder().WithBaseAndEndpoint(Path.Combine(BaseUrl, UrlPath), Path.Combine("tags", tagId.ToString())));
			return await ExecuteAsync<Tag>(requestContainer).ConfigureAwait(false);
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

				Task.Run(() => EndpointRequestCountDelegate?.Invoke(requestEndpoint, EndpointRequestCount[requestEndpoint]));
			}
			catch { }
		}

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

		protected virtual async Task<Response<T>> PutRequestAsync<T>(Request request, CancellationToken cancellationToken = default) where T : class {
			throw new NotImplementedException();
		}

		protected virtual async Task<Response<T>> DeleteRequestAsync<T>(Request request, CancellationToken cancellationToken = default) where T : class {
			throw new NotImplementedException();
		}

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
						httpRequest.Content = new FormUrlEncodedContent(request.FormBody);
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

						if(GlobalResponsePreprocessorFunc != null && !GlobalResponsePreprocessorFunc.Invoke(responseContent)) {
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
						return responseContainer.SetValue(JsonConvert.DeserializeObject<T>(responseContent));
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

						if (GlobalResponsePreprocessorFunc != null && !GlobalResponsePreprocessorFunc.Invoke(responseContent)) {
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

						return responseContainer.SetValue(JsonConvert.DeserializeObject<T>(responseContent));
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

		protected virtual void SetResponseContainerValues<T>(ref Stopwatch watch, ref Response<T> responseContainer, HttpResponseMessage response) {
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

		/*
		 * P% = 12 ÷ 40			
		 */
		private static double CalculateProgress(int currentProgress, int total) => (currentProgress / total) * 100;

		public enum AuthorizationType {
			Basic,
			Jwt,
			NoAuth
		}
	}
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WordpressCore.Models.Requests;
using WordpressCore.Models.Responses;
using static WordpressCore.Models.Requests.Enums;

namespace WordpressCore.Demo {
	internal class Program {
		private static async Task<int> Main(string[] args) {
			CookieContainer container = new CookieContainer();
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			WordpressClient client = new WordpressClient("http://demo.wp-api.org/wp-json/", maxConcurrentRequestsPerInstance: 8, timeout: 60)

			// add default user agent
			.WithDefaultUserAgent("SampleUserAgent")

			// use pre configured cookie container
			.WithCookieContainer(ref container)

			// pass custom json serializer settings if required
			.WithJsonSerializerSetting(new JsonSerializerSettings() {
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore
			})

			// pre process responses received from the api (can be used for custom validation logic etc)
			.WithGlobalResponseProcessor((responseReceived) => {
				if (string.IsNullOrEmpty(responseReceived)) {
					return false;
				}

				// keep in mind that, returning true here completes the request by deserilizing internally, and then returning the response object.
				// returning false will terminate the request and returns a Response object with error status to the caller.
				return true;
			})

			// add default request headers
			.WithDefaultRequestHeaders(new KeyValuePair<string, string>("X-Client", "Mobile"), // allows to add custom headers for requests send from this instance
									   new KeyValuePair<string, string>("X-Version", "1.0"));

			Response <Post> post = await client.CreatePostAsync((builder) => builder
			.WithHttpBody<PostBuilder, HttpContent>((post) => post
				.WithCategories(10, 31, 44)
				.WithCommentStatus(CommentStatusValue.Open)
				.WithContent("This is post content!")
				.WithExcerpt("This is an Excerpt!")
				.WithFormat(PostFormat.Standard)
				.WithPassword("super_secure_encrypted_password")
				.WithPingStatus(PingStatusValue.Open)
				.WithSlug("post-slug")
				.WithStatus(PostStatus.Publish)
				.WithTags(0, 23, 42)
				.WithFeaturedImage(7831)
				.WithTitle("This is a post title!")
				.Create())
			.CreateWithCallback(new Callback(OnException, OnResponseReceived, OnRequestStatus)));

			if (!post.Status) {
				// Request failed
				Console.WriteLine(post.Message);
				return -1;
			}

			Console.WriteLine("Post created: " + post.Value.Id);
			Console.ReadKey();
			return 0;
		}

		private static void OnRequestStatus(RequestStatus obj) {
			// handle request status externally
		}

		private static void OnResponseReceived(string obj) {
			// handle the raw json response
		}

		private static void OnException(Exception obj) {
			// handle exceptions occured internally during request process
		}
	}
}

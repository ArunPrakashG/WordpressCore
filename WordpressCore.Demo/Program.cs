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
			WordpressClient client = new WordpressClient("base url", maxConcurrentRequestsPerInstance: 8, timeout: 60)
			.WithDefaultUserAgent("WordpressClient")
			.WithCookieContainer(ref container)			
			.WithJsonSerializerSetting(new JsonSerializerSettings() {
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			await client.WithDefaultAuthorization(new WordpressAuthorization("tst", "test", type: WordpressClient.AuthorizationType.Jwt)).ConfigureAwait(false);

			Response <Post> post = await client.CreatePostAsync((builder) => builder
			.WithHttpBody<PostBuilder, HttpContent>((post) => post
				.WithCategories(10, 31, 44)
				.WithCommentStatus(CommentStatusValue.Open)
				.WithContent("This is post content!")
				.WithExcerpt("This is an Excerpt!")
				.WithFormat(PostFormat.Standard)
				.WithPingStatus(PingStatusValue.Open)
				.WithSlug("post-slug")
				.WithStatus(PostStatus.Draft)
				.WithTags(0, 23, 42)
				.WithFeaturedImage(client, "585e4da1cb11b227491c339c.png").GetAwaiter().GetResult()
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

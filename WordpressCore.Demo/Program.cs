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
using static WordpressCore.Models.Requests.PopularPostsBuilder;

namespace WordpressCore.Demo {
	internal class Program {
		private static async Task<int> Main(string[] args) {
			CookieContainer container = new CookieContainer();
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			WordpressClient client = new WordpressClient("test", maxConcurrentRequestsPerInstance: 8, timeout: 60)			
			.WithCookieContainer(ref container)			
			.WithJsonSerializerSetting(new JsonSerializerSettings() {
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			Response<IEnumerable<Post>> posts = await client.GetPopularPostsAsync((builder) => builder
				.WithPopularPostsBody((pop) => pop
					.WithLimit(50)
					.WithPopularPostsOrder(OrderPopularPostsBy.Views)
					.WithRange(TimeRange.Last24Hours)
					.Create())
				.WithCancellationToken(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token)
				.CreateWithCallback(new Callback(OnException, OnResponseReceived, OnRequestStatus)));

			if (!posts.Status) {
				// Request failed
				Console.WriteLine(posts.Message);
				return -1;
			}

			foreach(var post in posts.Value) {
				Console.WriteLine(post.Id);
			}

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

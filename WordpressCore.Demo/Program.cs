using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
			WordpressClient client = new WordpressClient("https://www.example.com/wp-json/", maxConcurrentRequestsPerInstance: 8, timeout: 60)			
			.WithCookieContainer(ref container)			
			.WithJsonSerializerSetting(new JsonSerializerSettings() {
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			var collection = new List<Post>();

			Response<IEnumerable<Post>> posts = await client.GetPopularPostsAsync((builder) => builder
				.WithPopularPostsQuery((pop) => pop
					.WithLimit(20)
					.WithOffset(collection.Count)
					.WithPopularPostsOrder(OrderPopularPostsBy.Views)
					.WithRange(TimeRange.Custom)
					.WithTimeQuantity(64)
					.WithUnit(TimeUnit.Day)
					.Create())
				.CreateWithCallback(new Callback(OnException, OnResponseReceived, OnRequestStatus)));

			if (!posts.Status) {
				// Request failed
				Console.WriteLine(posts.Message);
				return -1;
			}

			foreach(var post in posts.Value) {
				collection.Add(post);
				Console.WriteLine(post.Id);
			}

			Console.WriteLine("--------------------------------------");
			posts = await client.GetPopularPostsAsync((builder) => builder
				.WithPopularPostsQuery((pop) => pop
					.WithLimit(20)
					.WithOffset(collection.Count)
					.WithPopularPostsOrder(OrderPopularPostsBy.Views)
					.WithRange(TimeRange.Custom)
					.WithTimeQuantity(64)
					.WithUnit(TimeUnit.Day)
					.Create())
				.CreateWithCallback(new Callback(OnException, OnResponseReceived, OnRequestStatus)));

			if (!posts.Status) {
				// Request failed
				Console.WriteLine(posts.Message);
				return -1;
			}

			foreach (var post in posts.Value) {
				if(IsRepeat(collection, post.Id)) {
					Console.WriteLine($"{post.Id} is repeat");
					continue;
				}

				collection.Add(post);
				Console.WriteLine(post.Id);
			}

			Console.ReadKey();
			return 0;
		}

		private static bool IsRepeat(List<Post> posts, int id) {
			return posts.Where(x => x.Id == id).Any();
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

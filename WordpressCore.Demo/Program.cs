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
			})
			.WithGlobalResponseProcessor((response) => {
				Console.WriteLine(response);
				return true;
			});

			WordpressAuthorization auth = new("test", "test", WordpressClient.AuthorizationType.Jwt);
			Console.WriteLine(await auth.IsLoggedInAsync(client).ConfigureAwait(false));

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

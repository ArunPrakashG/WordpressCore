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
			.WithActivityCallback((type) => {
				Console.WriteLine(type);
				return Task.CompletedTask;
			})
			.WithJsonSerializerSetting(new JsonSerializerSettings() {
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore
			})
			.WithGlobalResponseProcessor((response) => {
				Console.WriteLine(response);
				return true;
			});

			redo:
			WordpressAuthorization auth = new WordpressAuthorization("username", "password", WordpressClient.AuthorizationType.Jwt);

			if(await client.IsLoggedInAsync()) {
				Console.WriteLine("Already logged in.");				
			}
			else {
				if(await client.LoginAsync(auth).ConfigureAwait(false)) {
					Console.WriteLine("Logged in now");
				}
			}

			var currentUser = await client.GetCurrentUserAsync((builder) => builder.Create());
			Console.WriteLine(currentUser.Message);

			Console.ReadKey();
			goto redo;
			
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

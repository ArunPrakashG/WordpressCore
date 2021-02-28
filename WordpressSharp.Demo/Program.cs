using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WordpressSharp.Models.Responses;
using static WordpressSharp.Models.Requests.Enums;

namespace WordpressSharp.Demo {
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

			// create a Posts request
			Response<IEnumerable<Post>> posts = await client.GetPostsAsync((request) => request.OrderResultBy(Order.Ascending)
				// only get posts with published status
				.SetAllowedStatus(Status.Published)

				// specifys the response should contain embed field
				.SetEmbeded(true)

				// set scope of the request, default is view, set scope as edit for edit requests
				.SetScope(Scope.View)

				// set allowed categories of post. only posts in these categories will be in response. should be category id.
				.AllowCategories(51, 32)

				// set allowed authors of post. only posts by these authors will be in response. should be author id.
				.AllowAuthors(47, 32, 13, 53)

				// adds a cancellation token to the request, allowing to cancel the request anytime as needed
				.WithCancellationToken(cancellationTokenSource.Token)

				// sets the maximum number of posts in a single page
				.WithPerPage(20)

				// gets the first page of posts containg 20 posts, specifying 2 here will get next page. used for pagenation	
				.WithPageNumber(1)

				// adds request specific authorization. can be BasicAuth or Jwt Authentication methods. Use plugin for Jwt
				.WithAuthorization(new WordpressAuthorization("username", "password", type: WordpressClient.AuthorizationType.Jwt))

				// specifiys a response validator/processor for current request
				.WithResponseValidationOverride((response) => {
					if (string.IsNullOrEmpty(response)) {
						return false;
					}

					// returning true completes the request by returning deserialized response
					// returning false terminates the request with an error message
					return true;
				})

				// Should be called at the end of the builder to build the request as a Request object
				// pass a Callback container to get events on internal activity for this request
				.CreateWithCallback(new Callback(OnException, OnResponseReceived, OnRequestStatus))).ConfigureAwait(false);

			if (!posts.Status) {
				// Request failed
				Console.WriteLine(posts.Message);
				return -1;
			}

			foreach (Post post in posts.Value) {
				// do yer magic!
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

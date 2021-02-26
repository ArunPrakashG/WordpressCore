# WordpressSharp
Library to interact with Wordpress REST API in a fluent pattern.

## Supported Platforms
* .net standard 2.1
* .net 5
* .net core 3.0
* .net core 3.1

## NOTE
This library was developed for one of my client projects to fix many limitations of existing libraries.
Library is not yet complete by any means. I have only implemented basic requests and structure. i hope to complete this soon as my time allows me.

## Example
***You can checkout full sample [Here](WordpressSharp.Demo/Program.cs)***

```cs
CookieContainer container = new CookieContainer();
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			WordpressClient client = new WordpressClient("http://demo.wp-api.org/wp-json/", threadSafe: true, maxConcurrentRequestsPerInstance: 8, timeout: 60)
				.WithDefaultUserAgent("SampleUserAgent")
				.WithCookieContainer(ref container)
				.WithGlobalResponseProcessor((responseReceived) => {
					if (string.IsNullOrEmpty(responseReceived)) {
						return false;
					}

					// Specifys a global response processor / validator
					// Or deserilize this response here, apply your own logic etc
					// keep in mind that, returning true here completes the request by deserilizing internally, and then returning the response object.
					// returning false will terminate the request and returns a Response object with error status to the caller.
					return true;
				})
				.WithDefaultRequestHeaders(new KeyValuePair<string, string>("X-Client", "Mobile"), // allows to add custom headers for requests send from this instance
										   new KeyValuePair<string, string>("X-Version", "1.0"));

			// enumerate through each index of the returned response array
			await foreach (Response<Post> post in client.GetPostsAsync((request) => request.OrderResultBy(Order.Ascending)
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

				// adds a cancelleation token to the request, allowing to cancel the request anytime as needed
				.WithCancelleationToken(cancellationTokenSource.Token)

				// sets the maximum number of posts in a single page
				.WithPerPage(20)

				// gets the first page of posts containg 20 posts, specifying 2 here will get next page. used for pagenation	
				.WithPageNumber(1)

				// adds request specific authorization. can be BasicAuth or Jwt Authentication methods. Use plugin for BasicAuth
				.WithAuthorization(new WordpressAuthorization("username", "password", type: WordpressClient.AuthorizationType.Jwt))

				// specifiys a response validator/processor for current request
				.WithResponseValidationOverride((response) => {
					if (string.IsNullOrEmpty(response)) {
						return false;
					}
					
					// Or deserilize this response here, apply your own logic etc
					// keep in mind that, returning true here completes the request by deserilizing internally, and then returning the response object.
					// returning false will terminate the request and returns a Response object with error status to the caller.
					return true;
				})

				// Should be called at the end of the builder to build the request as a Request object
				// you can also pass a method to handle Progress of the request. IProgress<double>
				.CreateWithCallback(new Callback(OnException, OnResponseReceived, OnRequestStatus)), new Progress<double>(HandleProgressUpdates))) {

			}
```

## Goals
* Implement rest of the endpoints of REST API
* Reduce memory consumption further internally
* Replace `IAsyncEnumerable` with an alternative to support more platforms

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WordpressSharp.Models.Responses;
using static WordpressSharp.Models.Requests.RequestBuilder;

namespace WordpressSharp.Demo {
	internal class Program {
		static async Task<int> Main(string[] args) {
			WordpressClient client = new WordpressClient("test", threadSafe: false, maxConcurrentRequestsPerInstance: 10, timeout: 60);
			Response<IEnumerable<Category>> result = await client._GetCategoriesAsync((requestBuilder) => requestBuilder.WithPerPage(100).Create()).ConfigureAwait(false);

			if (!result.Status) {
				Console.WriteLine("Error occured.");
				Console.WriteLine(result.Message);
			}
			else {
				foreach (var category in result.Value) {
					Console.WriteLine(category.Slug);
				}
			}
			
			Console.ReadKey();
			return 0;
		}
	}
}

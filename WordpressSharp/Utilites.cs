using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordpressSharp
{
	internal static class Utilites {
		internal static string AppendUrlQueryParameter(this string url, string key, string val, bool hasQueryParameter) {
			if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(val)) {
				return string.Empty;
			}

			if (hasQueryParameter) {
				return url += $"&{key}={val}";
			}

			return url += $"?{key}={val}";
		}

		internal static string CleanContent(this string input) {
			var sanitizer = new HtmlSanitizer();
			sanitizer.KeepChildNodes = false;
			return sanitizer.Sanitize(input);
			//Regex.Replace(input, "<.*?>", String.Empty).Replace("&#8211;", "—").Replace("&nbsp;", " ");
		}

		public static string Base64Encode(string plainText) {
			if (string.IsNullOrEmpty(plainText)) {
				return string.Empty;
			}

			return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
		}

		public static HttpRequestMessage TryAddHeaders(this HttpRequestMessage request, Dictionary<string,string> headers) {
			if(headers == null || headers.Count <= 0) {
				return request;
			}

			try {
				foreach (var val in headers) {
					request.Headers.TryAddWithoutValidation(val.Key, val.Value);
				}

				return request;
			}
			catch {
				return request;
			}			
		}

		public static async Task<bool> AuthorizeRequest(HttpRequestMessage request, HttpClient client, string baseUrl, Authorization auth, Callback callback = null) {
			if (auth.IsDefault || string.IsNullOrEmpty(baseUrl)) {
				return false;
			}

			if(auth.AuthorizationType == RequesterClient.AuthorizationType.Jwt) {
				bool isTokenReceived = await auth.HandleJwtAuthentication(baseUrl, client, callback).ConfigureAwait(false);

				if (!isTokenReceived) {
					return false;
				}
			}

			request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, auth.EncryptedAccessToken);
			return true;
		}
	}
}

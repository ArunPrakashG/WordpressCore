using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WordpressSharp.Models.Responses.JWT;
using static WordpressSharp.WordpressClient;

namespace WordpressSharp {
	public class WordpressAuthorization {
		public static WordpressAuthorization Default => new WordpressAuthorization(string.Empty, string.Empty, type: AuthorizationType.NoAuth);
		public bool IsDefault => string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password);
		internal readonly string UserName;
		internal readonly string Password;
		internal readonly string JwtToken;
		internal readonly AuthorizationType AuthorizationType;
		internal readonly string Scheme;
		internal string EncryptedAccessToken;
		private bool HasValidatedOnce;

		public WordpressAuthorization(string userName, string passWord, string jwtToken = null, AuthorizationType type = AuthorizationType.NoAuth) {
			UserName = userName;
			Password = passWord;
			JwtToken = jwtToken;
			AuthorizationType = type;
			Scheme = string.Empty;
			EncryptedAccessToken = string.Empty;
			HasValidatedOnce = false;

			if (!IsDefault) {
				switch (type) {
					case AuthorizationType.Basic:
						Scheme = "Basic";
						EncryptedAccessToken = Utilites.Base64Encode($"{UserName}:{Password}");
						break;
					case AuthorizationType.Jwt:
						Scheme = "Bearer";
						EncryptedAccessToken = jwtToken;
						break;
				}
			}
		}

		internal async Task<bool> HandleJwtAuthentication(string baseUrl, HttpClient client, Callback callback = null) {
			if (AuthorizationType != AuthorizationType.Jwt || client == null || string.IsNullOrEmpty(baseUrl)) {
				return false;
			}

			if (HasValidatedOnce && !string.IsNullOrEmpty(EncryptedAccessToken)) {
				return true;
			}

			if (!string.IsNullOrEmpty(EncryptedAccessToken) && await ValidateExistingToken(baseUrl, client).ConfigureAwait(false)) {
				return true;
			}

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Path.Combine(baseUrl, "jwt-auth/v1/token"))) {
				request.Content = new FormUrlEncodedContent(new[] {
					new KeyValuePair<string, string>("username", UserName),
					new KeyValuePair<string, string>("password", Password)
				});

				try {
					using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false)) {
						if (!response.IsSuccessStatusCode) {
							return false;
						}

						Token token = JsonConvert.DeserializeObject<Token>(await response.Content.ReadAsStringAsync());
						EncryptedAccessToken = token.Container.Token;
						return true;
					}
				}
				catch (Exception e) {
					callback?.UnhandledExceptionCallback?.Invoke(e);
					return false;
				}
			}
		}

		private async Task<bool> ValidateExistingToken(string baseUrl, HttpClient client) {
			if (AuthorizationType != AuthorizationType.Jwt || client == null || string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(EncryptedAccessToken)) {
				return false;
			}

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Path.Combine(baseUrl, "jwt-auth/v1/token/validate"))) {
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", EncryptedAccessToken);

				try {
					using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false)) {
						if (!response.IsSuccessStatusCode) {
							return false;
						}

						Validate validation = JsonConvert.DeserializeObject<Validate>(await response.Content.ReadAsStringAsync());
						HasValidatedOnce = validation.IsSuccess;
						return validation.IsSuccess;
					}
				}
				catch {
					return false;
				}
			}
		}
	}
}

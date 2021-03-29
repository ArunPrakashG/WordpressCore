using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WordpressCore.Models.Responses.JWT;
using static WordpressCore.WordpressClient;

namespace WordpressCore {
	/// <summary>
	/// Class which handles authorization system
	/// <para>Inheritable and overridable to support custom system of authorization.</para>
	/// </summary>
	public class WordpressAuthorization {
		internal bool IsDefault => string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password);

		internal static WordpressAuthorization Default => new(string.Empty, string.Empty, type: AuthorizationType.NoAuth);		
		internal readonly string UserName;
		internal readonly string Password;
		internal readonly string JwtToken;
		internal readonly AuthorizationType AuthorizationType;
		internal readonly string Scheme;
		internal string EncryptedAccessToken;
		private bool HasValidatedOnce;

		/// <summary>
		/// Returns true if user access token is fetched.
		/// </summary>
		public bool IsValidAuth => !string.IsNullOrEmpty(EncryptedAccessToken);

		/// <summary>
		/// Instantiate an authorization handler for requests.
		/// </summary>
		/// <param name="userName">The user name</param>
		/// <param name="passWord">The password</param>
		/// <param name="jwtToken">The JWT Token if it is already stored within the calling context. (will skip requesting it)</param>
		/// <param name="type">The type of authorization method to use.</param>
		public WordpressAuthorization(string userName, string passWord, AuthorizationType type = AuthorizationType.Basic, string jwtToken = null) {
			UserName = userName ?? throw new ArgumentNullException(nameof(userName));
			Password = passWord ?? throw new ArgumentNullException(nameof(passWord));
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

		/// <summary>
		/// Checks if current user is logged in.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public async Task<bool> IsLoggedInAsync(WordpressClient client, Callback callback = null) {
			var currentUser = await client.GetCurrentUserAsync((builder) => builder
				.WithAuthorization(this)
				.CreateWithCallback(callback));

			return currentUser != null && currentUser.Status;
		}

		/// <summary>
		/// Handles JWT Authentication with the Username and Password supplied while creating this instance.
		/// </summary>
		/// <param name="baseUrl"></param>
		/// <param name="client"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		internal virtual async Task<bool> HandleJwtAuthentication(string baseUrl, HttpClient client, Callback callback = null) {
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
						EncryptedAccessToken = token.TokenValue;
						return true;
					}
				}
				catch (Exception e) {
					callback?.UnhandledExceptionCallback?.Invoke(e);
					return false;
				}
			}
		}

		/// <summary>
		/// Handles validation of an existing JWT Token.
		/// </summary>
		/// <param name="baseUrl">The base url of the wordpress site</param>
		/// <param name="client">The HttpClient to use</param>
		/// <returns></returns>
		protected virtual async Task<bool> ValidateExistingToken(string baseUrl, HttpClient client) {
			if (AuthorizationType != AuthorizationType.Jwt || client == null || string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(EncryptedAccessToken)) {
				return false;
			}

			using (HttpRequestMessage request = new(HttpMethod.Post, Path.Combine(baseUrl, "jwt-auth/v1/token/validate"))) {
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", EncryptedAccessToken);

				try {
					using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false)) {
						if (!response.IsSuccessStatusCode) {
							return false;
						}

						Validate validation = JsonConvert.DeserializeObject<Validate>(await response.Content.ReadAsStringAsync());

						if (validation == null || string.IsNullOrEmpty(validation.JwtStatusCode)) {
							return false;
						}

						HasValidatedOnce = validation.JwtStatusCode.Equals("jwt_auth_valid_token");
						return validation.JwtStatusCode.Equals("jwt_auth_valid_token");
					}
				}
				catch {
					return false;
				}
			}
		}

		/// <summary>
		/// Validate existing JWT Token
		/// </summary>
		/// <param name="baseUrl"></param>
		/// <param name="accessToken"></param>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task<bool> ValidateJwtToken(string baseUrl, string accessToken, WordpressClient client) {
			if(string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(accessToken) || client == null) {
				return false;
			}

			using (HttpRequestMessage request = new(HttpMethod.Post, Path.Combine(baseUrl, "jwt-auth/v1/token/validate"))) {
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				try {
					using (HttpResponseMessage response = await client.Client.SendAsync(request).ConfigureAwait(false)) {
						if (!response.IsSuccessStatusCode) {
							return false;
						}

						Validate validation = JsonConvert.DeserializeObject<Validate>(await response.Content.ReadAsStringAsync());
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

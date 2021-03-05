using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using WordpressCore.Interfaces;

namespace WordpressCore.Models.Requests {
	/// <summary>
	/// Builder used to build CreateUser request
	/// </summary>
	public class UserBuilder : QueryBuilder<UserBuilder>, IRequestBuilder<UserBuilder, HttpContent> {
		private string UserName;
		private string Name;
		private string FirstName;
		private string LastName;
		private string Email;
		private string Url;
		private string Description;
		private string Locale;
		private string NickName;
		private string Slug;
		private string[] Roles;
		private string Password;

		/// <summary>
		/// Constructor
		/// </summary>
		public UserBuilder() { }

		/// <summary>
		/// Sets the Username (Login Name) for the user. (REQUIRED)
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>
		public UserBuilder WithUserName(string userName) {
			UserName = userName ?? throw new ArgumentNullException($"{nameof(userName)} can't be an empty value.");
			return this;
		}

		/// <summary>
		/// Sets the User Name for the user.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public UserBuilder WithName(string name) {
			Name = name;
			return this;
		}

		/// <summary>
		/// Sets the First Name of the user.
		/// </summary>
		/// <param name="firstName"></param>
		/// <returns></returns>
		public UserBuilder WithFirstName(string firstName) {
			FirstName = firstName;
			return this;
		}

		/// <summary>
		/// Sets the Last Name of the user.
		/// </summary>
		/// <param name="lastName"></param>
		/// <returns></returns>
		public UserBuilder WithLastName(string lastName) {
			LastName = lastName;
			return this;
		}

		/// <summary>
		/// Sets the Email associated with this user account. (REQUIRED)
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public UserBuilder WithEmail(string email) {
			Email = email ?? throw new ArgumentNullException($"{nameof(email)} can't be an empty value.");			
			return this;
		}

		/// <summary>
		/// Sets the Url of the user.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public UserBuilder WithUrl(string url) {
			Url = url;
			return this;
		}

		/// <summary>
		/// Sets the Description of the user.
		/// </summary>
		/// <param name="description"></param>
		/// <returns></returns>
		public UserBuilder WithDescription(string description) {
			Description = description;
			return this;
		}

		/// <summary>
		/// Sets the Locale of the user.
		/// </summary>
		/// <param name="locale"></param>
		/// <returns></returns>
		public UserBuilder WithLocale(string locale = "en_US") {
			Locale = locale;
			return this;
		}

		/// <summary>
		/// Sets the Nickname of the user.
		/// </summary>
		/// <param name="nickName"></param>
		/// <returns></returns>
		public UserBuilder WithNickName(string nickName) {
			NickName = nickName;
			return this;
		}

		/// <summary>
		/// Sets the Slug of the user. Should only contain Alphanumeric charecters. (a-Z, 0-9)
		/// </summary>
		/// <param name="slug"></param>
		/// <returns></returns>
		public UserBuilder WithSlug(string slug) {
			if (!Utilites.IsAlphanumeric(slug)) {
				throw new ArgumentException($"{nameof(slug)} can only contain alphanumeric charecters. (a-Z, 0-9)");
			}

			Slug = slug;
			return this;
		}

		/// <summary>
		/// Sets the Roles associated with this user.
		/// </summary>
		/// <param name="roles"></param>
		/// <returns></returns>
		public UserBuilder WithRoles(params string[] roles) {
			Roles = roles;
			return this;
		}

		/// <summary>
		/// Sets the password for the post
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		public UserBuilder WithPassword(string password) {
			Password = password ?? throw new ArgumentNullException($"{nameof(password)} can't be an empty value.");
			return this;
		}

		/// <summary>
		/// Generates a random password of specified length and returns it using out parameter.
		/// </summary>
		/// <param name="generatedPassword">The generated password</param>
		/// <param name="passwordLength">The password length. Default is 13</param>
		/// <returns></returns>
		public UserBuilder WithPassword(out string generatedPassword, int passwordLength = 13) {
			Password = Utilites.GenerateToken(passwordLength);
			generatedPassword = Password;
			return this;
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public HttpContent Create() {
			var formData = new Dictionary<string, string>();

			if (string.IsNullOrEmpty(UserName)) {
				throw new ArgumentNullException($"{nameof(UserName)} can't be an empty value.");
			}

			if (string.IsNullOrEmpty(Email)) {
				throw new ArgumentNullException($"{nameof(Email)} can't be an empty value.");
			}

			if (string.IsNullOrEmpty(Password)) {
				throw new ArgumentNullException($"{nameof(Password)} can't be an empty value.");
			}

			formData.TryAdd("username", UserName);
			formData.TryAdd("email", Email);
			formData.TryAdd("password", Password);

			if (!string.IsNullOrEmpty(Name)) {
				formData.TryAdd("name", Name);
			}

			if (!string.IsNullOrEmpty(FirstName)) {
				formData.TryAdd("first_name", FirstName);
			}

			if (!string.IsNullOrEmpty(LastName)) {
				formData.TryAdd("last_name", LastName);
			}

			if (!string.IsNullOrEmpty(Url)) {
				formData.TryAdd("url", Url);
			}

			if (!string.IsNullOrEmpty(Description)) {
				formData.TryAdd("description", Description);
			}

			if (!string.IsNullOrEmpty(Locale)) {
				formData.TryAdd("locale", Locale);
			}

			if (!string.IsNullOrEmpty(NickName)) {
				formData.TryAdd("nickname", NickName);
			}

			if (!string.IsNullOrEmpty(Slug)) {
				formData.TryAdd("slug", Slug);
			}

			if (Roles != null && Roles.Length > 0) {
				formData.TryAdd("roles", string.Join(",", Roles));
			}

			return new StringContent(JsonConvert.SerializeObject(formData));
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public UserBuilder InitializeWithDefaultValues() {
			Locale = "en_US";
			return this;
		}
	}
}

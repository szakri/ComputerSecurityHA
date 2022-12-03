using Backend;
using Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;
using static BackendTest.Utility;
using System.Net.Http.Json;
using System.Runtime.Intrinsics.X86;

namespace BackendTest
{
    public class UsersTests
    {
        private readonly HttpClient _client;

        public UsersTests(InMemoryDbFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [BeforeAfter]
        [Fact]
        public void Get_ReturnsAllUsers()
        {
            // Arrange
            Reset(_client).Wait();
            var register1 = new RegisterDTO
            {
                Username = "test1",
                Password = "test1"
            };
            var register2 = new RegisterDTO
            {
                Username = "test2",
                Password = "test2"
            };
            var user1 = AddUser(_client, register1).Result;
            var user2 = AddUser(_client, register2).Result;
            Login(_client, register1).Wait();

            // Act
            var response = _client.GetAsync(UsersUrl).Result;
            var users = response.Content.ReadFromJsonAsync<List<UserDTO>>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(users);
            Assert.Collection(users, user => CompareUserDTOs(user, user1), user => CompareUserDTOs(user, user2));
        }

        [BeforeAfter]
        [Fact]
        public void Get_RetrunsCorrectUser()
        {
            // Arrange
            Reset(_client).Wait();
            LoginWithAdmin(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var registeredUser = AddUser(_client, register).Result;

            // Act
            var response = _client.GetAsync($"{UsersUrl}/{registeredUser.Id}").Result;
            var user = response.Content.ReadFromJsonAsync<UserDTO>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(user);
            Assert.True(CompareUserDTOs(registeredUser, user));
        }

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Get_RetrunsWithBadRequest(string userId)
        {
            // Arrange
            Reset(_client).Wait();
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.GetAsync($"{UsersUrl}/{userId}").Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"No User was found with the id {userId}!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Post_CreatesSuccessfully()
        {
            // Arrange
            Reset(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };

            // Act
            var response = _client.PostAsync(UsersUrl,
                new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json")).Result;
            var user = response.Content.ReadFromJsonAsync<UserDTO>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(user);
            Assert.Equal(register.Username, user.Username);
        }

        [BeforeAfter]
        [Fact]
        public void Post_RetrunsWithBadRequestWithEmptyUsername()
        {
            // Arrange
            Reset(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "",
                Password = "test"
            };

            // Act
            var response = _client.PostAsync(UsersUrl,
                new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("The Username must not be empty!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Post_RetrunsWithBadRequestWithEmptyPassword()
        {
            // Arrange
            Reset(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = ""
            };

            // Act
            var response = _client.PostAsync(UsersUrl,
                new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("The Password must not be empty!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Post_RetrunsWithBadRequestWithUsernameWithWhiteSpace()
        {
            // Arrange
            Reset(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test test",
                Password = "test"
            };

            // Act
            var response = _client.PostAsync(UsersUrl,
                new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("The Username must not contain whitespaces!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Post_RetrunsWithBadRequestWithExsitsingUsername()
        {
            // Arrange
            Reset(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            _client.PostAsync(UsersUrl,
                new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json")).Wait();

            // Act
            var response = _client.PostAsync(UsersUrl,
                new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("The Username is already in use!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Delete_RemovesUser()
        {
            // Arrange
            Reset(_client).Wait();
            LoginWithAdmin(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var userId = AddUser(_client, register).Result.Id;

            // Act
            var response = _client.DeleteAsync($"{UsersUrl}/{userId}").Result;

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = _client.GetAsync(UsersUrl).Result;
            var users = response.Content.ReadFromJsonAsync<List<UserDTO>>().Result;
            Assert.NotNull(users);
            // Because of the admin login
            var user = Assert.Single(users);
            Assert.Equal("Admin", user.Username);
		}

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Delete_RetrunsWithBadRequest(string userId)
        {
            // Arrange
            Reset(_client).Wait();
            LoginWithAdmin(_client).Wait();

            // Act
            var response = _client.DeleteAsync($"{UsersUrl}/{userId}").Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"No User was found with the id {userId}!", content);
        }
    }
}

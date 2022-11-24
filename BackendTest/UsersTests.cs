using Backend;
using Backend.Controllers;
using Backend.Services;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BackendTest
{
    public class UsersTests : IClassFixture<InMemoryDbFactory<Program>>
    {
        private readonly string url = "https://localhost:7206/api/Users";
        private readonly HttpClient _client;

        public UsersTests(InMemoryDbFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task ResetDB()
        {
            await _client.GetAsync("https://localhost:7206/api/Test/reset");
        }

        private async Task<string> AddUser(string url, RegisterDTO user)
        {
            var response = await _client.PostAsync(url,
                new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDTO>(content).Id;
        }

        private static bool CompareUserDTOs(UserDTO user1, UserDTO user2)
        {
            return user1.Id == user2.Id && user2.Username == user2.Username;
        }

        [Fact]
        public async Task Get_ReturnsAllUsers()
        {
            // Arrange
            await ResetDB();
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
            var user1 = new UserDTO
            {
                Id = await AddUser(url, register1),
                Username = register1.Username
            };
            var user2 = new UserDTO
            {
                Id = await AddUser(url, register2),
                Username = register2.Username
            };

            // Act
            var response = await _client.GetAsync($"{url}");
            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserDTO>>(content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(users);
            Assert.Collection(users, user => CompareUserDTOs(user, user1), user => CompareUserDTOs(user, user2));
        }

        [Fact]
        public async Task Get_RetrunsCorrectUser()
        {
            // Arrange
            await ResetDB();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var userId = await AddUser(url, register);

            // Act
            var response = await _client.GetAsync($"{url}/{userId}");
            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDTO>(content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(user);
            Assert.Equal("test", user.Username);
        }

        [Theory]
        [InlineData("1")]
        public async Task Get_RetrunsWithBadRequest(string userId)
        {
            // Arrange
            await ResetDB();

            // Act
            var response = await _client.GetAsync($"{url}/{userId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"No User was found with the id {userId}!", content);
        }

        [Fact]
        public async Task Post_CreatesSuccessfully()
        {
            // Arrange
            await ResetDB();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };

            // Act
            var response = await _client.PostAsync(url,
                new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDTO>(content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(user);
            Assert.Equal("test", user.Username);
        }

        [Fact]
        public async Task Delete_RemovesUser()
        {
            // Arrange
            await ResetDB();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var userId = await AddUser(url, register);

            // Act
            var response = await _client.DeleteAsync($"{url}/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{url}");
            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserDTO>>(content);
            Assert.Empty(users);
        }

        [Theory]
        [InlineData("1")]
        public async Task Delete_RetrunsWithBadRequest(string userId)
        {
            // Arrange
            await ResetDB();

            // Act
            var response = await _client.DeleteAsync($"{url}/{userId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"No User was found with the id {userId}!", content);
        }
    }
}

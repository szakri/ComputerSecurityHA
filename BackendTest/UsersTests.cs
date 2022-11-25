using Backend;
using Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BackendTest
{
    public class UsersTests
    {
        private readonly HttpClient _client;

        public UsersTests(InMemoryDbFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public void Get_ReturnsAllUsers()
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();
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
                    Id = Utility.AddUser(_client, register1).Result.Id,
                    Username = register1.Username
                };
                var user2 = new UserDTO
                {
                    Id = Utility.AddUser(_client, register2).Result.Id,
                    Username = register2.Username
                };

                // Act
                var response = _client.GetAsync($"{Utility.UsersUrl}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var users = JsonSerializer.Deserialize<List<UserDTO>>(content);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(users);
                Assert.Collection(users, user => Utility.CompareUserDTOs(user, user1), user => Utility.CompareUserDTOs(user, user2));
            }
        }

        [Fact]
        public void Get_RetrunsCorrectUser()
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();
                var register = new RegisterDTO
                {
                    Username = "test",
                    Password = "test"
                };
                var registeredUser = Utility.AddUser(_client, register).Result;

                // Act
                var response = _client.GetAsync($"{Utility.UsersUrl}/{registeredUser.Id}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var user = JsonSerializer.Deserialize<UserDTO>(content);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(user);
                Assert.True(Utility.CompareUserDTOs(registeredUser, user));
            }
        }

        [Theory]
        [InlineData("1")]
        public void Get_RetrunsWithBadRequest(string userId)
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();

                // Act
                var response = _client.GetAsync($"{Utility.UsersUrl}/{userId}").Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal($"No User was found with the id {userId}!", content);
            }
        }

        [Fact]
        public void Post_CreatesSuccessfully()
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();
                var register = new RegisterDTO
                {
                    Username = "test",
                    Password = "test"
                };

                // Act
                var response = _client.PostAsync(Utility.UsersUrl,
                    new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json")).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var user = JsonSerializer.Deserialize<UserDTO>(content);

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(user);
                Assert.Equal(register.Username, user.Username);
            }
        }

        [Fact]
        public void Delete_RemovesUser()
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();
                var register = new RegisterDTO
                {
                    Username = "test",
                    Password = "test"
                };
                var userId = Utility.AddUser(_client, register).Result.Id;

                // Act
                var response = _client.DeleteAsync($"{Utility.UsersUrl}/{userId}").Result;

                // Assert
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                response = _client.GetAsync($"{Utility.UsersUrl}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var users = JsonSerializer.Deserialize<List<UserDTO>>(content);
                Assert.NotNull(users);
                Assert.Empty(users);
            }
        }

        [Theory]
        [InlineData("1")]
        public void Delete_RetrunsWithBadRequest(string userId)
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();

                // Act
                var response = _client.DeleteAsync($"{Utility.UsersUrl}/{userId}").Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal($"No User was found with the id {userId}!", content);
            }
        }
    }
}

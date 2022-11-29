using Backend;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Models;
using System.Net.Http.Json;
using System.Net;
using static BackendTest.Utility;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Net.Http.Headers;

namespace BackendTest
{
    public class AuthTests
    {
        private readonly HttpClient _client;
        public static IEnumerable<object[]> AuthorizedGetUrls => new List<object[]>
        {
            new object[] { CaffsUrl },
            new object[] { $"{CaffsUrl}/1" },
            new object[] { $"{CaffsUrl}/1/download" },
            new object[] { $"{CaffsUrl}/1/preview" },
            new object[] { CommentsUrl },
            new object[] { $"{CommentsUrl}/1" },
            new object[] { UsersUrl },
            new object[] { $"{UsersUrl}/1" },
        };
        public static IEnumerable<object[]> AuthorizedDeleteUrls => new List<object[]>
        {
            new object[] { $"{CaffsUrl}/1" },
            new object[] { $"{CommentsUrl}/1" },
            new object[] { $"{UsersUrl}/1" },
        };

        public AuthTests(InMemoryDbFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [BeforeAfter]
        [Fact]
        public void PostUsers_ShouldNotBeAuthorized()
        {
            // Arrange
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };

            // Act
            var response = _client.PostAsync(UsersUrl,
                new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json")).Result;

            // Assert
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [BeforeAfter]
        [Theory]
        [MemberData(nameof(AuthorizedGetUrls))]
        public void Get_ShouldBeAuthorized(string url)
        {
            // Arrange

            // Act
            var response = _client.GetAsync(url).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [BeforeAfter]
        [Fact]
        public void PostCaffs_ShouldBeAuthorized()
        {
            // Arrange
            var current = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(current, "Resources", "test.caff");
            UriBuilder builder = new(CaffsUrl);
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var user = AddUser(_client, register).Result;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["userId"] = user.Id;
            builder.Query = query.ToString();
            var fileStreamContent = new StreamContent(File.OpenRead(filePath));
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/caff");
            var multipartFormContent = new MultipartFormDataContent();
            var fileName = "test";
            multipartFormContent.Add(fileStreamContent, name: "file", fileName: $"{fileName}.caff");

            // Act
            var response = _client.PostAsync(builder.ToString(), multipartFormContent).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [BeforeAfter]
        [Fact]
        public void PatchCaffs_ShouldBeAuthorized()
        {
            // Arrange
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var caff = AddCaff(_client, user).Result;
            var newName = "patch";

            // Act
            var response = _client.PatchAsync($"{CaffsUrl}/{caff.Id}",
                new StringContent(JsonSerializer.Serialize(newName), Encoding.UTF8, "application/json")).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [BeforeAfter]
        [Fact]
        public void PostComments_ShouldBeAuthorized()
        {
            // Arrange
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var caff = AddCaff(_client, user).Result;

            var commentText = "Test comment";
            var newComment = new NewCommentDTO()
            {
                CaffId = caff.Id,
                UserId = user.Id,
                CommentText = commentText
            };

            // Act
            var response = _client.PostAsync(CommentsUrl,
                new StringContent(JsonSerializer.Serialize(newComment), Encoding.UTF8, "application/json")).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [BeforeAfter]
        [Fact]
        public void PatchComments_ShouldBeAuthorized()
        {
            // Arrange
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var caff = AddCaff(_client, user).Result;
            var commentText = "Test";
            var comment = AddComment(_client, user, caff, commentText).Result;
            var newCommentText = "patch";

            // Act
            var response = _client.PatchAsync($"{CommentsUrl}/{comment.Id}",
                new StringContent(JsonSerializer.Serialize(newCommentText), Encoding.UTF8, "application/json")).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [BeforeAfter]
        [Theory]
        [MemberData(nameof(AuthorizedDeleteUrls))]
        public void Delete_ShouldBeAuthorized(string url)
        {
            // Arrange, Act
            var response = _client.DeleteAsync(url).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [BeforeAfter]
        [Fact]
        public void PatchCaffs_ShouldBeAdminAuthorized()
        {
            // Arrange
            ResetDB(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var user = AddUser(_client, register).Result;
            var caff = AddCaff(_client, user).Result;
            var newName = "patch";
            Login(_client, register).Wait();

            // Act
            var response = _client.PatchAsync($"{CaffsUrl}/{caff.Id}",
                new StringContent(JsonSerializer.Serialize(newName), Encoding.UTF8, "application/json")).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [BeforeAfter]
        [Theory]
        [MemberData(nameof(AuthorizedDeleteUrls))]
        public void Delete_ShouldBeAdminAuthorized(string url)
        {
            // Arrange
            ResetDB(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            AddUser(_client, register).Wait();
            Login(_client, register).Wait();

            // Act
            var response = _client.DeleteAsync(url).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}

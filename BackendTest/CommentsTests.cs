using Backend;
using Microsoft.Win32;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using static BackendTest.Utility;

namespace BackendTest
{
    public class CommentsTests
    {
        private readonly HttpClient _client;

        public CommentsTests(InMemoryDbFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [BeforeAfter]
        [Fact]
        public void Get_NoCommentsOnInit()
        {
            // Arrange
            Reset(_client).Wait();
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.GetAsync(CommentsUrl).Result;
            var comments = response.Content.ReadFromJsonAsync<List<CommentDTO>>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(comments);
            Assert.Empty(comments);
        }

        [BeforeAfter]
        [Fact]
        public void Get_ReturnsAllComments()
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
            var user1 = new UserDTO
            {
                Id = AddUser(_client, register1).Result.Id,
                Username = register1.Username
            };
            var user2 = new UserDTO
            {
                Id = AddUser(_client, register2).Result.Id,
                Username = register2.Username
            };
            var caff = AddCaff(_client, user1).Result;
            var commentText1 = "First";
            var commentText2 = "Second";
            var comment1 = AddComment(_client, user1, caff, commentText1).Result;
            var comment2 = AddComment(_client, user2, caff, commentText2).Result;
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.GetAsync(CommentsUrl).Result;
            var comments = response.Content.ReadFromJsonAsync<List<CommentDTO>>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(comments);
            Assert.Collection(comments, comment => CompareCommentDTOs(comment, comment1),
                comment => CompareCommentDTOs(comment, comment2));
        }

        [BeforeAfter]
        [Fact]
        public void Get_RetrunsCorrectComment()
        {
            // Arrange
            Reset(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var user = new UserDTO
            {
                Id = AddUser(_client, register).Result.Id,
                Username = register.Username
            };
            var caff = AddCaff(_client, user).Result;
            var commentText = "Test comment";
            var newComment = AddComment(_client, user, caff, commentText).Result;
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.GetAsync($"{CommentsUrl}/{newComment.Id}").Result;
            var comment = response.Content.ReadFromJsonAsync<CommentDTO>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(comment);
            Assert.True(CompareCommentDTOs(newComment, comment));
        }

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Get_RetrunsWithBadRequest(string commentId)
        {
            // Arrange
            Reset(_client).Wait();
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.GetAsync($"{CommentsUrl}/{commentId}").Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"\"No Comment was found with the id {commentId}!\"", content);
        }

        [BeforeAfter]
        [Fact]
        public void Post_CreatesSuccessfully()
        {
            // Arrange
            Reset(_client).Wait();
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
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.PostAsync(CommentsUrl,
                new StringContent(JsonSerializer.Serialize(newComment), Encoding.UTF8, "application/json")).Result;
            var comment = response.Content.ReadFromJsonAsync<CommentDTO>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(comment);
            Assert.Equal(commentText, comment.Text);
            Assert.True(CompareUserDTOs(user, comment.User));
        }

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Post_RetrunsWithNotFoundWithBadCaff(string caffId)
        {
            // Arrange
            Reset(_client).Wait();
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var commentText = "Test comment";
            var newComment = new NewCommentDTO()
            {
                CaffId = caffId,
                UserId = user.Id,
                CommentText = commentText
            };
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.PostAsync(CommentsUrl,
                new StringContent(JsonSerializer.Serialize(newComment), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"\"No CAFF file was found with the id {caffId}!\"", content);
        }

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Post_RetrunsWithNotFoundWithBadUser(string userId)
        {
            // Arrange
            Reset(_client).Wait();
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
                UserId = userId,
                CommentText = commentText
            };
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.PostAsync(CommentsUrl,
                new StringContent(JsonSerializer.Serialize(newComment), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"\"No User was found with the id {userId}!\"", content);
        }

        [BeforeAfter]
        [Theory]
        [InlineData("")]
        public void Post_RetrunsWithBadRequestWithEmptyComment(string commentText)
        {
            // Arrange
            Reset(_client).Wait();
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var caff = AddCaff(_client, user).Result;
            var newComment = new NewCommentDTO()
            {
                CaffId = caff.Id,
                UserId = user.Id,
                CommentText = commentText
            };
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.PostAsync(CommentsUrl,
                new StringContent(JsonSerializer.Serialize(newComment), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("\"The commentText must be at least 1 character long!\"", content);
        }

        [BeforeAfter]
        [Fact]
        public void Patch_ModifiesSuccessfully()
        {
            // Arrange
            Reset(_client).Wait();
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var caff = AddCaff(_client, user).Result;
            var commentText = "Test";
            var comment = AddComment(_client, user, caff, commentText).Result;
            var newCommentText = "patch";
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.PatchAsync($"{CommentsUrl}/{comment.Id}",
                new StringContent(JsonSerializer.Serialize(newCommentText), Encoding.UTF8, "application/json")).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var modified = GetComment(_client, comment.Id).Result;
            Assert.NotNull(modified);
            Assert.Equal(comment.Id, modified.Id);
            Assert.Equal(newCommentText, modified.Text);
            Assert.True(CompareUserDTOs(user, modified.User));
        }

        [BeforeAfter]
        [Theory]
        [InlineData("")]
        public void Patch_RetrunsWithBadRequestWithEmptyComment(string newCommentText)
        {
            // Arrange
            Reset(_client).Wait();
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var caff = AddCaff(_client, user).Result;
            var comment = AddComment(_client, user, caff, "test").Result;
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.PatchAsync($"{CommentsUrl}/{comment.Id}",
                new StringContent(JsonSerializer.Serialize(newCommentText), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("\"The commentText must be at least 1 character long!\"", content);
        }

        [BeforeAfter]
        [Fact]
        public void Delete_RemovesComment()
        {
            // Arrange
            Reset(_client).Wait();
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var caff = AddCaff(_client, user).Result;
            var commentText = "Test";
            var comment = AddComment(_client, user, caff, commentText).Result;
			LoginWithAdmin(_client).Wait();

			// Act
			var response = _client.DeleteAsync($"{CommentsUrl}/{comment.Id}").Result;

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = _client.GetAsync(CommentsUrl).Result;
            var comments = response.Content.ReadFromJsonAsync<List<UserDTO>>().Result;
            Assert.NotNull(comments);
            Assert.Empty(comments);
        }

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Delete_RetrunsWithBadRequest(string commentId)
        {
            // Arrange
            Reset(_client).Wait();
            LoginWithAdmin(_client).Wait();

            // Act
            var response = _client.DeleteAsync($"{CommentsUrl}/{commentId}").Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"\"No Comment was found with the id {commentId}!\"", content);
        }
    }
}

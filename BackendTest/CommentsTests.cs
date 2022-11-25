using Backend;
using Microsoft.Win32;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace BackendTest
{
    public class CommentsTests
    {
        private readonly HttpClient _client;

        public CommentsTests(InMemoryDbFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public void Get_ReturnsAllComments()
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
                var caff = Utility.AddCaff(_client, user1).Result;
                var commentText1 = "First";
                var commentText2 = "Second";
                var comment1 = Utility.AddComment(_client, user1, caff, commentText1).Result;
                var comment2 = Utility.AddComment(_client, user2, caff, commentText2).Result;

                // Act
                var response = _client.GetAsync($"{Utility.CommentsUrl}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var comments = JsonSerializer.Deserialize<List<CommentDTO>>(content);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(comments);
                Assert.Collection(comments, comment => Utility.CompareCommentDTOs(comment, comment1),
                    comment => Utility.CompareCommentDTOs(comment, comment2));
            }
        }

        [Fact]
        public void Get_RetrunsCorrectComment()
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
                var user = new UserDTO
                {
                    Id = Utility.AddUser(_client, register).Result.Id,
                    Username = register.Username
                };
                var caff = Utility.AddCaff(_client, user).Result;
                var commentText = "Test comment";
                var newComment = Utility.AddComment(_client, user, caff, commentText).Result;

                // Act
                var response = _client.GetAsync($"{Utility.CommentsUrl}/{newComment.Id}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var comment = JsonSerializer.Deserialize<CommentDTO>(content);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(comment);
                Assert.True(Utility.CompareCommentDTOs(newComment, comment));
            }
        }

        [Theory]
        [InlineData("1")]
        public void Get_RetrunsWithBadRequest(string commentId)
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();

                // Act
                var response = _client.GetAsync($"{Utility.CommentsUrl}/{commentId}").Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal($"No Comment was found with the id {commentId}!", content);
            }
        }

        [Fact]
        public void Post_CreatesSuccessfully()
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();
                var user = Utility.AddUser(_client, new RegisterDTO
                {
                    Username = "test",
                    Password = "test"
                }).Result;
                var caff = Utility.AddCaff(_client, user).Result;

                var commentText = "Test comment";
                var newComment = new NewCommentDTO()
                {
                    CaffId = caff.Id,
                    UserId = user.Id,
                    CommentText = commentText
                };

                // Act
                var response = _client.PostAsync(Utility.CommentsUrl,
                    new StringContent(JsonSerializer.Serialize(newComment), Encoding.UTF8, "application/json")).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var comment = JsonSerializer.Deserialize<CommentDTO>(content);

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(comment);
                Assert.Equal(commentText, comment.Text);
                Assert.True(Utility.CompareUserDTOs(user, comment.User));
            }
        }

        [Fact]
        public void Patch_ModifiesSuccessfully()
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();
                var user = Utility.AddUser(_client, new RegisterDTO
                {
                    Username = "test",
                    Password = "test"
                }).Result;
                var caff = Utility.AddCaff(_client, user).Result;
                var commentText = "Test";
                var comment = Utility.AddComment(_client, user, caff, commentText).Result;
                var newCommentText = "patch";

                // Act
                var response = _client.PatchAsync($"{Utility.CommentsUrl}/{comment.Id}",
                    new StringContent(JsonSerializer.Serialize(newCommentText), Encoding.UTF8, "application/json")).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Assert
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                var modified = Utility.GetComment(_client, comment.Id).Result;
                Assert.NotNull(modified);
                Assert.Equal(comment.Id, modified.Id);
                Assert.Equal(newCommentText, modified.Text);
                Assert.True(Utility.CompareUserDTOs(user, modified.User));
            }
        }

        [Fact]
        public void Delete_RemovesComment()
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();
                var user = Utility.AddUser(_client, new RegisterDTO
                {
                    Username = "test",
                    Password = "test"
                }).Result;
                var caff = Utility.AddCaff(_client, user).Result;
                var commentText = "Test";
                var comment = Utility.AddComment(_client, user, caff, commentText).Result;

                // Act
                var response = _client.DeleteAsync($"{Utility.CommentsUrl}/{comment.Id}").Result;

                // Assert
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                response = _client.GetAsync($"{Utility.CommentsUrl}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var comments = JsonSerializer.Deserialize<List<UserDTO>>(content);
                Assert.NotNull(comments);
                Assert.Empty(comments);
            }
        }

        [Theory]
        [InlineData("1")]
        public void Delete_RetrunsWithBadRequest(string commentId)
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();

                // Act
                var response = _client.DeleteAsync($"{Utility.CommentsUrl}/{commentId}").Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal($"No Comment was found with the id {commentId}!", content);
            }
        }
    }
}

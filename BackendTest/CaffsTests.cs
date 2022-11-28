using Backend;
using Models;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using static BackendTest.Utility;

namespace BackendTest
{
    public class CaffsTests
    {
        private readonly HttpClient _client;

        public CaffsTests(InMemoryDbFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [BeforeAfter]
        [Fact]
        public void Get_NoCaffsOnInit()
        {
            // Arrange
            ResetDB(_client).Wait();

            // Act
            var response = _client.GetAsync(CaffsUrl).Result;
            var caffs = response.Content.ReadFromJsonAsync<List<CaffDTO>>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(caffs);
            Assert.Empty(caffs);
        }

        [BeforeAfter]
        [Fact]
        public void Get_ReturnsAllCaffs()
        {
            // Arrange
            ResetDB(_client).Wait();
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
            var caff1 = AddCaff(_client, user1).Result;
            var caff2 = AddCaff(_client, user2).Result;

            // Act
            var response = _client.GetAsync(CaffsUrl).Result;
            var caffs = response.Content.ReadFromJsonAsync<List<CaffDTO>>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(caffs);
            Assert.Collection(caffs, caff => CompareCaffDTOs(caff, caff1), caff => CompareCaffDTOs(caff, caff2));
        }

        [BeforeAfter]
        [Theory]
        [InlineData(@"name == ""caff1""", 2)]
        [InlineData(@"name == ""caff3""", 0)]
        [InlineData(@"uploader.Username == ""test1""", 2)]
        [InlineData(@"uploader.Username == ""test3""", 0)]
        public void Get_ReturnsAllFilteredCaffs(string searchBy, int matchNumber)
        {
            // Arrange
            ResetDB(_client).Wait();
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
            AddCaff(_client, user1, "caff1").Wait();
            AddCaff(_client, user2, "caff1").Wait();
            AddCaff(_client, user1, "caff2").Wait();
            UriBuilder builder = new(CaffsUrl);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["searchBy"] = searchBy;
            builder.Query = query.ToString();

            // Act
            var response = _client.GetAsync(builder.ToString()).Result;
            var caffs = response.Content.ReadFromJsonAsync<List<CaffDTO>>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(caffs);
            Assert.Equal(matchNumber, caffs.Count);
        }

        [BeforeAfter]
        [Theory]
        [InlineData(@"uploader == ""test1""")]
        public void Get_ReturnsBadRequestWithBadSearchBy(string searchBy)
        {
            // Arrange
            ResetDB(_client).Wait();
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
            AddCaff(_client, user1, "caff1").Wait();
            AddCaff(_client, user2, "caff1").Wait();
            AddCaff(_client, user1, "caff2").Wait();
            UriBuilder builder = new(CaffsUrl);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["searchBy"] = searchBy;
            builder.Query = query.ToString();

            // Act
            var response = _client.GetAsync(builder.ToString()).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Invalid searchBy term!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Get_RetrunsCorrectCaff()
        {
            // Arrange
            ResetDB(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var uploader = new UserDTO
            {
                Id = AddUser(_client, register).Result.Id,
                Username = register.Username
            };
            var upload = AddCaff(_client, uploader).Result;

            // Act
            var response = _client.GetAsync($"{CaffsUrl}/{upload.Id}").Result;
            var caff = response.Content.ReadFromJsonAsync<CaffDTO>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(caff);
            Assert.True(CompareCaffDTOs(upload, caff));
        }

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Get_RetrunsWithBadRequest(string caffId)
        {
            // Arrange
            ResetDB(_client).Wait();

            // Act
            var response = _client.GetAsync($"{CaffsUrl}/{caffId}").Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"No CAFF file was found with the id {caffId}!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Get_ReturnsCorrectDownloadFile()
        {
            // Arrange
            ResetDB(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var uploader = new UserDTO
            {
                Id = AddUser(_client, register).Result.Id,
                Username = register.Username
            };
            var upload = AddCaff(_client, uploader, "download").Result;

            // Act
            var response = _client.GetAsync($"{CaffsUrl}/{upload.Id}/download").Result;
            var content = response.Content.ReadAsByteArrayAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4_002_260, content.Length);
        }

        [BeforeAfter]
        [Fact]
        public void Get_ReturnsCorrectPreviewFile()
        {
            // Arrange
            ResetDB(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var uploader = new UserDTO
            {
                Id = AddUser(_client, register).Result.Id,
                Username = register.Username
            };
            var upload = AddCaff(_client, uploader, "preview").Result;

            // Act
            var response = _client.GetAsync($"{CaffsUrl}/{upload.Id}/preview").Result;
            var content = response.Content.ReadAsByteArrayAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(897_517, content.Length);
        }

        [BeforeAfter]
        [Fact]
        public void Post_CreatesSuccessfully()
        {
            // Arrange
            ResetDB(_client).Wait();
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
            var caff = response.Content.ReadFromJsonAsync<CaffDTO>().Result;

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(caff);
            Assert.Equal(fileName, caff.Name);
            Assert.Equal(register.Username, caff.UploaderUsername);
            Assert.Empty(caff.Comments);
        }

        [BeforeAfter]
        [Theory]
        [InlineData("txt")]
        [InlineData("gif")]
        public void Post_RetrunsWithBadRequestWithBadExtension(string extension)
        {
            // Arrange
            ResetDB(_client).Wait();
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
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue($"image/{extension}");
            var multipartFormContent = new MultipartFormDataContent();
            var fileName = "test";
            multipartFormContent.Add(fileStreamContent, name: "file", fileName: $"{fileName}.{extension}");

            // Act
            var response = _client.PostAsync(builder.ToString(), multipartFormContent).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Bad file extension!", content);
        }

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Post_RetrunsWithNoContentWithBadUser(string userId)
        {
            // Arrange
            ResetDB(_client).Wait();
            var current = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(current, "Resources", "test.caff");
            UriBuilder builder = new(CaffsUrl);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["userId"] = userId;
            builder.Query = query.ToString();
            var fileStreamContent = new StreamContent(File.OpenRead(filePath));
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue($"image/caff");
            var multipartFormContent = new MultipartFormDataContent();
            var fileName = "test";
            multipartFormContent.Add(fileStreamContent, name: "file", fileName: $"{fileName}.caff");

            // Act
            var response = _client.PostAsync(builder.ToString(), multipartFormContent).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"No User was found with the id {userId}!", content);
        }

        [BeforeAfter]
        [Theory]
        [InlineData("exception")]
        public void Post_RetrunsWithBadRequestWithBadCaff(string fileName)
        {
            // Arrange
            ResetDB(_client).Wait();
            var current = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(current, "Resources", "test.caff");
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var user = AddUser(_client, register).Result;
            UriBuilder builder = new(CaffsUrl);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["userId"] = user.Id;
            builder.Query = query.ToString();
            var fileStreamContent = new StreamContent(File.OpenRead(filePath));
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue($"image/caff");
            var multipartFormContent = new MultipartFormDataContent();
            multipartFormContent.Add(fileStreamContent, name: "file", fileName: $"{fileName}.caff");

            // Act
            var response = _client.PostAsync(builder.ToString(), multipartFormContent).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("The CAFF file was not correct!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Patch_ModifiesSuccessfully()
        {
            // Arrange
            ResetDB(_client).Wait();
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
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var modified = GetCaff(_client, caff.Id).Result;
            Assert.NotNull(modified);
            Assert.Equal(caff.Id, modified.Id);
            Assert.Equal(newName, modified.Name);
            Assert.Equal(caff.UploaderId, modified.UploaderId);
            Assert.Equal(caff.UploaderUsername, modified.UploaderUsername);
            Assert.Empty(modified.Comments);
        }

        [BeforeAfter]
        [Theory]
        [InlineData("")]
        public void Patch_RetrunsWithBadRequest(string newName)
        {
            // Arrange
            ResetDB(_client).Wait();
            var user = AddUser(_client, new RegisterDTO
            {
                Username = "test",
                Password = "test"
            }).Result;
            var caff = AddCaff(_client, user).Result;

            // Act
            var response = _client.PatchAsync($"{CaffsUrl}/{caff.Id}",
                new StringContent(JsonSerializer.Serialize(newName), Encoding.UTF8, "application/json")).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("The name must be at least 1 character long!", content);
        }

        [BeforeAfter]
        [Fact]
        public void Delete_RemovesUser()
        {
            // Arrange
            ResetDB(_client).Wait();
            var register = new RegisterDTO
            {
                Username = "test",
                Password = "test"
            };
            var uploader = new UserDTO
            {
                Id = AddUser(_client, register).Result.Id,
                Username = register.Username
            };
            var caff = AddCaff(_client, uploader).Result;

            // Act
            var response = _client.DeleteAsync($"{CaffsUrl}/{caff.Id}").Result;

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = _client.GetAsync(CaffsUrl).Result;
            var caffs = response.Content.ReadFromJsonAsync<List<UserDTO>>().Result;
            Assert.NotNull(caffs);
            Assert.Empty(caffs);
        }

        [BeforeAfter]
        [Theory]
        [InlineData("1")]
        public void Delete_RetrunsWithBadRequest(string caffId)
        {
            // Arrange
            ResetDB(_client).Wait();

            // Act
            var response = _client.DeleteAsync($"{CaffsUrl}/{caffId}").Result;
            var content = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal($"No CAFF file was found with the id {caffId}!", content);
        }
    }
}

using Backend;
using Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace BackendTest
{
    public class CaffsTests
    {
        private readonly HttpClient _client;

        public CaffsTests(InMemoryDbFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public void Get_ReturnsAllCaffs()
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
                var caff1 = Utility.AddCaff(_client, user1).Result;
                var caff2 = Utility.AddCaff(_client, user2).Result;

                // Act
                var response = _client.GetAsync($"{Utility.CaffsUrl}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var caffs = JsonSerializer.Deserialize<List<CaffDTO>>(content);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(caffs);
                Assert.Collection(caffs, caff => Utility.CompareCaffDTOs(caff, caff1), caff => Utility.CompareCaffDTOs(caff, caff2));
            }
        }

        [Theory]
        [InlineData("")]
        public void Get_ReturnsAllFilteredCaffs(string searchBy)
        {
            // TODO
        }

        [Fact]
        public void Get_RetrunsCorrectCaff()
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
                var uploader = new UserDTO
                {
                    Id = Utility.AddUser(_client, register).Result.Id,
                    Username = register.Username
                };
                var upload = Utility.AddCaff(_client, uploader).Result;

                // Act
                var response = _client.GetAsync($"{Utility.CaffsUrl}/{upload.Id}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var caff = JsonSerializer.Deserialize<CaffDTO>(content);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(caff);
                Assert.True(Utility.CompareCaffDTOs(upload, caff));
            }
        }

        [Theory]
        [InlineData("1")]
        public void Get_RetrunsWithBadRequest(string caffId)
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();

                // Act
                var response = _client.GetAsync($"{Utility.CaffsUrl}/{caffId}").Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal($"No CAFF file was found with the id {caffId}!", content);
            }
        }

        [Fact]
        public void Post_CreatesSuccessfully()
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();
                var current = Directory.GetCurrentDirectory();
                var filePath = Path.Combine(current, "Resources", "test.caff");
                UriBuilder builder = new(Utility.CaffsUrl);
                var register = new RegisterDTO
                {
                    Username = "test",
                    Password = "test"
                };
                var user = Utility.AddUser(_client, register).Result;
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
                var content = response.Content.ReadAsStringAsync().Result;
                var caff = JsonSerializer.Deserialize<CaffDTO>(content);

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(caff);
                Assert.Equal(fileName, caff.Name);
                Assert.Equal(register.Username, caff.UploaderUsername);
                Assert.Empty(caff.Comments);
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
                var newName = "patch";

                // Act
                var response = _client.PatchAsync($"{Utility.CaffsUrl}/{caff.Id}",
                    new StringContent(JsonSerializer.Serialize(newName), Encoding.UTF8, "application/json")).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Assert
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                var modified = Utility.GetCaff(_client, caff.Id).Result;
                Assert.NotNull(modified);
                Assert.Equal(caff.Id, modified.Id);
                Assert.Equal(newName, modified.Name);
                Assert.Equal(caff.UploaderId, modified.UploaderId);
                Assert.Equal(caff.UploaderUsername, modified.UploaderUsername);
                Assert.Empty(modified.Comments);
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
                var uploader = new UserDTO
                {
                    Id = Utility.AddUser(_client, register).Result.Id,
                    Username = register.Username
                };
                var caff = Utility.AddCaff(_client, uploader).Result;

                // Act
                var response = _client.DeleteAsync($"{Utility.CaffsUrl}/{caff.Id}").Result;

                // Assert
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                response = _client.GetAsync($"{Utility.CaffsUrl}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var caffs = JsonSerializer.Deserialize<List<UserDTO>>(content);
                Assert.NotNull(caffs);
                Assert.Empty(caffs);
            }
        }

        [Theory]
        [InlineData("1")]
        public void Delete_RetrunsWithBadRequest(string caffId)
        {
            lock (Utility.Lock)
            {
                // Arrange
                Utility.ResetDB(_client).Wait();

                // Act
                var response = _client.DeleteAsync($"{Utility.CaffsUrl}/{caffId}").Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal($"No CAFF file was found with the id {caffId}!", content);
            }
        }
    }
}

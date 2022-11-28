using Microsoft.Win32;
using Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;
using Xunit.Sdk;

namespace BackendTest
{
    internal static class Utility
    {
        public static readonly string UsersUrl = "https://localhost:7206/api/Users";
        public static readonly string CaffsUrl = "https://localhost:7206/api/Caffs";
        public static readonly string CommentsUrl = "https://localhost:7206/api/Comments";
        public static readonly object Lock = new();

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class BeforeAfter : BeforeAfterTestAttribute
        {

            public override void Before(MethodInfo methodUnderTest)
            {
                Monitor.Enter(Lock);
            }

            public override void After(MethodInfo methodUnderTest)
            {
                Monitor.Exit(Lock);
            }
        }

        internal static async Task ResetDB(HttpClient client)
        {
            await client.GetAsync("https://localhost:7206/api/Test/reset");
        }

        internal static async Task<UserDTO> AddUser(HttpClient client, RegisterDTO user)
        {
            var response = await client.PostAsync("https://localhost:7206/api/Users",
                new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDTO>(content);
        }

        internal static async Task<CaffDTO> AddCaff(HttpClient client, UserDTO uploader, string fileName = "test")
        {
            var current = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(current, "Resources", "test.caff");
            UriBuilder builder = new("https://localhost:7206/api/Caffs");
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["userId"] = uploader.Id;
            builder.Query = query.ToString();
            var fileStreamContent = new StreamContent(File.OpenRead(filePath));
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/caff");
            var multipartFormContent = new MultipartFormDataContent();
            multipartFormContent.Add(fileStreamContent, name: "file", fileName: $"{fileName}.caff");
            var response = await client.PostAsync(builder.ToString(), multipartFormContent);
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CaffDTO>(content);
        }

        internal static async Task<CommentDTO> AddComment(HttpClient client, UserDTO user, CaffDTO caff, string commentText)
        {
            var newComment = new NewCommentDTO()
            {
                CaffId = caff.Id,
                UserId = user.Id,
                CommentText = commentText
            };
            var response = await client.PostAsync(Utility.CommentsUrl,
                new StringContent(JsonSerializer.Serialize(newComment), Encoding.UTF8, "application/json"));
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<CommentDTO>(content);
        }

        internal static bool CompareUserDTOs(UserDTO user1, UserDTO user2)
        {
            return user1.Id == user2.Id && user1.Username == user2.Username;
        }

        internal static bool CompareCaffDTOs(CaffDTO caff1, CaffDTO caff2)
        {
            bool sameComments = false;
            if ((caff1.Comments == null && caff2.Comments != null) ||
                (caff1.Comments != null && caff2.Comments == null))
            {
                return false;
            }
            if (caff1.Comments == null && caff2.Comments == null ||
                caff1.Comments?.Count == 0 && caff2.Comments?.Count == 0)
            {
                sameComments = true;
            }
            else
            {
                if (caff1.Comments?.Count != caff2.Comments?.Count)
                {
                    return false;
                }
                for (int i = 0; i < caff1.Comments?.Count; i++)
                {
                    sameComments = CompareCommentDTOs(caff1.Comments[i], caff2.Comments[i]);
                }
            }
            return caff1.Name == caff2.Name &&
                   caff1.UploaderId == caff2.UploaderId &&
                   caff1.UploaderUsername == caff2.UploaderUsername &&
                   sameComments;

        }

        internal static bool CompareCommentDTOs(CommentDTO comment1, CommentDTO comment2)
        {
            return comment1.Id == comment2.Id &&
                   comment1.Text == comment2.Text &&
                   CompareUserDTOs(comment1.User, comment2.User);
        }

        internal static async Task<CaffDTO> GetCaff(HttpClient client, string id)
        {
            var response = await client.GetAsync($"{CaffsUrl}/{id}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CaffDTO>(content);
        }

        internal static async Task<CommentDTO> GetComment(HttpClient client, string id)
        {
            var response = await client.GetAsync($"{CommentsUrl}/{id}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CommentDTO>(content);
        }

        internal static async Task Login(HttpClient client, RegisterDTO user)
        {
            UriBuilder builder = new("https://localhost:7206/api/test/login");
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["username"] = user.Username;
            query["password"] = user.Password;
            builder.Query = query.ToString();
            var response = await client.GetAsync(builder.ToString());
            var token = await response.Content.ReadAsStringAsync();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}

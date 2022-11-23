using HashidsNet;
using Models;

namespace Backend.Services
{
    public static class Mapper
    {
        private static Hashids userHasher = new("user", 8);
        private static Hashids caffHasher = new("caff", 8);
        private static Hashids commentHasher = new("comment", 8);

        public static void SetHeasher(string salt, int minLength)
        {
            userHasher = new Hashids(salt + "user", minLength);
            caffHasher = new Hashids(salt + "caff", minLength);
            commentHasher = new Hashids(salt + "comment", minLength);
        }

        public static int GetUserId(string key)
        {
            return userHasher.DecodeSingle(key);
        }

        public static string GetUserHash(int id)
        {
            return userHasher.Encode(id);
        }

        public static int GetCaffId(string key)
        {
            return caffHasher.DecodeSingle(key);
        }

        public static string GetCaffHash(int id)
        {
            return caffHasher.Encode(id);
        }

        public static int GetCommentId(string key)
        {
            return commentHasher.DecodeSingle(key);
        }

        public static string GetCommentHash(int id)
        {
            return commentHasher.Encode(id);
        }

        public static UserDTO ToUserDTO(User user)
        {
            return new UserDTO
            {
                Id = userHasher.Encode(user.Id),
                UserName = user.Username,
            };
        }

        public static CaffDTO ToCaffDTO(Caff caff)
        {
            var comments = new List<CommentDTO>();
            if (caff.Comments != null)
            {
                comments = caff.Comments.Select(c => ToCommentDTO(c)).ToList();
            }
            return new CaffDTO
            {
                Id = caffHasher.Encode(caff.Id),
                Name = caff.Name,
                Comments = comments,
                UploaderId = userHasher.Encode(caff.Uploader.Id),
                UploaderUsername = caff.Uploader.Username
            };
        }

        public static CommentDTO ToCommentDTO(Comment comment)
        {
            return new CommentDTO
            {
                Id = commentHasher.Encode(comment.Id),
                Text = comment.Text,
                User = ToUserDTO(comment.User)
            };
        }
    }
}

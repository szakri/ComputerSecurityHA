using Models;

namespace Backend.Services
{
    public class CaffException : Exception
    {
        public CaffException(string? message) : base(message){ }
    }

    public static class FileManager
    {
        public static string SaveFile(IFormFile file, User user)
        {
            if (file.FileName == "exception.caff")
            {
                throw new CaffException("The CAFF file was not correct!");
            }
            if (file.FileName == "download.caff")
            {
                var dir = Directory.GetCurrentDirectory();
                return Path.Combine(dir, "Files", "Test", "test");
            }
            if (file.FileName == "preview.caff")
            {
                var dir = Directory.GetCurrentDirectory();
                return Path.Combine(dir, "Previews", "preview");
            }
            var hashedId = Mapper.GetUserHash(user.Id);
            var current = Directory.GetCurrentDirectory();
            var combined = Path.Combine(current, "Files", hashedId);
            Directory.CreateDirectory(combined);
            var filePath = Path.Combine($"{hashedId}",
                $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.UtcNow:yyyy.MM.dd.HH-mm-ss-f-ff}.caff");
            file.CopyTo(new FileStream(Path.Combine("Files", filePath), FileMode.Create));
            GeneratePreview(filePath);
            return filePath;
        }

        private static void GeneratePreview(string filePath)
        {
            var current = Directory.GetCurrentDirectory();
            var combined = Path.Combine(current, "Previews", Path.GetDirectoryName(filePath));
            Directory.CreateDirectory(combined);
            var fileName = Path.GetFileName(filePath);
            var dotIndex = fileName.LastIndexOf('.');
            var withoutExtension = fileName.Substring(0, dotIndex);
            File.Copy(Path.Combine("Previews", "test.gif"), Path.Combine(combined, $"{withoutExtension}.gif"));
        }
    }
}

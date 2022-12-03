using Models;
using System.Runtime.InteropServices;

namespace Backend.Services
{
    public class CaffException : Exception
    {
        public CaffException(string? message = null) : base(message){ }
    }

    public static class FileManager
    {
        [DllImport(@"\Services\parser.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int main(int argc, string[] argv);

        public static string SaveFile(IFormFile file, User user)
        {
            var hashedId = Mapper.GetUserHash(user.Id);
            var current = Directory.GetCurrentDirectory();
            var combined = Path.Combine(current, "Files", hashedId);
            Directory.CreateDirectory(combined);
            var filePath = Path.Combine($"{hashedId}",
                $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.UtcNow:yyyy.MM.dd.HH-mm-ss-f-ff}.caff");
            file.CopyTo(new FileStream(Path.Combine("Files", filePath), FileMode.Create));
            try
            {
                GeneratePreview(filePath);
                return filePath;
            }
            catch (CaffException ex)
            {
                //File.Delete(Path.Combine("Files", filePath));
                throw ex;
            }
        }

        private static void GeneratePreview(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (directory == null)
            {
                throw new CaffException();
            }
			var current = Directory.GetCurrentDirectory();
            var previews = Path.Combine(current, "Previews", directory);
            Directory.CreateDirectory(previews);
            var fileName = Path.GetFileName(filePath);
            var dotIndex = fileName.LastIndexOf('.');
            var withoutExtension = fileName[..dotIndex];
            var erno = main(3, new string[] { "parser", Path.Combine(current, "Files", filePath),
                Path.Combine(previews, $"{withoutExtension}.gif") });
            if (erno != 0)
            {
                throw new CaffException("The CAFF file was not correct!");
            }
        }
    }
}

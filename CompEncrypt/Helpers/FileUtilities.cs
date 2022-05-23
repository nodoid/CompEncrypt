using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using CommunityToolkit.Mvvm.Messaging;

using CompEncrypt.Models;

namespace CompEncrypt.Helpers
{
    public class FileUtilities
    {
        IMessenger messenger = (IMessenger)Startup.ServiceProvider.GetService(typeof(IMessenger));

        static Guid AppCuid { get; set; }
        public string Filename { get; set; }
        public string StoreDir { get; set; }
        public FileUtilities(string image)
        {
            StoreDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Filename = $"{StoreDir}/minion.jpg";
            if (File.Exists(Filename))
                File.Delete(Filename);

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("CompEncrypt.Resources.Images.minion.jpg");
                var fileStream = File.Create(Filename);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            stream.Close();
        }

        public bool DeleteFile()
        {
            var ok = true;
            try
            {
                File.Delete($"{StoreDir}/{Filename}");
            }
            catch (Exception)
            {
#if DEBUG
                Console.WriteLine($"File {Filename} not found");
#endif
                ok = false;
            }
            return ok;
        }

        public async Task<string> ConvertToBase64()
        {
            AppCuid = Guid.NewGuid();

            if (string.IsNullOrEmpty(Filename))
            {
                messenger.Send(new BooleanMessage { BoolValue = false, Message = "Done" });
                return "ImageNotFound";
            }

            if (!File.Exists(Filename))
            {
                messenger.Send(new BooleanMessage { BoolValue = false, Message = "Done" });
                return "ImageNotFound";
            }

            var base64ImageRepresentation = "ImageNotFound";
            
            var imageArray = await File.ReadAllBytesAsync(Filename);
            base64ImageRepresentation = Convert.ToBase64String(imageArray);

            while (base64ImageRepresentation == "ImageNotFound") { }
            messenger.Send(new BooleanMessage { BoolValue = true, Message = "Done" });
            return base64ImageRepresentation;
        }

        public ImageSource ConvertToImage(string baseCode) => 
            ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(baseCode)));
      
        public bool EncryptFile(string data)
        {
            var enc = Encoding.ASCII.GetBytes(AppCuid.ToString());
            try
            {
                using FileStream myStream = new FileStream($"{StoreDir}/encrypted.enc",
                    FileMode.OpenOrCreate);

                using Aes aes = Aes.Create();
                aes.Key = enc;

                var iv = aes.IV;
                myStream.Write(iv, 0, iv.Length);

                using CryptoStream cryptStream = new CryptoStream(
                    myStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

                using var sWriter = new StreamWriter(cryptStream);
                sWriter.WriteLine(data);
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Could not encrypt : {ex.Message}");
#endif
            }
            return false;
        }

        public async Task CompressFile()
        {
            var fileToBeCompressed = $"{StoreDir}/encrypted.enc";
            var zipFilename = $"{StoreDir}/encrypted.zip";

            using (var target = new FileStream(zipFilename, FileMode.Create, FileAccess.Write))
            using (var alg = new GZipStream(target, CompressionMode.Compress))
            {
                var data = await File.ReadAllBytesAsync(fileToBeCompressed);
                alg.Write(data, 0, data.Length);
                alg.Flush();
                messenger.Send(new BooleanMessage { BoolValue = true, Message = "Compress" });
            }
        }

        public async Task DecompressFile()
        {
            var compressedFile = $"{StoreDir}/encrypted.zip";
            var originalFileName = $"{StoreDir}/encrypted.enc";

            using (var zipFile = new FileStream(compressedFile, FileMode.Open, FileAccess.Read))
            using (var originalFile = new FileStream(originalFileName, FileMode.Create, FileAccess.Write))
            using (var alg = new GZipStream(zipFile, CompressionMode.Decompress))
            {
                while (true)
                {
                    var filesize = File.OpenRead(compressedFile).Length;
                    var buffer = new byte[(int)filesize];
                    var bytesRead = await alg.ReadAsync(buffer, 0, buffer.Length);

                    originalFile.Write(buffer, 0, bytesRead);

                    if (bytesRead != buffer.Length)
                        break;
                    else
                        messenger.Send(new BooleanMessage { BoolValue = true, Message = "Decompress" });
                }
            }
        }

        public bool DecryptFile()
        {
            var enc = Encoding.ASCII.GetBytes(AppCuid.ToString());
            try
            {
                using FileStream myStream = new FileStream($"{StoreDir}/encrypted.enc",
                    FileMode.Open);

                using Aes aes = Aes.Create();
                var iv = new byte[aes.IV.Length];
                myStream.Read(iv, 0, iv.Length);

                using CryptoStream cryptStream = new CryptoStream(
                   myStream, aes.CreateDecryptor(enc, iv), CryptoStreamMode.Read);

                var outData = "";
                using (var sWriter = new StreamWriter(cryptStream))
                {
                    using (var sReader = new StreamReader(cryptStream))
                        outData = sReader.ReadToEnd();
                    sWriter.WriteLine(outData);
                }
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Decrypt failed : {ex.Message}");
#endif
            }
            return false;
        }
    }
}


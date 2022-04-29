using System.IO.Compression;
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
            public bool DeleteFile(string filename)
        {
            var ok = true;
            try
            {
                File.Delete($"{Constants.Constants.DataPath}\\{filename}");
            }
            catch (Exception)
            {
#if DEBUG
                Console.WriteLine($"File {filename} not found");
#endif
                ok = false;
            }
            return ok;
        }

        public async Task<string> ConvertToBase64(string filename)
        {
            AppCuid = Guid.NewGuid();

            if (string.IsNullOrEmpty(filename))
            {
                messenger.Send<BooleanMessage>(new BooleanMessage { BoolValue = false, Message = "Done" });
                return "ImageNotFound";
            }

            string base64ImageRepresentation = "ImageNotFound";
            if (File.Exists(filename))
            {
                var imageArray = await File.ReadAllBytesAsync(filename);
                base64ImageRepresentation = Convert.ToBase64String(imageArray);
            }
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
                using FileStream myStream = new FileStream($"{Constants.Constants.CompressPath}\\encrypted.enc",
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
            var fileToBeCompressed = $"{Constants.Constants.CompressPath}\\encrypted.enc";
            var zipFilename = $"{Constants.Constants.CompressPath}\\encrypted.zip";

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
            var compressedFile = $"{Constants.Constants.CompressPath}\\encrypted.zip";
            var originalFileName = $"{Constants.Constants.CompressPath}\\encrypted.enc";

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

        public bool DecryptFile(string filename)
        {
            var enc = Encoding.ASCII.GetBytes(AppCuid.ToString());
            try
            {
                using FileStream myStream = new FileStream($"{Constants.Constants.CompressPath}\\encrypted.enc",
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


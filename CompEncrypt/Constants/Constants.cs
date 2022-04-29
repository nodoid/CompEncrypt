namespace CompEncrypt.Constants
{
    public class Constants
    {
        public static string DataPath { get; private set; } = Environment.SpecialFolder.MyPictures.ToString();
        public static string CompressPath { get; private set; } = Environment.SpecialFolder.MyDocuments.ToString();
    }
}

using System.Reflection;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using CompEncrypt.Helpers;
using CompEncrypt.Models;

namespace CompEncrypt.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        IMessenger messenger = (IMessenger)Startup.ServiceProvider.GetService(typeof(IMessenger));
        int posn = 0;

        public void Init()
        {
            messenger.Register<BooleanMessage>(this, (m, t) =>
            {
                switch(t.Message)
                {
                    case "Done":
                        CanContinue = t.BoolValue;
                        break;
                    case "Decompress":
                        DecompressDone = t.BoolValue;
                        if (t.BoolValue)
                            posn++;
                        break;
                    case "Compress":
                        CompressDone = t.BoolValue;
                        if (t.BoolValue)
                        {
                            Message = Languages.Resources.Pause;
                            Device.StartTimer(TimeSpan.FromSeconds(5), () => false);
                            posn++;
                        }

                        break;
                }
            });
        }

        [ObservableProperty]
        bool compressDone;

        [ObservableProperty]
        bool decompressDone;

        bool isBusy = false;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                SetProperty(ref isBusy, value);
                WeakReferenceMessenger.Default.Send(new BooleanMessage { BoolValue = value, Message = "IsBusy" });
            }
        }

        [ObservableProperty]
        string message;

        [ObservableProperty]
        bool canContinue;

        [ObservableProperty]
        ImageSource imageSource;

        [ObservableProperty]
        string stringImage;

        RelayCommand startCompressionCommand;
        public RelayCommand StartCompressionCommand => startCompressionCommand ?? new RelayCommand(async () =>
        {
            IsBusy = true;
            var assembly = Assembly.GetExecutingAssembly().FullName.Split(',').FirstOrDefault();
            var image = $"{assembly}.Resources.Images.minion.jpg";

            var helpers = new FileUtilities(image);
            switch (posn)
            {
                case 0:
                    Message = Languages.Resources.Stage1;
                    StringImage = await helpers.ConvertToBase64();
                    posn++;
                    break;
                case 1:
                    while (!CanContinue) { }
                    Message = Languages.Resources.Stage2;
                    if (helpers.EncryptFile(stringImage))
                    {
                        posn++;
                    }
                    break;
                case 2:
                    Message = Languages.Resources.Stage3;
                    await helpers.CompressFile();
                    break;
                case 3:
                    Message = Languages.Resources.Stage4;
                    await helpers.DecompressFile();
                    break;
                case 4:
                    Message = Languages.Resources.Stage5;
                    if (helpers.DecryptFile())
                        posn++;
                    break;
                case 5:
                    Message = Languages.Resources.Stage6;
                    IsBusy = false;
                    ImageSource = helpers.ConvertToImage(StringImage);
                    break;
            }
        })
        {

        };
    }
}

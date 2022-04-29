namespace CompEncrypt;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        ViewModel.Init();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsBusy")
        {
            if (ViewModel.IsBusy)
            {
                await LabelLoad.RotateTo(360, 1000, Easing.Linear);
                LabelLoad.Rotation = 0;
            }
        }
    }
}


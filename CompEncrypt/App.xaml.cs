using System.Globalization;

using CompEncrypt.Interfaces;

namespace CompEncrypt;

public partial class App : Application
{
	public static IServiceProvider Service { get; set; }
	public App()
	{
		var netLanguage = DependencyService.Get<ILocalize>().GetCurrent();
		Languages.Resources.Culture = new CultureInfo(netLanguage);
		DependencyService.Get<ILocalize>().SetLocale();

		InitializeComponent();

		Service = Startup.Init();
		InitializeComponent();

		MainPage = new AppShell();
	}
}

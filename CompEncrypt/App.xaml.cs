using System.Globalization;
using CompEncrypt.Setups;

namespace CompEncrypt;

public partial class App : Application
{
	public static IServiceProvider Service { get; set; }
	public App()
	{
		var nl = new Locales();
		Languages.Resources.Culture = new CultureInfo(nl.GetCurrent());
		nl.SetLocale();
		InitializeComponent();
		Service = Startup.Init();

		MainPage = new AppShell();
	}
}

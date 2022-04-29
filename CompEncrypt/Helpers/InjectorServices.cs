using CommunityToolkit.Mvvm.Messaging;

using CompEncrypt.ViewModels;

namespace CompEncrypt.Helpers
{
    public static class InjectionContainer
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            var i = new ServiceCollection();

            i.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

            services = i;

            return services;
        }

        public static IServiceCollection ConfigureViewModels(this IServiceCollection services)
        {
            services.AddTransient<MainViewModel>();

            return services;
        }

    }
}

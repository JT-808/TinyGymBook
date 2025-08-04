using Uno.Resizetizer;
using Tiny_GymBook.Services.Trainingsplanservice;

namespace Tiny_GymBook;

public partial class App : Application
{

    public App()
    {
        // Diesen Aufruf brauchst du
        this.InitializeComponent();

    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }




    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .UseLocalization()
                .UseSerialization((context, services) => services
                    .AddContentSerializer(context))
                .UseHttp((context, services) =>
                {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<ShellViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<SecondViewModel>();
                    services.AddTransient<PlanDetailViewModel>();
                    //     services.AddSingleton<ITrainingsplanService, JsonTrainingsplanService>();
                    services.AddSingleton<ITrainingsplanService, SqliteTrainingsplanService>();
                    // Hier Ihre eigenen Services registrieren
                })
                .UseNavigation(RegisterRoutes)
            );

        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<MainPage, MainViewModel>(),
             new ViewMap<SecondPage, SecondViewModel>(),
            //  new ViewMap<PlanDetail, PlanDetailViewModel>(),
            new DataViewMap<PlanDetail, PlanDetailViewModel, Trainingsplan>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new ("Main", View: views.FindByViewModel<MainViewModel>(), IsDefault:true),
                    new ("Second", View: views.FindByViewModel<SecondViewModel>()),
                    new ("PlanDetail", View: views.FindByViewModel<PlanDetailViewModel>())
                ]
            )
        );
    }
}

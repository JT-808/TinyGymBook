using Uno.Resizetizer;
using Tiny_GymBook.Services.Trainingsplanservice;
using System.Diagnostics;

namespace Tiny_GymBook;

public partial class App : Application
{

    public App()
    {
        // Diesen Aufruf brauchst du
        this.InitializeComponent();

        // wenn App minimiert wird
        this.Suspending += OnSuspending;

    }

    private async void OnSuspending(object sender, SuspendingEventArgs e)
    {
        // TODO: ViewModel/Service aufrufen und speichern!
        var deferral = e.SuspendingOperation.GetDeferral();
        try
        {
            // Beispiel: alle Änderungen speichern
            await SaveAllDataAsync();
        }
        finally
        {
            deferral.Complete();
        }
    }

    private async Task SaveAllDataAsync()
    {
        // Zugriff auf dein ViewModel/Service
        // z.B. (singleton, DI, ServiceLocator ...)
        // await DeinTrainingsplanService.SpeichereAlleOffenenEintraegeAsync();
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

        if (MainWindow.Content is FrameworkElement fe)
            Debug.WriteLine($"[DEBUG] MainWindow.Content DataContext: {fe.DataContext}");
        else
            Debug.WriteLine($"[DEBUG] MainWindow.Content ist kein FrameworkElement!");


        var trainingsplanService = Host.Services.GetRequiredService<ITrainingsplanService>();
        await trainingsplanService.InitAsync();

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

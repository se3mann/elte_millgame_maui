using Mill.Persistence;
using Mill.Model;
using Mill.ViewModel;

namespace Mill;

public partial class App : Application
{
    private const string SuspendedGameSavePath = "SuspendedGame";

    private readonly AppShell _appShell;
    private readonly IMillDataAccess _millDataAccess;
    private readonly MillGameModel _millGameModel;
    private readonly MillViewModel _millViewModel;
    private readonly IStore _millStore;
    public App()
	{
		InitializeComponent();

        _millStore = new MillStore();
        _millDataAccess = new MillFileDataAccess(FileSystem.AppDataDirectory);

        _millGameModel = new MillGameModel(_millDataAccess);
        _millViewModel = new MillViewModel(_millGameModel);

        _appShell = new AppShell(_millStore, _millDataAccess, _millGameModel, _millViewModel)
        {
            BindingContext = _millViewModel
        };

		MainPage = _appShell;
	}

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        // az alkalmazás indításakor
        window.Created += (s, e) =>
        {
            // új játékot indítunk
            _millGameModel.NewGame();
        };

        // amikor az alkalmazás fókuszba kerül
        window.Activated += (s, e) =>
        {
            if (!File.Exists(Path.Combine(FileSystem.AppDataDirectory, SuspendedGameSavePath)))
                return;

            Task.Run(async () =>
            {
                // betöltjük a felfüggesztett játékot, amennyiben van
                try
                {
                    await _millGameModel.LoadGameAsync(SuspendedGameSavePath);
                }
                catch
                {
                }
            });
        };

        // amikor az alkalmazás fókuszt veszt
        window.Deactivated += (s, e) =>
        {
            Task.Run(async () =>
            {
                try
                {
                    // elmentjük a jelenleg folyó játékot
                    await _millGameModel.SaveGameAsync(SuspendedGameSavePath);
                }
                catch
                {
                }
            });
        };
        return window;
    }
}

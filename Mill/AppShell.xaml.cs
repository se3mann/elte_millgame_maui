using Mill.Persistence;
using Mill.Model;
using Mill.ViewModel;
using Mill.View;

namespace Mill;

public partial class AppShell : Shell
{
    #region Fields
    private IMillDataAccess _millDataAccess;
    private readonly MillGameModel _millGameModel;
    private readonly MillViewModel _millViewModel;

    private readonly IDispatcherTimer _timer;

    private readonly IStore _store;
    private readonly StoredGameBrowserModel _storedGameBrowserModel;
    private readonly StoredGameBrowserViewModel _storedGameBrowserViewModel;

    #endregion

    #region Application methods
    public AppShell(IStore millStore,
        IMillDataAccess millDataAccess,
        MillGameModel millGameModel,
        MillViewModel millViewModel)
	{
		InitializeComponent();

        _store= millStore;
        _millDataAccess= millDataAccess;
        _millViewModel= millViewModel;
        _millGameModel= millGameModel;

        _millGameModel.GameOver += MillGameModel_GameOver;
        _millGameModel.PlayerHasToPass += Model_PlayerHasToPass;

        _millViewModel.NewGame += MillViewModel_NewGame;
        _millViewModel.LoadGame += MillViewModel_LoadGame;
        _millViewModel.SaveGame += MillViewModel_SaveGame;
        _millViewModel.ExitGame += MillViewModel_ExitGame;

        // a játékmentések kezelésének összeállítása
        _storedGameBrowserModel = new StoredGameBrowserModel(_store);
        _storedGameBrowserViewModel = new StoredGameBrowserViewModel(_storedGameBrowserModel);
        _storedGameBrowserViewModel.GameLoading += StoredGameBrowserViewModel_GameLoading;
        _storedGameBrowserViewModel.GameSaving += StoredGameBrowserViewModel_GameSaving;
    }
    #endregion

    #region Model event handlers

    /// <summary>
    ///     Játék végének eseménykezelője.
    /// </summary>
    private async void MillGameModel_GameOver(object? sender, MillEventArgs e)
    {
        String strPlayer;
        strPlayer = e.CurrentPlayer == Player.Player1 ? "Első játékos" : "Második játékos";

        // győzelemtől függő üzenet megjelenítése
        await DisplayAlert("Malom játék",
            "Gratulálok, győztél!" + Environment.NewLine + strPlayer + "!",
            "OK"); 
    }

    private void Model_PlayerHasToPass(object? sender, PassingEventArgs e)
    {
        String strPlayer;
        String action;
        strPlayer = e.CurrentPlayer == Player.Player1 ? "Első játékos" : "Második játékos";
        action = e.MoveToken == true ? "mozogni" : "ellenséges bábut levenni";
        DisplayAlert(strPlayer + " passzolnod kell, nem tusz " + action + "!" + Environment.NewLine,
                            "Passzolj!", "OK");                            
    }

    #endregion

    #region ViewModel event handlers

    private async void MillViewModel_ExitGame(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage
        {
            BindingContext = _millViewModel
        }); // átnavigálunk a beállítások lapra
    }

    private async void MillViewModel_SaveGame(object sender, EventArgs e)
    {
        await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
        await Navigation.PushAsync(new SaveGamePage
        {
            BindingContext = _storedGameBrowserViewModel
        }); // átnavigálunk a lapra
    }

    private async void MillViewModel_LoadGame(object sender, EventArgs e)
    {
        await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
        await Navigation.PushAsync(new LoadGamePage
        {
            BindingContext = _storedGameBrowserViewModel
        }); // átnavigálunk a lapra
    }

    private void MillViewModel_NewGame(object sender, EventArgs e)
    {
        _millGameModel.NewGame();
    }

    /// <summary>
    ///     Betöltés végrehajtásának eseménykezelője.
    /// </summary>
    private async void StoredGameBrowserViewModel_GameLoading(object? sender, StoredGameEventArgs e)
    {
        await Navigation.PopAsync(); // visszanavigálunk

        // betöltjük az elmentett játékot, amennyiben van
        try
        {
            await _millGameModel.LoadGameAsync(e.Name);

            // sikeres betöltés
            await Navigation.PopAsync(); // visszanavigálunk a játék táblára
            await DisplayAlert("Malom játék", "Sikeres betöltés.", "OK");

            // csak akkor indul az időzítő, ha sikerült betölteni a játékot
        }
        catch
        {
            await DisplayAlert("Malom játék", "Sikertelen betöltés.", "OK");
        }
    }

    /// <summary>
    ///     Mentés végrehajtásának eseménykezelője.
    /// </summary>
    private async void StoredGameBrowserViewModel_GameSaving(object? sender, StoredGameEventArgs e)
    {
        await Navigation.PopAsync(); // visszanavigálunk
        try
        {
            // elmentjük a játékot
            await _millGameModel.SaveGameAsync(e.Name);
            await DisplayAlert("Malom játék", "Sikeres mentés.", "OK");
        }
        catch
        {
            await DisplayAlert("Malom játék", "Sikertelen mentés.", "OK");
        }
    }
    #endregion
}

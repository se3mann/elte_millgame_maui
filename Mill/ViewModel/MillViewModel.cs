using Mill.Model;
using Mill.Persistence;
using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Mill.ViewModel
{
    public class MillViewModel : ViewModelBase
    {
        #region Fields

        private MillGameModel _model;
        private Int32 _tableSize;

        #endregion

        #region Properties

        public DelegateCommand NewGameCommand { get; private set; }

        public DelegateCommand LoadGameCommand { get; private set; }
        
        public DelegateCommand SaveGameCommand { get; private set; }

        public DelegateCommand ExitCommand { get; private set; }

        public ObservableCollection<MillField> Fields { get; set; }

        public List<(int, int)> GridPoints { get; set; }

        public String CurrentPlayer { get { return _model.CurrentPlayer == Player.Player1? "Első játékos" : "Második játékos"; } }

        public String CurrentAction { 
            get 
            {
                string nextStep = "";
                if (_model.CurrentAction == Mill.Model.Action.Adding) nextStep = "Tegyél fel egy korongot!";
                if (_model.CurrentAction == Mill.Model.Action.Removing) nextStep = "Vegyél le az ellenféltől egy korongot!";
                if (_model.CurrentAction == Mill.Model.Action.MoveDest) nextStep = "Jelöld ki mivel akarsz mozogni!";
                if (_model.CurrentAction == Mill.Model.Action.MoveTarget) nextStep = "Jelöld ki hova akarsz mozogni!";
                return nextStep;
            } 
        }

        public int Player1Talon { get { return _model.Table.Player1UnusedToken; } }

        public int Player2Talon { get { return _model.Table.Player2UnusedToken; } }

        public int TableSize
        {
            get => _tableSize;
            set
            {
                _tableSize = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GameTableRows));
                OnPropertyChanged(nameof(GameTableColumns));
            }
        }

        public RowDefinitionCollection GameTableRows
        {
            get => new RowDefinitionCollection(Enumerable.Repeat(new RowDefinition(GridLength.Star), TableSize).ToArray());
        }

        public ColumnDefinitionCollection GameTableColumns
        {
            get => new ColumnDefinitionCollection(Enumerable.Repeat(new ColumnDefinition(GridLength.Star), TableSize).ToArray());
        }
        #endregion

        #region Events 

        public event EventHandler? NewGame;
        public event EventHandler? LoadGame;
        public event EventHandler? ExitGame;
        public event EventHandler? SaveGame;

        #endregion

        #region Constructors

        public MillViewModel(MillGameModel model) 
        {
            _model= model;
            _model.GameAdvanced += Model_GameAdvanced;
            _model.GameOver += Model_GameOver;
            _model.PlayerHasToPass += Model_PlayerHasToPass;
            _model.GameCreated += Model_GameCreated;

            NewGameCommand = new DelegateCommand(param => OnNewGame());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());

            TableSize = 7;

            SetupPoints();
            Fields = new ObservableCollection<MillField>();
            GenerateTable();
            RefreshTable();
        }

        

        #endregion

        #region Private methods

        private void RefreshTable()
        {
            for (int i = 0; i < _model.Table.Fields.Length; i++)
            {
                int index = ModelTableIndexToGridListIndex(i);
                int player = _model.Table.GetField(i).Player;
                switch (player)
                {
                    case 0:
                        Fields[index].Player = 0;
                        break;
                    case 1:
                        Fields[index].Player = 1;
                        break;
                    case 2:
                        Fields[index].Player = 2;
                        break;
                    default:
                        Fields[index].Player = 0;
                        break;
                }
            }

            OnPropertyChanged("CurrentPlayer");
            OnPropertyChanged("CurrentAction");
            OnPropertyChanged("Player1Talon");
            OnPropertyChanged("Player2Talon");
        }

        private void StepGame(int index)
        {
            _model.Step(index);
            Fields[ModelTableIndexToGridListIndex(index)].Player = _model.Table.GetField(index).Player;
        }

        private void GenerateTable()
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Fields.Add(new MillField
                    {
                        X = i,
                        Y = j,
                        InGame = false,
                        Number= -1,
                });
                }
            }

            for (int i = 0; i < GridPoints.Count; i++)
            {
                for (int j = 0; j < Fields.Count; j++)
                {
                    if (GridPoints[i].Item1 == Fields[j].X && GridPoints[i].Item2 == Fields[j].Y)
                    {
                        Fields[j].InGame = true;
                        Fields[j].Number = i;
                        Fields[j].Player = 0;
                        Fields[j].StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)));
                    }
                }
            }
        }

        private void SetupPoints()
        {
            GridPoints = new List<(int, int)>();
            GridPoints.Add((0, 0));
            GridPoints.Add((0, 3));
            GridPoints.Add((0, 6));
            GridPoints.Add((1, 1));
            GridPoints.Add((1, 3));
            GridPoints.Add((1, 5));
            GridPoints.Add((2, 2));
            GridPoints.Add((2, 3));
            GridPoints.Add((2, 4));
            GridPoints.Add((3, 0));
            GridPoints.Add((3, 1));
            GridPoints.Add((3, 2));
            GridPoints.Add((3, 4));
            GridPoints.Add((3, 5));
            GridPoints.Add((3, 6));
            GridPoints.Add((4, 2));
            GridPoints.Add((4, 3));
            GridPoints.Add((4, 4));
            GridPoints.Add((5, 1));
            GridPoints.Add((5, 3));
            GridPoints.Add((5, 5));
            GridPoints.Add((6, 0));
            GridPoints.Add((6, 3));
            GridPoints.Add((6, 6));
        }

        private int ModelTableIndexToGridListIndex(int index)
        {
            switch (index+1)
            {
                case 1: return 0;
                case 2: return 3;
                case 3: return 6;
                case 4: return 8;
                case 5: return 10;
                case 6: return 12;
                case 7: return 16;
                case 8: return 17;
                case 9: return 18;
                case 10: return 21;
                case 11: return 22;
                case 12: return 23;
                case 13: return 25;
                case 14: return 26;
                case 15: return 27;
                case 16: return 30;
                case 17: return 31;
                case 18: return 32;
                case 19: return 36;
                case 20: return 38;
                case 21: return 40;
                case 22: return 42;
                case 23: return 45;
                case 24: return 48;

                default:
                    return -1;
            }
        }

        #endregion

        #region Game event handlers

        private void Model_GameAdvanced(object? sender, MillEventArgs e)
        {
            OnPropertyChanged("CurrentPlayer");
            OnPropertyChanged("CurrentAction");
            OnPropertyChanged("Player1Talon");
            OnPropertyChanged("Player2Talon");
        }

        private void Model_PlayerHasToPass(object? sender, PassingEventArgs e)
        {
            //
        }

        private void Model_GameOver(object? sender, MillEventArgs e)
        {
            //
        }

        private void Model_GameCreated(object? sender, MillEventArgs e)
        {
            //Fields.Clear();
            //GenerateTable();
            RefreshTable();
        }

        #endregion

        #region Event methods

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}

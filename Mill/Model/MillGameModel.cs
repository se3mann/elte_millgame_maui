using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mill.Persistence;

namespace Mill.Model
{
    public enum Player { Player1, Player2 }
    public enum GamePhase { Phase1, Phase2 }
    public enum Action { Adding, MoveDest, MoveTarget, Removing}
    public class MillGameModel
    {
        #region Fields
        private MillTable _table;
        private Player _currentPlayer;
        private Boolean _currentPlayerWon;
        private GamePhase _currentPhase;
        private int _lastField;
        private Action _currentAction;
        private IMillDataAccess _dataAccess;
        #endregion

        #region Properties
        public Player CurrentPlayer { get { return _currentPlayer; } }
        public MillTable Table { get { return _table; } }
        public Boolean IsGameOver { get { return (_currentPlayerWon); } }
        public int LastField { get { return _lastField; } set { _lastField = value; } }
        public Action CurrentAction { get { return _currentAction; } set { _currentAction = value; } }
        public GamePhase CurrentPhase { get { return _currentPhase; } set { _currentPhase = value; } }
        public bool CurrentPlayerWon { get { return _currentPlayerWon; } set { _currentPlayerWon = value; } }
        #endregion

        #region Events
        public event EventHandler<MillEventArgs>? GameOver;
        public event EventHandler<MillEventArgs>? GameAdvanced;
        public event EventHandler<PassingEventArgs>? PlayerHasToPass;
        public event EventHandler<MillEventArgs>? GameCreated;
        #endregion

        #region Constructor
        public MillGameModel(IMillDataAccess dataAccess)
        {
            _table = new MillTable();
            _currentPlayer = Player.Player1;
            _currentPlayerWon = false;
            _currentPhase = GamePhase.Phase1;
            _dataAccess = dataAccess;
        }
        #endregion

        #region Public methods
        public void NewGame()
        {
            _table = new MillTable();
            _currentPlayer = Player.Player1;
            _currentPlayerWon = false;
            _currentPhase = GamePhase.Phase1;
            _currentAction = Action.Adding;
            OnGameCreated();
        }

        public void Step(int field)
        {
            if (_currentPhase == GamePhase.Phase1) //we can only add or remove token
            {
                if (CurrentAction == Action.Adding) AddTokenFromTalon(field);
                if (CurrentAction == Action.Removing)
                {
                    RemoveToken(field);
                }
                //after action in game phase 1, check if continue with phase 2
                SetNextPhaseIfPossible();
            }
            else //we can only move or remove token
            {
                if (CurrentAction == Action.MoveDest) MoveToTargetNextStep(field);
                if (CurrentAction == Action.MoveTarget) MoveToTarget(field);
                if (CurrentAction == Action.Removing)
                {
                    RemoveToken(field);
                }
            }
            OnGameAdvanced();
        }

        public async Task SaveGameAsync(String path)
        {
            switch (CurrentAction)
            {
                case Action.Adding:
                    _table.CurrentAction = 0;
                    break;
                case Action.MoveDest:
                    _table.CurrentAction = 1;
                    break;
                case Action.MoveTarget:
                    _table.CurrentAction = 2;
                    break;
                case Action.Removing:
                    _table.CurrentAction = 3;
                    break;
                default:
                    break;
            }
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            await _dataAccess.SaveAsync(path, _table);
        }

        public async Task LoadGameAsync(String path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            _table = await _dataAccess.LoadAsync(path);
            if (_table.LastPlayer == 1)
            {
                _currentPlayer = Player.Player2; 
            }
            else
            {
                _currentPlayer = Player.Player1;
            }

            if (_table.Player1UnusedToken == 0 && _table.Player2UnusedToken == 0) CurrentPhase = GamePhase.Phase2;
            else CurrentPhase = GamePhase.Phase1;

            switch (_table.CurrentAction)
            {
                case 0:
                    CurrentAction = Action.Adding;
                    break;
                case 1:
                    CurrentAction = Action.MoveDest;
                    break;
                case 2:
                    CurrentAction = Action.MoveTarget;
                    break;
                case 3:
                    CurrentAction = Action.Removing;
                    break;
                default:
                    CurrentAction = Action.Adding;
                    break;
            }

            OnGameCreated();
        }
        #endregion

        #region Private methods
        private void MoveToTarget(int field)
        {
            if (CurrentPlayer == Player.Player1)
            {
                if (_table.Fields[field].Player == 0 && _table.Fields[LastField].Neighbours.Contains(field))
                {
                    _table.Fields[field].Player = 1;
                    if (_table.CheckFieldInMill(1, field))
                    {
                        RemoveTokenNextStep();
                    }
                    else
                    {
                        SwapPlayer();
                        CurrentAction = Action.MoveDest;
                    }
                }
            }
            else
            {
                if (_table.Fields[field].Player == 0 && _table.Fields[LastField].Neighbours.Contains(field))
                {
                    _table.Fields[field].Player = 2;
                    if (_table.CheckFieldInMill(2, field))
                    {
                        RemoveTokenNextStep();
                    }
                    else
                    {
                        SwapPlayer();
                        CurrentAction = Action.MoveDest;
                    }
                }
            }
        }
        private void MoveToTargetNextStep(int field)
        {
            if (CurrentPlayer == Player.Player1)
            {
                if (_table.Fields[field].Player == 1 && CheckFieldMovable(field))
                {
                    LastField = field;
                    CurrentAction = Action.MoveTarget;
                    Table.GetField(field).Player = 0;
                }
            }
            else
            {
                if (_table.Fields[field].Player == 2 && CheckFieldMovable(field))
                {
                    LastField = field;
                    CurrentAction = Action.MoveTarget;
                    Table.GetField(field).Player = 0;
                }
            }
        }
        private void AddTokenFromTalon(int field)
        {
            if (_currentPlayer == Player.Player1)
            {
                if (_table.Fields[field].Player == 0 && _table.Player1UnusedToken > 0)
                {
                    _table.Fields[field].Player = 1;
                    _table.Player1UnusedToken--;
                    _table.Player1TokenInTable++;
                    if (_table.CheckFieldInMill(1, field))
                    {
                        RemoveTokenNextStep();
                    } 
                    else
                    {
                        SwapPlayer();
                        CurrentAction = Action.Adding;
                    }
                }
            }
            else
            {
                if (_table.Fields[field].Player == 0 && _table.Player2UnusedToken > 0)
                {
                    _table.Fields[field].Player = 2;
                    _table.Player2UnusedToken--;
                    _table.Player2TokenInTable++;
                    if (_table.CheckFieldInMill(2, field))
                    {
                        RemoveTokenNextStep();
                    }
                    else
                    {
                        SwapPlayer();
                        CurrentAction = Action.Adding;
                    }
                }
            }
        }

        private void RemoveToken(int field)
        {
            if (_currentPlayer == Player.Player1)
            {
                if (_table.Fields[field].Player == 2 && !_table.CheckFieldInMill(2, field))
                {
                    _table.Fields[field].Player = 0;
                    _table.Player2TokenInTable--;
                    CheckCurrentPlayerWon();
                    if (!IsGameOver) SwapPlayer();
                    SetActionAfterRemove();
                }
            }
            else
            {
                if (_table.Fields[field].Player == 1 && !_table.CheckFieldInMill(1, field))
                {
                    _table.Fields[field].Player = 0;
                    _table.Player1TokenInTable--;
                    CheckCurrentPlayerWon();
                    if (!IsGameOver) SwapPlayer();
                    SetActionAfterRemove();
                }
            }
        }

        private void SwapPlayer()
        {
            if (_currentPlayer == Player.Player1)
            {
                _currentPlayer = Player.Player2;
                _table.LastPlayer = 1; 
            }
            else
            {
                _currentPlayer = Player.Player1;
                _table.LastPlayer = 2;
            }

            if (!CheckCurrentPlayerCanMove())
            {
                OnPlayerHasToPass(true);
            }
        }

        private void SetNextPhaseIfPossible()
        {
            if (_table.Player1UnusedToken == 0 && _table.Player2UnusedToken == 0 && CurrentAction != Action.Removing)
            {
                CurrentPhase = GamePhase.Phase2;
                CurrentAction = Action.MoveDest;
            }
        }

        private void RemoveTokenNextStep()
        {
            _currentAction = Action.Removing;
            if (!CheckCurrentPlayerCanRemove()) //when removing step happens, check player can remove one token from enemy
            {
                OnPlayerHasToPass(false);
                SetActionAfterRemove();
            }
        }

        private void SetActionAfterRemove()
        {
            if (CurrentPhase == GamePhase.Phase1) CurrentAction = Action.Adding;
            else CurrentAction = Action.MoveDest;
        }

        private void OnGameOver()
        {
            CurrentPlayerWon = true;
            if (GameOver != null) GameOver(this, new MillEventArgs(CurrentPlayer, true, CurrentAction, _table.Player1UnusedToken, _table.Player2UnusedToken));
        }

        private void OnGameAdvanced()
        {
            if (_table != null)
                if (GameAdvanced != null)
                        GameAdvanced(this, new MillEventArgs(CurrentPlayer, false, CurrentAction, _table.Player1UnusedToken, _table.Player2UnusedToken));
        }

        private void CheckCurrentPlayerWon()
        {
            if (CurrentPlayer == Player.Player1)
            {
                if (_table.Player2TokenInTable + _table.Player2UnusedToken < 3)
                {
                    OnGameOver();
                }
            }
            else
            {
                if (_table.Player1TokenInTable + _table.Player1UnusedToken < 3)
                {
                    OnGameOver();
                }
            }
        }

        private bool CheckFieldMovable(int field)
        {
            bool retval = false;
            foreach (int i in _table.Fields[field].Neighbours)
            {
                retval = retval || _table.Fields[i].Player == 0;
            }
            return retval;
        }

        private bool CheckCurrentPlayerCanRemove()
        {
            if (CurrentAction == Action.Removing)
            {
                int playerFromRemove = 2; //1 or 2 - not the current player
                if (CurrentPlayer == Player.Player2) playerFromRemove = 1;
                
                bool retval = false;
                for (int i = 0; i < Table.Fields.Length; i++)
                {
                    if (Table.GetField(i).Player == playerFromRemove)
                    {
                        retval = retval || !(Table.CheckFieldInMill(playerFromRemove, i)); //if there is one field, thet not in mill, retval will be true
                    }
                }
                return retval;
            }
            else return true; 
        }

        private bool CheckCurrentPlayerCanMove()
        {
            int player = 1; //1 or 2
            if (CurrentPlayer == Player.Player2) player = 2;
            if (CurrentPhase == GamePhase.Phase2)
            {
                bool retval = false;
                foreach (Field field in _table.Fields)
                {
                    if (field.Player == player)
                    {
                        foreach (int neighboor in field.Neighbours)
                        {
                            retval = retval || _table.GetField(neighboor).Player == 0; //if one neighboor field of current player token is free, he can move
                        }
                    }
                }
                return retval;
            }
            else return true;
        }
        private void OnPlayerHasToPass(bool moveToken)
        {
            if (PlayerHasToPass != null) PlayerHasToPass(this, new PassingEventArgs(moveToken, CurrentPlayer));
            //when player passed, change back player, performance loss when SwapPlayer() called back 
            if (_currentPlayer == Player.Player1)
            {
                _currentPlayer = Player.Player2;
                _table.LastPlayer = 1;
            }
            else
            {
                _currentPlayer = Player.Player1;
                _table.LastPlayer = 2;
            }
        }

        private void OnGameCreated()
        {
            if (GameCreated != null)
            {
                GameCreated(this, new MillEventArgs(CurrentPlayer, false, CurrentAction, Table.Player1UnusedToken, Table.Player2UnusedToken));
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mill.Model
{
    public class MillEventArgs : EventArgs
    {
        private Player _currentPlayer;
        private bool _currentPlayerWin;
        private Action _currentAction;
        private int _player1Talon;
        private int _player2Talon;

        public Player CurrentPlayer { get { return _currentPlayer; } }
        public bool CurrentPlayerWin { get { return _currentPlayerWin; } }
        public Action CurrentAction { get { return _currentAction; } }
        public int Player1Talon { get { return _player1Talon; } }
        public int Player2Talon { get { return _player2Talon; } }

        public MillEventArgs(Player currentPlayer, bool currentPlayerWin, Action currentAction, int player1Talon, int player2Talon)
        {
            _currentPlayer = currentPlayer;
            _currentPlayerWin = currentPlayerWin;
            _currentAction = currentAction;
            _player1Talon = player1Talon;
            _player2Talon = player2Talon;
        }
    }


}

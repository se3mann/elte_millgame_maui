using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mill.Model
{
    public class PassingEventArgs : EventArgs
    {
        private bool _moveToken; //true: can't move token, false: can't remove token
        private Player _currentPlayer;
        public Player CurrentPlayer { get { return _currentPlayer; } }

        public bool MoveToken { get { return _moveToken; } }

        public PassingEventArgs(bool moveToken, Player currentPlayer)
        {
            _moveToken= moveToken;
            _currentPlayer = currentPlayer;
        }
    }
}

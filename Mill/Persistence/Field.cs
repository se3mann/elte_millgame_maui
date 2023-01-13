using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mill.Persistence
{
    public class Field
    {
        #region Fields
        private int _player; //0 if free or 1 if player1 or 2 if player2
        #endregion

        #region Propterties
        public int Player { get { return _player; } set { _player = value; } }
        public int[] Neighbours { get; set; }
        #endregion

        #region Constructors
        public Field()
        {
            _player = 0;
            Neighbours = Array.Empty<int>();
        }
        public Field(int player, int[] neighbours) : this()
        {
            Player = player;
            Neighbours = neighbours;
        }

        #endregion
    }
}

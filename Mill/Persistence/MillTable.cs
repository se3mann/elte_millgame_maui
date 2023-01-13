using System;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Serialization;
using System.Linq;
using System.Diagnostics;

namespace Mill.Persistence
{
    public class MillTable
    {
        #region Fields

        private int _lastPlayer; //for saving and loading
        private int _currentAction; //for saving and loading
        private int _player1UnusedToken;
        private int _player2UnusedToken;
        private int _player1TokenInTable;
        private int _player2TokenInTable;
        private Field[] _fields;

        private readonly int[][] _mills;
        
        public const Int32 MAX_TOKEN = 9;

        #endregion

        #region Properties
        public int Player1TokenInTable { get { return _player1TokenInTable; } set { _player1TokenInTable = value; } }
        public int Player2TokenInTable { get { return _player2TokenInTable; } set { _player2TokenInTable = value; } }
        public int Player1UnusedToken { get { return _player1UnusedToken; } set { _player1UnusedToken = value; } }
        public int Player2UnusedToken { get { return _player2UnusedToken; } set { _player2UnusedToken = value; } }
        public int LastPlayer { get { return _lastPlayer; } set { _lastPlayer = value; } }
        public int CurrentAction { get { return _currentAction; } set { _currentAction = value; } }
        public Field[] Fields { get { return _fields; } }
        #endregion

        #region Constructors
        public MillTable()
        {
            _lastPlayer = 2; //the Player1 starts the game
            _currentAction = 0; //only used when save or load
            _player1UnusedToken = MAX_TOKEN;
            _player2UnusedToken = MAX_TOKEN;
            _player1TokenInTable = 0;
            _player2TokenInTable = 0;
            _fields = new Field[24];
            _mills = new int[16][];
            SetupMills();
            SetupGameFields();
            SetupFieldsNeighbours();
        }
        #endregion

        #region Public methods
        public Field GetField(int i)
        {
            if (i < 0 || i >= _fields.Length)
                throw new ArgumentOutOfRangeException("i", "The index out of range.");

            return _fields[i];
        }

        public bool CheckFieldInMill(int player, int field)
        {
            if (player < 1 || player > 2) return false;
            bool mill = false;
            for (int i = 0; i < _mills.Length; i++)
            {
                if (_mills[i].Contains(field))
                {
                    mill = mill || (Fields[_mills[i][0]].Player == player && Fields[_mills[i][1]].Player == player && Fields[_mills[i][2]].Player == player);
                }
            }
            return mill;   
        }

        #endregion

        #region Private methods

        /*
         * Connection between game fields and the array index in the code
         * The numbers represents the fields in the board as seen below, and also they are the index in the array of Fields in modell
         0			1			2
            3		4		5
                6	7	8
         9	10	11		12	13	14
                15	16	17
            18		19		20
         21			22			23
         */

        private void SetupGameFields()
        {
            for (int i = 0; i < _fields.Length; i++)
            {
                _fields[i] = new Field();
            }
        }

        private void SetupFieldsNeighbours()
        {
            for(int i = 0; i < _fields.Length; i++)
            {
                _fields[i].Neighbours = GetNeighbours(i);
            }
        }

        private void SetupMills()
        {
            _mills[0] = new int[] { 0, 1, 2 };
            _mills[1] = new int[] { 3, 4, 5 };
            _mills[2] = new int[] { 6, 7, 8 };
            _mills[3] = new int[] { 9, 10, 11 };
            _mills[4] = new int[] { 12, 13, 14 };
            _mills[5] = new int[] { 15, 16, 17 };
            _mills[6] = new int[] { 18, 19, 20 };
            _mills[7] = new int[] { 21, 22, 23 };
            _mills[8] = new int[] { 0, 9, 21 };
            _mills[9] = new int[] { 3, 10, 18 };
            _mills[10] = new int[] { 6, 11, 15 };
            _mills[11] = new int[] { 1, 4, 7 };
            _mills[12] = new int[] { 16, 19, 22 };
            _mills[13] = new int[] { 8, 12, 17 };
            _mills[14] = new int[] { 5, 13, 20 };
            _mills[15] = new int[] { 2, 14, 23 };
        }

        private Int32[] GetNeighbours(Int32 i)
        {
            return i switch
            {
                0 => new int[] { 1, 9 },
                1 => new int[] { 0, 2, 4 },
                2 => new int[] { 1, 14 },
                3 => new int[] { 4, 10 },
                4 => new int[] { 1, 3, 5, 7 },
                5 => new int[] { 4, 13 },
                6 => new int[] { 7, 11 },
                7 => new int[] { 4, 6, 8 },
                8 => new int[] { 7, 12 },
                9 => new int[] { 0, 10, 21 },
                10 => new int[] { 3, 9, 11, 18 },
                11 => new int[] { 6, 10, 15 },
                12 => new int[] { 8, 13, 17 },
                13 => new int[] { 5, 12, 14, 20 },
                14 => new int[] { 2, 23 },
                15 => new int[] { 11, 16 },
                16 => new int[] { 15, 17, 19 },
                17 => new int[] { 12, 16 },
                18 => new int[] { 10, 19 },
                19 => new int[] { 16, 18, 20, 22 },
                20 => new int[] { 13, 19 },
                21 => new int[] { 9, 22 },
                22 => new int[] { 19, 21, 23 },
                23 => new int[] { 14, 22 },
                _ => Array.Empty<int>(),
            };
        }
        #endregion
    }
}

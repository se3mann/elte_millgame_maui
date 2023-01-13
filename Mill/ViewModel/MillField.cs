using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mill.ViewModel
{
    public class MillField : ViewModelBase
    {
        #region Fields

        private int _player;
        
        #endregion

        #region Properties

        public int Player {
            get { return _player; } 
            set
            {
                if (_player != value)
                {
                    _player = value;
                    OnPropertyChanged();
                }
            } 
        }

        public int X { get; set; }

        public int Y { get; set; }

        public bool InGame { get; set; }

        public int Number { get; set; }    

        public DelegateCommand StepCommand { get; set; }

        #endregion
    }
}

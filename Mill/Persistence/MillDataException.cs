using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mill.Persistence
{
    public class MillDataException : Exception
    {
        public MillDataException(String path)
        {
            _path = path;
        }

        String _path;
        public String Path { get { return _path; } }
    }
}

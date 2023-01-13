using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mill.Persistence
{
    public interface IMillDataAccess
    {
        Task<MillTable> LoadAsync(String path);
        Task SaveAsync(String path, MillTable table);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtuoPhone;

public class PointerMemory
{
    private List<int> streams = new List<int>();
    public IEnumerable<int> GetStreamList() => streams;
    public void ClearStreamList() => streams.Clear();
}

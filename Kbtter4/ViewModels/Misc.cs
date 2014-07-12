using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter4.ViewModels
{
    public class FileDragDropResult
    {
        public string[] Files { get; private set; }

        public FileDragDropResult(string[] f)
        {
            Files = f;
        }
    }
}

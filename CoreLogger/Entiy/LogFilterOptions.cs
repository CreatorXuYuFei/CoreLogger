using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLogger.Entiy
{
    public class LogFilterOptions
    {
        public bool Enabled { get; set; } = true;
        public List<string> IncludeModules { get; set; } = [];
        public List<string> ExcludeModules { get; set; } = [];
        public List<string> BlockKeywords { get; set; } = [];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jf.Models
{
    public class IdentifyObject
    {
        public string Name { get; set; }
        public double Confidence { get; set; }
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
    }
}

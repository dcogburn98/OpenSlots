using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSlots
{
    public class PayTable
    {
        public Reel.Tokens[] Line;
        public int Multiplier;

        public PayTable(Reel.Tokens[] Line, int Multiplier)
        {
            this.Line = Line;
            this.Multiplier = Multiplier;
        }
    }
}

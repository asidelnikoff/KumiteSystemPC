using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentsBracketsBase
{
    public interface IRound
    {
        List<IMatch> Matches { get;  }
        int ID { get; set; }
        bool IsFinished();
    }
}

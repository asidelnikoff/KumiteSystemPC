using SharedComponentsLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentsBracketsBase;

namespace SharedComponentsLibrary
{
    public interface ICategoryViewer
    {
        public Action<RoundDTO, IMatch> GotMatch { get; set; }
        public Action<RoundDTO, IMatch> GotNextMatch { get; set; }
        public Action<IList<ICompetitor>> GotCategoryResults { get; set; }
        public void WriteMatchResults(RoundDTO round, IMatch match);
        public void LoadMatch(RoundDTO round, IMatch match);
    }
}

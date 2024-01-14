using SharedComponentsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary.DTO
{
    public class RoundDTO
    {
        public long Id { get; set; }

        public long Category { get; set; }

        public long? Repechage { get; set; }

        public long MatchesCount { get; set; }

        public override string ToString()
        {
            if (Repechage == null || Repechage == -1)
                return $"Round {Id + 1}";
            else if (Repechage == 0)
                return "Repechage 1";
            else if (Repechage == 1)
                return "Repechage 2";
            else if (Repechage == 2)
                return "Bronze Match";

            return $"Round {Id}";
        }
    }
}

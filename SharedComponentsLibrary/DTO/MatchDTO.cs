using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary.DTO
{
    internal class MatchDTO
    {
        public long Id { get; set; }

        public long Round { get; set; }

        public long Category { get; set; }

        public long? Aka { get; set; }

        public long? Ao { get; set; }

        public long? Winner { get; set; }

        public long? Looser { get; set; }

        public long? IsFinished { get; set; }

        public string? AkaScore { get; set; }

        public string? AoScore { get; set; }

        public long? AkaC1 { get; set; }

        public long? AoC1 { get; set; }

        public long? AkaC2 { get; set; }

        public long? AoC2 { get; set; }

        public long? Senshu { get; set; }
    }
}

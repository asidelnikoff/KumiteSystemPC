using System;
using System.Collections.Generic;

namespace SharedComponentsLibrary.Models;

public partial class Match
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

    public virtual Competitor? AkaNavigation { get; set; }

    public virtual Competitor? AoNavigation { get; set; }

    public virtual Competitor? LooserNavigation { get; set; }

    public virtual Round RoundNavigation { get; set; } = null!;

    public virtual Competitor? WinnerNavigation { get; set; }
}

using System;
using System.Collections.Generic;

namespace SharedComponentsLibrary.Models;

public partial class Round
{
    public long Id { get; set; }

    public long Category { get; set; }

    public long? Repechage { get; set; }

    public virtual Category CategoryNavigation { get; set; } = null!;

    public virtual ICollection<Match> Matches { get; } = new List<Match>();
}

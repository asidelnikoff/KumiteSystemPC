using System;
using System.Collections.Generic;

namespace SharedComponentsLibrary.Models;

public partial class CompetitorCategory
{
    public long Id { get; set; }

    public long Category { get; set; }

    public long Competitor { get; set; }

    public long CompetitorStatus { get; set; }

        
    public virtual Category CategoryNavigation { get; set; } = null!;

    public virtual Competitor CompetitorNavigation { get; set; } = null!;
}

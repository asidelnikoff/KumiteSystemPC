using System;
using System.Collections.Generic;

namespace SharedComponentsLibrary.Models;

public partial class Winner
{
    public long Id { get; set; }

    public long Category { get; set; }

    public long Competitor { get; set; }

    public long? Place { get; set; }

    public virtual Category CategoryNavigation { get; set; } = null!;

    public virtual Competitor CompetitorNavigation { get; set; } = null!;
}

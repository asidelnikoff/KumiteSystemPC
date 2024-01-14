using System;
using System.Collections.Generic;

namespace SharedComponentsLibrary.Models;

public partial class Competitor
{
    public long Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Club { get; set; }

    public long? Status { get; set; }

    public long? IsBye { get; set; }

    public virtual ICollection<CompetitorCategory> CompetitorCategories { get; } = new List<CompetitorCategory>();

    public virtual ICollection<Match> MatchAkaNavigations { get; } = new List<Match>();

    public virtual ICollection<Match> MatchAoNavigations { get; } = new List<Match>();

    public virtual ICollection<Match> MatchLooserNavigations { get; } = new List<Match>();

    public virtual ICollection<Match> MatchWinnerNavigations { get; } = new List<Match>();

    public virtual ICollection<Winner> Winners { get; } = new List<Winner>();
}

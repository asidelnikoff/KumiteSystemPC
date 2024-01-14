using System;
using System.Collections.Generic;

namespace SharedComponentsLibrary.Models;

public partial class Category
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public long Tournament { get; set; }

    public long Type { get; set; }

    public virtual ICollection<CompetitorCategory> CompetitorCategories { get; } = new List<CompetitorCategory>();

    public virtual ICollection<Round> Rounds { get; } = new List<Round>();

    public virtual Tournament TournamentNavigation { get; set; } = null!;

    public virtual ICollection<Winner> Winners { get; } = new List<Winner>();
}

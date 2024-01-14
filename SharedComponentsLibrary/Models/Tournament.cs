using System;
using System.Collections.Generic;

namespace SharedComponentsLibrary.Models;

public partial class Tournament
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Category> Categories { get; } = new List<Category>();

    public override string ToString()
    {
        return Name;
    }
}

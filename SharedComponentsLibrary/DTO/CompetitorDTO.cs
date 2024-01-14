using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary.DTO
{
    public class CompetitorDTO
    {
        public long Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? Club { get; set; }

        public long? Status { get; set; }

        public long? IsBye { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}

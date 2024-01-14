using SharedComponentsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageLibrary;

namespace SharedComponentsLibrary.DTO
{
    public class CategoryDTO
    {
        public static Dictionary<long, string> CategoryTypes = new Dictionary<long, string>()
        {
            { 0, Resources.SingleElimination2 },
            { 1, Resources.SingleElimination1 },
            { 2, Resources.RoundRobin },
        };
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public long Type { get; set; }

        public long Tournament { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

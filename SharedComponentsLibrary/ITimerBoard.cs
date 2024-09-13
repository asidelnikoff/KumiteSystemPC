using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponentsLibrary
{
    public interface ITimerBoard
    {
        public void Close();
        public Action Closed { get; set; }
    }
}

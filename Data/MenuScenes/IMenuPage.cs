using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.MenuScenes
{
    internal interface IMenuPage
    {
        bool Visible { get; set; }
        void OnOpened();
    }
}

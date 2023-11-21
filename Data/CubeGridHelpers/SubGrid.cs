using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.CubeGridHelpers
{
    /// <summary>
    /// CubeGrid specifically designed for being connected to another CubeGrid.
    /// </summary>
    public partial class SubGrid : CubeGrid
    {
        public CubeGrid Parent { get; internal set; }

        public override void _Ready()
        {
            base._Ready();
            Parent = GetParent<CubeGrid>();
            if (Parent == null)
                Close();

            Parent.subGrids.Add(this);
        }

        public override void Close()
        {
            base.Close();
            Parent.subGrids.Remove(this);
        }

        public CubeGrid GetRootGrid()
        {
            if (Parent is SubGrid subGrid)
                return subGrid.GetRootGrid();
            return Parent;
        }
    }
}

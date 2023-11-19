using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellacrum.Data.CubeObjects.Power
{
    public partial class ReactorMagnet : PowerConsumer
    {
        GpuParticlesAttractorBox3D fwdBox;
        //GpuParticlesAttractorBox3D downBox;

        public ReactorMagnet(string subTypeId, Godot.Collections.Dictionary<string, Variant> blockData, bool verbose = false) : base(subTypeId, blockData, verbose)
        {
            fwdBox = new GpuParticlesAttractorBox3D()
            {
                Size = Vector3.One * 2.5f,
                Strength = 400,
                Directionality = 1,
            };

            AddChild(fwdBox);
        }
    }
}

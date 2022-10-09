using System.Collections.Generic;
using Verse;
using VFE.Mechanoids;

namespace MechanoidFoundry
{
    public class CompMechanoidPad : CompMachineChargingStation
    {
        public override void SpawnMyPawn()
        {
            wantsRespawn = false;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

        }

    }
}


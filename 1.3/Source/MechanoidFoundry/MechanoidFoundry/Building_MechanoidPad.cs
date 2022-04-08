using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MechanoidFoundry
{
    public class Building_MechanoidPad : Building_Bed
    {
        public CompPowerTrader powerComp;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.Medical = false;
        }

        public bool IsActive => this.powerComp.PowerOn;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (!(gizmo is Command_Toggle toggleCommand && (toggleCommand.icon.name == "AsMedical")))
                {
                    yield return gizmo;
                }
            }
        }
    }
}

